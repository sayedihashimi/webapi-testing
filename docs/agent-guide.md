# Using the @skill-eval Agent

The Copilot agent provides a conversational interface for the entire evaluation workflow. Instead of running CLI commands, you can chat with the agent to set up, execute, and interpret evaluations.

## Available Workflows

| Workflow | Trigger | What It Does |
|----------|---------|-------------|
| **Setup** | "evaluate my skill" | Asks about your stack and scenarios, generates `eval.yaml` and prompts |
| **Scenario Authoring** | "help write scenario" | Helps write detailed app specifications |
| **Execution** | "run the evaluation" | Runs the generate → verify → analyze pipeline |
| **Interpretation** | "what did analysis find?" | Summarizes findings, highlights strengths and weaknesses |
| **Iteration** | "improve my skill" | Suggests skill improvements based on analysis, re-runs evaluation |

## Example Conversation

```
You:   @skill-eval I wrote a custom skill for React component generation.
       I want to see if it actually makes Copilot produce better code.

Agent: Great! Let me help you set up an evaluation. I'll need a few details:
       1. What framework/version are you targeting?
       2. What kinds of apps should we generate for comparison?
       ...
```

```
You:   @skill-eval what did the analysis find?

Agent: Your skill (react-components) vs baseline across 2 apps:

       ✅ Strong wins:
       - Component structure: proper atomic design (baseline uses monolithic)
       - TypeScript: 100% typed props (baseline has mixed any/unknown)

       ⚠️ Mixed:
       - State management: Context used correctly, but missed React Query

       ❌ No difference:
       - CSS: both used inline styles — add CSS module guidance to your skill

       Want me to suggest specific changes to your SKILL.md?
```

## Boundaries

The agent:
- Does **not** modify skills without user confirmation
- Does **not** run the pipeline without confirmation
- Does **not** generate app code directly (only via the CLI pipeline)
- Focuses on the evaluation workflow, not general coding

## Setup

The agent is defined in `.github/agents/skill-eval.agent.md`. It requires the Copilot agent feature to be available in your environment.
