# Best Practices for Creating Copilot CLI and Claude Code Plugins

**Last Updated:** 2026-04-17  
**Research Mode:** Standard  
**Total Sources:** 12

---

## Executive Summary

GitHub Copilot CLI and Claude Code now share a converging plugin architecture that bundles agents, skills, hooks, MCP servers, LSP servers, and monitors into distributable packages. This report examines best practices for authoring, testing, and distributing plugins across both platforms based on official documentation, reference implementations, and community marketplace analysis.

- **Key Finding 1:** Copilot CLI and Claude Code plugins share near-identical structural conventions — a `plugin.json` manifest, standard component directories (`agents/`, `skills/`, `hooks/`), and marketplace-based distribution — making cross-platform plugins feasible with minor path adjustments [1][2].
- **Key Finding 2:** The manifest file (`plugin.json`) is the only required file. Its `name` field is the sole mandatory field, but including `description`, `version`, `author`, and semantic versioning significantly improves discoverability and marketplace compatibility [3][4].
- **Key Finding 3:** Plugins should follow the single-responsibility principle — one coherent domain per plugin — while leveraging the full component spectrum (skills for instructions, agents for specialized personas, hooks for automation, MCP for external tools) [5][6].
- **Key Finding 4:** Both platforms provide local testing workflows (`copilot plugin install ./path` and `claude --plugin-dir ./path`) that should be used iteratively before marketplace publication [1][2].
- **Key Finding 5:** Marketplace distribution is the primary sharing mechanism, with support for GitHub repos, git URLs, local paths, and (Claude Code only) npm packages as plugin sources [3][7].

**Primary Recommendation:** Start with a single well-scoped skill or hook, test locally, then expand to a full plugin with marketplace distribution once the core value is validated.

**Confidence Level:** High — based on authoritative official documentation from both GitHub and Anthropic, plus 15+ real-world plugin examples.

---

## Introduction

### Research Question

What are the best practices for creating plugins for GitHub Copilot CLI and Claude Code, covering architecture, component authoring, testing, and distribution?

### Scope & Methodology

This research investigates the plugin systems of two leading AI-assisted coding tools: GitHub Copilot CLI and Anthropic's Claude Code. Both tools introduced plugin architectures in 2025-2026 that allow developers to package and distribute custom functionality as installable units.

The research covers: plugin structure and manifest conventions, component authoring (skills, agents, hooks, MCP/LSP servers, monitors), local development and testing workflows, marketplace creation and distribution, cross-platform compatibility considerations, and real-world patterns from official plugin examples.

Sources consulted include official GitHub documentation (5 pages), official Anthropic/Claude Code documentation (3 pages), official plugin repositories (github/copilot-plugins, github/awesome-copilot, anthropics/claude-code), and real-world plugin manifest files. A total of 12 primary sources were consulted across official documentation, repository READMEs, and marketplace catalogs.

### Key Assumptions

- **Plugin systems are stable:** Both Copilot CLI and Claude Code plugin APIs are documented and in use, though they may continue evolving.
- **Cross-platform interest:** Readers want plugins that work across both platforms where possible.
- **Technical audience:** Readers are developers familiar with CLI tools, markdown, and JSON configuration.

---

## Main Analysis

### Finding 1: A Converging Plugin Architecture Across Copilot CLI and Claude Code

GitHub Copilot CLI and Claude Code have independently developed plugin systems that arrived at remarkably similar architectures. Both define a plugin as a directory containing a JSON manifest and optional component subdirectories for agents, skills, hooks, and MCP server configurations [1][2].

The Copilot CLI plugin system uses a `plugin.json` file at the plugin root (or in `.plugin/`, `.github/plugin/`, or `.claude-plugin/` directories) [3]. Claude Code uses `.claude-plugin/plugin.json` as the canonical manifest location [4]. Both manifests share the same core fields: `name` (required, kebab-case), `description`, `version`, `author`, `license`, and `keywords`.

The component layout is also nearly identical. Both platforms expect agents in an `agents/` directory as markdown files (`.agent.md` or plain `.md` with frontmatter), skills in a `skills/` directory as subdirectories containing `SKILL.md` files, hooks as JSON configuration, and MCP servers in `.mcp.json` [1][2].

This convergence is not accidental. Copilot CLI documentation explicitly notes that it looks for manifests in `.claude-plugin/` directories, and Claude Code's plugin format supports installation via Copilot CLI commands [3][7]. The practical implication is that a well-structured plugin can work on both platforms with minimal or no modifications.

