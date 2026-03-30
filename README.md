# Copilot Skill Evaluation Framework

A framework for evaluating how **GitHub Copilot custom skills** impact code generation quality. It generates the same apps under different skill configurations, runs multi-stage verification, and produces a weighted comparative analysis — for **any tech stack**.

## How It Works

```
┌──────────────────────────────────────────────────────────────┐
│  @skill-eval agent  (or  skill-eval CLI)                     │
│                                                              │
│  1. GENERATE  — Build the same apps with different           │
│                 skill configs using Copilot CLI               │
│  2. VERIFY    — Build, format-check, security-scan, & run    │
│  3. ANALYZE   — Score quality across weighted dimensions     │
│                 and produce per-run + aggregated reports      │
│                                                              │
│  Supports N runs per config with parallel execution,         │
│  watchdog timeouts, session tracing, and token tracking.     │
└──────────────────────────────────────────────────────────────┘
```

You define:
- **Scenarios** — realistic app specifications (prompt files)
- **Configurations** — skill sets to compare (baseline vs your skills)
- **Dimensions** — weighted quality criteria grouped by tier (critical → low)

The framework generates identical apps under each configuration, verifies they build and run, then uses an AI model to score and compare the results across all dimensions.

## Quick Start

### Option A: Use the Agent (Recommended)

If you have the Copilot agent available:

```
@skill-eval I wrote a custom skill for React component generation.
I want to see if it actually makes Copilot produce better code.
```

The agent will walk you through setup, execution, and interpretation.

### Option B: Use the CLI

#### 1. Install

```bash
pip install -e .
```

#### 2. Initialize a new evaluation

```bash
python -m skill_eval init
```

This interactively creates:
- `eval.yaml` — your evaluation configuration
- `prompts/scenarios/*.prompt.md` — starter scenario prompts
- `skills/` — directory for your skill definitions

#### 3. Configure

Edit `eval.yaml` to define your scenarios, skill configurations, and analysis dimensions. Edit the scenario prompt files to describe the apps you want generated.

Copy your skills into the `skills/` directory.

#### 4. Run the evaluation

```bash
python -m skill_eval run
```

This runs the full pipeline: generate → verify → analyze. Reports land in `reports/`.

#### Individual steps

```bash
python -m skill_eval generate                     # Generate code only
python -m skill_eval generate -c my-skill         # Generate one configuration
python -m skill_eval generate --runs 3            # Generate 3 runs per config
python -m skill_eval generate --resume            # Skip runs where output exists
python -m skill_eval verify                       # Build/run verification only
python -m skill_eval analyze                      # Analysis only
python -m skill_eval analyze --model gpt-5.3-codex  # Use specific analysis model
python -m skill_eval run --runs 3                 # Full pipeline with 3 runs
python -m skill_eval run --skip-generate          # Skip generation, verify + analyze
python -m skill_eval run --skip-verify            # Skip verification, generate + analyze
python -m skill_eval run --analyze-only           # Only run analysis
python -m skill_eval validate-config              # Check eval.yaml is valid
```

#### Global options

```bash
python -m skill_eval --config path/to/eval.yaml run   # Custom config path
python -m skill_eval --project-root /path run         # Custom project root
```

## Configuration: `eval.yaml`

```yaml
name: "My Skill Evaluation"
description: "Evaluate my React skills"
runs: 3
analysis_model: "gpt-5.3-codex"

scenarios:
  - name: Dashboard
    prompt: prompts/scenarios/dashboard.prompt.md
    description: "Admin dashboard with charts and data tables"
  - name: Blog
    prompt: prompts/scenarios/blog.prompt.md
    description: "Blog with comments and user authentication"

configurations:
  - name: no-skills
    label: "Baseline (no skills)"
    skills: []
  - name: my-react-skill
    label: "My React Skill"
    skills:
      - skills/react-components

verification:
  build:
    command: "npm run build"
    success_pattern: "compiled successfully"
  format:
    command: "npm run lint"
  security:
    vulnerability_scan: true
  run:
    command: "npm start"
    timeout_seconds: 10
    health_check:
      url: "http://localhost:3000"
      expected_status: 200

dimensions:
  - name: "Component Architecture"
    description: "Component composition and reusability"
    what_to_look_for: "Check for atomic design, custom hooks, compound components."
    why_it_matters: "Good architecture enables reuse and simplifies testing."
    tier: high
    evaluation_method: llm

  - name: "Type Safety"
    description: "TypeScript usage and strictness"
    what_to_look_for: "Check for typed props, strict mode, no any types."
    why_it_matters: "Type safety catches bugs at compile time."

  # Add as many dimensions as you want...

output:
  directory: output
  reports_directory: reports
  analysis_file: analysis.md
  notes_file: build-notes.md
  per_run_analysis_pattern: "analysis-run-{run}.md"
  verification_data_file: "verification-data.json"
  scores_data_file: "scores-data.json"
```

