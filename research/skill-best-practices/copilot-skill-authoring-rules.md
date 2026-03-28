# Copilot Skill Authoring Rules

> Compact, reusable rules for prompting Copilot to create or review skills.
> Reference this file when asking Copilot to generate, update, or review a skill.

**Last Updated:** 2026-03-28

---

## Structure Rules

1. Every skill is a directory containing `SKILL.md` plus optional supporting files.
2. Place project skills in `.github/skills/<name>/`. Place personal skills in `~/.copilot/skills/<name>/`.
3. Folder name must exactly match the `name` field in YAML frontmatter.

## Metadata Rules

4. YAML frontmatter must include `name` and `description`. Optionally include `license`.
5. `name`: lowercase, hyphens only, 1-64 characters. No spaces, underscores, or uppercase.
6. `description`: 10-1024 characters. Behavior-driven. Include trigger keywords.
7. Do not add unsupported frontmatter fields. Only `name`, `description`, and `license` are recognized.

## Description Rules

8. State what the skill does AND when to activate it.
9. Include specific technologies, frameworks, file types, or task types as trigger keywords.
10. Avoid vague terms like "helps with", "improves", or "assists" without specifics.
11. Good pattern: `"Generates [output] for [context]. Use when [trigger conditions]."`

## Body Rules

12. Write instructions as numbered steps with clear success criteria per step.
13. Use imperative voice: "Create", "Run", "Verify" — not "You might consider".
14. Keep the body under 1500 words. Move verbose content to `reference/`.
15. Include at least one inline example showing expected input → output.
16. Reference supporting files with relative paths: `See reference/api-guide.md`.
17. Do not embed repo-wide policies — those belong in `copilot-instructions.md`.
18. Do not hardcode version numbers, URLs, or other volatile data. Put those in `reference/` files with freshness dates.

## Supporting Files Rules

19. Place background docs in `reference/`, golden examples in `examples/`, file templates in `templates/`, executable scripts in `scripts/`.
20. Every skill should have at least one golden example in `examples/` showing a complete, realistic scenario.
21. Scripts must be self-contained with documented dependencies and exact run commands in SKILL.md.

## Design Rules

22. One skill = one workflow. If a skill covers multiple unrelated workflows, split it.
23. Prefer small, composable skills over large monolithic ones.
24. Before creating a skill, verify the guidance does not belong in instructions (always-on rules) or a prompt file (manual one-off tasks) instead.

## Quality Rules

25. After writing a skill, review it against the checklist in `research/copilot-skill-checklist.md`.
26. Test the skill with at least 3 different prompt variations to verify activation and output quality.
27. Ensure no placeholder text remains (TBD, TODO, FIXME, "add later").

## Maintenance Rules

28. Review skills quarterly: verify links, update version references, refresh examples.
29. Update CHANGELOG when making significant changes to shared skills.
30. When the agentskills.io specification changes, review all skills for compliance.