**Key differences** remain, however. Claude Code supports additional component types not present in Copilot CLI: background monitors (`monitors/monitors.json`), LSP servers (`.lsp.json`), output styles (`output-styles/`), executables (`bin/`), and default settings (`settings.json`) [4]. Claude Code also supports `userConfig` for prompted configuration values and a persistent data directory (`${CLAUDE_PLUGIN_DATA}`) that survives plugin updates [4]. Copilot CLI does not currently document equivalents for these features.

**Sources:** [1], [2], [3], [4]

---

### Finding 2: Plugin Manifest Best Practices

The plugin manifest (`plugin.json`) is the only required file in a plugin. Getting it right is critical for discoverability, installation, and cross-platform compatibility.

**The `name` field is the sole required field** in both Copilot CLI and Claude Code manifests [3][4]. It must be kebab-case (letters, numbers, hyphens only), max 64 characters for Copilot CLI. This name serves as the plugin's unique identifier, the namespace prefix for skills (e.g., `/my-plugin:deploy`), and the installation reference (e.g., `copilot plugin install my-plugin@marketplace`).

**Semantic versioning is strongly recommended.** Both platforms support a `version` field using semver format (e.g., `"1.2.0"`) [3][4]. Versioning enables meaningful updates, allows marketplace pinning, and communicates breaking changes to users. Claude Code's marketplace additionally supports `ref` and `sha` pinning for precise version control [7].

**Write descriptive, actionable descriptions.** The `description` field (max 1024 characters on Copilot CLI) is displayed in plugin managers and marketplace browsers [3]. According to official guidance, descriptions should clearly state what the plugin does and when to use it — mirroring the best practice for skill descriptions [1][2].

A well-structured manifest from the official Claude Code `code-review` plugin demonstrates these principles:

```json
{
  "name": "code-review",
  "description": "Automated code review for pull requests using multiple specialized agents with confidence-based scoring",
  "version": "1.0.0",
  "author": {
    "name": "Boris Cherny",
    "email": "boris@anthropic.com"
  }
}
```
[8]

**Component path fields are optional with sensible defaults.** Both platforms default to standard directories (`agents/`, `skills/`, `hooks/hooks.json`, `.mcp.json`) if manifest path fields are omitted [3][4]. Override these defaults only when you have a specific organizational need. When you do override, note that custom paths replace (not supplement) the defaults — to keep the default and add more, include both in an array: `"skills": ["./skills/", "./extras/"]` [4].

**Sources:** [3], [4], [8]

---

### Finding 3: Component Authoring — Skills, Agents, Hooks, and MCP

A plugin's value comes from its components. Each component type serves a distinct purpose and follows specific authoring patterns.

#### Skills

Skills are the most common plugin component. They provide model-invoked instructions that Claude/Copilot activates automatically based on task context. Each skill lives in a `skills/<name>/SKILL.md` directory with YAML frontmatter [1][2].

The `description` field in the frontmatter is critical — it determines when the AI activates the skill. Write it as a behavior-driven trigger list:

```yaml
---
description: Reviews code for best practices and potential issues. Use when reviewing code, checking PRs, or analyzing code quality.
---
```
[2]

Skills can include supporting files alongside `SKILL.md`: reference documents, scripts, and templates. Use `$ARGUMENTS` to capture user input for dynamic behavior [2].

#### Agents

Plugin agents are specialized subagents defined as markdown files in `agents/`. They support frontmatter fields for `name`, `description`, `model`, `effort`, `maxTurns`, `tools`, `disallowedTools`, and more [4].

From the official `code-review` plugin, a pattern emerges of using multiple focused agents that run in parallel — five Sonnet agents for different review aspects (CLAUDE.md compliance, bug detection, historical context, PR history, and code comments) [5]. This demonstrates the best practice of task decomposition: instead of one monolithic agent, use several specialized agents that each excel at one aspect.

For security reasons, plugin agents cannot define their own hooks, MCP servers, or permission modes [4]. This is an important constraint to be aware of when designing plugin architecture.

#### Hooks

Hooks are event handlers that respond to lifecycle events. They are defined in `hooks/hooks.json` (or inline in `plugin.json`) and can execute shell commands, HTTP requests, LLM prompts, or agentic verifiers [4].

The `security-guidance` plugin from Anthropic demonstrates an effective hook pattern — a `PreToolUse` hook that monitors file edits for 9 security patterns including command injection, XSS, eval usage, and dangerous HTML [5][9]. This is a "guardrail" pattern: using hooks to enforce policies without requiring user action.

