"""Render Jinja2 prompt templates from eval.yaml configuration."""

from __future__ import annotations

import json
from pathlib import Path

import yaml
from jinja2 import Environment, FileSystemLoader

from skill_eval.config import Configuration, EvalConfig, Scenario

# Templates are bundled inside the package for standalone installs.
# Also check the repo-root templates/ for development convenience.
_PACKAGE_TEMPLATES = Path(__file__).resolve().parent / "templates"
_REPO_TEMPLATES = Path(__file__).resolve().parent.parent.parent / "templates"


def _get_env(project_root: Path) -> Environment:
    """Build a Jinja2 environment that searches project-local then bundled templates."""
    search_paths = []

    local_templates = project_root / "templates"
    if local_templates.is_dir():
        search_paths.append(str(local_templates))

    # Prefer in-package templates (works with pip/pipx installs)
    if _PACKAGE_TEMPLATES.is_dir():
        search_paths.append(str(_PACKAGE_TEMPLATES))
    # Fall back to repo-root templates (development mode)
    elif _REPO_TEMPLATES.is_dir():
        search_paths.append(str(_REPO_TEMPLATES))

    if not search_paths:
        raise FileNotFoundError(
            "No templates directory found. Expected templates/ in your project root "
            f"or the bundled templates at {_PACKAGE_TEMPLATES}"
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
    scenario: Scenario | None = None,
    run_id: int | None = None,
) -> str:
    """Render the generation prompt for a specific configuration.

    If *scenario* is provided, renders a single-app prompt using
    ``create-single-app.md.j2``.  Otherwise falls back to the
    multi-app ``create-all-apps.md.j2`` template.

    If *run_id* is provided, output goes under a ``run-{N}`` subdirectory.
    """
    cfg = next(c for c in config.configurations if c.name == configuration_name)
    has_skills = bool(cfg.skills or cfg.plugins)
    env = _get_env(project_root)

    output_dir = f"{config.output.directory}/{configuration_name}"
    if run_id is not None:
        output_dir = f"{output_dir}/run-{run_id}"

    if scenario is not None:
        # Use analysis template when the scenario includes existing directories
        if scenario.include_directories:
            template = env.get_template("analyze-code.md.j2")
            return template.render(
                scenario=scenario,
                output_directory=output_dir,
                has_skills=has_skills,
                include_directories=scenario.include_directories,
            )
        template = env.get_template("create-single-app.md.j2")
        return template.render(
            scenario=scenario,
            output_directory=output_dir,
            has_skills=has_skills,
        )

    template = env.get_template("create-all-apps.md.j2")
    return template.render(
        scenarios=config.scenarios,
        output_directory=output_dir,
        has_skills=has_skills,
    )


def render_analyze_prompt(
    config: EvalConfig,
    project_root: Path,
    run_id: int | None = None,
) -> str:
    """Render the analysis prompt from configured dimensions.

    If *run_id* is provided, the prompt targets output under ``run-{N}``
    subdirectories for each configuration.

    Selects ``analyze-text.md.j2`` for text_output evals, otherwise
    ``analyze.md.j2``.
    """
    env = _get_env(project_root)
    template_name = "analyze-text.md.j2" if config.is_text_output else "analyze.md.j2"
    template = env.get_template(template_name)

    output_directory = config.output.directory
    run_suffix = f"/run-{run_id}" if run_id is not None else ""

    return template.render(
        scenarios=config.scenarios,
        configurations=config.configurations,
        dimensions=config.dimensions,
        output_directory=output_directory,
        reports_directory=config.output.reports_directory,
        analysis_file=config.output.analysis_file,
        run_suffix=run_suffix,
    )


def render_scenario_template(scenario_name: str, project_root: Path) -> str:
    """Render a starter scenario prompt template."""
    env = _get_env(project_root)
    template = env.get_template("scenario.prompt.md.j2")
    return template.render(scenario_name=scenario_name)


