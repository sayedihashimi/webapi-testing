# Copilot Skill Evaluation Framework

Evaluate how **GitHub Copilot custom skills** impact code generation quality. Generate the same apps under different skill configurations, verify they build and run, then produce a weighted comparative analysis — for **any tech stack**.

## Install

```bash
pipx install git+https://github.com/sayedihashimi/copilot-skill-eval
```

> **Prerequisites:** Python 3.10+, the [GitHub Copilot CLI](https://docs.github.com/en/copilot) (`copilot` command), and your tech stack's build tools (e.g., `dotnet`, `npm`, `go`).
>
> Don't have pipx? Install it first:
> ```bash
> pip install pipx
> pipx ensurepath
> ```
> Then restart your terminal.

## How It Works

You define what to evaluate in two files:

| File | Purpose |
|------|---------|
| **`eval.yaml`** | Scenarios (apps to generate), configurations (skill sets to compare), verification commands, and analysis dimensions |
| **`skill-sources.yaml`** | Where to get skills/plugins — local paths or git repos (cloned and cached automatically) |

The framework then runs a three-stage pipeline:

1. **Generate** — Builds the same apps with each skill configuration using Copilot CLI
2. **Verify** — Compiles, format-checks, security-scans, and runs each generated project
3. **Analyze** — Scores quality across weighted dimensions and produces a comparative report

## Quick Start

### 1. Initialize a project

```bash
skill-eval init
```

The interactive wizard walks you through creating `eval.yaml`, `skill-sources.yaml`, and starter scenario prompt files. It asks about your tech stack, scenarios, and skill sources (local or remote git repos).

### 2. Edit your scenario prompts

Scenario prompts in `prompts/scenarios/` describe the apps Copilot should generate. The more specific, the better the comparison. A good prompt includes:

| Section | Purpose |
|---------|---------|
| **Overview** | What the app does in 2–3 sentences |
| **Technology Stack** | Exact frameworks and versions |
| **Entities** | Domain models with fields and relationships |
| **Business Rules** | Non-trivial constraints and logic |
| **Endpoints** | API routes or pages to generate |

See the [Scenario Authoring Guide](docs/authoring-guide.md) for full details and examples.

### 3. Run the evaluation

```bash
skill-eval run
```

Reports land in `reports/`. The main output is `reports/analysis.md` with per-dimension scores and a weighted comparison across all configurations.

## Configuration

### `eval.yaml`

```yaml
name: "My Skill Evaluation"
description: "Evaluate my React skills"
runs: 3                          # runs per configuration (increases reliability)
generation_model: "claude-opus-4.6"  # model for code generation
analysis_model: "gpt-5.3-codex"     # model for comparative analysis

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
    plugins: []
  - name: my-react-skill
    label: "My React Skill"
    skills:
      - source: react-components      # references skill-sources.yaml

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
    tier: high                       # critical (3×), high (2×), medium (1×), low (0.5×)
    evaluation_method: llm           # llm, automated, or hybrid
```

Configurations can reference skills/plugins in two ways:
- **Source reference** — `{source: my-source}` or `{source: my-source, path: subfolder}` (looks up `skill-sources.yaml`)
- **Local path** — `skills/my-skill` (plain string, relative to project root)

### `skill-sources.yaml`

Define where to get your skills and plugins:

```yaml
sources:
  - name: react-components
    type: git
    url: https://github.com/myorg/react-copilot-skill
    ref: main                    # optional: branch or tag
    path: "."                    # optional: subfolder within repo

  - name: my-local-skills
    type: local
    path: ./my-skills            # relative to project root
```

Git sources are cloned into `~/.skill-eval/cache/` on first use and updated on subsequent runs. Override the cache location with `--cache-dir` or set `cache_dir` in `skill-sources.yaml`.

### Analysis Dimensions

Dimensions are the quality criteria used to score and compare generated code:

| Field | Purpose |
|-------|---------|
| `name` | Short label (e.g., "Error Handling") |
| `description` | What this dimension measures |
| `what_to_look_for` | Concrete things to check in the code |
| `why_it_matters` | Why this matters for production quality |
| `tier` | Weight tier: `critical` (3×), `high` (2×), `medium` (1×), `low` (0.5×) |
| `evaluation_method` | How it's evaluated: `llm`, `automated`, `hybrid` |

**Tips:**
- Start with 5–10 dimensions — you can always add more
- Be specific in `what_to_look_for` — vague criteria produce vague scores
- `skill-eval init` suggests stack-specific dimensions automatically

## CLI Reference

### Pipeline commands

```bash
skill-eval run                              # Full pipeline: generate → verify → analyze
skill-eval run --runs 5                     # 5 runs per config for higher reliability
skill-eval run --skip-generate              # Re-verify + re-analyze existing output
skill-eval run --analyze-only               # Re-analyze only
skill-eval run -c my-skill -c baseline      # Only run specific configurations
skill-eval run --generation-model claude-sonnet-4  # Override generation model
skill-eval run -m gpt-5.3-codex            # Override analysis model

skill-eval generate                         # Generate code only
skill-eval generate --resume                # Skip runs where output already exists
skill-eval generate --generation-model claude-opus-4.6  # Specify generation model
skill-eval verify                           # Verify builds only
skill-eval analyze                          # Analyze only
skill-eval analyze -m gpt-5.3-codex        # Specify analysis model
```

### Setup & validation

```bash
skill-eval init                             # Interactive project setup
skill-eval validate-config                  # Check config is valid, sources resolve
skill-eval ci-setup                         # Generate GitHub Actions workflow
skill-eval ci-setup --schedule "0 6 * * 1"  # With weekly schedule
```

### Global options

```bash
skill-eval --config path/to/eval.yaml run                 # Custom config path
skill-eval --skill-sources path/to/skill-sources.yaml run # Custom sources path
skill-eval --cache-dir /tmp/skill-cache run               # Custom cache directory
skill-eval --output-dir ./my-output run                   # Custom output directory
skill-eval --reports-dir ./my-reports run                  # Custom reports directory
skill-eval --project-root /path/to/project run            # Custom project root
```

## CI / GitHub Actions

Generate a workflow that runs evaluations in CI:

```bash
skill-eval ci-setup
```

This creates `.github/workflows/skill-eval.yml` with:
- Manual trigger via `workflow_dispatch` (with skip/analyze-only options)
- Python + skill-eval installation
- Config validation → pipeline execution
- Report and output artifact upload

Options: `--runs-on`, `--python-version`, `--schedule`, `--timeout`.

## Using the Agent

If you have the Copilot agent feature, you can use `@skill-eval` for a conversational workflow:

```
@skill-eval I wrote a custom skill for ASP.NET Core Web APIs.
I want to see if it actually improves code generation quality.
```

The agent handles setup, execution, and interpretation. See the [Agent Guide](docs/agent-guide.md) for details.

## Examples

The `examples/` directory contains complete evaluation projects you can run immediately to see the framework in action.

| Example | Scenarios | Configurations | Dimensions |
|---------|-----------|---------------|------------|
| **[aspnet-webapi](examples/aspnet-webapi/)** | Fitness Studio API, Library API, Vet Clinic API | 5 configs (baseline + 4 skill sets) | 24 dimensions across 4 tiers |
| **[aspnet-razor-pages](examples/aspnet-razor-pages/)** | Event Registration, Property Management, Employee Directory | 5 configs (baseline + 4 skill sets) | 23 dimensions across 4 tiers |

Each example includes `eval.yaml`, `skill-sources.yaml` (with git references to external repos), and detailed scenario prompts.

### Running an example

```bash
# Clone the repo
git clone https://github.com/sayedihashimi/copilot-skill-eval
cd copilot-skill-eval

# Install the tool
pipx install .

# Navigate to an example and validate the config
# (this also clones the skill repos into ~/.skill-eval/cache/)
cd examples/aspnet-webapi
skill-eval validate-config

# Run the full pipeline
skill-eval run

# Or run individual steps
skill-eval generate                     # Generate code only
skill-eval generate -c dotnet-webapi    # Generate just one configuration
skill-eval verify                       # Verify builds only
skill-eval analyze                      # Analyze only

# Run with more passes for higher reliability
skill-eval run --runs 5
```

Reports are written to `reports/`. The main output is `reports/analysis.md`.

## Further Reading

- [Scenario Authoring Guide](docs/authoring-guide.md) — how to write effective scenario prompts
- [Architecture & Internals](docs/architecture.md) — pipeline details, session tracing, source resolution
- [Agent Guide](docs/agent-guide.md) — using the conversational `@skill-eval` agent

## License

See [LICENSE](LICENSE) for details.