Another powerful pattern is the `SessionStart` hook used for initialization. The `explanatory-output-style` plugin injects educational context at the start of each session [5]. The plugin reference documentation shows a `SessionStart` hook that installs npm dependencies on first run and re-installs when `package.json` changes — a robust pattern for managing plugin runtime dependencies [4].

Both platforms support a rich set of hook events. Claude Code supports 25+ events including `PreToolUse`, `PostToolUse`, `Stop`, `SessionStart`, `FileChanged`, `CwdChanged`, and `ConfigChange` [4]. Copilot CLI hooks use a similar model but with fewer documented events.

#### MCP Servers

MCP (Model Context Protocol) servers connect the AI to external tools and services. Plugin MCP configurations live in `.mcp.json` at the plugin root [1][2].

Use `${CLAUDE_PLUGIN_ROOT}` to reference scripts and binaries bundled with the plugin, and `${CLAUDE_PLUGIN_DATA}` for persistent state that survives updates [4]. This variable substitution is essential because plugins are cached (copied) to a local directory on installation — hardcoded paths will break.

```json
{
  "mcpServers": {
    "plugin-database": {
      "command": "${CLAUDE_PLUGIN_ROOT}/servers/db-server",
      "args": ["--config", "${CLAUDE_PLUGIN_ROOT}/config.json"]
    }
  }
}
```
[4]

#### LSP Servers (Claude Code only)

LSP servers give Claude real-time code intelligence — diagnostics, go-to-definition, and hover information. Claude Code ships with official LSP plugins for Pyright, TypeScript, and rust-analyzer [4]. If you need to support an additional language, create a custom LSP plugin with an `.lsp.json` file specifying the language server binary and file extension mappings.

**Sources:** [1], [2], [4], [5], [8], [9]

---

### Finding 4: Testing and Development Workflow

Both platforms provide local development workflows that should be used iteratively before publishing.

**Copilot CLI** supports local installation with `copilot plugin install ./my-plugin` [1]. This copies the plugin to the local cache. A critical caveat: when you install a plugin, its components are cached, so subsequent sessions read from the cache. To pick up changes, you must re-run `copilot plugin install ./my-plugin` [1].

**Claude Code** offers a more developer-friendly `--plugin-dir ./my-plugin` flag that loads the plugin directly without installation [2]. Additionally, the `/reload-plugins` command reloads all plugins, skills, agents, hooks, and MCP servers without restarting the session [2]. This makes the development loop significantly faster:

1. Make changes to plugin files
2. Run `/reload-plugins` in the active session
3. Test the changes immediately

**Verification steps** for both platforms:
- Check plugin loaded: `copilot plugin list` or `/plugin list`
- Check agents: `/agent` or `/agents`
- Check skills: `/skills list` or test with `/plugin-name:skill-name`
- Check hooks: verify hooks fire on expected events
- Use `claude --debug` (Claude Code) to see plugin loading details and error messages [4]

**Test components individually.** If a plugin isn't working, debug each skill, agent, and hook separately before investigating interactions [2].

**Sources:** [1], [2], [4]

---

### Finding 5: Marketplace Distribution

Marketplaces are the primary mechanism for sharing plugins. Both platforms use a `marketplace.json` file to define a registry of plugins.

**Marketplace structure** is nearly identical between platforms. A `marketplace.json` file placed in `.github/plugin/` (Copilot CLI) or `.claude-plugin/` (Claude Code) defines the marketplace name, owner, and a list of plugin entries with their sources [3][7].

```json
{
  "name": "my-marketplace",
  "owner": {
    "name": "Your Organization",
    "email": "plugins@example.com"
  },
  "plugins": [
    {
      "name": "frontend-design",
      "description": "Create professional frontend interfaces",
      "version": "2.1.0",
      "source": "./plugins/frontend-design"
    }
  ]
}
```
[3][7]

**Default marketplaces** are pre-registered. Copilot CLI ships with `copilot-plugins` and `awesome-copilot` by default [6]. Claude Code has its own official marketplace accessible via `claude.ai/settings/plugins` [2].

**Plugin sources** in marketplace entries support multiple formats [3][7]:

| Source Type | Copilot CLI | Claude Code |
|------------|-------------|-------------|
| Relative path | `"./plugins/name"` | `"./plugins/name"` |
| GitHub repo | `OWNER/REPO` | `{"source": "github", "repo": "owner/repo"}` |
| Git URL | `https://...` | `{"source": "url", "url": "https://..."}` |
| Git subdirectory | `OWNER/REPO:PATH` | `{"source": "git-subdir", ...}` |
| npm package | Not supported | `{"source": "npm", "package": "@org/plugin"}` |

