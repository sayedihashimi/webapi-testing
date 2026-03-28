# Source Inventory

Sources consulted for the GitHub Copilot Skills Best Practices research.

**Research Date:** 2026-03-28

---

## Source List

### [1] Creating agent skills for GitHub Copilot

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/create-skills |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Primary source for skill structure, metadata fields, and file placement conventions. The definitive reference for what a skill is and how to create one. |
| **Limitations** | Does not cover advanced patterns, testing, or cross-platform portability in depth. |

---

### [2] Use Agent Skills in VS Code

| Field | Value |
|-------|-------|
| **URL** | https://code.visualstudio.com/docs/copilot/customization/agent-skills |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official VS Code documentation) |
| **Why It Matters** | Covers VS Code-specific skill discovery, the `/skills` command, and `chat.agentSkillsLocations` settings. |
| **Limitations** | VS Code-specific; does not cover CLI or coding agent behavior. |

---

### [3] GitHub Copilot Agent Skills Guide (SmartScope)

| Field | Value |
|-------|-------|
| **URL** | https://smartscope.blog/en/generative-ai/github-copilot/github-copilot-skills-guide/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — detailed, well-researched, consistent with official docs) |
| **Why It Matters** | Comprehensive walkthrough from SKILL.md creation to troubleshooting. Fills gaps in official documentation with practical examples. |
| **Limitations** | Community source; recommendations may include subjective opinions. |

---

### [4] Agent Skills Specification (agentskills.io)

| Field | Value |
|-------|-------|
| **URL** | https://agentskills.io/specification |
| **Source Type** | Open specification |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — maintained by Anthropic, adopted by GitHub/Microsoft/OpenAI) |
| **Why It Matters** | Defines the cross-platform standard for SKILL.md format, metadata fields, and portability guarantees. The foundational specification that GitHub Copilot skills implement. |
| **Limitations** | Specification defines what should work, not necessarily what every implementation supports. |

---

### [5] Agent Skills Specification (DeepWiki Analysis)

| Field | Value |
|-------|-------|
| **URL** | https://deepwiki.com/anthropics/skills/5.1-agent-skills-specification |
| **Source Type** | Community analysis |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★ (Moderate — useful analysis but derivative of primary spec) |
| **Why It Matters** | Provides accessible explanation of the specification with implementation examples. |
| **Limitations** | Third-party interpretation; may lag behind spec changes. |

---

### [6] Progressive Disclosure Pattern (DeepWiki)

| Field | Value |
|-------|-------|
| **URL** | https://deepwiki.com/microsoft/agent-skills/5.3-progressive-disclosure-pattern |
| **Source Type** | Community analysis of Microsoft repo |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — based on Microsoft's agent-skills repository) |
| **Why It Matters** | Best available description of the three-tier loading model that determines how skills inject into model context. |
| **Limitations** | Analysis of a public repo, not official documentation of Copilot's internal behavior. |

---

### [7] Use GitHub Copilot Agent Skills Without Blowing Your Context Window

| Field | Value |
|-------|-------|
| **URL** | https://www.cloudproinc.com.au/index.php/2026/02/02/use-github-copilot-agent-skills-without-blowing-your-context-window/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — practical, specific, consistent with official guidance) |
| **Why It Matters** | Most detailed available guidance on context efficiency and modularization strategies for skills. |
| **Limitations** | Token cost estimates are approximate and may vary by model/platform. |

---

### [8] GitHub Copilot Customization: Instructions, Prompts, Agents and Skills

| Field | Value |
|-------|-------|
| **URL** | https://blog.cloud-eng.nl/2025/12/22/copilot-customization/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — comprehensive comparison, well-structured) |
| **Why It Matters** | Best available comparison of all five customization surfaces with decision guidance. |
| **Limitations** | Published December 2025; skills ecosystem has evolved since then. |

---

### [9] GitHub Copilot Instructions vs Prompts vs Custom Agents vs Skills

| Field | Value |
|-------|-------|
| **URL** | https://dev.to/pwd9000/github-copilot-instructions-vs-prompts-vs-custom-agents-vs-skills-vs-x-vs-why-339l |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — detailed feature comparison with practical examples) |
| **Why It Matters** | Clear decision framework for choosing between customization surfaces. Includes practical examples for each. |
| **Limitations** | Community perspective; some recommendations may be subjective. |

---

### [10] Copilot Customization Cheat Sheet

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/reference/customization-cheat-sheet |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Comprehensive reference table covering all customization mechanisms including hooks and MCP servers. |
| **Limitations** | Cheat sheet format — provides breadth but not depth on any single topic. |

---

### [11] Adding Custom Instructions for GitHub Copilot CLI

| Field | Value |
|-------|-------|
| **URL** | https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official GitHub documentation) |
| **Why It Matters** | Documents path-specific instructions with `applyTo` glob patterns and instruction merging behavior. |
| **Limitations** | CLI-specific; some behaviors may differ in VS Code or coding agent. |

---

### [12] Custom Instructions and Memory System (DeepWiki)

| Field | Value |
|-------|-------|
| **URL** | https://deepwiki.com/github/awesome-copilot/4-custom-instructions-and-memory-system |
| **Source Type** | Community analysis |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★ (Moderate — useful technical analysis of awesome-copilot repo) |
| **Why It Matters** | Detailed coverage of glob pattern mechanics and instruction loading behavior. |
| **Limitations** | Third-party analysis; may not reflect all edge cases. |

