"""Generate improvement suggestions for skill/plugin configurations."""

from __future__ import annotations

import os
import subprocess
import time
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig
from skill_eval.prompt_renderer import render_improvement_prompt
from skill_eval.source_resolver import SourceResolver


def _suggest_for_configuration(
    config: EvalConfig,
    configuration: Configuration,
    project_root: Path,
    resolver: SourceResolver,
    model: str,
    focus_dimensions: list[str] | None = None,
    idle_timeout: int = 600,
    research_dir: Path | None = None,
    lessons_context: str | None = None,
) -> tuple[str, bool]:
    """Generate improvement suggestions for a single configuration.

    Returns (config_name, success).
    """
    from skill_eval.generate import _kill_process_tree, _watchdog_wait

    reports_dir = project_root / config.output.reports_directory
    improvements_file = config.output.improvements_file_pattern.format(
        config=configuration.name
    )
    improvements_path = reports_dir / improvements_file

    # Resolve skill/plugin paths
    skill_paths = [resolver.resolve(ref) for ref in configuration.skills]
    plugin_paths = [resolver.resolve(ref) for ref in configuration.plugins]

    if not skill_paths and not plugin_paths:
        click.echo(
            f"  ⚠️  {configuration.name}: no skills or plugins to analyze, skipping"
        )
        return configuration.name, False

    prompt = render_improvement_prompt(
        config, configuration, project_root, skill_paths, plugin_paths,
        focus_dimensions=focus_dimensions,
        research_dir=research_dir,
        lessons_context=lessons_context,
    )

    cmd = ["copilot", "-p", prompt, "--yolo"]
    if model:
        cmd.extend(["--model", model])

    env = {**os.environ, "NODE_OPTIONS": "--max-old-space-size=8192"}

    start_time = time.time()
    proc = subprocess.Popen(cmd, cwd=project_root, env=env)

    timed_out = _watchdog_wait(proc, idle_timeout)
    elapsed = time.time() - start_time

    if timed_out:
        _kill_process_tree(proc)
        return configuration.name, False

    success = improvements_path.exists()
    if proc.returncode != 0 and success:
        click.echo(
            f"  ⚠️  {configuration.name}: Copilot exited with code "
            f"{proc.returncode} but improvements file was written — treating as success"
        )

    if success:
        mins, secs = divmod(int(elapsed), 60)
        click.echo(f"  ✅ {configuration.name}: improvements written ({mins}m {secs}s)")
    else:
        click.echo(f"  ❌ {configuration.name}: improvements file not written")

    return configuration.name, success


def run_suggest_improvements(
    config: EvalConfig,
    project_root: Path,
    resolver: SourceResolver,
    model_override: str | None = None,
    focus_dimensions: list[str] | None = None,
    research_dir: Path | None = None,
    lessons_context: str | None = None,
) -> None:
    """Generate improvement suggestions for all configurations marked with suggest_improvements."""
    targets = config.improvement_targets
    if not targets:
        click.echo("\n  ℹ️  No configurations have suggest_improvements: true — skipping")
        return

    reports_dir = project_root / config.output.reports_directory
    reports_dir.mkdir(parents=True, exist_ok=True)

    # Check that analysis has been run
    analysis_path = reports_dir / config.output.analysis_file
    if not analysis_path.exists():
        raise click.ClickException(
            f"Analysis report not found: {analysis_path}\n"
            "Run 'skill-eval analyze' first to generate the analysis report."
        )

    model = model_override or config.effective_improvement_model

    click.echo(f"\n{'=' * 60}")
    click.echo("Generating improvement suggestions")
    click.echo(f"{'=' * 60}")
    click.echo(f"  Targets: {', '.join(t.name for t in targets)}")
    click.echo(f"  Model:   {model}")
    if research_dir:
        click.echo(f"  Research: {research_dir}")

    max_retries = 2
    for configuration in targets:
        click.echo(f"\n  --- {configuration.name} ({configuration.label}) ---")
        succeeded = False
        for attempt in range(1, max_retries + 1):
            if attempt > 1:
                click.echo(f"  🔄 Retry {attempt}/{max_retries} for {configuration.name}")
                # Remove partial output from previous attempt
                partial = reports_dir / config.output.improvements_file_pattern.format(
                    config=configuration.name
                )
                if partial.exists():
                    partial.unlink()
            try:
                _, success = _suggest_for_configuration(
                    config, configuration, project_root, resolver, model,
                    focus_dimensions=focus_dimensions,
                    research_dir=research_dir,
                    lessons_context=lessons_context,
                )
                if success:
                    succeeded = True
                    break
                else:
                    click.echo(
                        f"  ⚠️  {configuration.name}: suggestion generation failed or timed out"
                    )
            except Exception as e:
                click.echo(f"  ❌ {configuration.name} error: {e}")

        if not succeeded:
            click.echo(
                f"  ❌ {configuration.name}: failed after {max_retries} attempts"
            )

    click.echo(f"\n  --- Improvement suggestions complete ---")
    for configuration in targets:
        imp_file = config.output.improvements_file_pattern.format(
            config=configuration.name
        )
        imp_path = reports_dir / imp_file
        if imp_path.exists():
            click.echo(f"  📄 {imp_path}")
        else:
            click.echo(f"  ❌ {imp_path} (not generated)")
