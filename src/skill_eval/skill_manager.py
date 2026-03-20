"""Manage Copilot skill directory registration in ~/.copilot/config.json."""

from __future__ import annotations

import json
from pathlib import Path

COPILOT_CONFIG = Path.home() / ".copilot" / "config.json"


def _read_config() -> dict:
    """Read the Copilot config file, returning an empty dict if missing."""
    if COPILOT_CONFIG.exists():
        return json.loads(COPILOT_CONFIG.read_text(encoding="utf-8"))
    return {}


def _write_config(config: dict) -> None:
    """Write the Copilot config file (creating parent dirs if needed)."""
    COPILOT_CONFIG.parent.mkdir(parents=True, exist_ok=True)
    COPILOT_CONFIG.write_text(
        json.dumps(config, indent=2), encoding="utf-8"
    )


def add_skill_directories(paths: list[Path]) -> list[Path]:
    """Register skill directories in Copilot config.

    Returns the list of paths that were actually added (not already present).
    """
    if not paths:
        return []

    config = _read_config()
    existing: list[str] = config.get("skill_directories", [])
    added: list[Path] = []

    for p in paths:
        abs_path = str(p.resolve())
        if abs_path not in existing:
            existing.append(abs_path)
            added.append(p)

    config["skill_directories"] = existing
    _write_config(config)
    return added


def remove_skill_directories(paths: list[Path]) -> None:
    """Unregister skill directories from Copilot config."""
    if not paths or not COPILOT_CONFIG.exists():
        return

    config = _read_config()
    existing: list[str] = config.get("skill_directories", [])
    if not existing:
        return

    abs_paths = {str(p.resolve()) for p in paths}
    config["skill_directories"] = [d for d in existing if d not in abs_paths]
    _write_config(config)
