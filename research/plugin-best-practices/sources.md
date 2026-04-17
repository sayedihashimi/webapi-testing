# Source Inventory

Sources consulted for the Copilot CLI & Claude Code Plugin Best Practices research.

**Research Date:** 2026-04-17

---

## Source List

### [1] Creating a plugin for GitHub Copilot CLI

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-creating |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Primary source for Copilot CLI plugin structure, creation workflow, and testing procedures. |
| **Limitations** | Does not cover advanced hook patterns or cross-platform considerations. |

---

### [2] Plugins — Claude Code Documentation

| Field | Value |
|-------|-------|
| **URL** | https://code.claude.com/docs/en/plugins |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official Anthropic documentation) |
| **Why It Matters** | Primary source for Claude Code plugin creation, including quickstart, component overview, and distribution. |
| **Limitations** | References separate plugins-reference page for detailed schemas. |

---

### [3] GitHub Copilot CLI plugin reference

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/reference/cli-plugin-reference |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Complete reference for plugin.json schema, marketplace.json schema, CLI commands, file locations, and loading order. |
| **Limitations** | Focused on Copilot CLI specifics; does not address Claude Code compatibility. |

---

### [4] Plugins reference — Claude Code Documentation

| Field | Value |
|-------|-------|
| **URL** | https://code.claude.com/docs/en/plugins-reference |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official Anthropic documentation) |
| **Why It Matters** | Complete technical specification for Claude Code plugins: manifest schema, component types (skills, agents, hooks, MCP, LSP, monitors), environment variables, CLI commands, and debugging tools. |
| **Limitations** | Very long document; some sections were truncated during retrieval. |

---

### [5] Claude Code Plugins Directory — README

| Field | Value |
|-------|-------|
| **URL** | https://github.com/anthropics/claude-code/blob/main/plugins/README.md |
| **Source Type** | Official repository |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — maintained by Anthropic) |
| **Why It Matters** | Lists 15 official Claude Code plugins with component breakdowns, demonstrating real-world patterns for commands, agents, skills, and hooks. |
| **Limitations** | Examples are Claude Code-specific; not all patterns transfer to Copilot CLI. |

---

### [6] awesome-copilot — Community Copilot Resources

| Field | Value |
|-------|-------|
| **URL** | https://github.com/github/awesome-copilot |
| **Source Type** | Official repository |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★☆ (High — maintained by GitHub, community-contributed content) |
| **Why It Matters** | Default Copilot CLI marketplace, demonstrating community plugin organization and contribution patterns. |
| **Limitations** | Community-contributed plugins may vary in quality. |

---

### [7] Plugin Marketplaces — Claude Code Documentation

| Field | Value |
|-------|-------|
| **URL** | https://code.claude.com/docs/en/plugin-marketplaces |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official Anthropic documentation) |
| **Why It Matters** | Complete guide for creating and distributing plugin marketplaces, including all source types (relative, GitHub, git URL, git-subdir, npm). |
| **Limitations** | Some content truncated during retrieval. |

---

### [8] code-review plugin.json (Anthropic)

| Field | Value |
|-------|-------|
| **URL** | https://github.com/anthropics/claude-code/blob/main/plugins/code-review/.claude-plugin/plugin.json |
| **Source Type** | Real-world plugin manifest |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official Anthropic plugin) |
| **Why It Matters** | Reference implementation of a well-structured plugin manifest with all recommended metadata fields. |
| **Limitations** | Single example; may not cover all manifest patterns. |

---

### [9] security-guidance plugin.json (Anthropic)

| Field | Value |
|-------|-------|
| **URL** | https://github.com/anthropics/claude-code/blob/main/plugins/security-guidance/.claude-plugin/plugin.json |
| **Source Type** | Real-world plugin manifest |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official Anthropic plugin) |
| **Why It Matters** | Example of a hook-focused plugin using PreToolUse event for security pattern detection. |
| **Limitations** | Hook-only plugin; does not demonstrate skills or agents. |

---

### [10] About plugins for GitHub Copilot CLI

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/concepts/agents/copilot-cli/about-cli-plugins |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Conceptual overview of what plugins are, what they contain, and why to use them. |
| **Limitations** | Overview only; no implementation details. |

---

### [11] Finding and installing plugins for GitHub Copilot CLI

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-finding-installing |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Covers plugin discovery, marketplace browsing, installation from all source types, and plugin management. |
| **Limitations** | Focused on user/consumer perspective, not authoring. |

---

### [12] Creating a plugin marketplace for GitHub Copilot CLI

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-marketplace |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-04-17 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Step-by-step guide for creating marketplace.json and distributing plugins via marketplace repositories. |
| **Limitations** | Simpler than Claude Code marketplace docs; fewer source type options. |