## Writing Scenario Prompts

Scenario prompts describe the apps Copilot should generate. The more specific, the better the comparison. A good prompt includes:

| Section | Purpose |
|---------|---------|
| **Overview** | What the app does in 2–3 sentences |
| **Technology Stack** | Exact frameworks and versions |
| **Entities** | Domain models with fields and relationships |
| **Business Rules** | Non-trivial constraints and logic |
| **Endpoints** | API routes or pages to generate |
| **Seed Data** | Sample data for testing |
| **HTTP File** | Test file instructions for exercising the API |
| **Cross-Cutting Concerns** | Error handling, validation, logging, OpenAPI, pagination |

**📖 For the full authoring guide** — including core principles, section-by-section reference, anti-patterns, and a pre-submission checklist — see **[docs/authoring-guide.md](docs/authoring-guide.md)**.

See `templates/scenario.prompt.md.j2` for a starter template, or browse `examples/aspnet-webapi/prompts/scenarios/` for real-world examples.

## Defining Analysis Dimensions

Dimensions are the quality criteria used to compare generated code. Each dimension needs:

| Field | Purpose |
|-------|---------|
| `name` | Short label (e.g., "Error Handling") |
| `description` | What this dimension measures |
| `what_to_look_for` | Concrete things to check in the code |
| `why_it_matters` | Why this matters for production quality |
| `tier` | Priority tier: critical, high, medium, low (default: medium) |
| `evaluation_method` | How it's evaluated: llm, automated, hybrid (default: llm) |

**Dimension Tiers** determine weighted scoring in the analysis report:
- **critical** (weight 3×) — Security, functional correctness, build failures
- **high** (weight 2×) — Error handling, async patterns, core architecture
- **medium** (weight 1×) — Code patterns, organization, documentation
- **low** (weight 0.5×) — Style conventions, syntactic preferences

**Tips:**
- Start with 5–10 dimensions — you can always add more later
- Be specific in `what_to_look_for` — vague criteria produce vague analysis
- The `skill-eval init` command suggests stack-specific dimensions automatically

## Repository Structure

```
copilot-skill-eval/
├── eval.yaml                        # Your evaluation configuration
├── .github/
│   ├── agents/
│   │   └── skill-eval.agent.md      # Copilot agent (primary UX)
│   └── prompts/                     # Research & planning docs
├── prompts/scenarios/               # Your app specification prompts
├── skills/                          # Skills to evaluate (you bring these)
├── plugins/                         # Plugins to evaluate (optional)
├── src/skill_eval/                  # Python CLI (automation layer)
│   ├── cli.py                       # CLI entry point (Click framework)
│   ├── config.py                    # YAML config + Pydantic models
│   ├── generate.py                  # Step 1: Copilot code generation
│   ├── verify.py                    # Step 2: Build, format, security, run
│   ├── analyze.py                   # Step 3: Comparative analysis
│   ├── aggregator.py                # Cross-run score aggregation
│   ├── session_tracer.py            # Skill/plugin usage tracing per run
│   ├── skill_manager.py             # Copilot skill registration
│   ├── prompt_renderer.py           # Jinja2 template rendering
│   └── init_cmd.py                  # Interactive project setup
├── templates/                       # Jinja2 prompt templates
│   ├── create-all-apps.md.j2        # Batch generation prompt
│   ├── create-single-app.md.j2      # Single app generation prompt
│   ├── analyze.md.j2                # Analysis prompt template
│   ├── scenario.prompt.md.j2        # Starter scenario template
│   ├── Directory.Build.props        # Roslyn analyzer injection for .NET
│   └── .editorconfig                # Code formatting defaults
├── output/                          # Generated code (gitignored)
├── reports/                         # Analysis reports
├── examples/
│   ├── aspnet-webapi/               # Web API example (3 scenarios, 24 dims)
│   └── aspnet-razor-pages/          # Razor Pages example (3 scenarios, 23 dims)
├── docs/
│   └── authoring-guide.md           # Scenario prompt authoring guide
├── research/                        # Research artifacts
└── pyproject.toml                   # Python dependencies
```

## Examples

### ASP.NET Core Web API

The `examples/aspnet-webapi/` directory contains a complete working evaluation that tests five Copilot skill configurations across three realistic .NET Web API apps:

**Scenarios:** Fitness Studio Booking API, Community Library API, Veterinary Clinic API