---

### [13] Use Prompt Files in VS Code

| Field | Value |
|-------|-------|
| **URL** | https://code.visualstudio.com/docs/copilot/customization/prompt-files |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official VS Code documentation) |
| **Why It Matters** | Definitive reference for prompt file format, frontmatter fields, and usage patterns. |
| **Limitations** | VS Code-specific features may not apply to other editors. |

---

### [14] Prompt Files and Instructions Files Explained

| Field | Value |
|-------|-------|
| **URL** | https://devblogs.microsoft.com/dotnet/prompt-files-and-instructions-files-explained/ |
| **Source Type** | Official blog (Microsoft) |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — official Microsoft developer blog) |
| **Why It Matters** | Clear explanation of the distinction between prompt files and instruction files with practical examples. |
| **Limitations** | .NET-focused examples; concepts are universal but examples are language-specific. |

---

### [15] Custom Agents in VS Code

| Field | Value |
|-------|-------|
| **URL** | https://code.visualstudio.com/docs/copilot/customization/custom-agents |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official VS Code documentation) |
| **Why It Matters** | Definitive reference for custom agent definition format, tool restrictions, and persona configuration. |
| **Limitations** | VS Code-specific. |

---

### [16] Setting Up GitHub Copilot Agent Skills in Your Repository

| Field | Value |
|-------|-------|
| **URL** | https://mkabumattar.com/devtips/post/github-copilot-agent-skills-setup/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — detailed, practical, consistent with official docs) |
| **Why It Matters** | Step-by-step setup guide with emphasis on supporting file organization and golden examples. |
| **Limitations** | Community source. |

---

### [17] Agent Skills Anti-Patterns

| Field | Value |
|-------|-------|
| **URL** | https://github.com/duskmoon314/keine/blob/main/docs/2026-03-11-agent-skills-anti-patterns.md |
| **Source Type** | Community document (GitHub repo) |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★ (Moderate — personal documentation but specific and practical) |
| **Why It Matters** | Focused collection of anti-patterns specific to agent skills authoring. |
| **Limitations** | Individual perspective; not peer-reviewed. |

---

### [18] Test with GitHub Copilot

| Field | Value |
|-------|-------|
| **URL** | https://code.visualstudio.com/docs/copilot/guides/test-with-copilot |
| **Source Type** | Official documentation |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★★ (Authoritative — official VS Code documentation) |
| **Why It Matters** | Covers Copilot's testing capabilities, though focused on code testing rather than skill testing. |
| **Limitations** | Does not address skill-specific testing or validation. |

---

### [19] GitHub Copilot Agent Skills: Teaching AI Your Repository Patterns

| Field | Value |
|-------|-------|
| **URL** | https://dev.to/qa-leaders/github-copilot-agent-skills-teaching-ai-your-repository-patterns-1oa8 |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★ (Moderate — QA-focused perspective with practical testing suggestions) |
| **Why It Matters** | One of few sources addressing skill quality validation from a testing perspective. |
| **Limitations** | Community source; testing recommendations are not officially endorsed. |

---

### [20] Agent Skills Part 1: Getting Started with SKILL.md

| Field | Value |
|-------|-------|
| **URL** | https://aridanemartin.dev/blog/agent-skills-getting-started/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — detailed beginner guide with progressive disclosure explanation) |
| **Why It Matters** | Accessible introduction to skills with emphasis on the progressive disclosure model. |
| **Limitations** | Introductory level; does not cover advanced patterns. |

---

### [21] How to Create a Skill for GitHub Copilot (Code Inside Blog)

| Field | Value |
|-------|-------|
| **URL** | https://blog.codeinside.eu/2026/03/17/how-to-create-a-skill-for-github-copilot/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★ (Moderate — recent, practical, but brief) |
| **Why It Matters** | Recent (March 2026) practical guide confirming current SKILL.md conventions. |
| **Limitations** | Brief; limited depth. |

---

### [22] Agent Skills, Plugins and Marketplace: The Complete Guide

| Field | Value |
|-------|-------|
| **URL** | https://chris-ayers.com/posts/agent-skills-plugins-marketplace/ |
| **Source Type** | Community guide |
| **Date Accessed** | 2026-03-28 |
| **Trust Level** | ★★★★ (High — comprehensive coverage of skills ecosystem including marketplace) |
| **Why It Matters** | Broader ecosystem perspective covering skill distribution, discovery, and the emerging marketplace model. |
| **Limitations** | Some marketplace features may be speculative or not yet GA. |

---

## Trust Level Legend

| Rating | Meaning |
|--------|---------|
| ★★★★★ | **Authoritative** — Official documentation from GitHub, Microsoft, or specification maintainers |
| ★★★★ | **High** — Well-researched community source, consistent with official docs, from recognized author |
| ★★★ | **Moderate** — Useful community source with practical value, but not independently verified |
| ★★ | **Low** — Single-perspective source, may contain inaccuracies or opinions presented as facts |
| ★ | **Unreliable** — Not used in this research |

## Source Distribution

| Category | Count |
|----------|-------|
| Official GitHub/VS Code documentation | 7 |
| Open specification (agentskills.io) | 1 |
| Community analysis (DeepWiki) | 3 |
| Community guides (blogs, DEV.to) | 11 |
| **Total** | **22** |
