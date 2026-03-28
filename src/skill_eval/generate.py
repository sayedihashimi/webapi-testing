"""Generate code by invoking the Copilot CLI for each skill configuration.

Each configuration runs inside an isolated staging directory so the Copilot
CLI only auto-discovers the skills/plugins that belong to that configuration.
Without this isolation, Copilot discovers *all* ``skills/`` and ``plugins/``
directories in the project root, contaminating configurations that should
have fewer (or no) skills.
"""

from __future__ import annotations

import os
import platform
import shutil
import subprocess
import sys
import tempfile
import threading
import time
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig
from skill_eval.prompt_renderer import render_generate_prompt
from skill_eval.skill_manager import add_skill_directories, remove_skill_directories

# Default: kill Copilot if no CPU activity for this many seconds
_IDLE_TIMEOUT = 300  # 5 minutes


# ---------------------------------------------------------------------------
# Staging-directory helpers
# ---------------------------------------------------------------------------

def _link_directory(target: Path, link: Path) -> None:
    """Create a directory junction (Windows) or symlink (other platforms).

    Windows junctions do not require elevated privileges.
    """
    if platform.system() == "Windows":
        subprocess.run(
            ["cmd", "/c", "mklink", "/J", str(link), str(target.resolve())],
            check=True,
            capture_output=True,
        )
    else:
        os.symlink(target.resolve(), link)


def _create_staging_dir(
    project_root: Path,
    cfg: Configuration,
) -> tuple[Path, list[Path]]:
    """Create an isolated working directory for a Copilot invocation.

    The staging directory contains directory links to ``prompts/`` and
    ``output/`` (so relative paths in the prompt still resolve correctly)
    and *only* the ``skills/`` and ``plugins/`` that the configuration
    declares.  This prevents Copilot from auto-discovering skills/plugins
    that belong to other configurations.

    Returns ``(staging_dir, created_links)`` so the caller can clean up.
    """
    staging = Path(tempfile.mkdtemp(prefix=f"skill-eval-{cfg.name}-"))
    links: list[Path] = []

    # Link prompts/ so @file references in the prompt resolve
    prompts_src = project_root / "prompts"
    if prompts_src.exists():
        prompts_link = staging / "prompts"
        _link_directory(prompts_src, prompts_link)
        links.append(prompts_link)

    # Link output/ so generated code lands in the real output tree
    output_src = project_root / "output"
    output_src.mkdir(parents=True, exist_ok=True)
    output_link = staging / "output"
    _link_directory(output_src, output_link)
    links.append(output_link)

    # Only surface the skills declared by this configuration
    if cfg.skills:
        skills_dir = staging / "skills"
        skills_dir.mkdir()
        for skill_rel in cfg.skills:
            skill_src = (project_root / skill_rel).resolve()
            skill_name = Path(skill_rel).name
            link = skills_dir / skill_name
            _link_directory(skill_src, link)
            links.append(link)

    # Only surface the plugins declared by this configuration
    if cfg.plugins:
        plugins_dir = staging / "plugins"
        plugins_dir.mkdir()
        for plugin_rel in cfg.plugins:
            plugin_src = (project_root / plugin_rel).resolve()
            plugin_name = Path(plugin_rel).name
            link = plugins_dir / plugin_name
            _link_directory(plugin_src, link)
            links.append(link)

    return staging, links


def _rmtree(path: Path) -> None:
    """Remove a directory tree, handling locked files on Windows.

    Falls back to ``rd /s /q`` on Windows when ``shutil.rmtree`` fails
    (e.g. because dotnet build/run left DLLs locked).
    """
    try:
        shutil.rmtree(path)
    except PermissionError:
        if platform.system() == "Windows":
            subprocess.run(
                ["cmd", "/c", "rd", "/s", "/q", str(path)],
                check=False,
                capture_output=True,
            )
            if path.exists():
                raise
        else:
            raise


