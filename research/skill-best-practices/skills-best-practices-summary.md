# Executive Summary: GitHub Copilot Skills Best Practices

**Last Updated:** 2026-03-28

---

## What You Need to Know

GitHub Copilot Agent Skills are modular instruction packages (SKILL.md + supporting assets) that auto-activate when relevant. They follow the agentskills.io open standard, making them portable across Copilot, Claude, Codex, and Gemini.

## Key Takeaways

### 1. Understand the Five Customization Surfaces

| Use This | When You Need |
|----------|--------------|
| Custom instructions | Always-on repo/project rules |
| Path-specific instructions | File/language-scoped rules (via glob patterns) |
| Prompt files | Repeatable one-off tasks (manual trigger with `/`) |
| Custom agents | Role-based personas with tool restrictions |
| **Skills** | **Complex multi-step workflows with auto-activation** |

### 2. The Description Is Everything

The `description` field determines if/when a skill activates. Write it as a behavior-driven trigger list:

❌ `"Helps with testing."`
✅ `"Generates Playwright E2E tests for React components. Use when creating integration tests, browser tests, or E2E test suites."`

### 3. Keep SKILL.md Lean

- Body under 1500 words / ~2000 tokens
- Move verbose content to `reference/` (loads on-demand)
- Include 1+ inline example in the body
- Use numbered steps with clear success criteria

### 4. One Skill = One Workflow

- Split broad skills into focused, composable units
- Each skill should do one thing well
- Better activation accuracy, less context waste

### 5. Context Efficiency Matters

Skills use **progressive disclosure** (three-tier loading):
1. **Metadata only** (~50-100 tokens) — always loaded
2. **Full instructions** (~500-2000 tokens) — loaded when matched
3. **Supporting files** — loaded on-demand

### 6. Required Metadata

```yaml
---
name: skill-name          # 1-64 chars, lowercase, hyphens, must match folder
description: ...          # 10-1024 chars, behavior-driven, trigger keywords
license: MIT              # Optional
---
```

Only `name`, `description`, and `license` are supported. Do not add custom fields.

### 7. Watch for Anti-Patterns

| Anti-Pattern | Fix |
|-------------|-----|
| Too much content in SKILL.md | Move to `reference/` files |
| Vague description | Add specific triggers, frameworks, tasks |
| Hardcoded version numbers/URLs | Put in `reference/` with freshness dates |
| One giant catch-all skill | Split into focused skills |
| No examples | Add golden input/output pairs |
| Repo policy in skill (not instructions) | Move to `copilot-instructions.md` |

### 8. No Formal Testing Yet — Use These Instead

- **Golden examples** in `examples/` for regression checking
- **Peer review** of SKILL.md using a checklist
- **Test prompts** that exercise the skill under various conditions
- **Metadata linting** for format/constraint validation
- **Quarterly freshness reviews** for links and version references

## Quick-Reference File Structure

```
.github/skills/my-skill/
  SKILL.md          # Lean instructions + metadata
  reference/        # Verbose docs, API specs
  examples/         # Golden input/output pairs
  templates/        # File templates to generate
  scripts/          # Executable scripts
```

## Before You Author a Skill, Ask

1. Should this be an always-on instruction instead?
2. Is the scope narrow enough for one skill?
3. Does the description include specific trigger keywords?
4. Is the body under 1500 words?
5. Do I have at least one golden example?
