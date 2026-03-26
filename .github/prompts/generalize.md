# Generalize the Copilot Skill Evaluation Framework

This document is a plan for generalizing the current repo into a reusable tool that any skill author can use to evaluate their Copilot skills, regardless of tech stack.

**Target audience:** Skill authors who want to measure whether their custom Copilot skills actually improve code generation quality.

---

## Problem Statement

This repo is a working Copilot skill evaluation framework — but it's hardcoded for ASP.NET Core Web APIs. Everything is baked in: the 3 app scenarios (Fitness, Library, Vet), the 4 skill configurations, the 15 .NET-specific analysis dimensions, and the `dotnet build`/`dotnet run` verification commands. A skill author working in React, Go, Python, or any other stack can't use this without rewriting the entire repo.

**Goal:** Generalize this into a reusable, config-driven tool that any skill author can use to evaluate their Copilot skills.

---

## Form Factor Analysis

Three approaches were considered. **Option A is recommended.**

### Option A: Config-Driven Framework (repo you fork/clone) ✅ Recommended

Users clone the repo, edit a YAML config file + write their own prompt files, and run the pipeline.

| Pros | Cons |
|------|------|
| Simplest to build | Requires forking — harder to get updates |
| Full visibility into internals | Users must learn repo structure |
| Easy to customize beyond config | Not a "polished tool" feel |
| Familiar model (like Docusaurus, Jekyll) | |

### Option B: CLI Tool (install & run)

Users install a tool (`pip install copilot-skill-eval`) and run commands like `skill-eval init`, `skill-eval generate`, `skill-eval analyze`.

| Pros | Cons |
|------|------|
| Clean, polished UX | More complex to build (CLI framework, packaging) |
| Upgradeable via package manager | Less transparent internals |
| Feels like a real tool | Needs a runtime assumption |
| Best for wide adoption | Harder to deeply customize |

### Option C: Template/Scaffold Generator

Users run a scaffolder that generates a new evaluation project for their stack.

| Pros | Cons |
|------|------|
| Users own their generated project | No upgrade path once scaffolded |
| Full customization freedom | Two codebases to maintain |
| Best of both worlds | Most complex to build |

### Why Option A

1. **Skill authors are technical** — they're already writing SKILL.md files and working with Copilot prompts
2. **The pipeline is inherently prompt-driven** — the real work happens in markdown files and Copilot CLI calls, not compiled code
3. **Customization is key** — skill authors will want to tweak dimensions, add scenarios, modify prompts
4. **Shipping speed** — can deliver an MVP by restructuring what already exists
5. **Upgrade path** — GitHub template repo + ability to pull upstream changes
6. **Future-proof** — can evolve into Option B later without throwing away work

---

## Architecture: What Changes

### Current State (Hardcoded)

```
Hardcoded app specs → Hardcoded skill configs → Hardcoded PS script → Hardcoded analysis dimensions
       ↓                      ↓                       ↓                        ↓
  3 .NET apps           4 .NET skills         dotnet build/run         15 .NET dimensions
```

### Target State (Agent-Driven + Config-Driven)

The primary UX is a **Copilot agent** (`@skill-eval`) that guides users conversationally. Underneath, a **Python CLI** handles automation and can be used directly for scripting/CI.

```
┌─────────────────────────────────────────────────┐
│  @skill-eval agent (primary UX)                 │
│  "Help me evaluate my React skills"             │
│                                                 │
│  ┌─ Setup ──────────────────────────────────┐   │
│  │ Asks questions → generates eval.yaml     │   │
│  │ Helps write scenario prompts             │   │
│  └──────────────────────────────────────────┘   │
│  ┌─ Execute ────────────────────────────────┐   │
│  │ Calls: skill-eval generate               │   │
│  │ Calls: skill-eval verify                 │   │
│  │ Calls: skill-eval analyze                │   │
│  └──────────────────────────────────────────┘   │
│  ┌─ Interpret ──────────────────────────────┐   │
│  │ Reads reports/analysis.md                │   │
│  │ Summarizes findings & suggests changes   │   │
│  │ Guides skill iteration                   │   │
│  └──────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
         │ invokes
         ▼
┌─────────────────────────────────────────────────┐
│  skill-eval CLI (automation layer)              │
│                                                 │
│  eval.yaml (user config)                        │
│      ├── scenarios: [user's prompt files]       │
│      ├── configurations: [skill sets to compare]│
│      ├── verification: { build_cmd, run_cmd }   │
│      └── dimensions: [quality criteria]         │
│           ↓                                     │
│  skill-eval generate → Copilot CLI per config   │
│  skill-eval verify   → build/run commands       │
│  skill-eval analyze  → dynamic analysis prompt  │
│           ↓                                     │
│  reports/analysis.md                            │
└─────────────────────────────────────────────────┘
```

### What Becomes Generic vs What Stays Domain-Specific

| Component | Currently | After Generalization |
|-----------|-----------|---------------------|
| App scenario prompts | 3 hardcoded .NET prompts | User-written, referenced by config |
| Skill configurations | 4 hardcoded .NET skills | User-defined in config |
| Build/run commands | `dotnet build` / `dotnet run` | User-specified in config |
| Analysis dimensions | 15 hardcoded .NET dimensions | User-defined in config |
| Generation script | Hardcoded `$allRuns` hashtable | Reads from `eval.yaml` |
| Orchestration prompt | `generate-apps.md` (static) | Generated from config via Jinja2 templates |
| Analysis prompt | `analyze.md` (static .NET dims) | Generated from config dimensions via Jinja2 templates |
| Repo structure | Flat, mixed concerns | Separated: config / prompts / scripts / output |

---

## Proposed Directory Structure