def _cleanup_staging_dir(staging: Path, links: list[Path]) -> None:
    """Remove the staging directory, safely removing junctions/symlinks first.

    ``os.rmdir`` removes junctions and symlinks without following them.
    """
    for link in links:
        try:
            os.rmdir(link)
        except OSError:
            pass
    shutil.rmtree(staging, ignore_errors=True)


# ---------------------------------------------------------------------------
# Copilot invocation
# ---------------------------------------------------------------------------

def _run_copilot(
    prompt: str,
    configuration: Configuration,
    *,
    cwd: Path | None = None,
    project_root: Path | None = None,
    idle_timeout: int = _IDLE_TIMEOUT,
    max_retries: int = 1,
) -> None:
    """Invoke the Copilot CLI with a watchdog that kills hung processes.

    Monitors the Copilot process CPU usage. If the process is idle
    (no CPU consumption) for *idle_timeout* seconds, it is killed and
    retried up to *max_retries* times.
    """
    cmd = ["copilot", "-p", prompt, "--yolo"]

    for plugin in configuration.plugins:
        if project_root:
            abs_plugin = str((project_root / plugin).resolve())
        else:
            abs_plugin = plugin
        cmd.extend(["--plugin-dir", abs_plugin])

    click.echo(f"  Running: copilot -p <prompt> --yolo" + (
        f" --plugin-dir {' '.join(configuration.plugins)}"
        if configuration.plugins else ""
    ))
    if cwd:
        click.echo(f"  Working directory: {cwd}")

    for attempt in range(1 + max_retries):
        if attempt > 0:
            click.echo(f"  ⚠️  Retry {attempt}/{max_retries} after idle timeout")

        proc = subprocess.Popen(cmd, cwd=cwd)
        timed_out = _watchdog_wait(proc, idle_timeout)

        if timed_out:
            click.echo(f"  ⚠️  Copilot idle for {idle_timeout}s — killing (PID {proc.pid})")
            _kill_process_tree(proc)
            if attempt < max_retries:
                continue
            raise RuntimeError(
                f"Copilot CLI hung after {max_retries + 1} attempts "
                f"for configuration '{configuration.name}'"
            )

        if proc.returncode != 0:
            raise RuntimeError(
                f"Copilot CLI exited with code {proc.returncode} "
                f"for configuration '{configuration.name}'"
            )
        return  # success


def _watchdog_wait(proc: subprocess.Popen, idle_timeout: int) -> bool:
    """Wait for *proc* to finish, killing it if idle too long.

    Returns True if the process was killed due to idle timeout.
    Uses a background thread to monitor CPU usage via psutil-like
    approach (polling /proc or tasklist).
    """
    idle_since = time.monotonic()
    last_cpu = _get_process_cpu(proc.pid)
    poll_interval = 15  # seconds between CPU checks

    while proc.poll() is None:
        time.sleep(poll_interval)
        if proc.poll() is not None:
            break

        current_cpu = _get_process_cpu(proc.pid)
        if current_cpu is None:
            break  # process gone

        if current_cpu != last_cpu:
            idle_since = time.monotonic()
            last_cpu = current_cpu
        elif time.monotonic() - idle_since > idle_timeout:
            return True  # timed out

    return False


def _get_process_cpu(pid: int) -> float | None:
    """Get cumulative CPU time for a process. Returns None if process not found."""
    try:
        if sys.platform == "win32":
            result = subprocess.run(
                ["powershell", "-NoProfile", "-Command",
                 f"(Get-Process -Id {pid} -ErrorAction SilentlyContinue).CPU"],
                capture_output=True, text=True, timeout=10,
            )
            val = result.stdout.strip()
            return float(val) if val else None
        else:
            stat = Path(f"/proc/{pid}/stat")
            if stat.exists():
                fields = stat.read_text().split()
                utime = int(fields[13])
                stime = int(fields[14])
                return float(utime + stime)
            return None
    except Exception:
        return None


def _kill_process_tree(proc: subprocess.Popen) -> None:
    """Kill a process and its children."""
    if sys.platform == "win32":
        try:
            subprocess.run(
                f"taskkill /F /T /PID {proc.pid}",
                shell=True, capture_output=True, timeout=10,
            )
        except Exception:
            pass
    else:
        import signal as _signal
        try:
            os.killpg(os.getpgid(proc.pid), _signal.SIGKILL)
        except (ProcessLookupError, OSError):
            pass
    try:
        proc.wait(timeout=10)
    except Exception:
        pass


