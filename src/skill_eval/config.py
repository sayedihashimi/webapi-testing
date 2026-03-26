"""Configuration schema and reader for eval.yaml."""

from __future__ import annotations

from pathlib import Path

import yaml
from pydantic import BaseModel, field_validator


class Scenario(BaseModel):
    """A code-generation scenario (an app to be built)."""

    name: str
    prompt: str  # relative path to the prompt file
    description: str = ""


class Configuration(BaseModel):
    """A skill configuration to evaluate."""

    name: str
    label: str = ""
    skills: list[str] = []  # relative paths to skill directories
    plugins: list[str] = []  # relative paths to plugin directories


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


class Verification(BaseModel):
    """Build and run verification settings."""

    build: BuildVerification
    run: RunVerification | None = None


class Dimension(BaseModel):
    """An analysis dimension for comparative evaluation."""

    name: str
    description: str
    what_to_look_for: str
    why_it_matters: str


class OutputSettings(BaseModel):
    """Where generated code and reports are stored."""

    directory: str = "output"
    reports_directory: str = "reports"
    analysis_file: str = "analysis.md"
    notes_file: str = "build-notes.md"


class EvalConfig(BaseModel):
    """Root configuration model for eval.yaml."""

    name: str
    description: str = ""
    scenarios: list[Scenario]
    configurations: list[Configuration]
    verification: Verification
    dimensions: list[Dimension]
    output: OutputSettings = OutputSettings()

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
