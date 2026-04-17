# Plugin Repository Template

A recommended file/folder structure for creating a Copilot CLI / Claude Code plugin repository.

---

## Standard Plugin Layout

```
my-plugin/
│
├── .claude-plugin/                 # Claude Code manifest directory
│   └── plugin.json                 # Plugin manifest (works for both platforms)
│
├── .github/
│   └── plugin/
│       └── plugin.json             # Copilot CLI manifest (symlink or copy)
│
├── agents/                         # Custom agent definitions
│   ├── reviewer.agent.md           # Specialized code reviewer
│   └── helper.agent.md             # General helper agent
│
├── skills/                         # Auto-activated skills
│   ├── deploy/
│   │   ├── SKILL.md                # Deployment skill instructions
│   │   ├── reference.md            # Additional reference material
│   │   └── scripts/                # Supporting scripts
│   │       └── deploy.sh
│   └── test-runner/
│       └── SKILL.md                # Test execution skill
│
├── commands/                       # Flat slash commands (legacy, prefer skills/)
│   └── status.md                   # /my-plugin:status command
│
├── hooks/                          # Event handlers
│   └── hooks.json                  # Hook configuration
│
├── .mcp.json                       # MCP server configurations
│
├── .lsp.json                       # LSP server configs (Claude Code only)
│
├── monitors/                       # Background monitors (Claude Code only)
│   └── monitors.json
│
├── bin/                            # Executables added to PATH (Claude Code only)
│   └── my-tool
│
├── settings.json                   # Default settings (Claude Code only)
│
├── scripts/                        # Utility scripts used by hooks/MCP
│   ├── format-code.sh
│   └── validate.py
│
├── README.md                       # Plugin documentation
├── CHANGELOG.md                    # Version history
└── LICENSE                         # License file
```

---

## Minimal Plugin (Skill Only)

```
my-skill-plugin/
├── .claude-plugin/
│   └── plugin.json
├── skills/
│   └── my-skill/
│       └── SKILL.md
└── README.md
```

### plugin.json
```json
{
  "name": "my-skill-plugin",
  "description": "Brief description of what this plugin does",
  "version": "1.0.0",
  "author": {
    "name": "Your Name"
  }
}
```

### SKILL.md
```markdown
---
name: my-skill
description: What this skill does. Use when [specific triggers].
---

Instructions for the AI when this skill is activated...
```

---

## Marketplace Repository Layout

```
my-marketplace/
├── .github/
│   └── plugin/
│       └── marketplace.json        # Marketplace manifest
│
├── .claude-plugin/
│   └── marketplace.json            # Same file, for Claude Code discovery
│
├── plugins/
│   ├── plugin-a/                   # First plugin
│   │   ├── .claude-plugin/
│   │   │   └── plugin.json
│   │   ├── skills/
│   │   └── README.md
│   │
│   └── plugin-b/                   # Second plugin
│       ├── .claude-plugin/
│       │   └── plugin.json
│       ├── agents/
│       ├── hooks/
│       └── README.md
│
└── README.md                       # Marketplace documentation
```

### marketplace.json
```json
{
  "name": "my-marketplace",
  "owner": {
    "name": "Your Organization",
    "email": "plugins@example.com"
  },
  "metadata": {
    "description": "Curated plugins for our team",
    "version": "1.0.0"
  },
  "plugins": [
    {
      "name": "plugin-a",
      "description": "Description of plugin A",
      "version": "1.0.0",
      "source": "./plugins/plugin-a"
    },
    {
      "name": "plugin-b",
      "description": "Description of plugin B",
      "version": "2.0.0",
      "source": "./plugins/plugin-b"
    }
  ]
}
```