def run_generate(
    config: EvalConfig,
    project_root: Path,
    configurations: list[str] | None = None,
    resume: bool = False,
) -> None:
    """Generate code for each configuration defined in the config.

    Each scenario is generated in its own Copilot invocation so that
    large skills/plugins don't exhaust the context window before later
    scenarios can be built.

    When ``config.runs > 1``, each configuration is generated N times with
    output stored under ``output/{config}/run-{N}/{scenario}/``.

    Args:
        config: The parsed evaluation configuration.
        project_root: Root directory of the evaluation project.
        configurations: Optional list of configuration names to run.
                        If None, runs all configurations.
        resume: If True, skip runs where output already exists.
    """
    configs_to_run = config.configurations
    if configurations:
        valid_names = {c.name for c in config.configurations}
        invalid = set(configurations) - valid_names
        if invalid:
            raise click.ClickException(
                f"Unknown configuration(s): {', '.join(sorted(invalid))}. "
                f"Valid: {', '.join(sorted(valid_names))}"
            )
        configs_to_run = [c for c in config.configurations if c.name in configurations]

    output_base = project_root / config.output.directory
    total_scenarios = len(config.scenarios)
    num_runs = config.runs

    for cfg in configs_to_run:
        label = cfg.label or cfg.name

        click.echo(f"\n{'=' * 60}")
        click.echo(f"Generating: {label}")
        click.echo(f"Runs:       {num_runs}")
        click.echo(f"Scenarios:  {total_scenarios} available (1 randomly selected per run)")
        click.echo(f"{'=' * 60}")

        # Register skills once for this config
        staging_dir, staging_links = _create_staging_dir(project_root, cfg)
        added_skills: list[Path] = []
        try:
            if cfg.skills:
                skill_paths = [project_root / s for s in cfg.skills]
                added_skills = add_skill_directories(skill_paths)
                if added_skills:
                    click.echo(
                        f"  Registered skills: "
                        f"{', '.join(str(p) for p in added_skills)}"
                    )

            for run_id in range(1, num_runs + 1):
                run_output = output_base / cfg.name / f"run-{run_id}"

                # Resume support: skip if output exists
                if resume and run_output.exists() and any(run_output.iterdir()):
                    click.echo(f"\n  ⏭️  Run {run_id}/{num_runs} — skipping (output exists)")
                    continue

                # Select one scenario for this run (round-robin for coverage)
                scenario = config.scenarios[(run_id - 1) % total_scenarios]

                click.echo(f"\n  --- Run {run_id}/{num_runs} → {scenario.name} ---")

                # Clean previous output for this run
                if run_output.exists():
                    _rmtree(run_output)
                run_output.mkdir(parents=True, exist_ok=True)

                scenario_output = run_output / scenario.name
                scenario_output.mkdir(parents=True, exist_ok=True)

                click.echo(f"    {scenario.description}")

                prompt = render_generate_prompt(
                    config, cfg.name, project_root,
                    scenario=scenario, run_id=run_id,
                )
                try:
                    _run_copilot(
                        prompt, cfg, cwd=staging_dir, project_root=project_root,
                    )
                    click.echo(f"    ✅ {scenario.name} done")
                except RuntimeError as e:
                    click.echo(f"    ❌ {scenario.name} failed: {e}")
                    continue

        finally:
            if added_skills:
                remove_skill_directories(added_skills)
                click.echo("  Unregistered skills")
            _cleanup_staging_dir(staging_dir, staging_links)
            click.echo(f"  Cleaned up staging directory")

        click.echo(f"  ✅ Done: {label}")

    click.echo(f"\n{'=' * 60}")
    click.echo(f"Generation complete: {len(configs_to_run)} configuration(s) × {num_runs} run(s)")
    click.echo(f"{'=' * 60}")
