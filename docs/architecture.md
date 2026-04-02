# Architecture & Internals

Technical details about the Copilot Skill Evaluation Framework internals. For end-user documentation, see the [README](../README.md).

## Pipeline Overview

```
┌──────────────────────────────────────────────────────────────┐
│  @skill-eval agent  (or  skill-eval CLI)                     │
│                                                              │
│  1. GENERATE  — Build the same apps with different           │
│                 skill configs using Copilot CLI              │
│  2. VERIFY    — Build, format-check, security-scan, & run    │
│  3. ANALYZE   — Score quality across weighted dimensions     │
│                 and produce per-run + aggregated reports     │
│                                                              │
│  Supports N runs per config with parallel execution,         │
│  watchdog timeouts, session tracing, and token tracking.     │
└──────────────────────────────────────────────────────────────┘
```

## Source Code Structure

```
src/skill_eval/
├── cli.py                       # CLI entry point (Click framework)
├── config.py                    # YAML config + Pydantic models
├── source_config.py             # skill-sources.yaml schema
├── source_resolver.py           # Git clone/cache for remote skill sources
├── generate.py                  # Step 1: Copilot code generation
├── verify.py                    # Step 2: Build, format, security, run
├── analyze.py                   # Step 3: Comparative analysis
├── aggregator.py                # Cross-run score aggregation
├── session_tracer.py            # Skill/plugin usage tracing per run
├── skill_manager.py             # Copilot skill registration
├── prompt_renderer.py           # Jinja2 template rendering
├── init_cmd.py                  # Interactive project setup
└── templates/                   # Bundled Jinja2 templates
    ├── create-single-app.md.j2  # Single app generation prompt
    ├── create-all-apps.md.j2    # Batch generation prompt
    ├── analyze.md.j2            # Analysis prompt template
    ├── scenario.prompt.md.j2    # Starter scenario template
    ├── ci-workflow.yml.j2       # GitHub Actions workflow template
    ├── Directory.Build.props    # Roslyn analyzer injection for .NET
    └── .editorconfig            # Code formatting defaults
```

## Verification Pipeline

The verify step runs multiple checks beyond basic build and run:

| Check | What It Does | Configuration |
|-------|-------------|---------------|
| **Build** | Compiles the project, counts Roslyn analyzer warnings | `verification.build` |
| **Format** | Runs `dotnet format --check` for code style compliance | `verification.format` (optional) |
| **Security** | Runs `dotnet list package --vulnerable` for known CVEs | `verification.security` (optional) |
| **Run** | Starts the app and optionally checks health endpoint | `verification.run` (optional) |

The framework automatically injects a `Directory.Build.props` file to enable Roslyn analyzers before building. Additional analyzer packages can be specified:

```yaml
verification:
  analyzers:
    - "Meziantou.Analyzer"
    - "Roslynator.Analyzers"
```

Build warnings are parsed and categorized (naming, performance, security, reliability) and included in the build notes report.

## Multi-Run Pipeline

Running multiple evaluation passes increases result reliability and surfaces variance:

```bash
skill-eval run --runs 3
```

Each run:
1. **Generates** code for a randomly-selected scenario per config
2. **Verifies** the generated project (build, format, security, run)
3. **Analyzes** quality across all dimensions, producing `analysis-run-{N}.md`

After all runs complete, the **aggregator** parses per-run scores and produces:
- Aggregated score tables with weighted totals per config
- A scoring methodology section explaining tier weights
- "Overview" and "What Was Tested" sections summarizing the evaluation
- `scores-data.json` with structured score data

Runs execute with parallel generation across configs, a watchdog timeout with network-connection health checks, and per-config token usage tracking (`generation-usage.json`).

## Session Tracing

The session tracer monitors which Copilot skills, plugins, and instruction files are actually loaded during code generation. This:

- **Prevents skill contamination** — isolates `skill_directories` per config so one config's skills don't leak into another
- **Verifies skill activation** — confirms the intended skills were loaded by parsing Copilot CLI session events from `~/.copilot/session-state`
- **Uses path-based matching** as the sole authority for skill detection

### Staging Directory Isolation

Each configuration runs inside an isolated staging directory with selective symlinks/junctions:

| Layer | Mechanism | Purpose |
|-------|-----------|---------|
| Staging Dirs | Temp directory with selective symlinks/junctions | Only declared skills/plugins visible to Copilot |
| Skill Registration | Global config managed per config | Skills cleared before each config's generation |
| Session Tracing | Path-based verification post-generation | Detects if wrong skills leaked in |

## Source Resolution

The `SourceResolver` handles fetching remote skill sources:

1. **Git sources** are cloned into `~/.skill-eval/cache/` (configurable)
2. On subsequent runs, existing clones are updated with `git pull`
3. URL normalization converts GitHub web URLs to clone-able git URLs
4. Both HTTPS and SSH git URLs are supported
5. Optional `ref` (branch/tag) and `path` (subfolder) parameters

## Reports

The pipeline produces several report files:

| File | Description |
|------|-------------|
| `reports/analysis.md` | Final weighted comparison across all configs |
| `reports/analysis-run-{N}.md` | Per-run analysis scores |
| `reports/build-notes.md` | Build, run, format, and security results |
| `reports/verification-data.json` | Machine-readable verification metrics |
| `reports/scores-data.json` | Parsed dimension scores |
| `reports/generation-usage.json` | Token usage per config/run |
