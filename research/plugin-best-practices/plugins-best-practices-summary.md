# Executive Summary: Copilot CLI & Claude Code Plugin Best Practices

**Last Updated:** 2026-04-17

---

## What You Need to Know

Copilot CLI and Claude Code plugins are distributable packages that bundle agents, skills, hooks, MCP servers, and other components into installable units. They share a converging architecture — a `plugin.json` manifest plus standard component directories — making cross-platform plugins feasible.

## Key Takeaways

### 1. Understand When to Use Plugins vs Standalone

| Use This | When You Need |
|----------|--------------|
| Standalone (`.claude/` or `.github/`) | Project-specific, personal customizations |
| **Plugins** | **Reusable, shareable, versioned packages across projects and teams** |

### 2. The Manifest Is Simple — Only `name` Is Required

```json
{
  "name": "my-plugin",
  "description": "What it does and when to use it",
  "version": "1.0.0",
  "author": { "name": "Your Name" }
}
```

### 3. Standard Directory Structure

```
my-plugin/
├── plugin.json (or .claude-plugin/plugin.json)
├── agents/              # Custom agent profiles (.md files)
├── skills/              # Skill directories with SKILL.md
├── hooks/hooks.json     # Event handler configuration
├── .mcp.json            # MCP server configurations
└── README.md            # Documentation
```

### 4. Component Quick Reference

| Component | Purpose | Key File |
|-----------|---------|----------|
| Skills | Model-invoked instructions | `skills/<name>/SKILL.md` |
| Agents | Specialized AI personas | `agents/<name>.agent.md` |
| Hooks | Event-driven automation | `hooks/hooks.json` |
| MCP Servers | External tool integration | `.mcp.json` |
| LSP Servers* | Code intelligence | `.lsp.json` |
| Monitors* | Background watchers | `monitors/monitors.json` |

*Claude Code only

### 5. Test → Iterate → Publish

1. **Test locally:** `copilot plugin install ./my-plugin` or `claude --plugin-dir ./my-plugin`
2. **Iterate:** Re-install (Copilot) or `/reload-plugins` (Claude Code)
3. **Publish:** Add to a marketplace via `marketplace.json`

### 6. Cross-Platform Tips

- Place manifest where both platforms find it (root `plugin.json` or both `.github/plugin/` and `.claude-plugin/`)
- Use only shared component types for core functionality
- Claude Code-only features (monitors, LSP, bin/) are silently ignored by Copilot CLI

### 7. Top Patterns from Official Plugins

- **Single-command entry point** — one primary slash command per plugin
- **Agent teams** — multiple specialized agents for complex tasks
- **Hook guardrails** — `PreToolUse` hooks for security/quality checks
- **Comprehensive README** — document commands, agents, and usage examples
