"""Generate code by invoking the Copilot CLI for each skill configuration."""

from __future__ import annotations

import shutil
import subprocess
from pathlib import Path

import click

from skill_eval.config import Configuration, EvalConfig
from skill_eval.prompt_renderer import render_generate_prompt
from skill_eval.skill_manager import add_skill_directories, remove_skill_directories


def _run_copilot(prompt: str, configuration: Configuration) -> None:
    """Invoke the Copilot CLI with the given prompt and configuration."""
    cmd = ["copilot", "-p", prompt, "--yolo"]

    for plugin in configuration.plugins:
        cmd.extend(["--plugin-dir", plugin])

    click.echo(f"  Running: copilot -p <prompt> --yolo" + (
        f" --plugin-dir {' '.join(configuration.plugins)}"
        if configuration.plugins else ""
    ))

    result = subprocess.run(cmd)
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

    for cfg in configs_to_run:
        config_output = output_base / cfg.name
        label = cfg.label or cfg.name

        click.echo(f"\n{'=' * 60}")
        click.echo(f"Generating: {label}")
        click.echo(f"Output:     {config_output}")
        click.echo(f"{'=' * 60}")

        # Clean previous output for this configuration
        if config_output.exists():
            click.echo(f"  Removing previous output: {config_output}")
            shutil.rmtree(config_output)
        config_output.mkdir(parents=True, exist_ok=True)

        # Render the generation prompt
        prompt = render_generate_prompt(config, cfg.name, project_root)

        # Register skills and run Copilot, always cleaning up
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

            _run_copilot(prompt, cfg)

        finally:
            if added_skills:
                remove_skill_directories(added_skills)
                click.echo("  Unregistered skills")

        click.echo(f"  ✅ Done: {label}")

    click.echo(f"\n{'=' * 60}")
    click.echo(f"Generation complete: {len(configs_to_run)} configuration(s)")
    click.echo(f"{'=' * 60}")