Claude Code additionally supports version pinning via `ref` (branch/tag) and `sha` (commit hash) fields for precise version control [7].

**The `strict` mode** (Claude Code only) controls whether `plugin.json` or the marketplace entry is authoritative for component definitions. Default `true` means `plugin.json` wins; `false` means the marketplace entry defines everything [7]. This is useful when a marketplace operator wants to curate which components from a plugin are exposed.

**Sources:** [3], [6], [7]

---

### Finding 6: Cross-Platform Compatibility Strategies

Given the architectural convergence between Copilot CLI and Claude Code, creating cross-platform plugins is practical but requires awareness of platform differences.

**Use the lowest common denominator structure.** Both platforms support: `plugin.json` at the root or in `.github/plugin/` / `.claude-plugin/`, agents in `agents/`, skills in `skills/`, hooks in `hooks.json` or `hooks/hooks.json`, and MCP configs in `.mcp.json` [1][2][3][4].

**Include both manifest locations.** Copilot CLI checks `.plugin/plugin.json`, `plugin.json`, `.github/plugin/plugin.json`, and `.claude-plugin/plugin.json` (in that order) [3]. Claude Code checks `.claude-plugin/plugin.json` [4]. For maximum compatibility, place your manifest in both `.github/plugin/plugin.json` and `.claude-plugin/plugin.json`, or simply at the root as `plugin.json`.

**Be cautious with Claude Code-only features.** Monitors, LSP servers, output styles, `bin/` executables, `settings.json`, `userConfig`, channels, and `${CLAUDE_PLUGIN_DATA}` are Claude Code-specific [4]. These features will be silently ignored by Copilot CLI but won't cause errors. Design your plugin so core functionality doesn't depend on these features unless you're targeting Claude Code exclusively.

**Loading order and precedence differ.** Copilot CLI uses first-found-wins for agents and skills (project-level overrides plugins), and last-wins for MCP servers (plugins override user configs) [3]. Claude Code uses similar precedence but with more granular scope levels (user, project, local, managed) [4]. Document any precedence assumptions in your plugin README.

**Sources:** [1], [2], [3], [4]

---

### Finding 7: Patterns from Official Plugin Examples

Analysis of 15 official plugins from the `anthropics/claude-code` repository reveals recurring best practices [5].

**Pattern 1: Single-command entry points.** Most plugins define a primary slash command as the main entry point. The `code-review` plugin has `/code-review`, `commit-commands` has `/commit` and `/commit-push-pr`, and `feature-dev` has `/feature-dev` [5]. This gives users a clear, discoverable starting point.

**Pattern 2: Specialized agent teams.** Complex plugins decompose work into multiple focused agents. The `code-review` plugin uses 5 parallel Sonnet agents, each reviewing a different aspect. The `feature-dev` plugin uses `code-explorer`, `code-architect`, and `code-reviewer` agents for different development phases [5]. This pattern improves quality by ensuring each agent has clear boundaries and expertise.

**Pattern 3: Hook-based guardrails.** The `security-guidance` plugin monitors all file edits for security patterns via a `PreToolUse` hook [9]. The `explanatory-output-style` plugin injects session context via a `SessionStart` hook [5]. These demonstrate hooks as passive quality gates rather than user-triggered actions.

**Pattern 4: Self-documenting plugins.** Every official plugin includes a comprehensive `README.md` documenting commands, agents, usage examples, and architecture [5]. The `plugin-dev` plugin even includes a `/plugin-dev:create-plugin` command — an 8-phase guided workflow for building new plugins [5].

**Pattern 5: Comprehensive manifests.** Real-world `plugin.json` files are minimal but complete — `name`, `description`, `version`, and `author` are consistently included [8][9]. This aligns with the principle of providing enough metadata for marketplace discovery while keeping the manifest lean.

**Sources:** [5], [8], [9]

---

## Synthesis & Insights

### Patterns Identified

**Pattern 1: The Plugin as Packaging Layer**

Plugins are fundamentally a packaging and distribution layer, not a distinct programming model. Every component in a plugin (skills, agents, hooks, MCP servers) works identically to its standalone counterpart — the plugin just bundles them for portability [1][2]. This means existing standalone configurations can be converted to plugins with minimal effort, and plugin authors can prototype components standalone before packaging them.

**Pattern 2: Description-Driven Discovery**

