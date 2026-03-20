"""Render Jinja2 prompt templates from eval.yaml configuration."""

from __future__ import annotations

from pathlib import Path

from jinja2 import Environment, FileSystemLoader

from skill_eval.config import EvalConfig

# Templates ship with the repo in the templates/ directory.
# Users can override by placing templates in their project root.
_BUNDLED_TEMPLATES = Path(__file__).resolve().parent.parent.parent / "templates"


def _get_env(project_root: Path) -> Environment:
    """Build a Jinja2 environment that searches project-local then bundled templates."""
    search_paths = []

    local_templates = project_root / "templates"
    if local_templates.is_dir():
        search_paths.append(str(local_templates))

    if _BUNDLED_TEMPLATES.is_dir():
        search_paths.append(str(_BUNDLED_TEMPLATES))

    if not search_paths:
        raise FileNotFoundError(
            "No templates directory found. Expected templates/ in your project root "
            f"or the bundled templates at {_BUNDLED_TEMPLATES}"
        )

    return Environment(
        loader=FileSystemLoader(search_paths),
        keep_trailing_newline=True,
        trim_blocks=True,
        lstrip_blocks=True,
    )


def render_generate_prompt(
    config: EvalConfig,
    configuration_name: str,
    project_root: Path,
) -> str:
    """Render the generation prompt for a specific configuration."""
    env = _get_env(project_root)
    template = env.get_template("create-all-apps.md.j2")
    return template.render(
        scenarios=config.scenarios,
        output_directory=f"{config.output.directory}/{configuration_name}",
    )


def render_analyze_prompt(config: EvalConfig, project_root: Path) -> str:
    """Render the analysis prompt from configured dimensions."""
    env = _get_env(project_root)
    template = env.get_template("analyze.md.j2")
    return template.render(
        scenarios=config.scenarios,
        configurations=config.configurations,
        dimensions=config.dimensions,
        output_directory=config.output.directory,
        reports_directory=config.output.reports_directory,
        analysis_file=config.output.analysis_file,
    )


def render_scenario_template(scenario_name: str, project_root: Path) -> str:
    """Render a starter scenario prompt template."""
    env = _get_env(project_root)
    template = env.get_template("scenario.prompt.md.j2")
    return template.render(scenario_name=scenario_name)