# ---------------------------------------------------------------------------
# Plugin / Skill structure analysis
# ---------------------------------------------------------------------------

_PLUGIN_MANIFEST_LOCATIONS = [
    "plugin.json",
    ".plugin/plugin.json",
    ".github/plugin/plugin.json",
    ".claude-plugin/plugin.json",
]


def _find_plugin_manifest(plugin_root: Path) -> dict | None:
    """Find and parse the plugin manifest from standard locations."""
    for rel in _PLUGIN_MANIFEST_LOCATIONS:
        candidate = plugin_root / rel
        if candidate.is_file():
            try:
                return json.loads(candidate.read_text(encoding="utf-8"))
            except (json.JSONDecodeError, OSError):
                pass
    return None


def _analyze_skill_dir(skill_path: Path) -> dict:
    """Analyze a single skill directory and return structural metadata."""
    analysis: dict = {
        "path": str(skill_path),
        "name": skill_path.name,
        "is_skill": False,
        "has_skill_md": False,
        "skill_md_word_count": 0,
        "description": "",
        "has_reference_dir": False,
        "has_examples_dir": False,
        "has_templates_dir": False,
        "has_scripts_dir": False,
        "file_count": 0,
    }

    skill_md = skill_path / "SKILL.md"
    if skill_md.is_file():
        analysis["has_skill_md"] = True
        analysis["is_skill"] = True
        try:
            content = skill_md.read_text(encoding="utf-8")
            analysis["skill_md_word_count"] = len(content.split())
            # Extract frontmatter
            if content.startswith("---"):
                parts = content.split("---", 2)
                if len(parts) >= 3:
                    try:
                        fm = yaml.safe_load(parts[1])
                        if isinstance(fm, dict):
                            analysis["description"] = fm.get("description", "")
                    except yaml.YAMLError:
                        pass
        except (OSError, UnicodeDecodeError):
            pass

    analysis["has_reference_dir"] = (skill_path / "reference").is_dir()
    analysis["has_examples_dir"] = (skill_path / "examples").is_dir()
    analysis["has_templates_dir"] = (skill_path / "templates").is_dir()
    analysis["has_scripts_dir"] = (skill_path / "scripts").is_dir()

    if skill_path.is_dir():
        analysis["file_count"] = sum(1 for _ in skill_path.rglob("*") if _.is_file())

    return analysis