Both platforms rely heavily on text descriptions for component activation and marketplace discovery. The `description` field in both `plugin.json` and individual `SKILL.md` frontmatter serves as the primary mechanism by which AI agents decide when to use a component [1][2]. This makes description authoring one of the highest-leverage activities for plugin quality.

### Novel Insights

**Insight 1: The "Plugin Layer Cake"**

A well-designed plugin follows a layered pattern: manifest (identity) → skills (instructions) → agents (personas) → hooks (automation) → MCP/LSP (integration). Each layer adds capability without requiring the layers above it. A plugin can be useful with just a manifest and a single skill. This suggests a progressive enhancement strategy for plugin development: start minimal, add layers as needs emerge.

**Insight 2: Cross-Platform as Default Posture**

Given that Copilot CLI explicitly looks for `.claude-plugin/` directories and both platforms converge on nearly identical formats, the cost of cross-platform compatibility is near zero for simple plugins. Plugin authors should treat cross-platform as the default posture and only go platform-specific when using features unique to one platform (monitors, LSP, output styles).

### Implications

**For Plugin Authors:** Focus 80% of effort on skill descriptions and hook logic — these are the highest-impact components. Use the official plugin examples from `anthropics/claude-code` as reference implementations. Test locally before publishing to any marketplace.

**For Teams:** Use project-scoped plugin installation to share team-standard plugins via version control. This ensures all team members get the same tools without manual marketplace setup.

**For the Ecosystem:** The convergence between Copilot CLI and Claude Code plugin formats suggests a potential future standard for AI agent extensibility, similar to how VS Code's extension API became a de facto standard for editor extensions.

---

## Limitations & Caveats

### Known Gaps

**Gap 1: Copilot CLI hook documentation is sparse.** While Claude Code documents 25+ hook events with detailed schemas, Copilot CLI's hook documentation is less comprehensive. The actual hook capabilities may be more extensive than documented.

**Gap 2: Cross-platform testing is untested at scale.** While the architectures converge, no large-scale empirical data exists on plugins running across both platforms without modification. Edge cases around path resolution, variable substitution, and hook event naming may exist.

**Gap 3: Plugin security model.** Neither platform documents a formal security review process for marketplace plugins beyond basic structural validation. The `awesome-copilot` repository notes: "The customizations here are sourced from third-party developers. Please inspect any agent and its documentation before installing" [6].

### Areas of Uncertainty

**Uncertainty 1: Evolution pace.** Both plugin systems are new (2025-2026) and evolving rapidly. Best practices documented today may be superseded by new features or architectural changes within months.

**Uncertainty 2: Claude Code-specific features adoption.** It is unclear whether Copilot CLI will adopt Claude Code-specific features like monitors, LSP servers, persistent data directories, and user configuration prompts, or whether these will remain platform differentiators.

---

## Recommendations

### Immediate Actions

1. **Start with a single skill plugin.** Create a plugin with one well-scoped `SKILL.md` file, test it locally, and validate it works before adding complexity. Use the quickstart guides from official documentation [1][2].

2. **Use the official examples as templates.** Clone `anthropics/claude-code` and study the `plugins/` directory for real-world patterns. The `plugin-dev` plugin even includes a guided workflow for creating new plugins [5].

3. **Write behavior-driven descriptions.** Invest time in the `description` fields of both `plugin.json` and `SKILL.md` frontmatter. These are the primary discovery and activation mechanisms.

4. **Test locally before publishing.** Use `copilot plugin install ./path` or `claude --plugin-dir ./path` to validate all components work before marketplace distribution.

5. **Use semantic versioning from day one.** Even for initial releases, use `"version": "1.0.0"` format to enable proper update tracking.

### Next Steps

1. **Create a team marketplace** for internal plugin distribution. A single GitHub repository with `marketplace.json` is sufficient to start.

2. **Convert existing standalone configurations** (`.claude/` or `.github/` directory customizations) into plugins for reuse across projects.

3. **Design for cross-platform compatibility** by using only shared component types and placing manifests where both platforms find them.

### Further Research Needs

1. **Plugin performance benchmarks.** How do large plugins with many components affect session startup time and memory usage?

2. **Security best practices for MCP-heavy plugins.** Plugins that bundle MCP servers execute arbitrary code — what guardrails should authors implement?

3. **Plugin composition patterns.** How should plugins declare dependencies on other plugins? Claude Code supports a `dependencies` field but its practical use is undocumented.

---

## Bibliography

