"""Configuration schema and reader for eval.yaml."""

from __future__ import annotations

from pathlib import Path
from typing import Annotated, Union

import yaml
from pydantic import BaseModel, BeforeValidator, field_validator, model_validator


class IncludeDirectory(BaseModel):
    """A directory to link into the staging workspace so Copilot can access it."""

    path: str  # relative to eval.yaml location
    name: str | None = None  # name in the staging dir; defaults to basename


class Scenario(BaseModel):
    """A code-generation scenario (an app to be built)."""

    name: str
    prompt: str  # relative path to the prompt file
    description: str = ""
    include_directories: list[IncludeDirectory] = []


class SkillReference(BaseModel):
    """A reference to a skill/plugin, either a local path or a named source.

    When parsed from YAML:
    - A plain string ``"skills/my-skill"`` becomes a local path reference.
    - A mapping ``{source: "my-source", path: "subfolder"}`` references
      a named source from skill-sources.yaml.
    """

    source: str | None = None  # name in skill-sources.yaml
    path: str | None = None  # sub-path within the source, or a local relative path
    local_path: str | None = None  # set when this is a legacy plain-string path

    @property
    def is_source_ref(self) -> bool:
        """True when this references a named source from skill-sources.yaml."""
        return self.source is not None

    @property
    def display_name(self) -> str:
        """Human-readable label for logging."""
        if self.is_source_ref:
            parts = [self.source]
            if self.path:
                parts.append(self.path)
            return ":".join(parts)
        return self.local_path or ""


def _coerce_skill_reference(v: object) -> object:
    """Accept plain strings as legacy local-path skill references."""
    if isinstance(v, str):
        return {"local_path": v}
    return v


SkillRef = Annotated[SkillReference, BeforeValidator(_coerce_skill_reference)]


class Configuration(BaseModel):
    """A skill configuration to evaluate."""

    name: str
    label: str = ""
    skills: list[SkillRef] = []
    plugins: list[SkillRef] = []
    suggest_improvements: bool = False

    def get_local_skill_paths(self) -> list[str]:
        """Return legacy local paths for skills (backward compat helper)."""
        return [r.local_path for r in self.skills if r.local_path]

    def get_local_plugin_paths(self) -> list[str]:
        """Return legacy local paths for plugins (backward compat helper)."""
        return [r.local_path for r in self.plugins if r.local_path]

    def has_any_skills_or_plugins(self) -> bool:
        """True if this configuration declares at least one skill or plugin."""
        return bool(self.skills or self.plugins)


class BuildVerification(BaseModel):
    """How to build a generated project."""

    command: str
    working_directory: str = "."
    success_pattern: str | None = None


class HealthCheck(BaseModel):
    """Optional HTTP health check for a running app."""

    url: str
    expected_status: int = 200


class RunVerification(BaseModel):
    """How to run a generated project."""

    command: str
    timeout_seconds: int = 15
    health_check: HealthCheck | None = None


class FormatVerification(BaseModel):
    """Code formatting verification via dotnet format."""

    command: str = "dotnet format --check"
    working_directory: str = "."


class SecurityVerification(BaseModel):
    """Security vulnerability scanning."""

    vulnerability_scan: bool = True
    static_analysis: bool = False
    analyzers: list[str] = []


class MetricsVerification(BaseModel):
    """Code metrics computation."""

    compute: bool = True
    metrics: list[str] = ["warnings", "errors"]


class Verification(BaseModel):
    """Build and run verification settings."""

    build: BuildVerification
    run: RunVerification | None = None
    format: FormatVerification | None = None
    security: SecurityVerification | None = None
    metrics: MetricsVerification | None = None
    analyzers: list[str] = []  # Roslyn analyzer NuGet packages to inject


