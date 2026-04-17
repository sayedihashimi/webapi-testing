# Plugin Authoring Checklist

A step-by-step checklist for creating high-quality Copilot CLI / Claude Code plugins.

---

## Pre-Development

- [ ] **Define the plugin's scope** — one coherent domain per plugin
- [ ] **Choose component types** — skills, agents, hooks, MCP, or a combination
- [ ] **Decide target platforms** — Copilot CLI only, Claude Code only, or both
- [ ] **Review official examples** — study `anthropics/claude-code/plugins/` and `github/awesome-copilot`

## Plugin Structure

- [ ] **Create manifest file** — `plugin.json` with at minimum `name` (kebab-case)
- [ ] **Add metadata** — `description`, `version` (semver), `author`, `license`
- [ ] **Place manifest correctly:**
  - Copilot CLI: root `plugin.json`, `.plugin/`, `.github/plugin/`, or `.claude-plugin/`
  - Claude Code: `.claude-plugin/plugin.json`
  - Cross-platform: use both `.github/plugin/` and `.claude-plugin/`, or root `plugin.json`
- [ ] **Use standard directory layout:**
  ```
  my-plugin/
  ├── plugin.json (or .claude-plugin/plugin.json)
  ├── agents/
  ├── skills/
  ├── hooks/hooks.json
  ├── .mcp.json
  └── README.md
  ```

## Skills

- [ ] **Write behavior-driven descriptions** in SKILL.md frontmatter
- [ ] **Keep SKILL.md focused** — one skill = one workflow
- [ ] **Include examples** in the skill body
- [ ] **Use `$ARGUMENTS`** for dynamic user input
- [ ] **Add reference files** alongside SKILL.md for verbose content

## Agents

- [ ] **Define clear boundaries** for each agent's expertise
- [ ] **Set appropriate frontmatter** — `name`, `description`, `model`, `maxTurns`
- [ ] **Restrict tools** — use `tools` or `disallowedTools` to limit scope
- [ ] **Use focused agents** — prefer multiple specialized agents over one monolithic agent

## Hooks

- [ ] **Choose appropriate events** — `PreToolUse`, `PostToolUse`, `SessionStart`, `Stop`, etc.
- [ ] **Use matchers** to target specific tools (e.g., `"matcher": "Write|Edit"`)
- [ ] **Use `${CLAUDE_PLUGIN_ROOT}`** in command paths (plugins are cached)
- [ ] **Test hook scripts independently** before integrating

## MCP Servers

- [ ] **Use `${CLAUDE_PLUGIN_ROOT}`** for bundled scripts/binaries
- [ ] **Use `${CLAUDE_PLUGIN_DATA}`** for persistent state (Claude Code only)
- [ ] **Use environment variables** for secrets — never hardcode credentials
- [ ] **Document required dependencies** in README

## Testing

- [ ] **Test locally first:**
  - Copilot CLI: `copilot plugin install ./my-plugin`
  - Claude Code: `claude --plugin-dir ./my-plugin`
- [ ] **Verify all components loaded:** check agents, skills, hooks
- [ ] **Test each component individually** before testing interactions
- [ ] **Use debug mode** — `claude --debug` for Claude Code
- [ ] **Re-test after changes** — re-install (Copilot) or `/reload-plugins` (Claude Code)

## Documentation

- [ ] **Include comprehensive README.md** — installation, usage, commands, examples
- [ ] **Document prerequisites** — required tools, language servers, etc.
- [ ] **Add CHANGELOG.md** for version tracking
- [ ] **Include LICENSE** file

## Distribution

- [ ] **Choose distribution method:**
  - Marketplace (recommended for wide distribution)
  - Direct install from GitHub repo
  - Local path (for team/org internal use)
- [ ] **Create marketplace.json** if using marketplace distribution
- [ ] **Use semantic versioning** — `MAJOR.MINOR.PATCH`
- [ ] **Pin versions** in marketplace entries for stability

## Cross-Platform (if targeting both Copilot CLI and Claude Code)

- [ ] **Use shared component types only** for core functionality
- [ ] **Avoid platform-specific features** in critical paths (monitors, LSP, bin/)
- [ ] **Test on both platforms** before publishing
- [ ] **Document platform differences** in README
