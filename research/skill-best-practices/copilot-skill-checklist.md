# Copilot Skill Checklist

Use this checklist when creating or reviewing a GitHub Copilot agent skill.

**Last Updated:** 2026-03-28

---

## Phase 1: Design

### Scope & Surface Selection
- [ ] Confirmed this belongs in a **skill** (not instructions, prompt file, or agent)
- [ ] Skill covers a single, focused workflow (not a catch-all)
- [ ] Skill name is descriptive, lowercase, hyphens only (1-64 chars)
- [ ] Skill does not duplicate guidance already in `copilot-instructions.md`

### Description Planning
- [ ] Description includes specific trigger keywords (frameworks, tools, task types)
- [ ] Description states when to use the skill (not just what it does)
- [ ] Description is 10-1024 characters
- [ ] Description is behavior-driven (focuses on actions, not abstract concepts)

---

## Phase 2: Authoring

### SKILL.md Frontmatter
- [ ] `name` field present and matches folder name exactly
- [ ] `description` field present and within 10-1024 character limit
- [ ] `license` field present (if skill will be shared)
- [ ] No unsupported metadata fields (only `name`, `description`, `license`)

### SKILL.md Body
- [ ] Instructions use numbered steps with clear success criteria
- [ ] Instructions are imperative and unambiguous ("Create X", not "You might consider X")
- [ ] Body is under 1500 words (~2000 tokens)
- [ ] At least one inline example (input → expected output)
- [ ] No hardcoded version numbers, URLs, or volatile facts in the body
- [ ] References to supporting files use relative paths
- [ ] No repo-wide policy statements (those belong in instructions)

### Supporting Files
- [ ] Verbose documentation is in `reference/` (not in SKILL.md body)
- [ ] At least one golden example exists in `examples/`
- [ ] Templates (if any) are in `templates/`
- [ ] Scripts (if any) are in `scripts/` with exact run commands in SKILL.md
- [ ] All referenced files actually exist at the specified paths

---

## Phase 3: Validation

### Metadata Validation
- [ ] Folder name matches `name` field in frontmatter
- [ ] Name uses only lowercase letters, numbers, and hyphens
- [ ] Description is between 10 and 1024 characters
- [ ] YAML frontmatter parses without errors

### Content Quality
- [ ] Instructions make sense when read independently (no assumed context)
- [ ] No placeholder text (TBD, TODO, FIXME, "add later")
- [ ] No duplicated guidance across multiple skills
- [ ] Examples are realistic and complete (not trivial stubs)

### Activation Testing (Manual)
- [ ] Skill activates when a matching prompt is given
- [ ] Skill does NOT activate for unrelated prompts
- [ ] Output matches golden example quality and format
- [ ] Tested with at least 2-3 different prompt variations

### Script Verification (if applicable)
- [ ] Scripts execute successfully on their own
- [ ] Scripts have their own tests/validation
- [ ] Script dependencies are documented

---

## Phase 4: Maintenance

### Periodic Review (Quarterly Recommended)
- [ ] All referenced URLs still resolve
- [ ] Commands and API references still work with current versions
- [ ] Examples reflect current tool versions and APIs
- [ ] Description still accurately reflects skill behavior
- [ ] No conflicting guidance with updated `copilot-instructions.md`

### Change Management
- [ ] CHANGELOG updated for significant changes (if shared skill)
- [ ] Golden examples updated when instructions change
- [ ] Dependent skills or prompt files updated if this skill's interface changes

---

## Quick Pass/Fail Gate

A skill **fails** review if ANY of these are true:
- [ ] Missing `name` or `description` in frontmatter
- [ ] Folder name does not match `name` field
- [ ] Description is vague (no specific triggers, frameworks, or task types)
- [ ] Body exceeds 2000 words with no supporting files
- [ ] No examples anywhere (neither inline nor in `examples/`)
- [ ] Contains hardcoded volatile data with no freshness mechanism
- [ ] Duplicates repo-wide policy from `copilot-instructions.md`