```
copilot-skill-eval/                      # New repo name
├── eval.yaml                            # ⭐ Main configuration file
├── .github/
│   └── copilot/
│       └── agents/
│           └── skill-eval.md            # ⭐ Copilot agent definition (primary UX)
├── prompts/
│   └── scenarios/                       # User-written app specification prompts
│       └── (user's .prompt.md files)
├── skills/                              # Skills to evaluate (user brings these)
│   └── (user's skill directories)
├── plugins/                             # Plugins to evaluate (optional)
│   └── (user's plugin directories)
├── src/
│   └── skill_eval/                      # Python package (automation layer)
│       ├── __init__.py
│       ├── __main__.py                  # Entry point: `python -m skill_eval`
│       ├── cli.py                       # CLI commands (click/typer)
│       ├── config.py                    # YAML config reader & validator
│       ├── generate.py                  # Step 1: Generate code with Copilot CLI
│       ├── verify.py                    # Step 2: Build & run verification
│       ├── analyze.py                   # Step 3: Comparative analysis
│       ├── skill_manager.py             # Skill registration/unregistration
│       ├── prompt_renderer.py           # Renders Jinja2 templates from config
│       └── init.py                      # Init: scaffold a new eval.yaml + prompts
├── templates/
│   ├── create-all-apps.md.j2           # Jinja2 template for the generation prompt
│   ├── analyze.md.j2                   # Jinja2 template for the analysis prompt
│   └── scenario.prompt.md.j2           # Starter template for app scenarios
├── output/                              # Generated code (gitignored)
│   └── {config-name}/
│       └── {scenario-name}/
├── reports/                             # Analysis output
│   ├── analysis.md
│   └── build-notes.md
├── examples/                            # ⭐ Complete working examples
│   └── aspnet-webapi/                   # The current repo's content as an example
│       ├── eval.yaml
│       ├── prompts/scenarios/
│       │   ├── fitness-studio.prompt.md
│       │   ├── library.prompt.md
│       │   └── vet-clinic.prompt.md
│       └── skills/
│           ├── dotnet-webapi/
│           └── managedcode-dotnet-skills/
├── pyproject.toml                       # Python project config (deps, entry points)
├── README.md                            # New README: how to use the framework
├── CONTRIBUTING.md
└── LICENSE
```

---

## Configuration Schema: `eval.yaml`

```yaml
# Evaluation metadata
name: "My Skill Evaluation"
description: "Evaluate how my custom skills improve code generation"

# Scenarios: what apps/projects to generate
# Each scenario is a prompt file that describes what Copilot should build
scenarios:
  - name: TodoApi
    prompt: prompts/scenarios/todo-api.prompt.md
    description: "Simple CRUD API with authentication"
  - name: ECommerceApi
    prompt: prompts/scenarios/ecommerce-api.prompt.md
    description: "Product catalog with cart and checkout"

# Configurations: skill sets to compare
# Each configuration defines what skills/plugins Copilot has access to
configurations:
  - name: no-skills
    label: "Baseline (no skills)"
    skills: []        # No skills — pure Copilot baseline
    plugins: []
  - name: my-skill
    label: "My Custom Skill"
    skills:
      - skills/my-webapi-skill
    plugins: []
  - name: community-skills
    label: "Community Skills"
    skills:
      - skills/community-pack
    plugins: []

# Verification: how to check that generated code works
verification:
  build:
    command: "dotnet build"              # Command to build the project
    working_directory: "."               # Relative to project root
    success_pattern: "Build succeeded"   # Optional: regex to confirm success
  run:
    command: "dotnet run"                # Command to run the project
    timeout_seconds: 15                  # How long to wait before checking
    health_check:                        # Optional: HTTP health check
      url: "http://localhost:5000/health"
      expected_status: 200

# Analysis dimensions: what quality criteria to evaluate
# These drive the comparative analysis report
dimensions:
  - name: "API Style"
    description: "Controllers (MVC pattern) vs Minimal APIs (lambda-based)"
    what_to_look_for: |
      Check Program.cs and endpoint files. Look for MapGet/MapPost (minimal)
      vs [ApiController] decorated classes (controllers).
    why_it_matters: "Minimal APIs have lower overhead and are the modern .NET default"

  - name: "Error Handling"
    description: "How exceptions and error responses are structured"
    what_to_look_for: |
      Check for IExceptionHandler, middleware, try/catch patterns.
      Look at HTTP status codes returned for different error types.
    why_it_matters: "Consistent error handling is critical for API consumers"

  - name: "Type Safety"
    description: "Use of sealed types, records, and immutable patterns"
    what_to_look_for: |
      Check class/record declarations for 'sealed' modifier.
      Look at DTO design — records vs mutable classes.
    why_it_matters: "Sealed types enable JIT optimizations and signal design intent"

  # Users add as many dimensions as they want...

# Output settings
output:
  directory: output                  # Where generated code goes
  reports_directory: reports         # Where analysis reports go
  analysis_file: analysis.md        # Main analysis report filename
  notes_file: build-notes.md        # Build/run status report filename
```

---

## Agent Design: `@skill-eval`

The agent is the **primary user interface**. It lives at `.github/copilot/agents/skill-eval.md` and follows the standard Copilot agent format (YAML frontmatter + markdown body).

### Agent Definition (sketch)

```yaml
---
name: skill-eval
description: >
  Guides skill authors through evaluating their Copilot custom skills.
  Helps set up evaluations, write scenario prompts, run the generate → verify → analyze
  pipeline, interpret results, and iterate on skill quality.
  Triggers on: evaluate my skill, test my skill, skill evaluation, compare skills,
  how good is my skill, benchmark skill, skill quality.
capabilities:
  - Guide users through evaluation setup (eval.yaml, scenario prompts)
  - Help author scenario prompts for any tech stack
  - Run the full evaluation pipeline (generate, verify, analyze)
  - Interpret analysis results and surface key findings
  - Suggest specific improvements to skill definitions
  - Re-run individual pipeline stages for iteration
  - Explain how the evaluation framework works
tools:
  - Read
  - Edit
  - Grep
  - Glob
  - Bash
---
```

### Agent Workflow Sections

The agent body defines five conversational workflows:

#### 1. Setup Workflow
When the user wants to start a new evaluation:
- Ask what tech stack and what skills they want to evaluate
- Ask them to describe 2–3 realistic app scenarios for their domain
- Generate `eval.yaml` with their answers
- Create starter scenario prompt files from the template
- Explain what to do next (refine prompts, copy skills into `skills/`)

