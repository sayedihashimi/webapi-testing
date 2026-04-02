# Standalone CLI & Remote Skill Sources — Implementation Plan

## Problem Statement

The `copilot-skill-eval` tool currently requires skills/plugins to be checked into the
repo as local directories. This makes it hard for external users to adopt the tool to
evaluate their own skills. We need to:

1. Introduce a **two-file config** model: `eval.yaml` (evaluation definition) + `skill-sources.yaml` (skill/plugin sources with local and remote support)
2. Make the tool a **standalone CLI** installable via `pipx install` from GitHub
3. Support **remote git-based skill sources** with caching
4. Add **GitHub Actions CI workflow** generation capability
5. Update the **existing examples** to use remote references instead of checked-in skills

## Design Decisions

- **Config separation**: `eval.yaml` owns scenarios/verification/dimensions; `skill-sources.yaml` owns skill/plugin source definitions. Configurations in `eval.yaml` reference sources by name.
- **Remote sources**: Support both `https://` and `git@` URLs, plus `github.com` web URLs (auto-converted to git URLs). Optional `ref` (branch/tag) and `path` (subfolder) parameters.
- **Caching**: Cloned repos stored in `~/.skill-eval/cache/` by default, configurable via `--cache-dir` CLI option or `SKILL_EVAL_CACHE_DIR` env var. Existing clones are updated via `git pull`.
- **Installation**: `pipx install git+https://github.com/sayedihashimi/copilot-skill-eval`
- **dotnet-webapi skill**: Now lives at `https://github.com/sayedihashimi/dotnet-webapi-skill`; examples updated to reference it remotely.

---

## Phase 1: skill-sources.yaml Schema & Source Resolver

### Todo: define-skill-sources-schema
**Title:** Define `skill-sources.yaml` schema and Pydantic models  
**Description:**  
Create a new Pydantic model `SkillSourcesConfig` in `src/skill_eval/config.py` (or a new file `src/skill_eval/source_config.py`) with the following structure:

```yaml
# skill-sources.yaml
cache_dir: ~/.skill-eval/cache   # optional, override default

sources:
  - name: dotnet-webapi           # unique identifier
    type: git                     # "git" or "local"
    url: https://github.com/sayedihashimi/dotnet-webapi-skill
    ref: main                     # optional: branch, tag, or commit
    path: "."                     # optional: subfolder within repo (default: root)
    
  - name: dotnet-artisan
    type: git
    url: https://github.com/ArtisanCode/dotnet-artisan
    
  - name: my-local-skills
    type: local
    path: ./my-skills             # relative to project root
```

Models needed:
- `SkillSource`: name, type (enum: git/local), url (optional), ref (optional), path (optional, default "."), and validation (url required for git type, path required for local type)
- `SkillSourcesConfig`: cache_dir (optional), sources (list[SkillSource])
- Add loading function: `load_skill_sources(path: Path) -> SkillSourcesConfig`

### Todo: update-eval-config-references
**Title:** Update `eval.yaml` Configuration model to support source references  
**Description:**  
Currently, `Configuration.skills` and `Configuration.plugins` are `list[str]` of local paths. Update them to support a new reference format alongside the existing path format for backward compatibility:

```yaml
configurations:
  - name: dotnet-webapi
    label: "dotnet-webapi skill"
    skills:
      - source: dotnet-webapi              # references skill-sources.yaml by name
    plugins: []
  - name: dotnet-skills
    label: "Official .NET Skills"
    skills: []
    plugins:
      - source: dotnet-skills              # whole source as plugin
        path: dotnet                       # subfolder within the source
      - source: dotnet-skills
        path: dotnet-ai
```

Implementation:
- Add `SkillReference` model: either a plain string (legacy local path) or an object with `source` (name) and optional `path` (sub-path within source)
- Update `Configuration.skills` and `Configuration.plugins` types to `list[str | SkillReference]`
- Add a validator to parse both formats
- Maintain full backward compatibility with existing `eval.yaml` files that use plain string paths

### Todo: build-source-resolver
**Title:** Build source resolver that fetches/caches remote skills  
**Description:**  
Create `src/skill_eval/source_resolver.py` with a `SourceResolver` class:

```python
class SourceResolver:
    def __init__(self, sources_config: SkillSourcesConfig, project_root: Path, cache_dir: Path | None = None):
        ...
    
    def resolve(self, ref: SkillReference) -> Path:
        """Resolve a skill/plugin reference to a local absolute path."""
        # If ref is a plain string (legacy), return project_root / ref
        # If ref is a SkillReference:
        #   1. Find the source by name in skill-sources.yaml
        #   2. If type == "local": return project_root / source.path / ref.path
        #   3. If type == "git":
        #      a. Compute cache path: cache_dir / source.name
        #      b. If cache exists: git -C <cache_path> pull (update)
        #      c. If not cached: git clone <url> [--branch <ref>] <cache_path>
        #      d. If source.ref specified: git checkout <ref>
        #      e. Return cache_path / source.path / ref.path
    
    def resolve_all(self, config: EvalConfig) -> dict[str, ResolvedConfiguration]:
        """Resolve all skill/plugin references for all configurations."""
```

