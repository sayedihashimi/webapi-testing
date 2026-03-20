"""Verify generated projects by running build and run commands."""

from __future__ import annotations

import re
import signal
import subprocess
import time
from datetime import datetime, timezone
from pathlib import Path
from urllib.error import URLError
from urllib.request import urlopen

import click

from skill_eval.config import EvalConfig


def _run_build(
    command: str,
    project_dir: Path,
    working_directory: str,
    success_pattern: str | None,
) -> tuple[bool, str]:
    """Run the build command and return (success, output)."""
    cwd = project_dir / working_directory
    try:
        result = subprocess.run(
            command,
            shell=True,
            cwd=cwd,
            capture_output=True,
            text=True,
            timeout=120,
        )
        output = result.stdout + result.stderr

        if result.returncode != 0:
            return False, output

        if success_pattern and not re.search(success_pattern, output):
            return False, f"Build exited 0 but success pattern not found: {success_pattern}\n{output}"

        return True, output

    except subprocess.TimeoutExpired:
        return False, "Build timed out after 120 seconds"
    except Exception as e:
        return False, str(e)


def _run_app(
    command: str,
    project_dir: Path,
    timeout_seconds: int,
    health_check_url: str | None,
    expected_status: int,
) -> tuple[bool, str]:
    """Start the app, optionally check health, then stop it."""
    try:
        proc = subprocess.Popen(
            command,
            shell=True,
            cwd=project_dir,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )

        # Wait for the app to start
        time.sleep(timeout_seconds)

        # Check if the process is still running
        if proc.poll() is not None:
            stdout = proc.stdout.read().decode() if proc.stdout else ""
            stderr = proc.stderr.read().decode() if proc.stderr else ""
            return False, f"App exited early (code {proc.returncode}):\n{stdout}\n{stderr}"

        # Optional health check
        if health_check_url:
            try:
                resp = urlopen(health_check_url, timeout=10)
                if resp.status != expected_status:
                    _terminate(proc)
                    return False, f"Health check returned {resp.status}, expected {expected_status}"
            except URLError as e:
                _terminate(proc)
                return False, f"Health check failed: {e}"

        _terminate(proc)
        return True, "App started and responded successfully"

    except Exception as e:
        return False, str(e)


def _terminate(proc: subprocess.Popen) -> None:
    """Terminate a subprocess gracefully."""
    proc.terminate()
    try:
        proc.wait(timeout=5)
    except subprocess.TimeoutExpired:
        proc.kill()
        proc.wait(timeout=5)


def run_verify(config: EvalConfig, project_root: Path) -> None:
    """Verify all generated projects build and run."""
    output_base = project_root / config.output.directory
    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)

    results: list[dict] = []

    for cfg in config.configurations:
        config_dir = output_base / cfg.name
        if not config_dir.exists():
            click.echo(f"⚠️  Skipping {cfg.name}: output directory not found")
            continue

        for scenario in config.scenarios:
            project_dir = config_dir / scenario.name
            if not project_dir.exists():
                results.append({
                    "config": cfg.name,
                    "scenario": scenario.name,
                    "build": "⚠️ Not found",
                    "run": "⚠️ Not found",
                    "notes": "Project directory not found",
                })
                continue

            label = f"{cfg.name}/{scenario.name}"
            click.echo(f"  Verifying: {label}")

            # Build
            build_ok, build_output = _run_build(
                config.verification.build.command,
                project_dir,
                config.verification.build.working_directory,
                config.verification.build.success_pattern,
            )
            build_status = "✅ Pass" if build_ok else "❌ Fail"
            click.echo(f"    Build: {build_status}")

            # Run (optional)
            run_status = "⏭️ Skipped"
            run_notes = ""
            if config.verification.run and build_ok:
                hc = config.verification.run.health_check
                run_ok, run_output = _run_app(
                    config.verification.run.command,
                    project_dir,
                    config.verification.run.timeout_seconds,
                    hc.url if hc else None,
                    hc.expected_status if hc else 200,
                )
                run_status = "✅ Pass" if run_ok else "❌ Fail"
                if not run_ok:
                    run_notes = run_output[:200]
                click.echo(f"    Run:   {run_status}")

            results.append({
                "config": cfg.name,
                "scenario": scenario.name,
                "build": build_status,
                "run": run_status,
                "notes": run_notes if not build_ok else (
                    build_output[:200] if not build_ok else ""
                ),
            })

    # Write build-notes.md
    notes_path = reports_dir / config.output.notes_file
    _write_build_notes(notes_path, config, results)
    click.echo(f"\n📝 Build notes written to: {notes_path}")


def _write_build_notes(
    path: Path,
    config: EvalConfig,
    results: list[dict],
) -> None:
    """Write the build/run verification report."""
    lines = [
        f"# Build & Run Verification Report",
        f"",
        f"**Evaluation:** {config.name}",
        f"**Date:** {datetime.now(timezone.utc).strftime('%Y-%m-%d %H:%M UTC')}",
        f"**Configurations:** {len(config.configurations)}",
        f"**Scenarios:** {len(config.scenarios)}",
        f"**Total projects:** {len(config.configurations) * len(config.scenarios)}",
        f"",
        f"## Results",
        f"",
        f"| Configuration | Scenario | Build | Run | Notes |",
        f"|---|---|---|---|---|",
    ]

    for r in results:
        notes = r.get("notes", "").replace("\n", " ")[:100]
        lines.append(
            f"| {r['config']} | {r['scenario']} | {r['build']} | {r['run']} | {notes} |"
        )

    lines.append("")

    # Skill configuration summary
    lines.extend([
        "## Skill Configurations",
        "",
        "| Configuration | Label | Skills | Plugins |",
        "|---|---|---|---|",
    ])
    for cfg in config.configurations:
        skills = ", ".join(cfg.skills) if cfg.skills else "None"
        plugins = ", ".join(cfg.plugins) if cfg.plugins else "None"
        lines.append(f"| {cfg.name} | {cfg.label} | {skills} | {plugins} |")

    lines.append("")
    path.write_text("\n".join(lines), encoding="utf-8")