[1] GitHub (2026). "Creating a plugin for GitHub Copilot CLI". GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-creating (Retrieved: 2026-04-17)

[2] Anthropic (2026). "Plugins — Claude Code Documentation". code.claude.com. https://code.claude.com/docs/en/plugins (Retrieved: 2026-04-17)

[3] GitHub (2026). "GitHub Copilot CLI plugin reference". GitHub Docs. https://docs.github.com/en/copilot/reference/cli-plugin-reference (Retrieved: 2026-04-17)

[4] Anthropic (2026). "Plugins reference — Claude Code Documentation". code.claude.com. https://code.claude.com/docs/en/plugins-reference (Retrieved: 2026-04-17)

[5] Anthropic (2026). "Claude Code Plugins Directory — README". GitHub. https://github.com/anthropics/claude-code/blob/main/plugins/README.md (Retrieved: 2026-04-17)

[6] GitHub (2026). "awesome-copilot — Community Copilot Resources". GitHub. https://github.com/github/awesome-copilot (Retrieved: 2026-04-17)

[7] Anthropic (2026). "Plugin Marketplaces — Claude Code Documentation". code.claude.com. https://code.claude.com/docs/en/plugin-marketplaces (Retrieved: 2026-04-17)

[8] Boris Cherny / Anthropic (2026). "code-review plugin.json". GitHub. https://github.com/anthropics/claude-code/blob/main/plugins/code-review/.claude-plugin/plugin.json (Retrieved: 2026-04-17)

[9] David Dworken / Anthropic (2026). "security-guidance plugin.json". GitHub. https://github.com/anthropics/claude-code/blob/main/plugins/security-guidance/.claude-plugin/plugin.json (Retrieved: 2026-04-17)

[10] GitHub (2026). "About plugins for GitHub Copilot CLI". GitHub Docs. https://docs.github.com/en/copilot/concepts/agents/copilot-cli/about-cli-plugins (Retrieved: 2026-04-17)

[11] GitHub (2026). "Finding and installing plugins for GitHub Copilot CLI". GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-finding-installing (Retrieved: 2026-04-17)

[12] GitHub (2026). "Creating a plugin marketplace for GitHub Copilot CLI". GitHub Docs. https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-marketplace (Retrieved: 2026-04-17)

---

## Appendix: Methodology

### Research Process

**Phase 1 (SCOPE):** Defined research boundaries around Copilot CLI and Claude Code plugin authoring. Excluded VS Code extensions, GitHub Apps-based Copilot Extensions, and general MCP server development.

**Phase 2 (PLAN):** Identified primary sources (official documentation for both platforms), secondary sources (marketplace repos, real plugin examples), and key angles (structure, components, testing, distribution, cross-platform).

**Phase 3 (RETRIEVE):** Collected information from 12 primary sources across GitHub Docs, Claude Code docs, and official plugin repositories. Fetched full plugin manifests from real-world examples.

**Phase 4 (TRIANGULATE):** Cross-referenced architectural claims across Copilot CLI and Claude Code documentation. Validated manifest schemas against real plugin examples.

**Phase 5 (SYNTHESIZE):** Identified convergence patterns between platforms, extracted component-level best practices, and synthesized cross-platform compatibility strategies.

**Phase 8 (PACKAGE):** Generated this report with progressive section assembly.

### Sources Consulted

**Total Sources:** 12

**Source Types:**
- Official documentation (GitHub Docs): 5
- Official documentation (Anthropic/Claude): 3
- Repository READMEs / examples: 4

**Temporal Coverage:** 2025-2026 (both plugin systems are new)

### Claims-Evidence Table

| Claim ID | Major Claim | Evidence Type | Supporting Sources | Confidence |
|----------|-------------|---------------|-------------------|------------|
| C1 | Copilot CLI and Claude Code share near-identical plugin architecture | Direct documentation | [1], [2], [3], [4] | High |
| C2 | `name` is the only required manifest field | Direct documentation | [3], [4] | High |
| C3 | Marketplace distribution uses `marketplace.json` | Direct documentation | [3], [7], [12] | High |
| C4 | Official plugins demonstrate specialized agent teams | Repository analysis | [5], [8] | High |
| C5 | Cross-platform plugins are feasible with minor adjustments | Documentation synthesis | [1], [2], [3], [4] | Medium |
| C6 | Hooks support 25+ event types (Claude Code) | Direct documentation | [4] | High |
| C7 | Plugin components are cached after installation | Direct documentation | [1], [4] | High |

**Research Mode:** Standard  
**Generated:** 2026-04-17