Key behaviors:
- URL normalization: convert `https://github.com/user/repo` web URLs to `https://github.com/user/repo.git` git URLs
- Also support `git@github.com:user/repo.git` SSH URLs
- Cache key: use source name as the cache directory name (must be unique)
- On resolve, if cache dir exists and source is git type, run `git fetch && git checkout <ref> && git pull` (or just `git pull` if no specific ref)
- If cache dir exists and no URL was specified (local type), just use the local path
- Proper error messages for: git not found, clone failed, source name not found, path doesn't exist in cloned repo

---

## Phase 2: Integrate Source Resolver into Pipeline

### Todo: update-cli-options
**Title:** Update CLI to accept `--skill-sources`, `--cache-dir`, `--output-dir`, `--reports-dir`  
**Description:**  
Add new CLI options to the main command group and relevant subcommands:

- `--skill-sources` / `-s`: Path to `skill-sources.yaml` (default: `skill-sources.yaml` in project root)
- `--cache-dir`: Override cache directory for git clones (default: `~/.skill-eval/cache/`)
- `--output-dir`: Override output directory (default: `output/` in project root)
- `--reports-dir`: Override reports directory (default: `reports/` in project root)

Pass these through the Click context to all subcommands. If `--skill-sources` file doesn't exist, operate in legacy mode (all skills/plugins must be local paths).

### Todo: update-validate-config
**Title:** Update `validate-config` to resolve sources before validating paths  
**Description:**  
Currently `validate-config` checks that skill/plugin paths exist locally. Update it to:
1. Load `skill-sources.yaml` if it exists
2. Create a `SourceResolver`
3. Resolve each skill/plugin reference to a local path
4. Then validate that the resolved path exists
5. For git sources that haven't been cloned yet, either clone them or report that they need fetching

### Todo: update-generate-pipeline
**Title:** Update generate pipeline to use resolved paths  
**Description:**  
Update `src/skill_eval/generate.py`:
1. Accept a `SourceResolver` (or resolved paths dict)
2. In `_create_staging_dir`: use resolved absolute paths instead of `project_root / rel_path`
3. In skill registration: use resolved absolute paths
4. In plugin CLI flags: use resolved absolute paths
5. Session tracing: update allowed directories to use resolved paths

### Todo: update-verify-analyze
**Title:** Update verify and analyze steps for configurable output/reports paths  
**Description:**  
Update `verify.py` and `analyze.py` to respect `--output-dir` and `--reports-dir` options. Currently these are derived from `project_root/output` and `project_root/reports`. Make them configurable.

---

## Phase 3: Standalone CLI Packaging

### Todo: update-pyproject-metadata
**Title:** Update pyproject.toml for standalone distribution  
**Description:**  
Update `pyproject.toml`:
- Add `gitpython` or verify that subprocess git calls are sufficient (prefer subprocess to avoid extra dependency)
- Ensure all templates are included as package data
- Add proper classifiers, URLs, and project metadata
- Verify `skill-eval` entry point works correctly after install

### Todo: verify-pipx-install
**Title:** Verify pipx installation works end-to-end  
**Description:**  
Test the installation flow:
```bash
pipx install git+https://github.com/sayedihashimi/copilot-skill-eval
skill-eval --help
```
Ensure:
- All dependencies are installed
- Templates are bundled and accessible
- CLI entry point works
- `skill-eval init`, `skill-eval run`, etc. all function

---

## Phase 4: GitHub Actions CI Support

### Todo: create-ci-workflow-template
**Title:** Create GitHub Actions workflow template  
**Description:**  
Create a Jinja2 template at `templates/ci-workflow.yml.j2` that generates a GitHub Actions workflow:

```yaml
name: Skill Evaluation
on:
  workflow_dispatch:
    inputs:
      skip_generate:
        description: 'Skip generation step'
        type: boolean
        default: false
  # Optional: schedule, push triggers
  
jobs:
  evaluate:
    runs-on: ubuntu-latest   # or configurable
    steps:
      - uses: actions/checkout@v4
      - name: Install skill-eval
        run: pipx install git+https://github.com/sayedihashimi/copilot-skill-eval
      - name: Run evaluation
        run: skill-eval run --config eval.yaml --skill-sources skill-sources.yaml
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      - name: Upload reports
        uses: actions/upload-artifact@v4
        with:
          name: evaluation-reports
          path: reports/
```

