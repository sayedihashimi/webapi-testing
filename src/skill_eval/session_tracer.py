"""Parse Copilot CLI session events to extract loaded skills/plugins.

After each Copilot invocation the CLI writes telemetry to an events.jsonl
file under ``~/.copilot/session-state/{session-id}/``.  This module reads
that file and returns a structured summary of the session metadata and
every skill that was loaded (``skill.invoked`` events).
"""

from __future__ import annotations

import json
import shutil
from dataclasses import asdict, dataclass, field
from pathlib import Path


# Event types we care about --------------------------------------------------

_RESOURCE_EVENT_TYPES = frozenset({"skill.invoked"})
_META_EVENT_TYPES = frozenset({
    "session.start",
    "session.model_change",
    "user.message",
})


# Data models -----------------------------------------------------------------

@dataclass
class LoadedResource:
    """A skill or plugin that Copilot loaded during the session."""

    resource_type: str = "skill"
    name: str = ""
    path: str | None = None
    plugin_name: str | None = None
    description: str | None = None
    content_length: int = 0
    timestamp: str | None = None


@dataclass
class SessionTrace:
    """Summary of a Copilot CLI session parsed from events.jsonl."""

    session_id: str | None = None
    model: str | None = None
    copilot_version: str | None = None
    start_time: str | None = None
    cwd: str | None = None
    prompt: str | None = None
    resources: list[LoadedResource] = field(default_factory=list)
    total_events: int = 0
    events_file: str | None = None

    # Convenience helpers -----------------------------------------------------

    @property
    def skill_names(self) -> list[str]:
        return [r.name for r in self.resources if r.resource_type == "skill"]

    @property
    def plugin_names(self) -> list[str]:
        names: list[str] = []
        for r in self.resources:
            if r.plugin_name and r.plugin_name not in names:
                names.append(r.plugin_name)
        return names

    def to_dict(self) -> dict:
        """Serialise to a plain dict (for JSON output)."""
        return asdict(self)


# Parsing ---------------------------------------------------------------------

def _truncate(text: str | None, length: int = 200) -> str | None:
    if text is None:
        return None
    return text[:length] + "…" if len(text) > length else text


def parse_events_file(events_path: Path) -> SessionTrace:
    """Parse an ``events.jsonl`` file and return a :class:`SessionTrace`.

    Handles both the ``~/.copilot/session-state/`` format (which has
    ``session.start``) and the ``--output-format json`` stdout format
    (which uses ``result`` for session ID and ``session.tools_updated``
    for model).
    """
    trace = SessionTrace(events_file=str(events_path))
    first_user_seen = False

    with open(events_path, encoding="utf-8") as fh:
        for line in fh:
            line = line.strip()
            if not line:
                continue
            try:
                event = json.loads(line)
            except json.JSONDecodeError:
                continue

            trace.total_events += 1
            etype = event.get("type", "")
            data = event.get("data") or {}

            if etype == "session.start":
                trace.session_id = data.get("sessionId")
                trace.model = data.get("selectedModel")
                trace.copilot_version = data.get("copilotVersion")
                trace.start_time = data.get("startTime")
                ctx = data.get("context", {})
                trace.cwd = ctx.get("cwd")

            elif etype == "result":
                # --output-format json emits session ID here
                if not trace.session_id:
                    trace.session_id = event.get("sessionId")

            elif etype == "session.tools_updated":
                # --output-format json emits model here
                if not trace.model and data.get("model"):
                    trace.model = data["model"]

            elif etype == "session.model_change":
                trace.model = data.get("newModel") or trace.model

            elif etype == "user.message" and not first_user_seen:
                first_user_seen = True
                trace.prompt = _truncate(data.get("content"))

            elif etype == "skill.invoked":
                content = data.get("content", "")
                trace.resources.append(LoadedResource(
                    resource_type="skill",
                    name=data.get("name", "unknown"),
                    path=data.get("path"),
                    plugin_name=data.get("pluginName") or None,
                    description=_truncate(data.get("description"), 120),
                    content_length=len(content) if content else 0,
                    timestamp=event.get("timestamp"),
                ))

    return trace


# Session discovery -----------------------------------------------------------

def _find_latest_events_file(
    copilot_home: Path,
    created_after: float | None = None,
) -> Path | None:
    """Return the most recently modified ``events.jsonl`` under *copilot_home*.

    This is the legacy fallback used when no snapshot is available.
    """
    session_state = copilot_home / "session-state"
    if not session_state.is_dir():
        return None

    candidates: list[tuple[float, Path]] = []
    for d in session_state.iterdir():
        ef = d / "events.jsonl"
        if ef.is_file():
            mtime = ef.stat().st_mtime
            if created_after is not None and mtime < created_after:
                continue
            candidates.append((mtime, ef))

    if not candidates:
        return None

    candidates.sort(reverse=True)
    return candidates[0][1]