#### 2. Scenario Authoring Workflow
When the user needs help writing scenario prompts:
- Read existing prompts in `prompts/scenarios/` for context
- Ask the user to describe the domain and key entities
- Draft a scenario prompt following the template structure (Overview, Entities, Relationships, Business Rules, Endpoints, Seed Data)
- Iterate based on user feedback

#### 3. Execution Workflow
When the user wants to run the evaluation:
- Validate that `eval.yaml` exists and is well-formed
- Check that scenario prompts and skill directories exist
- Run `skill-eval generate` (or individual configurations if requested)
- Run `skill-eval verify` and report build/run results
- Run `skill-eval analyze` and wait for the report
- Handle errors: if a build fails, report which configuration/scenario and suggest fixes

#### 4. Interpretation Workflow
When the user wants to understand results:
- Read `reports/analysis.md` and `reports/build-notes.md`
- Summarize findings: which skill configuration performed best overall, and on which dimensions
- Highlight the most impactful differences (the ones that matter most for production readiness)
- Compare the user's skill against the baseline — what concrete improvements did it produce?
- If one dimension is consistently weak, point to the specific section of the skill that needs strengthening

#### 5. Iteration Workflow
When the user wants to improve their skill and re-evaluate:
- Read the current SKILL.md and the analysis findings
- Suggest specific additions/changes to the skill definition based on weak dimensions
- After the user edits their skill, re-run just that configuration: `skill-eval generate --configurations my-skill`
- Re-run analysis and compare with previous results
- Repeat until the user is satisfied

### Agent Boundaries