def _analyze_plugin_structure(plugin_path: Path) -> dict:
    """Analyze a plugin directory and return a comprehensive component inventory."""
    analysis: dict = {
        "path": str(plugin_path),
        "name": plugin_path.name,
        "is_plugin": False,
        "manifest": None,
        "manifest_location": None,
        "skills": [],
        "agents": [],
        "hooks_config": None,
        "has_hooks": False,
        "has_mcp_config": False,
        "has_lsp_config": False,
        "has_monitors": False,
        "has_readme": False,
        "has_changelog": False,
        "has_license": False,
        "has_bin_dir": False,
        "has_settings": False,
        "total_file_count": 0,
    }

    # Find manifest
    for rel in _PLUGIN_MANIFEST_LOCATIONS:
        candidate = plugin_path / rel
        if candidate.is_file():
            analysis["manifest_location"] = rel
            try:
                analysis["manifest"] = json.loads(
                    candidate.read_text(encoding="utf-8")
                )
                analysis["is_plugin"] = True
            except (json.JSONDecodeError, OSError):
                pass
            break

    # Discover skills
    skills_dir = plugin_path / "skills"
    if skills_dir.is_dir():
        for child in sorted(skills_dir.iterdir()):
            if child.is_dir():
                analysis["skills"].append(_analyze_skill_dir(child))

    # Also check if plugin root itself is a single skill
    if not analysis["skills"] and (plugin_path / "SKILL.md").is_file():
        analysis["skills"].append(_analyze_skill_dir(plugin_path))

    # Discover agents
    agents_dir = plugin_path / "agents"
    if agents_dir.is_dir():
        for f in sorted(agents_dir.iterdir()):
            if f.is_file() and f.suffix == ".md":
                agent_info: dict = {"name": f.stem, "file": f.name}
                try:
                    content = f.read_text(encoding="utf-8")
                    if content.startswith("---"):
                        parts = content.split("---", 2)
                        if len(parts) >= 3:
                            fm = yaml.safe_load(parts[1])
                            if isinstance(fm, dict):
                                agent_info["description"] = fm.get("description", "")
                                agent_info["model"] = fm.get("model", "")
                except (OSError, UnicodeDecodeError, yaml.YAMLError):
                    pass
                analysis["agents"].append(agent_info)

    # Check hooks
    hooks_paths = [
        plugin_path / "hooks.json",
        plugin_path / "hooks" / "hooks.json",
    ]
    for hp in hooks_paths:
        if hp.is_file():
            analysis["has_hooks"] = True
            try:
                analysis["hooks_config"] = json.loads(
                    hp.read_text(encoding="utf-8")
                )
            except (json.JSONDecodeError, OSError):
                pass
            break

    # Check MCP config
    for mcp_path in [plugin_path / ".mcp.json", plugin_path / ".github" / "mcp.json"]:
        if mcp_path.is_file():
            analysis["has_mcp_config"] = True
            break

    # Check LSP config
    for lsp_path in [plugin_path / ".lsp.json", plugin_path / "lsp.json"]:
        if lsp_path.is_file():
            analysis["has_lsp_config"] = True
            break

    # Check monitors
    monitors_path = plugin_path / "monitors" / "monitors.json"
    if monitors_path.is_file():
        analysis["has_monitors"] = True

    # Check documentation and metadata files
    analysis["has_readme"] = any(
        (plugin_path / name).is_file()
        for name in ["README.md", "readme.md", "README.MD"]
    )
    analysis["has_changelog"] = any(
        (plugin_path / name).is_file()
        for name in ["CHANGELOG.md", "changelog.md", "CHANGES.md"]
    )
    analysis["has_license"] = any(
        (plugin_path / name).is_file()
        for name in ["LICENSE", "LICENSE.md", "LICENSE.txt", "license"]
    )
    analysis["has_bin_dir"] = (plugin_path / "bin").is_dir()
    analysis["has_settings"] = (plugin_path / "settings.json").is_file()

    if plugin_path.is_dir():
        analysis["total_file_count"] = sum(
            1 for _ in plugin_path.rglob("*") if _.is_file()
        )

    return analysis


