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
import tempfile
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig
from skill_eval.prompt_renderer import render_generate_prompt
from skill_eval.skill_manager import add_skill_directories, remove_skill_directories


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
) -> None:
    """Invoke the Copilot CLI with the given prompt and configuration."""
    cmd = ["copilot", "-p", prompt, "--yolo"]

    # Resolve plugin paths to absolute so they work from the staging dir
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

    result = subprocess.run(cmd, cwd=cwd)
    if result.returncode != 0:
        raise RuntimeError(
            f"Copilot CLI exited with code {result.returncode} "
            f"for configuration '{configuration.name}'"
        )


def run_generate(
    config: EvalConfig,
    project_root: Path,
    configurations: list[str] | None = None,
) -> None:
    """Generate code for each configuration defined in the config.

    Each scenario is generated in its own Copilot invocation so that
    large skills/plugins don't exhaust the context window before later
    scenarios can be built.

    Args:
        config: The parsed evaluation configuration.
        project_root: Root directory of the evaluation project.
        configurations: Optional list of configuration names to run.
                        If None, runs all configurations.
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

    for cfg in configs_to_run:
        config_output = output_base / cfg.name
        label = cfg.label or cfg.name

        click.echo(f"\n{'=' * 60}")
        click.echo(f"Generating: {label}")
        click.echo(f"Output:     {config_output}")
        click.echo(f"Scenarios:  {total_scenarios} (one Copilot invocation each)")
        click.echo(f"{'=' * 60}")

        # Clean previous output for this configuration
        if config_output.exists():
            click.echo(f"  Removing previous output: {config_output}")
            _rmtree(config_output)
        config_output.mkdir(parents=True, exist_ok=True)

        # Create staging dir once per config and register skills
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

            # Generate each scenario in its own Copilot invocation
            for i, scenario in enumerate(config.scenarios, 1):
                scenario_output = config_output / scenario.name
                scenario_output.mkdir(parents=True, exist_ok=True)

                click.echo(f"\n  [{i}/{total_scenarios}] {scenario.name}")
                click.echo(f"    {scenario.description}")

                prompt = render_generate_prompt(
                    config, cfg.name, project_root, scenario=scenario,
                )
                _run_copilot(
                    prompt, cfg, cwd=staging_dir, project_root=project_root,
                )
                click.echo(f"    ✅ {scenario.name} done")

        finally:
            if added_skills:
                remove_skill_directories(added_skills)
                click.echo("  Unregistered skills")
            _cleanup_staging_dir(staging_dir, staging_links)
            click.echo(f"  Cleaned up staging directory")

        click.echo(f"  ✅ Done: {label}")

    click.echo(f"\n{'=' * 60}")
    click.echo(f"Generation complete: {len(configs_to_run)} configuration(s)")
    click.echo(f"{'=' * 60}")