class Dimension(BaseModel):
    """An analysis dimension for comparative evaluation."""

    name: str
    description: str
    what_to_look_for: str
    why_it_matters: str
    tier: str = "medium"  # critical, high, medium, low
    weight: float | None = None  # If None, derived from tier
    evaluation_method: str = "llm"  # llm, automated, hybrid

    @property
    def effective_weight(self) -> float:
        """Return explicit weight or derive from tier."""
        if self.weight is not None:
            return self.weight
        tier_weights = {"critical": 3.0, "high": 2.0, "medium": 1.0, "low": 0.5}
        return tier_weights.get(self.tier, 1.0)


class OutputSettings(BaseModel):
    """Where generated code and reports are stored."""

    directory: str = "output"
    reports_directory: str = "reports"
    analysis_file: str = "analysis.md"
    notes_file: str = "build-notes.md"
    per_run_analysis_pattern: str = "analysis-run-{run}.md"
    improvements_file_pattern: str = "improvements-{config}.md"
    verification_data_file: str = "verification-data.json"
    scores_data_file: str = "scores-data.json"


class EvalConfig(BaseModel):
    """Root configuration model for eval.yaml."""

    name: str
    description: str = ""
    eval_type: str = "code_generation"  # "code_generation" or "text_output"
    scenarios: list[Scenario]
    configurations: list[Configuration]
    verification: Verification | None = None
    dimensions: list[Dimension]
    output: OutputSettings = OutputSettings()
    runs: int = 3
    generation_model: str = "claude-opus-4.6"
    analysis_model: str = "gpt-5.3-codex"
    improvement_model: str | None = None
    research_dir: str | None = None  # Path to research/best-practices files

    @field_validator("eval_type")
    @classmethod
    def valid_eval_type(cls, v: str) -> str:
        allowed = {"code_generation", "text_output"}
        if v not in allowed:
            raise ValueError(
                f"eval_type must be one of {allowed}, got '{v}'"
            )
        return v

    @field_validator("scenarios")
    @classmethod
    def at_least_one_scenario(cls, v: list[Scenario]) -> list[Scenario]:
        if not v:
            raise ValueError("At least one scenario is required")
        return v

    @field_validator("configurations")
    @classmethod
    def at_least_two_configurations(
        cls, v: list[Configuration]
    ) -> list[Configuration]:
        if len(v) < 2:
            raise ValueError(
                "At least two configurations are required for comparison "
                "(typically a baseline + your skill)"
            )
        return v

    @field_validator("dimensions")
    @classmethod
    def at_least_one_dimension(cls, v: list[Dimension]) -> list[Dimension]:
        if not v:
            raise ValueError("At least one analysis dimension is required")
        return v

    @property
    def is_text_output(self) -> bool:
        """True when this eval produces text output rather than code."""
        return self.eval_type == "text_output"

    @property
    def effective_improvement_model(self) -> str:
        """Return improvement_model if set, otherwise fall back to analysis_model."""
        return self.improvement_model or self.analysis_model

    @property
    def improvement_targets(self) -> list[Configuration]:
        """Return configurations marked with suggest_improvements: true."""
        return [c for c in self.configurations if c.suggest_improvements]

    @model_validator(mode="after")
    def check_verification_required(self) -> "EvalConfig":
        """Require verification for code_generation evals."""
        if self.eval_type == "code_generation" and self.verification is None:
            raise ValueError(
                "verification is required for code_generation evals. "
                "Set eval_type: text_output if this eval produces text output."
            )
        return self


def load_config(config_path: Path | None = None) -> EvalConfig:
    """Load and validate eval.yaml from the given path or current directory."""
    if config_path is None:
        config_path = Path.cwd() / "eval.yaml"

    if not config_path.exists():
        raise FileNotFoundError(
            f"Configuration file not found: {config_path}\n"
            "Run 'skill-eval init' to create one, or specify a path with --config."
        )

    with open(config_path) as f:
        raw = yaml.safe_load(f)

    if raw is None:
        raise ValueError(f"Configuration file is empty: {config_path}")

    return EvalConfig.model_validate(raw)