def _format_plugin_analysis(analysis: dict) -> str:
    """Format a plugin analysis dict as a human-readable summary for the prompt."""
    lines: list[str] = []
    manifest = analysis.get("manifest") or {}

    lines.append(f"**Plugin: `{analysis['name']}`**")
    if manifest:
        name = manifest.get("name", "(unset)")
        desc = manifest.get("description", "(none)")
        ver = manifest.get("version", "(unset)")
        lines.append(f"- Manifest: `{analysis['manifest_location']}` — name: `{name}`, version: `{ver}`")
        lines.append(f"- Description: {desc}")
    else:
        lines.append("- ⚠️ No plugin.json manifest found")

    # Skills
    skills = analysis.get("skills", [])
    if skills:
        skill_names = [s["name"] for s in skills]
        lines.append(f"- Skills ({len(skills)}): {', '.join(skill_names)}")
        for s in skills:
            issues: list[str] = []
            if not s["has_skill_md"]:
                issues.append("missing SKILL.md")
            if s["skill_md_word_count"] > 1500:
                issues.append(f"body too long ({s['skill_md_word_count']} words)")
            if not s["description"]:
                issues.append("no description in frontmatter")
            if not s["has_reference_dir"]:
                issues.append("no reference/ directory")
            if not s["has_examples_dir"]:
                issues.append("no examples/ directory")
            if issues:
                lines.append(f"  - `{s['name']}`: ⚠️ {'; '.join(issues)}")
    else:
        lines.append("- Skills: ⚠️ none found")

    # Agents
    agents = analysis.get("agents", [])
    if agents:
        agent_names = [a["name"] for a in agents]
        lines.append(f"- Agents ({len(agents)}): {', '.join(agent_names)}")
    else:
        lines.append("- Agents: none")

    # Hooks
    lines.append(f"- Hooks: {'configured' if analysis['has_hooks'] else 'none'}")

    # MCP/LSP
    lines.append(f"- MCP servers: {'configured' if analysis['has_mcp_config'] else 'none'}")
    lines.append(f"- LSP servers: {'configured' if analysis['has_lsp_config'] else 'none'}")

    # Documentation
    doc_items: list[str] = []
    if not analysis["has_readme"]:
        doc_items.append("missing README.md")
    if not analysis["has_changelog"]:
        doc_items.append("missing CHANGELOG.md")
    if not analysis["has_license"]:
        doc_items.append("missing LICENSE")
    if doc_items:
        lines.append(f"- Documentation: ⚠️ {'; '.join(doc_items)}")
    else:
        lines.append("- Documentation: README, CHANGELOG, LICENSE present")

    # Manifest issues
    if manifest:
        manifest_issues: list[str] = []
        if not manifest.get("description"):
            manifest_issues.append("missing description")
        if not manifest.get("version"):
            manifest_issues.append("missing version")
        if not manifest.get("author"):
            manifest_issues.append("missing author")
        if not manifest.get("keywords") and not manifest.get("tags"):
            manifest_issues.append("missing keywords/tags")
        if manifest_issues:
            lines.append(f"- Manifest gaps: ⚠️ {'; '.join(manifest_issues)}")

    lines.append(f"- Total files: {analysis['total_file_count']}")

    return "\n".join(lines)


def _format_skill_analysis(analysis: dict) -> str:
    """Format a standalone skill analysis dict as a human-readable summary."""
    lines: list[str] = []
    lines.append(f"**Skill: `{analysis['name']}`**")

    if analysis["has_skill_md"]:
        desc = analysis["description"] or "(no description)"
        lines.append(f"- Description: {desc}")
        lines.append(f"- SKILL.md word count: {analysis['skill_md_word_count']}")
    else:
        lines.append("- ⚠️ No SKILL.md found")

    issues: list[str] = []
    if analysis["skill_md_word_count"] > 1500:
        issues.append(f"body too long ({analysis['skill_md_word_count']} words)")
    if not analysis["description"]:
        issues.append("no description in frontmatter")
    if not analysis["has_reference_dir"]:
        issues.append("no reference/ directory")
    if not analysis["has_examples_dir"]:
        issues.append("no examples/ directory")

    if issues:
        lines.append(f"- Issues: ⚠️ {'; '.join(issues)}")
    else:
        lines.append("- Structure: ✅ well-formed")

    lines.append(f"- Total files: {analysis['file_count']}")
    return "\n".join(lines)