### Todo: add-ci-setup-command
**Title:** Add `skill-eval ci-setup` CLI command  
**Description:**  
Add a new CLI command `ci-setup` (or integrate into `init`) that:
1. Reads the current eval.yaml and skill-sources.yaml
2. Renders the CI workflow template
3. Writes it to `.github/workflows/skill-eval.yml`
4. Provides guidance on required secrets (e.g., tokens for private repos)

---

## Phase 5: Update Examples

### Todo: update-aspnet-webapi-example
**Title:** Update aspnet-webapi example to use remote skill sources  
**Description:**  
1. Remove `examples/aspnet-webapi/skills/dotnet-webapi/` (now at https://github.com/sayedihashimi/dotnet-webapi-skill)
2. Create `examples/aspnet-webapi/skill-sources.yaml` with git references:
   ```yaml
   sources:
     - name: dotnet-webapi
       type: git
       url: https://github.com/sayedihashimi/dotnet-webapi-skill
     - name: managedcode-dotnet-skills
       type: git
       url: https://github.com/ArtisanCode/dotnet-skills  # or actual URL
     - name: dotnet-artisan
       type: git
       url: https://github.com/ArtisanCode/dotnet-artisan  # or actual URL
     - name: dotnet-skills
       type: git
       url: https://github.com/dotnet/skills  # or actual URL
   ```
3. Update `examples/aspnet-webapi/eval.yaml` configurations to use source references
4. Remove checked-in skills/plugins from the example
5. Update example README/docs

### Todo: cleanup-root-skills-plugins
**Title:** Clean up root-level skills/ and plugins/ directories  
**Description:**  
Once examples are updated, clean up the root-level `skills/` and `plugins/` directories. They should either be removed or repurposed (e.g., used only for development/testing of the tool itself).

---

## Phase 6 (Later): Update Init Command

### Todo: update-init-for-sources (LATER PHASE)
**Title:** Update `init` command for two-file workflow  
**Description:**  
Update the interactive `init` command to:
1. Ask if skills are local or remote
2. For remote: prompt for git URL, branch, subfolder
3. Generate both `eval.yaml` and `skill-sources.yaml`
4. Optionally generate CI workflow file

*This is marked as a later phase per user direction.*

---

## File Change Summary

| File | Action | Description |
|------|--------|-------------|
| `src/skill_eval/source_config.py` | **CREATE** | SkillSource, SkillSourcesConfig models + loader |
| `src/skill_eval/source_resolver.py` | **CREATE** | SourceResolver class for fetching/caching git sources |
| `src/skill_eval/config.py` | **MODIFY** | Add SkillReference model, update Configuration.skills/plugins types |
| `src/skill_eval/cli.py` | **MODIFY** | Add --skill-sources, --cache-dir, --output-dir, --reports-dir options |
| `src/skill_eval/generate.py` | **MODIFY** | Use resolved paths from SourceResolver |
| `src/skill_eval/verify.py` | **MODIFY** | Support configurable output/reports paths |
| `src/skill_eval/analyze.py` | **MODIFY** | Support configurable output/reports paths |
| `src/skill_eval/prompt_renderer.py` | **MODIFY** | Support configurable paths in template rendering |
| `pyproject.toml` | **MODIFY** | Update metadata for standalone distribution |
| `templates/ci-workflow.yml.j2` | **CREATE** | GitHub Actions workflow template |
| `examples/aspnet-webapi/skill-sources.yaml` | **CREATE** | Example skill sources config |
| `examples/aspnet-webapi/eval.yaml` | **MODIFY** | Update to use source references |
| `examples/aspnet-webapi/skills/` | **DELETE** | Remove checked-in skills (now remote) |
| `examples/aspnet-webapi/plugins/` | **DELETE** | Remove checked-in plugins (now remote) |
| `.github/prompts/standalone-cli-plan.md` | **CREATE** | This plan stored in .github/prompts |

## Implementation Order

```
Phase 1 (Foundation)
  ├── define-skill-sources-schema
  ├── update-eval-config-references
  └── build-source-resolver

Phase 2 (Integration)  ← depends on Phase 1
  ├── update-cli-options
  ├── update-validate-config
  ├── update-generate-pipeline
  └── update-verify-analyze

Phase 3 (Packaging)  ← depends on Phase 2
  ├── update-pyproject-metadata
  └── verify-pipx-install

Phase 4 (CI)  ← depends on Phase 3
  ├── create-ci-workflow-template
  └── add-ci-setup-command

Phase 5 (Examples)  ← depends on Phase 2
  ├── update-aspnet-webapi-example
  └── cleanup-root-skills-plugins

Phase 6 (Later)  ← depends on Phase 2
  └── update-init-for-sources
```
