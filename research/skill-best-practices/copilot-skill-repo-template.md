# Recommended Repository Structure for Copilot Skills

**Last Updated:** 2026-03-28

---

## Directory Tree

```
repo-root/
│
├── .github/
│   ├── copilot-instructions.md          # Repo-wide always-on rules
│   │
│   ├── instructions/                     # Path-specific instructions
│   │   ├── python.instructions.md        # applyTo: "**/*.py"
│   │   ├── typescript.instructions.md    # applyTo: "**/*.ts"
│   │   └── docs.instructions.md          # applyTo: "docs/**/*.md"
│   │
│   ├── prompts/                          # Reusable prompt templates
│   │   ├── create-skill.prompt.md        # Scaffold a new skill
│   │   ├── review-skill.prompt.md        # Review a skill against best practices
│   │   ├── generate-tests.prompt.md      # Generate tests for a file
│   │   └── review-code.prompt.md         # Perform a code review
│   │
│   ├── agents/                           # Custom agent definitions
│   │   ├── security-reviewer.md          # Security-focused review agent
│   │   └── documentation-writer.md       # Docs-focused agent
│   │
│   ├── skills/                           # Agent skills (the main attraction)
│   │   ├── skill-name-a/
│   │   │   ├── SKILL.md                  # Skill definition (lean)
│   │   │   ├── reference/                # Background docs, API specs
│   │   │   │   └── api-guide.md
│   │   │   ├── examples/                 # Golden input/output pairs
│   │   │   │   └── basic-usage.md
│   │   │   ├── templates/                # File templates to generate
│   │   │   │   └── component.template.ts
│   │   │   └── scripts/                  # Executable scripts
│   │   │       └── validate.sh
│   │   │
│   │   └── skill-name-b/
│   │       ├── SKILL.md
│   │       ├── reference/
│   │       └── examples/
│   │
│   └── hooks/                            # Workflow lifecycle hooks
│       └── pre-commit.json
│
├── research/                             # Best practices reference pack
│   ├── skills-best-practices-report.md   # Full research report
│   ├── skills-best-practices-summary.md  # Executive summary
│   ├── copilot-skill-checklist.md        # Review checklist
│   ├── copilot-skill-authoring-rules.md  # Compact rules for prompts
│   ├── copilot-skill-evaluation-rubric.md # Scoring rubric
│   ├── copilot-skill-repo-template.md    # This file
│   └── sources.md                        # Source inventory
│
└── README.md
```

---

## What Goes Where

### `.github/copilot-instructions.md`
- **Purpose:** Always-on repo-wide rules applied to every Copilot interaction
- **What belongs:** Coding standards, naming conventions, preferred libraries, project context
- **What does NOT belong:** Task-specific workflows, verbose documentation
- **Why it matters:** Ensures consistent baseline behavior across all interactions
- **Tip:** Keep under 500 words; every word costs context tokens on every interaction

### `.github/instructions/`
- **Purpose:** Scoped rules for specific file types or directories
- **What belongs:** Language-specific conventions, directory-specific patterns
- **What does NOT belong:** Universal rules (put those in `copilot-instructions.md`)
- **Why it matters:** Reduces context waste by applying rules only when editing matching files
- **Interaction:** Merged with repo-wide instructions; both apply when file matches

### `.github/prompts/`
- **Purpose:** Reusable task templates invoked manually via `/` commands
- **What belongs:** Repeatable one-off tasks: test generation, code review, scaffolding
- **What does NOT belong:** Always-on rules or multi-step workflows needing assets
- **Why it matters:** Standardizes common tasks across the team
- **Interaction:** Can reference other files (skills, research) for additional context

### `.github/agents/`
- **Purpose:** Specialized AI personas with defined roles and tool restrictions
- **What belongs:** Role definitions, behavioral instructions, tool allowlists
- **What does NOT belong:** Detailed workflow procedures (use skills for those)
- **Why it matters:** Enables purpose-built AI teammates (security reviewer, docs writer)
- **Interaction:** Agents can invoke skills; skills provide the workflow, agents provide the persona

### `.github/skills/<name>/`
- **Purpose:** Auto-activating, asset-rich workflow packages
- **What belongs:** Domain-specific procedures with supporting files
- **What does NOT belong:** Repo-wide policies, simple one-liner rules
- **Why it matters:** Most powerful customization surface for complex workflows

#### `SKILL.md`
- **Purpose:** Entry point and primary instruction set
- **Constraints:** YAML frontmatter (name, description, license) + Markdown body under 1500 words
- **Interaction:** Body loaded into context on activation (Tier 2)

#### `reference/`
- **Purpose:** Background documentation the agent may need
- **What belongs:** API docs, architectural guides, specification excerpts
- **What does NOT belong:** The skill's procedural instructions (those go in SKILL.md)
- **Why it matters:** Loaded on-demand (Tier 3), keeps SKILL.md lean

#### `examples/`
- **Purpose:** Golden input/output pairs demonstrating expected behavior
- **What belongs:** Complete, realistic scenarios with expected outputs
- **What does NOT belong:** Trivial "hello world" stubs
- **Why it matters:** Anchors agent behavior, serves as regression tests

#### `templates/`
- **Purpose:** File templates the skill generates or populates
- **What belongs:** Parameterized scaffolding files
- **What does NOT belong:** Complete, ready-to-use files (those are examples)
- **Why it matters:** Ensures consistent file structure in generated output

#### `scripts/`
- **Purpose:** Executable code referenced by skill instructions
- **What belongs:** Self-contained, documented scripts with exact run commands
- **What does NOT belong:** Scripts without documentation or with external dependencies not declared
- **Why it matters:** Enables deterministic, repeatable automation within skills

### `research/`
- **Purpose:** Best practices reference pack for skill authors
- **What belongs:** Research reports, authoring rules, checklists, rubrics, source inventories
- **What does NOT belong:** Skill definitions (those go in `.github/skills/`)
- **Why it matters:** Provides reusable guidance for Copilot when asked to create/review skills
- **Interaction:** Referenced by prompt files and skill reviews

---

## Optional vs. Recommended

| Component | Status | Notes |
|-----------|--------|-------|
| `copilot-instructions.md` | **Recommended** | Essential for any team using Copilot |
| `instructions/` | Optional | Useful for polyglot or monorepo projects |
| `prompts/` | **Recommended** | High value for team standardization |
| `agents/` | Optional | Useful for role-based workflows |
| `skills/` | **Recommended** | Core value for complex workflows |
| `skills/*/reference/` | **Recommended** | Keeps SKILL.md lean |
| `skills/*/examples/` | **Recommended** | Critical for quality and consistency |
| `skills/*/templates/` | Optional | Only when skill generates files |
| `skills/*/scripts/` | Optional | Only when skill runs automation |
| `hooks/` | Optional | For workflow lifecycle enforcement |
| `research/` | **Recommended** | Maintains institutional knowledge |
