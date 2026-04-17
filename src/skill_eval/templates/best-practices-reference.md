# Best Practices Reference: Skills & Plugins

This reference provides authoritative best practices for creating and improving
Copilot/Claude skills and plugins. Use it when evaluating and suggesting improvements.

---

## Part 1: Skill Best Practices

### Skill Structure

Every skill is a directory containing `SKILL.md` plus optional supporting files:

```
skill-name/
├── SKILL.md          # Required — frontmatter + instructions
├── reference/        # Verbose docs, API guides (loaded on demand)
├── examples/         # Golden examples showing complete scenarios
├── templates/        # File templates the skill uses
└── scripts/          # Executable scripts with documented dependencies
```

### SKILL.md Frontmatter

Required fields: `name` and `description`. Optional: `license`.

- `name`: lowercase, hyphens only, 1-64 chars. Must match folder name.
- `description`: 10-1024 chars. Behavior-driven with trigger keywords.
- No unsupported frontmatter fields — only `name`, `description`, `license`.

**Description quality is the single most impactful dimension.** It determines
when/if the skill activates.

❌ Bad: `"Helps with testing."`
✅ Good: `"Generates Playwright E2E tests for React components. Use when creating integration tests, browser tests, or E2E test suites."`

Pattern: `"[Action] [output] for [context]. Use when [trigger conditions]."`

### SKILL.md Body

- Numbered steps with clear success criteria per step
- Imperative voice: "Create", "Run", "Verify" — not "You might consider"
- Under 1500 words (~2000 tokens). Move verbose content to `reference/`
- At least one inline example (input → expected output)
- No hardcoded versions, URLs, or volatile facts in body
- No repo-wide policies (those go in `copilot-instructions.md`)
- Reference supporting files with relative paths

### Design Principles

- **One skill = one workflow.** Split broad skills into focused, composable units.
- **Prefer small, composable skills** over large monolithic ones.
- Every skill should have at least one golden example in `examples/`.

### Skill Evaluation Rubric (Score 1-3)

| Dimension | Excellent (3) | Poor (1) |
|-----------|--------------|----------|
| **Description Quality** | Behavior-driven, 2+ trigger keywords, states when to activate | Vague, no triggers, too short/long |
| **Instruction Clarity** | Numbered steps, success criteria, imperative, inline example | Narrative prose, ambiguous, no examples |
| **Context Efficiency** | Body <1000 words, verbose content in `reference/` | Body >1500 words, embedded docs |
| **Metadata Validity** | Name matches folder, proper format, clean YAML | Name mismatch, missing fields |
| **Supporting Files** | Has `reference/`, `examples/`, scripts documented | No supporting files |
| **Scope Focus** | Single workflow, no overlap with instructions | Multiple workflows, policy overlap |

---

## Part 2: Plugin Best Practices

### Plugin vs Standalone

Use **standalone** (`.claude/` or `.github/`) for project-specific, personal customizations.
Use **plugins** for reusable, shareable, versioned packages across projects and teams.

### Plugin Structure

```
my-plugin/
├── plugin.json                  # Or .claude-plugin/plugin.json
├── agents/                      # Custom agent profiles (.md files)
│   └── reviewer.agent.md
├── skills/                      # Skill directories with SKILL.md
│   ├── deploy/
│   │   └── SKILL.md
│   └── test-runner/
│       └── SKILL.md
├── hooks/hooks.json             # Event handler configuration
├── .mcp.json                    # MCP server configurations
├── .lsp.json                    # LSP server configs (Claude Code only)
├── monitors/monitors.json       # Background monitors (Claude Code only)
├── bin/                         # Executables added to PATH (Claude Code only)
├── settings.json                # Default settings (Claude Code only)
├── scripts/                     # Utility scripts for hooks/MCP
├── README.md                    # Documentation
├── CHANGELOG.md                 # Version history
└── LICENSE                      # License file
```

### Plugin Manifest (`plugin.json`)

Only `name` is required. Recommended fields:

```json
{
  "name": "my-plugin",
  "description": "What it does and when to use it",
  "version": "1.0.0",
  "author": { "name": "Author Name" },
  "license": "MIT",
  "keywords": ["domain", "technology"]
}
```

- `name`: kebab-case, letters/numbers/hyphens only, max 64 chars
- `description`: max 1024 chars, clearly state purpose and use cases
- `version`: semantic versioning (MAJOR.MINOR.PATCH)

### Component Best Practices

**Skills in plugins:**
- Same rules as standalone skills (see Part 1 above)
- Skills are namespaced: `/plugin-name:skill-name`
- Multiple skills should cover complementary aspects of the plugin's domain

**Agents:**
- Define clear boundaries for each agent's expertise
- Set `name`, `description`, `model`, `maxTurns` in frontmatter
- Use `tools` or `disallowedTools` to restrict scope
- Prefer multiple specialized agents over one monolithic agent
- Plugin agents cannot define hooks, MCP servers, or permission modes

**Hooks:**
- Choose appropriate events: `PreToolUse`, `PostToolUse`, `SessionStart`, `Stop`, etc.
- Use matchers to target specific tools (e.g., `"matcher": "Write|Edit"`)
- Use `${CLAUDE_PLUGIN_ROOT}` in command paths (plugins are cached on install)
- Common patterns: guardrails (PreToolUse for security), initialization (SessionStart), formatting (PostToolUse)

**MCP Servers:**
- Use `${CLAUDE_PLUGIN_ROOT}` for bundled scripts/binaries
- Use `${CLAUDE_PLUGIN_DATA}` for persistent state (survives updates)
- Use environment variables for secrets — never hardcode credentials
- Document required dependencies in README

### Key Patterns from Official Plugins

1. **Single-command entry point** — one primary slash command per plugin
2. **Agent teams** — multiple specialized agents for complex tasks (e.g., 5 parallel agents for code review)
3. **Hook guardrails** — PreToolUse hooks for security/quality checks
4. **SessionStart initialization** — inject context or install dependencies at session start
5. **Comprehensive README** — document commands, agents, and usage examples
6. **Self-documenting** — plugins that include guided creation workflows

### Common Plugin Gaps to Check

- Missing `description` or `version` in manifest
- No agents when the domain would benefit from specialized personas
- No hooks when quality guardrails would improve output
- Skills with vague descriptions that won't activate reliably
- No README or incomplete documentation
- No examples or reference material in skills
- Skills that are too broad (should be split into focused units)
- Missing `CHANGELOG.md` for version tracking