def render_improvement_prompt(
    config: EvalConfig,
    configuration: Configuration,
    project_root: Path,
    skill_paths: list[Path],
    plugin_paths: list[Path],
    focus_dimensions: list[str] | None = None,
    research_dir: Path | None = None,
    lessons_context: str | None = None,
) -> str:
    """Render the improvement suggestions prompt for a specific configuration.

    *skill_paths* and *plugin_paths* are resolved absolute paths to the
    skill/plugin directories for this configuration.

    When *focus_dimensions* is provided, the prompt instructs the LLM
    to prioritize suggestions for those specific dimensions.

    When *research_dir* is provided, key best-practice files from that
    directory are included in the prompt context.
    """
    env = _get_env(project_root)
    template = env.get_template("suggest-improvements.md.j2")

    reports_dir = config.output.reports_directory
    improvements_file = config.output.improvements_file_pattern.format(
        config=configuration.name
    )

    # Build paths to available data files
    build_notes_path = f"{reports_dir}/{config.output.notes_file}"
    scores_data_path = f"{reports_dir}/{config.output.scores_data_file}"
    verification_data_path = f"{reports_dir}/{config.output.verification_data_file}"

    # Check which data files actually exist
    build_notes_exists = (project_root / build_notes_path).exists()
    verification_data_exists = (project_root / verification_data_path).exists()
    scores_data_exists = (project_root / scores_data_path).exists()

    other_configurations = [
        c for c in config.configurations if c.name != configuration.name
    ]

    # Build display-friendly short paths for the report output.
    # Absolute paths are passed for reading; short paths for display.
    cache_dir = Path.home() / ".skill-eval" / "cache"

    def _short_path(p: Path) -> str:
        """Return a path relative to the skill-eval cache, or project root."""
        try:
            return str(p.relative_to(cache_dir))
        except ValueError:
            pass
        try:
            return str(p.relative_to(project_root))
        except ValueError:
            return str(p)

    # --- Plugin/Skill structure analysis ---
    plugin_analyses: list[str] = []
    for p in plugin_paths:
        if p.is_dir():
            analysis = _analyze_plugin_structure(p)
            plugin_analyses.append(_format_plugin_analysis(analysis))

    skill_analyses: list[str] = []
    for p in skill_paths:
        if p.is_dir():
            analysis = _analyze_skill_dir(p)
            skill_analyses.append(_format_skill_analysis(analysis))

    has_plugins = bool(plugin_paths)

    # --- Load best-practices reference ---
    best_practices_content = ""
    bp_path = _PACKAGE_TEMPLATES / "best-practices-reference.md"
    if not bp_path.is_file():
        bp_path = _REPO_TEMPLATES / "best-practices-reference.md"
    if bp_path.is_file():
        try:
            best_practices_content = bp_path.read_text(encoding="utf-8")
        except OSError:
            pass

    # --- Load custom research files if research_dir provided ---
    custom_research_content = ""
    if research_dir and research_dir.is_dir():
        research_snippets: list[str] = []
        # Look for key files in common patterns
        for pattern in ["*-summary.md", "*-checklist.md", "*-rules.md"]:
            for f in sorted(research_dir.glob(pattern)):
                try:
                    content = f.read_text(encoding="utf-8")
                    # Truncate individual files to keep prompt manageable
                    if len(content) > 4000:
                        content = content[:4000] + "\n\n[... truncated for brevity ...]"
                    research_snippets.append(
                        f"#### From `{f.name}`\n\n{content}"
                    )
                except (OSError, UnicodeDecodeError):
                    pass
        if research_snippets:
            custom_research_content = "\n\n---\n\n".join(research_snippets)

    return template.render(
        config_name=configuration.name,
        config_label=configuration.label,
        skill_paths=[str(p) for p in skill_paths],
        plugin_paths=[str(p) for p in plugin_paths],
        skill_display_paths=[_short_path(p) for p in skill_paths],
        plugin_display_paths=[_short_path(p) for p in plugin_paths],
        dimensions=config.dimensions,
        other_configurations=other_configurations,
        reports_directory=reports_dir,
        analysis_file=config.output.analysis_file,
        improvements_file=improvements_file,
        build_notes_path=build_notes_path if build_notes_exists else None,
        verification_data_path=verification_data_path if verification_data_exists else None,
        scores_data_path=scores_data_path if scores_data_exists else None,
        focus_dimensions=focus_dimensions or [],
        # New context variables for plugin-aware improvements
        plugin_analyses=plugin_analyses,
        skill_analyses=skill_analyses,
        has_plugins=has_plugins,
        best_practices_content=best_practices_content,
        custom_research_content=custom_research_content,
        lessons_context=lessons_context or "",
    )