**Configurations:**
| Config | What It Tests |
|--------|--------------|
| `no-skills` | Baseline — Copilot with no custom guidance |
| `dotnet-webapi` | A single custom skill for Web API patterns |
| `dotnet-artisan` | A full plugin chain with 9 skills + agents |
| `managedcode-dotnet-skills` | Community skills covering 6 .NET areas |
| `dotnet-skills` | Official .NET skills from [dotnet/skills](https://github.com/dotnet/skills) — 11 plugins covering core .NET, data, testing, build, and more |

**24 dimensions** across 4 tiers:
| Tier | Count | Examples |
|------|-------|----------|
| Critical (3×) | 6 | Build & Run Success, Security Scan, Minimal API Architecture, Input Validation, NuGet Discipline, EF Migrations |
| High (2×) | 8 | Business Logic Correctness, Prefer Built-in over 3rd Party, Modern C# Adoption, Error Handling & Middleware |
| Medium (1×) | 9 | Async/Await, Logging, Pagination, OpenAPI Documentation |
| Low (0.5×) | 1 | Style conventions |

**Reports generated:** 3 per-run analysis reports, aggregated scores JSON, verification data, build notes, and token usage stats.

To run it:
```bash
cd examples/aspnet-webapi
python -m skill_eval --config eval.yaml run
```

### ASP.NET Core Razor Pages

The `examples/aspnet-razor-pages/` directory evaluates skill impact on server-rendered Razor Pages apps with forms, tables, and multi-page workflows:

**Scenarios:** Event Registration Portal, Property Management, Employee Directory & HR Portal

**23 dimensions** across 4 tiers:
| Tier | Count | Examples |
|------|-------|----------|
| Critical (3×) | 5 | Build & Run Success, Security Scan, Input Validation, NuGet Discipline, EF Migrations |
| High (2×) | 9 | Page Model Design, Form Handling & Validation, Business Logic Correctness, Prefer Built-in, Modern C# |
| Medium (1×) | 8 | Tag Helpers, Bootstrap, Layout & Partials, Accessibility |
| Low (0.5×) | 1 | Style conventions |

To run it:
```bash
cd examples/aspnet-razor-pages
python -m skill_eval --config eval.yaml run
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
python -m skill_eval run --runs 3
```

Each run:
1. **Generates** code for a randomly-selected scenario per config
2. **Verifies** the generated project (build, format, security, run)
3. **Analyzes** quality across all dimensions, producing `analysis-run-{N}.md`

After all runs complete, the **aggregator** (`src/skill_eval/aggregator.py`) parses per-run scores and produces:
- Aggregated score tables with weighted totals per config
- A scoring methodology section explaining tier weights
- "Overview" and "What Was Tested" sections summarizing the evaluation
- `scores-data.json` with structured score data

Runs execute with parallel generation across configs, a watchdog timeout with network-connection health checks, and per-config token usage tracking (`generation-usage.json`).

## Session Tracing

The session tracer (`src/skill_eval/session_tracer.py`) monitors which Copilot skills, plugins, and instruction files are actually loaded during code generation. This:

- **Prevents skill contamination** — isolates `skill_directories` per config so one config's skills don't leak into another
- **Verifies skill activation** — confirms the intended skills were loaded by parsing Copilot CLI session events from `~/.copilot/session-state`
- **Uses path-based matching** as the sole authority for skill detection

## The Agent: `@skill-eval`

The Copilot agent provides a conversational interface for the entire workflow:

| Workflow | What It Does |
|----------|-------------|
| **Setup** | Asks about your stack and scenarios, generates `eval.yaml` and prompts |
| **Scenario Authoring** | Helps write detailed app specifications |
| **Execution** | Runs the generate → verify → analyze pipeline |
| **Interpretation** | Summarizes findings, highlights strengths and weaknesses |
| **Iteration** | Suggests skill improvements based on analysis, re-runs evaluation |

Example conversation:
```
You:   @skill-eval what did the analysis find?

Agent: Your skill (react-components) vs baseline across 2 apps:

       ✅ Strong wins:
       - Component structure: proper atomic design (baseline uses monolithic)
       - TypeScript: 100% typed props (baseline has mixed any/unknown)

       ⚠️ Mixed:
       - State management: Context used correctly, but missed React Query

       ❌ No difference:
       - CSS: both used inline styles — add CSS module guidance to your skill

       Want me to suggest specific changes to your SKILL.md?
```

## Prerequisites

- **Python 3.10+**
- **GitHub Copilot CLI** (`copilot` command available in your terminal)
- Your tech stack's build tools (e.g., `dotnet`, `npm`, `go`)

## License

See [LICENSE](LICENSE) for details.