def trace_session(
    copilot_home: Path | None = None,
    created_after: float | None = None,
    session_id: str | None = None,
) -> SessionTrace | None:
    """Find and parse a session's ``events.jsonl``.

    Parameters
    ----------
    copilot_home:
        Root of the Copilot home directory (contains ``session-state/``).
        Defaults to ``~/.copilot``.
    session_id:
        When provided, loads ``session-state/{session_id}/events.jsonl``
        directly.  This is the most reliable method.
    created_after:
        Legacy fallback — find by modification time when *session_id*
        is not available.
    """
    if copilot_home is None:
        copilot_home = Path.home() / ".copilot"

    events_path: Path | None = None
    if session_id:
        candidate = copilot_home / "session-state" / session_id / "events.jsonl"
        if candidate.is_file():
            events_path = candidate
    if events_path is None:
        events_path = _find_latest_events_file(copilot_home, created_after)

    if events_path is None:
        return None

    return parse_events_file(events_path)


# Comparison ------------------------------------------------------------------

def compare_resources(
    trace: SessionTrace,
    expected_skills: list[str],
    expected_plugins: list[str],
    allowed_dirs: list[str] | None = None,
) -> dict:
    """Compare loaded resources against expected configuration.

    Parameters
    ----------
    expected_skills:
        Skill directory names declared in the config (e.g. ``["dotnet-webapi"]``).
    expected_plugins:
        Plugin directory names declared in the config (e.g. ``["dotnet-artisan"]``).
    allowed_dirs:
        Absolute paths to the skill/plugin directories for this configuration.
        When provided, each loaded resource's ``path`` is checked against these
        directories.  A resource whose path does not fall under any allowed
        directory is flagged as **contamination** — meaning the eval framework
        failed to isolate it from this run.

    Returns a dict with match status, unexpected/missing lists, and any
    contaminated resources.
    """
    actual_skills = list(dict.fromkeys(trace.skill_names))
    actual_plugins = list(dict.fromkeys(trace.plugin_names))

    # --- Path-based contamination check (most reliable) ---
    contaminated: list[dict] = []
    legitimate: list[LoadedResource] = []
    if allowed_dirs:
        norm_dirs = [d.replace("/", "\\").rstrip("\\") for d in allowed_dirs]
        for r in trace.resources:
            rpath = (r.path or "").replace("/", "\\")
            belongs = any(rpath.startswith(d) for d in norm_dirs)
            if belongs:
                legitimate.append(r)
            else:
                contaminated.append({
                    "name": r.name,
                    "plugin_name": r.plugin_name,
                    "path": r.path,
                })
    else:
        legitimate = list(trace.resources)

    # --- Name-based comparison (informational) ---
    actual_skills = list(dict.fromkeys(trace.skill_names))
    actual_plugins = list(dict.fromkeys(trace.plugin_names))

    # When allowed_dirs is provided, match is determined solely by
    # path-based checks: no contamination means the run used only the
    # skills/plugins it was supposed to.  Name mismatches (e.g. config
    # says "managedcode-dotnet-skills" but sub-skills load as "dotnet",
    # "dotnet-aspnet-core") are expected and not a problem.
    if allowed_dirs:
        match = not contaminated
    else:
        expected_skills_set = set(expected_skills)
        actual_skills_set = set(actual_skills)
        expected_plugins_set = set(expected_plugins)
        actual_plugins_set = set(actual_plugins)
        match = (actual_skills_set == expected_skills_set
                 and actual_plugins_set == expected_plugins_set)

    return {
        "expected_skills": sorted(set(expected_skills)),
        "actual_skills": actual_skills,
        "expected_plugins": sorted(set(expected_plugins)),
        "actual_plugins": actual_plugins,
        "contaminated": contaminated,
        "match": match,
    }


# Events file preservation ----------------------------------------------------

def preserve_events_file(
    copilot_home: Path,
    destination: Path,
    created_after: float | None = None,
    session_id: str | None = None,
) -> Path | None:
    """Copy a session's ``events.jsonl`` to *destination*.

    Uses *session_id* for direct lookup when available, falling back
    to mtime-based lookup otherwise.

    Returns the destination path on success, or ``None``.
    """
    events_path: Path | None = None
    if session_id:
        candidate = copilot_home / "session-state" / session_id / "events.jsonl"
        if candidate.is_file():
            events_path = candidate
    if events_path is None:
        events_path = _find_latest_events_file(copilot_home, created_after)
    if events_path is None:
        return None

    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(events_path, destination)
    return destination