The agent explicitly does NOT:
- Modify the user's skill files without asking (it suggests changes, the user applies them)
- Run the full pipeline without confirmation (it explains what it's about to do first)
- Generate code directly — all code generation goes through Copilot CLI via the pipeline
- Handle CI/CD setup, PyPI publishing, or other operational concerns

### Example Conversations

**New user:**
```
User: @skill-eval I wrote a custom skill for React component generation.
      I want to see if it actually makes Copilot produce better code.

Agent: Great! Let's set up an evaluation. I'll need a few things:
       1. What React scenarios should we test? (e.g., dashboard app, e-commerce storefront)
       2. Do you have other skills to compare against, or just baseline vs your skill?
       3. How do you build/run your React apps? (e.g., npm run build, npm start)

User: Let's do a dashboard and a blog. Just baseline vs my skill. Standard npm commands.

Agent: I'll create your eval.yaml and starter prompt files...
       [creates eval.yaml, prompts/scenarios/dashboard.prompt.md, prompts/scenarios/blog.prompt.md]
       
       Next, flesh out those scenario prompts — describe what each app should do.
       When you're ready, say "run the evaluation" and I'll kick off the pipeline.
```

**Interpreting results:**
```
User: @skill-eval what did the analysis find?

Agent: Here's the summary from your evaluation:

       Your skill (react-components) vs baseline across 3 apps:
       
       ✅ Strong wins:
       - Component structure: Your skill produces proper atomic design (baseline uses monolithic components)
       - TypeScript usage: 100% typed props with your skill vs mixed any/unknown baseline
       
       ⚠️ Mixed:
       - State management: Your skill uses Context correctly, but missed React Query for the dashboard
       
       ❌ No difference:
       - CSS approach: Both used inline styles — consider adding CSS module guidance to your skill
       
       Want me to suggest specific changes to your SKILL.md?
```

---

## Implementation Plan

### Phase 1: Agent Definition + Core Framework

#### 1.1 Create the agent definition

- Write `.github/copilot/agents/skill-eval.md` with the frontmatter and workflow sections described above
- Define the five workflows: Setup, Scenario Authoring, Execution, Interpretation, Iteration
- Include explicit boundaries and trigger lexicon
- The agent invokes the Python CLI via Bash tool calls

#### 1.2 Project scaffolding

- Create `pyproject.toml` with dependencies: `pyyaml`, `click` (or `typer`), `jinja2`, `pydantic` (for config validation)
- Set up `src/skill_eval/` package structure
- Define CLI entry point: `skill-eval` command (via `pyproject.toml [project.scripts]`)
- Users run via `pip install .` then `skill-eval`, or `python -m skill_eval`

#### 1.3 Create configuration schema and reader

- Define the `eval.yaml` schema (as shown above)
- Write `src/skill_eval/config.py` — reads and validates eval.yaml using Pydantic models
- Pydantic gives typed config objects, clear error messages, and schema documentation for free
- Validate required fields, file path references, etc.

#### 1.4 Refactor generation to be config-driven

- Write `src/skill_eval/generate.py`
- Read scenarios and configurations from eval.yaml
- Dynamically build Copilot CLI commands based on config
- Register/unregister skills per config (port `Add-SkillDirectory`/`Remove-SkillDirectory` logic from existing PowerShell)
- Support `--configurations` filter (like current `-Apps`) and `--scenarios` filter
- Render the `create-all-apps.md.j2` Jinja2 template with scenario list
- CLI: `skill-eval generate [--configurations X,Y] [--scenarios A,B]`

#### 1.5 Refactor verification to be config-driven

- Write `src/skill_eval/verify.py`
- Read build/run commands from eval.yaml verification section
- Run build command for each generated project (via `subprocess`)
- Run the app with timeout and optional HTTP health check (via `httpx` or `urllib`)
- Output structured results to `reports/build-notes.md`
- CLI: `skill-eval verify`

#### 1.6 Refactor analysis to be config-driven

- Write `src/skill_eval/analyze.py`
- Read dimensions from eval.yaml
- Render the `analyze.md.j2` Jinja2 template with dimensions and config names
- Run Copilot CLI with the generated analysis prompt
- Output to `reports/analysis.md`
- CLI: `skill-eval analyze`

#### 1.7 Create the orchestrator

- Wire up `src/skill_eval/cli.py` with a top-level `skill-eval run` command
- Calls generate → verify → analyze in sequence
- Supports `--skip-generate`, `--skip-verify`, `--analyze-only` flags
- Handles errors gracefully at each stage

### Phase 2: Prompt Templates (Jinja2)

#### 2.1 Create template system for dynamic prompts

- `templates/create-all-apps.md.j2` — generates the "build all apps" prompt
  - Parameterized by: scenario names, scenario prompt paths, output directory
- `templates/analyze.md.j2` — generates the analysis prompt
  - Parameterized by: dimensions, configuration names, output directories
- Jinja2 provides clean template syntax with loops, conditionals, and filters

#### 2.2 Create starter templates for users

- `templates/scenario.prompt.md.j2` — a well-commented template for writing app scenario prompts
  - Sections: Overview, Entities, Relationships, Business Rules, Endpoints, Seed Data
  - Comments explaining what each section does and why

### Phase 3: Init / Getting Started Experience

#### 3.1 Create initialization command

- `skill-eval init`
- Interactive: asks user for project name, tech stack, number of scenarios (via click prompts)
- Generates a starter `eval.yaml` with sensible defaults
- Creates empty scenario prompt files from template
- Creates a `skills/` directory with a README explaining what to put there
- Prints a "next steps" guide

#### 3.2 Create example: ASP.NET Web API (current content)

- Move the current repo's domain-specific content into `examples/aspnet-webapi/`
  - The 3 app prompt files
  - The skill definitions (dotnet-webapi, managedcode-dotnet-skills)
  - The analysis dimensions (as yaml config)
  - A working `eval.yaml`
- This serves as both documentation and a testable example

### Phase 4: Documentation & Polish

#### 4.1 New README.md

- What this framework does (general, not .NET-specific)
- Quick Start: `@skill-eval help me set up an evaluation` (agent-first)
- Alternative: CLI Quick Start (`skill-eval init` → configure → `skill-eval run`)
- Agent reference: what the agent can do and example conversations
- Configuration reference (eval.yaml schema)
- Writing good scenario prompts (tips + examples)
- Defining analysis dimensions (tips + examples)
- The ASP.NET Web API example walkthrough
- How the pipeline works (architecture diagram)

#### 4.2 CONTRIBUTING.md

- How to add new examples for other tech stacks
- How to contribute to the core framework

#### 4.3 Example gallery

- The ASP.NET Web API example (migrated from current repo)
- Possibly stub examples for other stacks (React, Go, Python Flask) — even if just the eval.yaml + empty prompts — to show the framework's flexibility

---

## Key Design Decisions

### 1. YAML over JSON for config

- More readable for the complex nested structure
- Comments are supported (important for a config file users will edit)
- Widely familiar across tech stacks

### 2. Python as the runtime

- Near-universally available across all platforms and developer environments
- First-class YAML support (`pyyaml`) — no awkward parsing workarounds
- Mature CLI framework (`click`) for clean subcommand UX
- Jinja2 for prompt templates — the gold standard for text templating
- Pydantic for config validation — typed models, clear errors, auto-generated schema docs
- Opens the door to `pip install copilot-skill-eval` later (evolving to Option B without a rewrite)
- Familiar to the broadest audience of skill authors (regardless of their primary tech stack)

### 3. Jinja2 for prompt generation (not string concatenation)

- Analysis dimensions MUST be dynamic — the whole point is users define their own
- Generation prompts need to reference user-defined scenarios
- Jinja2 provides loops, conditionals, and filters — essential for rendering dimension lists, scenario tables, etc.
- Templates are readable markdown with minimal template syntax

### 4. Pydantic for config validation

- Typed config objects catch errors early with clear messages
- Schema can be exported as JSON Schema for editor autocomplete
- Default values, optional fields, and custom validators are built-in
- Far cleaner than manual dictionary validation

### 5. Output directory structure: `output/{config-name}/{scenario-name}/`

- Clean separation by configuration, then by scenario
- Easy to compare: just look at the same scenario across config directories
- Gitignored by default (generated code shouldn't be committed)

### 6. Skills/plugins live in the eval repo

- Users copy or symlink their skills into the `skills/` directory
- This keeps the evaluation self-contained and reproducible
- Alternative: reference skills by absolute path — but that breaks portability

---

## Risks & Mitigations

| Risk | Mitigation |
|------|-----------|
| Python version fragmentation | Require Python 3.10+ (widely available); document in README |
| Users may not know how to write good scenario prompts | Provide detailed template + tips in docs + the ASP.NET example |
| Analysis quality depends on Copilot understanding the dimensions | Keep dimension descriptions detailed and specific in the config |
| Different tech stacks need very different verification | Make verification commands fully configurable, with optional health checks |
| Copilot CLI availability/changes | Document required Copilot CLI version; abstract CLI calls behind a wrapper for future-proofing |

---

## Out of Scope (for now)

- **Automated scoring** — Currently the analysis is qualitative (Copilot-generated prose). Automated numeric scoring could come later.
- **CI/CD integration** — Running evaluations in GitHub Actions is a natural extension but not MVP.
- **PyPI distribution** — Publishing to PyPI (`pip install copilot-skill-eval`) is a natural Phase 5 but not needed for initial ship.
- **Visual dashboard** — The output is markdown reports. A web UI could come later.
- **Diff/regression tracking** — Comparing analysis results across multiple evaluation runs over time.

---

## Appendix A: File Migration Map

Every file in the current repo mapped to its destination in the new structure.

### Files that become Jinja2 templates

| Current File | New Location | What Changes |
|---|---|---|
| `.github/prompts/create-all-apps.md` | `templates/create-all-apps.md.j2` | Scenario names/paths become `{{ scenario.name }}` / `{{ scenario.prompt }}` loop variables; skill directives removed (handled by CLI) |
| `.github/prompts/analyze.md` | `templates/analyze.md.j2` | Hardcoded 15 dimensions become `{% for dim in dimensions %}` loop; `src-*` directory pattern becomes `{{ config.output_directory }}/{{ configuration.name }}` |

### Files that move to the ASP.NET example

| Current File | New Location |
|---|---|
| `.github/prompts/create-fitness-studio-api.prompt.md` | `examples/aspnet-webapi/prompts/scenarios/fitness-studio.prompt.md` |
| `.github/prompts/create-library-api.prompt.md` | `examples/aspnet-webapi/prompts/scenarios/library.prompt.md` |
| `.github/prompts/create-vet-clinic-api.prompt.md` | `examples/aspnet-webapi/prompts/scenarios/vet-clinic.prompt.md` |
| `contrib/skills/dotnet-webapi/` | `examples/aspnet-webapi/skills/dotnet-webapi/` |
| `contrib/skills/managedcode-dotnet-skills/` | `examples/aspnet-webapi/skills/managedcode-dotnet-skills/` |
| `contrib/plugins/dotnet-artisan/` | `examples/aspnet-webapi/plugins/dotnet-artisan/` |
| `analysis.md` | `examples/aspnet-webapi/reports/analysis.md` (as sample output) |
| `generate-all-apps-notes.md` | `examples/aspnet-webapi/reports/build-notes.md` (as sample output) |

### Files that are replaced by new equivalents

| Current File | Replaced By | Notes |
|---|---|---|
| `generate-apps.ps1` | `src/skill_eval/generate.py` + `cli.py` | Core logic ported to Python |
| `.github/prompts/generate-apps.md` | Agent workflow (Execution) + `skill-eval run` CLI | Orchestration moves to agent/CLI |
| `.github/prompts/create-script.md` | Not needed | Was used to create the PowerShell script |
| `.github/prompts/create-readme.md` | Not needed | Was used to create the old README |
| `.github/prompts/create-app-plan-files.md` | Not needed | Planning artifact, not part of framework |

### Files that stay (possibly updated)

| Current File | Notes |
|---|---|
| `README.md` | Rewritten for the generalized framework |
| `LICENSE` | Stays as-is |
| `.gitignore` | Updated to ignore `output/`, `reports/`, `__pycache__/`, etc. |

### New files to create

| File | Purpose |
|---|---|
| `eval.yaml` | Starter config (empty/minimal) |
| `examples/aspnet-webapi/eval.yaml` | Full working config for the ASP.NET example |
| `.github/copilot/agents/skill-eval.md` | Copilot agent definition |
| `pyproject.toml` | Python package config |
| `src/skill_eval/*.py` | Python CLI package (7-8 modules) |
| `templates/*.j2` | Jinja2 prompt templates (3 files) |
| `CONTRIBUTING.md` | Contribution guide |

### Directories to delete after migration

| Directory | Reason |
|---|---|
| `src-no-skills/` | Generated output — will be in `output/` going forward |
| `src-dotnet-webapi/` | Generated output |
| `src-dotnet-artisan/` | Generated output |
| `src-managedcode-dotnet-skills/` | Generated output |
| `contrib/` | Content moved to `examples/aspnet-webapi/` |
| `.github/prompts/` (most files) | Replaced by templates + examples; only `generalize.md` and possibly the agent remain |

---

## Appendix B: Jinja2 Template Content

### `templates/create-all-apps.md.j2`

This template renders the prompt that tells Copilot to build all scenario apps. It replaces the current hardcoded `create-all-apps.md`.

```jinja2
Build the following apps using the specification files listed below. Make sure to use the skills that are available.

{% for scenario in scenarios %}
## {{ scenario.name }}

Build the app described in @{{ scenario.prompt }}. Put the output in the `{{ output_directory }}/{{ scenario.name }}` directory.

{% endfor %}

I want you to output what skills you are using during the code generation. When you are done building the apps, create a file in `{{ output_directory }}` named "gen-notes.md" which includes details of the skills that were used during the generation of these apps.

When building these apps I want you to run them in a separate context and isolated from each other. I don't want any knowledge shared between the agents creating these apps.
```

### `templates/analyze.md.j2`

This template renders the analysis prompt dynamically from the user's configured dimensions. It replaces the current hardcoded `analyze.md`.

```jinja2
You are analyzing code generated by different Copilot configurations. Your task is to produce a detailed comparative analysis report.

## Instructions

1. **Discover source directories**: Find all directories matching the pattern `{{ output_directory }}/*`. Each directory contains the same set of apps generated with a different Copilot skill configuration.

2. **Identify configurations**: For each directory, check for a `gen-notes.md` file to determine what Copilot configuration was used. If none exists, infer from the directory name. The expected configurations are:
{% for config in configurations %}
   - `{{ config.name }}` — {{ config.label }}
{% endfor %}

3. **Identify scenarios**: Each configuration directory should contain:
{% for scenario in scenarios %}
   - `{{ scenario.name }}/` — {{ scenario.description }}
{% endfor %}

4. **Analyze each app across all directories**: For each app, compare the implementation across all configuration directories. Focus on the following dimensions:

{% for dim in dimensions %}
   - **{{ dim.name }}**: {{ dim.description }}
     - *What to look for:* {{ dim.what_to_look_for | replace('\n', ' ') }}
     - *Why it matters:* {{ dim.why_it_matters }}
{% endfor %}

   Add more dimensions if you discover additional meaningful differences beyond the list above.

5. **Produce the report**: Generate a Markdown file named `{{ reports_directory }}/{{ analysis_file }}` with the following structure:

   ### Required sections

   - **Title**: `# Comparative Analysis: {list of configuration names}`
   - **Intro paragraph**: Briefly describe the setup — how many configurations, what apps are in each, what configuration each directory used.
   - **Executive Summary**: A table with one row per dimension and one column per configuration, using ✅/❌/mixed indicators and short descriptions. This should let a reader quickly see the overall picture.
   - **Per-dimension sections** (numbered): One `## N. {Dimension Name}` section for each dimension. Each section must include:
     - What each configuration does (with **inline code examples** pulled from the actual source files)
     - A **Verdict** stating which approach is best and why (reference best practices, design guidelines, or performance implications)
   - **What All Versions Get Right**: A bulleted list of shared best practices across all configurations.
   - **Summary: Impact of Skills**: A final section ranking the most impactful differences and providing an overall assessment of each configuration.

   ### Style guidelines

   - Use fenced code blocks with appropriate syntax highlighting for all code examples
   - Keep code examples short (3–10 lines) and representative
   - Use tables for side-by-side comparisons where it aids readability
   - Include the configuration name in code block comments (e.g., `// {{ configurations[0].name }}`)
   - Be specific — cite actual file names and patterns, not generic advice
   - Include verdict/ranking judgments per dimension

6. **Output**: Write the report to `{{ reports_directory }}/{{ analysis_file }}`. If the file already exists, overwrite it.
```

### `templates/scenario.prompt.md.j2`

Starter template for users writing a new scenario prompt. Generated by `skill-eval init`.

```jinja2
# {{ scenario_name }}

> This is a starter template for a scenario prompt. Fill in each section to describe the app you want Copilot to generate. The more specific you are, the better the generated code will be — and the more meaningful the skill comparison.

## Overview

Describe the application in 2–3 sentences. What does it do? Who uses it?

<!-- Example: A REST API for managing a community library. Librarians use it to track books, loans, reservations, and overdue fines. -->

## Technology Stack

Specify the target technology. Be explicit about versions and frameworks.

<!-- Example: ASP.NET Core 10 Web API with Entity Framework Core and SQLite -->
<!-- Example: Next.js 15 with TypeScript, Prisma ORM, and PostgreSQL -->
<!-- Example: Go 1.23 with Chi router and GORM -->

## Entities

List the core domain entities with their fields and relationships.

<!-- Example:
- **Book**: Id, Title, Author, ISBN, Category, TotalCopies, AvailableCopies
- **Member**: Id, Name, Email, MembershipTier (Basic/Premium), JoinDate
- **Loan**: Id, BookId → Book, MemberId → Member, BorrowDate, DueDate, ReturnDate, Status
-->

## Business Rules

List the domain-specific rules and constraints that make this app non-trivial.

<!-- Example:
- Basic members can borrow up to 3 books; Premium members up to 10
- Overdue fine is $0.50 per day, capped at $25.00
- A book cannot be loaned if AvailableCopies is 0
- Reservations are served in FIFO order when a book becomes available
-->

## Endpoints

List the API endpoints the app should expose.

<!-- Example:
- GET /api/books — list all books (with pagination)
- GET /api/books/{id} — get a single book
- POST /api/books — add a new book
- POST /api/loans — borrow a book
- PUT /api/loans/{id}/return — return a book
- GET /api/members/{id}/loans — get a member's active loans
-->

## Seed Data

Describe what sample data should be included for development/testing.

<!-- Example: At least 10 books across 3 categories, 5 members (mix of Basic and Premium), and 3 active loans -->

## Additional Requirements

Any other requirements (authentication, logging, error handling patterns, etc.)

<!-- Example: Include an .http test file for all endpoints. Use SQLite for the database. -->
```

---

## Appendix C: Pydantic Configuration Models

Python data models for `src/skill_eval/config.py`:

```python
from __future__ import annotations
from pathlib import Path
from pydantic import BaseModel, field_validator


class Scenario(BaseModel):
    name: str
    prompt: str  # relative path to prompt file
    description: str = ""


class Configuration(BaseModel):
    name: str
    label: str = ""
    skills: list[str] = []    # relative paths to skill directories
    plugins: list[str] = []   # relative paths to plugin directories


class BuildVerification(BaseModel):
    command: str               # e.g., "dotnet build", "npm run build"
    working_directory: str = "."
    success_pattern: str | None = None  # optional regex


class HealthCheck(BaseModel):
    url: str
    expected_status: int = 200


class RunVerification(BaseModel):
    command: str               # e.g., "dotnet run", "npm start"
    timeout_seconds: int = 15
    health_check: HealthCheck | None = None


class Verification(BaseModel):
    build: BuildVerification
    run: RunVerification | None = None


class Dimension(BaseModel):
    name: str
    description: str
    what_to_look_for: str
    why_it_matters: str


class OutputSettings(BaseModel):
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
    def at_least_two_configurations(cls, v: list[Configuration]) -> list[Configuration]:
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
```

---

## Appendix D: Copilot CLI Invocation Details

The Python CLI must replicate what `generate-apps.ps1` does today. Here are the exact mechanics:

### Command format

```
copilot -p "<prompt text>" --yolo [--plugin-dir <path>]
```

- `-p` — The prompt to send to Copilot (always required)
- `--yolo` — Auto-confirm all actions (non-interactive mode)
- `--plugin-dir` — Optional, used for plugin-based configurations (e.g., dotnet-artisan)

### Skill registration (config.json manipulation)

Skills are registered by adding their **absolute paths** to the `skill_directories` array in `~/.copilot/config.json`:

```python
import json
from pathlib import Path

COPILOT_CONFIG = Path.home() / ".copilot" / "config.json"

def add_skill_directories(paths: list[Path]) -> list[Path]:
    """Register skill directories in Copilot config. Returns paths actually added."""
    config = json.loads(COPILOT_CONFIG.read_text()) if COPILOT_CONFIG.exists() else {}
    existing = config.get("skill_directories", [])
    added = []
    for p in paths:
        abs_path = str(p.resolve())
        if abs_path not in existing:
            existing.append(abs_path)
            added.append(p)
    config["skill_directories"] = existing
    COPILOT_CONFIG.parent.mkdir(parents=True, exist_ok=True)
    COPILOT_CONFIG.write_text(json.dumps(config, indent=2))
    return added

def remove_skill_directories(paths: list[Path]) -> None:
    """Unregister skill directories from Copilot config."""
    if not COPILOT_CONFIG.exists():
        return
    config = json.loads(COPILOT_CONFIG.read_text())
    existing = config.get("skill_directories", [])
    abs_paths = {str(p.resolve()) for p in paths}
    config["skill_directories"] = [d for d in existing if d not in abs_paths]
    COPILOT_CONFIG.write_text(json.dumps(config, indent=2))
```

### Generation loop (per configuration)

```python
import subprocess

def run_generation(config: EvalConfig, configuration: Configuration, 
                   rendered_prompt: str) -> None:
    """Run Copilot CLI for a single configuration."""
    added_skills: list[Path] = []
    try:
        # 1. Register skills
        if configuration.skills:
            skill_paths = [Path(s) for s in configuration.skills]
            added_skills = add_skill_directories(skill_paths)

        # 2. Build command
        cmd = ["copilot", "-p", rendered_prompt, "--yolo"]
        for plugin in configuration.plugins:
            cmd.extend(["--plugin-dir", plugin])

        # 3. Run Copilot
        result = subprocess.run(cmd, check=True)

    finally:
        # 4. Always clean up skills (even on failure)
        if added_skills:
            remove_skill_directories(added_skills)
```

### Key pattern: try/finally for skill cleanup

The current PowerShell script uses `try`/`finally` to ensure skill directories are **always** unregistered, even if Copilot fails. The Python implementation must preserve this pattern exactly — a dirty `config.json` would affect subsequent runs.

---

## Appendix E: ASP.NET Web API Example `eval.yaml`

Complete, working configuration for the current repo's content:

```yaml
name: "ASP.NET Core Web API Skill Evaluation"
description: >
  Evaluate how custom Copilot skills impact the quality of generated
  ASP.NET Core Web API code across three realistic application scenarios.

scenarios:
  - name: FitnessStudioApi
    prompt: prompts/scenarios/fitness-studio.prompt.md
    description: "Booking/membership system with class scheduling, waitlists, and instructor management"
  - name: LibraryApi
    prompt: prompts/scenarios/library.prompt.md
    description: "Book loans, reservations, overdue fines, and availability tracking"
  - name: VetClinicApi
    prompt: prompts/scenarios/vet-clinic.prompt.md
    description: "Pet healthcare with appointments, vaccinations, and medical records"

configurations:
  - name: no-skills
    label: "Baseline (no skills)"
    skills: []
    plugins: []
  - name: dotnet-webapi
    label: "dotnet-webapi skill"
    skills:
      - skills/dotnet-webapi
    plugins: []
  - name: dotnet-artisan
    label: "dotnet-artisan plugin chain"
    skills: []
    plugins:
      - plugins/dotnet-artisan
  - name: managedcode-dotnet-skills
    label: "Community managed-code skills"
    skills:
      - skills/managedcode-dotnet-skills
    plugins: []

verification:
  build:
    command: "dotnet build"
    success_pattern: "Build succeeded"
  run:
    command: "dotnet run"
    timeout_seconds: 15

dimensions:
  - name: "API Style"
    description: "Controllers (MVC pattern) vs Minimal APIs (lambda-based)"
    what_to_look_for: |
      Check Program.cs and endpoint files. Look for MapGet/MapPost (minimal)
      vs [ApiController] decorated classes (controllers).
    why_it_matters: "Minimal APIs have lower overhead, are the modern .NET default, and produce more concise code"

  - name: "Sealed Types"
    description: "Whether classes and records are declared sealed"
    what_to_look_for: |
      Check all class and record declarations for the 'sealed' modifier.
      Count sealed vs non-sealed across models, DTOs, services, and middleware.
    why_it_matters: "Sealed types enable JIT devirtualization optimizations and signal design intent"

  - name: "Primary Constructors"
    description: "C# 12 primary constructors vs traditional constructor injection"
    what_to_look_for: |
      Look for class/record declarations with parameter lists: `class MyService(IRepo repo)`
      vs traditional `private readonly IRepo _repo; public MyService(IRepo repo) { _repo = repo; }`
    why_it_matters: "Primary constructors reduce boilerplate and improve code clarity"

  - name: "DTO Design"
    description: "Records vs classes, immutability, naming conventions"
    what_to_look_for: |
      Check DTO types: are they records or classes? Sealed or not?
      Look at naming: *Request/*Response vs *Dto.
      Check immutability: positional records, init properties, or mutable setters.
    why_it_matters: "Immutable record DTOs are safer, more expressive, and produce cleaner API contracts"

  - name: "Service Abstraction"
    description: "Interface + implementation vs concrete-only registration"
    what_to_look_for: |
      Check DI registration: AddScoped<IService, Service>() vs AddScoped<Service>().
      Look for interface files and their organization.
    why_it_matters: "Interface-based registration enables testability and follows dependency inversion"

  - name: "CancellationToken Propagation"
    description: "Whether cancellation tokens are forwarded through all layers"
    what_to_look_for: |
      Check endpoint handlers for CancellationToken parameters.
      Trace the token through service methods to EF Core calls (ToListAsync, SaveChangesAsync).
      Look for breaks in the chain.
    why_it_matters: "Critical for production — prevents wasted server resources on cancelled requests"

  - name: "AsNoTracking Usage"
    description: "Whether read-only queries use AsNoTracking()"
    what_to_look_for: |
      Search for .AsNoTracking() in service/repository files.
      Check if read-only queries (GET endpoints) consistently use it.
    why_it_matters: "Major performance impact — tracking doubles memory usage and slows queries under load"

  - name: "Return Type Precision"
    description: "IReadOnlyList<T> vs List<T> vs IEnumerable<T>"
    what_to_look_for: |
      Check service method return types and collection properties on DTOs.
      Look for IReadOnlyList<T>, List<T>, IEnumerable<T>, or ICollection<T>.
    why_it_matters: "IReadOnlyList<T> prevents accidental mutation by consumers while maintaining indexing"

  - name: "Data Seeder Design"
    description: "Static methods vs injectable services vs HasData()"
    what_to_look_for: |
      Check how seed data is created: HasData() in OnModelCreating, static seeder methods
      called from Program.cs, or injectable seeder services.
    why_it_matters: "HasData() integrates with migrations; runtime seeders offer more flexibility"

  - name: "Middleware Style"
    description: "Convention-based RequestDelegate vs IMiddleware vs IExceptionHandler"
    what_to_look_for: |
      Check for global exception handling: IExceptionHandler (modern), convention-based middleware
      with RequestDelegate, or try/catch in individual endpoints.
    why_it_matters: "IExceptionHandler is composable, DI-aware, and the modern .NET recommended approach"

  - name: "Exception Handling Strategy"
    description: "Custom exceptions vs built-in types vs result patterns"
    what_to_look_for: |
      Look for custom exception classes (e.g., BusinessRuleException, NotFoundException).
      Check how HTTP status codes map to exception types.
      Look for result/option patterns as alternatives to exceptions.
    why_it_matters: "Well-designed exception hierarchies make error handling consistent and semantic"

  - name: "File Organization"
    description: "Per-entity file layout, middleware placement, interface location"
    what_to_look_for: |
      Check folder structure: per-entity folders vs per-concern folders.
      Look at where interfaces, DTOs, and middleware files are placed.
      Check for dedicated Endpoints/ or Controllers/ directories.
    why_it_matters: "Consistent file organization improves discoverability and maintainability"

  - name: "Pagination"
    description: "Pagination type design and defaults"
    what_to_look_for: |
      Check how pagination is implemented: sealed record vs mutable class.
      Look at computed properties (HasPrevious, HasNext) vs manual calculation.
      Check default page sizes and whether they're configurable.
    why_it_matters: "Immutable pagination types with computed properties are safer and more consistent"

  - name: "OpenAPI Metadata"
    description: "Richness of endpoint documentation"
    what_to_look_for: |
      Check for .WithName(), .WithSummary(), .WithDescription(), .Produces<T>(),
      .ProducesValidationProblem(), etc. on endpoint definitions.
      Check whether AddOpenApi()/MapOpenApi() or Swashbuckle is used.
    why_it_matters: "Rich OpenAPI metadata produces better API documentation and client generation"

  - name: "Collection Initialization"
    description: "Modern [] syntax vs new List<T>()"
    what_to_look_for: |
      Search for collection initialization patterns: modern `[]` (C# 12) vs
      `new List<T>()` or `new List<T> { ... }`.
    why_it_matters: "Modern collection expressions are more concise and signal C# version awareness"

output:
  directory: output
  reports_directory: reports
  analysis_file: analysis.md
  notes_file: build-notes.md
```

---

## Appendix F: Agent Body Content

Full markdown body for `.github/copilot/agents/skill-eval.md` (after the YAML frontmatter):

````markdown
# skill-eval

Copilot Skill Evaluation Agent. Guides skill authors through evaluating whether their
custom Copilot skills improve code generation quality.

## Preloaded Context

Always read these files at the start of a conversation to understand the evaluation state:

- `eval.yaml` — The evaluation configuration (scenarios, configurations, dimensions)
- `reports/analysis.md` — The latest analysis report (if it exists)
- `reports/build-notes.md` — The latest build/run status (if it exists)

## Workflow: Setup

**Trigger:** User wants to set up a new evaluation, or no `eval.yaml` exists yet.

1. Ask the user:
   - What tech stack are they evaluating? (e.g., React, Go, .NET, Python)
   - What skills do they want to compare? (their skill vs baseline, or multiple skills)
   - What 2–3 realistic app scenarios should we generate? (describe briefly)
   - How do they build and run projects in their stack? (e.g., `npm run build`, `dotnet build`)
2. Generate `eval.yaml` using their answers. Use the schema defined in the project.
3. For each scenario, create a starter prompt file in `prompts/scenarios/` using the template at `templates/scenario.prompt.md.j2` as a guide for structure.
4. Tell the user to:
   - Flesh out the scenario prompt files with domain details
   - Copy/symlink their skills into the `skills/` directory
   - Define analysis dimensions in eval.yaml (suggest 5–10 relevant to their stack)
5. If the user needs help defining dimensions, suggest common ones for their tech stack.

## Workflow: Scenario Authoring

**Trigger:** User asks for help writing or improving scenario prompts.

1. Read existing prompts in `prompts/scenarios/` for context.
2. Ask the user to describe the domain: what entities, what business rules, what endpoints.
3. Draft a scenario prompt following the template structure:
   - Overview, Technology Stack, Entities, Business Rules, Endpoints, Seed Data
4. Write the prompt file and ask for feedback.
5. Iterate until the user is satisfied.

## Workflow: Execution

**Trigger:** User says "run the evaluation", "generate", "evaluate", or similar.

1. Read `eval.yaml` and validate it has all required fields.
2. Check that all referenced prompt files and skill directories exist.
3. Confirm with the user what will happen:
   - "I'll generate {N} apps × {M} configurations = {N×M} total projects. This will take a while. Proceed?"
4. Run the pipeline using the CLI:
   ```bash
   skill-eval generate    # Generate code with each skill configuration
   skill-eval verify      # Build and run each generated project
   skill-eval analyze     # Produce the comparative analysis report
   ```
5. If any step fails, report the error clearly and ask if the user wants to:
   - Retry the failed step
   - Skip it and continue
   - Investigate the issue

## Workflow: Interpretation

**Trigger:** User asks about results, findings, or "what did the analysis find?"

1. Read `reports/analysis.md` and `reports/build-notes.md`.
2. Summarize the key findings:
   - Which configuration performed best overall?
   - Which dimensions showed the biggest differences?
   - What concrete improvements did the user's skill produce vs baseline?
3. For each weak dimension, explain:
   - What the skill-generated code is missing
   - What the expected best practice is
   - Which section of the skill definition would address this
4. Offer to help improve the skill (→ Iteration workflow).

## Workflow: Iteration

**Trigger:** User wants to improve their skill and re-evaluate.

1. Read the user's SKILL.md and the analysis findings.
2. Suggest specific additions/changes to the skill based on weak dimensions.
   - Be concrete: "Add a section about CancellationToken propagation with this example..."
   - Reference the analysis findings as evidence.
3. After the user edits their skill, offer to re-run:
   ```bash
   skill-eval generate --configurations <their-skill-name>
   skill-eval verify
   skill-eval analyze
   ```
4. Compare the new results with the previous analysis.
5. Repeat until the user is satisfied.

## Explicit Boundaries

- **Does NOT modify skill files without user confirmation** — suggests changes, user applies them
- **Does NOT run the full pipeline without confirmation** — always explains what it's about to do
- **Does NOT generate application code directly** — all code generation goes through Copilot CLI via the pipeline
- **Does NOT handle CI/CD setup, PyPI publishing, or operational concerns**
- **Does NOT handle non-evaluation tasks** — if the user asks about general coding, politely redirect

## Trigger Lexicon

This agent activates on: "evaluate my skill", "test my skill", "skill evaluation",
"compare skills", "how good is my skill", "benchmark skill", "skill quality",
"set up evaluation", "run evaluation", "what did the analysis find", "improve my skill",
"analysis results", "skill comparison", "evaluate copilot skills"
````
