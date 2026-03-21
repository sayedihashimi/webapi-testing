# Copilot Skill Evaluation Framework

A framework for evaluating how **GitHub Copilot custom skills** impact code generation quality. It generates the same apps under different skill configurations, then produces a detailed comparative analysis — for **any tech stack**.

## How It Works

```
┌──────────────────────────────────────────────────────────┐
│  @skill-eval agent  (or  skill-eval CLI)                 │
│                                                          │
│  1. GENERATE  — Build the same apps with different       │
│                 skill configs using Copilot CLI           │
│  2. VERIFY    — Build & run each generated project       │
│  3. ANALYZE   — Compare quality across N dimensions      │
│                 and produce a verdict report              │
└──────────────────────────────────────────────────────────┘
```

You define:
- **Scenarios** — realistic app specifications (prompt files)
- **Configurations** — skill sets to compare (baseline vs your skills)
- **Dimensions** — quality criteria to evaluate (type safety, error handling, etc.)

The framework generates identical apps under each configuration, then uses Copilot to analyze and compare the results.

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

This runs the full pipeline: generate → verify → analyze. The analysis report lands in `reports/analysis.md`.

#### Individual steps

```bash
python -m skill_eval generate                  # Generate code only
python -m skill_eval generate -c my-skill      # Generate one configuration
python -m skill_eval verify                    # Build/run verification only
python -m skill_eval analyze                   # Analysis only
python -m skill_eval run --skip-generate       # Skip generation, verify + analyze
python -m skill_eval run --analyze-only        # Only run analysis
python -m skill_eval validate-config           # Check eval.yaml is valid
```

## Configuration: `eval.yaml`

```yaml
name: "My Skill Evaluation"
description: "Evaluate my React skills"

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
│   └── prompts/
├── prompts/scenarios/               # Your app specification prompts
├── skills/                          # Skills to evaluate (you bring these)
├── plugins/                         # Plugins to evaluate (optional)
├── src/skill_eval/                  # Python CLI (automation layer)
│   ├── cli.py                       # CLI entry point
│   ├── config.py                    # YAML config + Pydantic models
│   ├── generate.py                  # Step 1: Copilot code generation
│   ├── verify.py                    # Step 2: Build & run checks
│   ├── analyze.py                   # Step 3: Comparative analysis
│   ├── skill_manager.py             # Copilot skill registration
│   ├── prompt_renderer.py           # Jinja2 template rendering
│   └── init_cmd.py                  # Interactive project setup
├── templates/                       # Jinja2 prompt templates
│   ├── create-all-apps.md.j2        # Generation prompt template
│   ├── analyze.md.j2                # Analysis prompt template
│   └── scenario.prompt.md.j2        # Starter scenario template
├── output/                          # Generated code (gitignored)
├── reports/                         # Analysis reports (gitignored)
├── examples/
│   └── aspnet-webapi/               # Complete working example
└── pyproject.toml                   # Python dependencies
```

## Example: ASP.NET Core Web API

The `examples/aspnet-webapi/` directory contains a complete working evaluation that tests four Copilot skill configurations across three realistic .NET Web API apps:

**Scenarios:** Fitness Studio Booking API, Community Library API, Veterinary Clinic API

**Configurations:**
| Config | What It Tests |
|--------|--------------|
| `no-skills` | Baseline — Copilot with no custom guidance |
| `dotnet-webapi` | A single custom skill for Web API patterns |
| `dotnet-artisan` | A full plugin chain with 9 skills + agents |
| `managedcode-dotnet-skills` | Community skills covering 6 .NET areas |

**15 dimensions:** API style, sealed types, primary constructors, DTO design, CancellationToken propagation, AsNoTracking usage, and more.

To run it:
```bash
cd examples/aspnet-webapi
python -m skill_eval --config eval.yaml run
```

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
