# Source Inventory: Copilot CLI Resource Usage Research

**Date Accessed:** 2026-03-28

---

## Source 1

- **Title:** GitHub Copilot CLI README (Official Repository)
- **URL:** https://github.com/github/copilot-cli
- **Source Type:** Official documentation (README)
- **Trust Level:** High (95/100) — Primary source, maintained by GitHub
- **Why It Matters:** Defines CLI features, slash commands, customization surfaces (skills, hooks, MCP, custom instructions). Lists all instruction file locations.
- **Limitations:** README is high-level; does not detail internal resource resolution logic.

## Source 2

- **Title:** Creating agent skills for GitHub Copilot CLI
- **URL:** https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/create-skills
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Defines SKILL.md format, skill directory locations, progressive disclosure model (discovery → selection → loading), `/skills` commands (list, info, reload, add, remove). Confirms skills are injected "when relevant."
- **Limitations:** Does not explain the algorithm for determining relevance. Does not say whether injection is logged or observable.

## Source 3

- **Title:** Using hooks with GitHub Copilot CLI
- **URL:** https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/use-hooks
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Defines hooks.json format and location (.github/hooks/ or cwd), hook lifecycle events, debugging patterns.
- **Limitations:** Hooks do not receive skill selection/injection events—only tool use events.

## Source 4

- **Title:** Hooks configuration reference
- **URL:** https://docs.github.com/en/copilot/reference/hooks-configuration
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Complete input/output JSON schema for all hook types. Confirms preToolUse input fields: timestamp, cwd, toolName, toolArgs. Confirms postToolUse adds toolResult. No skill-related fields.
- **Limitations:** No mention of skill name, skill path, or context-loading events in any hook type.

## Source 5

- **Title:** About hooks (Conceptual)
- **URL:** https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-hooks
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Lists all hook types including agentStop and subagentStop. Confirms hooks provide "programmable control or observability."
- **Limitations:** Hook types do not include resource-selection or context-loading events.

## Source 6

- **Title:** About GitHub Copilot CLI session data (Chronicle)
- **URL:** https://docs.github.com/en/copilot/concepts/agents/copilot-cli/chronicle
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Defines session data storage (JSON files in ~/.copilot/session-state/ + SQLite session store). Explains /chronicle subcommands. Confirms data includes prompts, responses, tools used, files modified.
- **Limitations:** Does not confirm whether skill selection/injection events are recorded in session data. No published schema.

## Source 7

- **Title:** GitHub Copilot CLI programmatic reference
- **URL:** https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-programmatic-reference
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Documents -p, -s, --share, --no-ask-user, --deny-tool, --allow-tool, --model, --agent, --no-custom-instructions, COPILOT_HOME env var. Critical for evaluator design.
- **Limitations:** No dry-run mode documented. No way to request resource-selection report.

## Source 8

- **Title:** Comparing GitHub Copilot CLI customization features
- **URL:** https://docs.github.com/en/copilot/concepts/agents/copilot-cli/comparing-cli-features
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Comprehensive comparison of custom instructions vs skills vs tools vs hooks vs MCP vs subagents vs custom agents vs plugins. Clarifies that skills are loaded "when relevant" and hooks provide "programmable control or observability."
- **Limitations:** Does not describe how to observe which skills were loaded.

## Source 9

- **Title:** About agent skills
- **URL:** https://docs.github.com/en/copilot/concepts/agents/about-agent-skills
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Confirms skills are an open standard (agentskills.io). Lists supported locations. Notes org/enterprise skills coming soon.
- **Limitations:** Minimal detail on selection algorithm.

## Source 10

- **Title:** Adding custom instructions for GitHub Copilot CLI
- **URL:** https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-custom-instructions
- **Source Type:** Official GitHub Docs
- **Trust Level:** High (95/100)
- **Why It Matters:** Documents --no-custom-instructions flag. Lists all instruction file locations and loading behavior.
- **Limitations:** No API to query which instructions were actually loaded.

## Source 11

- **Title:** Session Management & History (DeepWiki)
- **URL:** https://deepwiki.com/github/copilot-cli/3.3-session-management-and-history
- **Source Type:** Community documentation (DeepWiki — AI-generated from source)
- **Trust Level:** Medium (65/100)
- **Why It Matters:** Provides deeper technical detail about session state structure, timeline objects, tool logs, and message logs.
- **Limitations:** AI-generated from source code analysis; may contain inaccuracies. Not official.

## Source 12

- **Title:** Session State & Lifecycle Management (DeepWiki)
- **URL:** https://deepwiki.com/github/copilot-cli/6.2-session-state-and-lifecycle-management
- **Source Type:** Community documentation (DeepWiki)
- **Trust Level:** Medium (65/100)
- **Why It Matters:** Details session file format, checkpointing, and context compaction.
- **Limitations:** AI-generated; may not reflect current version.

## Source 13

- **Title:** Dispatch — A GitHub Copilot CLI Session Explorer
- **URL:** https://blog.jongallant.com/2026/03/dispatch-github-copilot-cli-session-explorer/
- **Source Type:** Community tool / blog post
- **Trust Level:** Medium (60/100)
- **Why It Matters:** Demonstrates that session data can be parsed, searched, and explored programmatically. Third-party validation that session files contain detailed tool/action records.
- **Limitations:** Third-party tool; may break with CLI updates.

## Source 14

- **Title:** copilot-session-tools (Arithmomaniac)
- **URL:** https://github.com/Arithmomaniac/copilot-session-tools/
- **Source Type:** Open-source tool
- **Trust Level:** Medium (60/100)
- **Why It Matters:** Directly queries session_store.db. Confirms "sessions" and "turns" as core tables.
- **Limitations:** Third-party; schema may differ across versions.

## Source 15

- **Title:** GitHub Copilot Hooks Complete Guide (SmartScope)
- **URL:** https://smartscope.blog/en/generative-ai/github-copilot/github-copilot-hooks-guide/
- **Source Type:** Community guide
- **Trust Level:** Medium (60/100)
- **Why It Matters:** Practical examples of hook configurations, including audit logging and security enforcement patterns.
- **Limitations:** Community content; may lag behind official updates.

## Source 16

- **Title:** Setting Up GitHub Copilot Agent Skills in Your Repository
- **URL:** https://mkabumattar.com/devtips/post/github-copilot-agent-skills-setup/
- **Source Type:** Community tutorial
- **Trust Level:** Medium (55/100)
- **Why It Matters:** Documents the three-stage progressive disclosure model for skills (discovery, instruction loading, resource access). Practical setup examples.
- **Limitations:** Community content; progressive disclosure model is described informally, not from official spec.

## Source 17

- **Title:** Copilot CLI Skills: A Practical Guide
- **URL:** https://dxrf.com/blog/2026/03/03/copilot-cli-skills-practical-guide/
- **Source Type:** Community blog
- **Trust Level:** Medium (55/100)
- **Why It Matters:** CLI-focused skill usage examples and observations.
- **Limitations:** Community content.

## Source 18

- **Title:** GitHub Copilot Customization: Instructions, Prompts, Agents and Skills
- **URL:** https://blog.cloud-eng.nl/2025/12/22/copilot-customization/
- **Source Type:** Community blog
- **Trust Level:** Medium (55/100)
- **Why It Matters:** Comparative analysis of all customization surfaces.
- **Limitations:** May not reflect latest CLI version.

## Source 19

- **Title:** Copilot CLI — Complete Reference (htekdev)
- **URL:** https://htekdev.github.io/copilot-cli-reference/
- **Source Type:** Community reference
- **Trust Level:** Medium (55/100)
- **Why It Matters:** Consolidated CLI option reference.
- **Limitations:** Unofficial; may be incomplete.

## Source 20

- **Title:** GitHub Copilot SDK (Python)
- **URL:** https://pypi.org/project/github-copilot-sdk/
- **Source Type:** Official SDK
- **Trust Level:** High (80/100)
- **Why It Matters:** Provides programmatic access to Copilot sessions with async support, permission handlers, and event streaming. Alternative to CLI invocation for evaluator.
- **Limitations:** SDK may have different resource resolution than CLI.

## Source 21

- **Title:** PreToolUseHookInput (Copilot SDK Java API)
- **URL:** https://github.github.io/copilot-sdk-java/latest/apidocs/com/github/copilot/sdk/json/PreToolUseHookInput.html
- **Source Type:** Official SDK API documentation
- **Trust Level:** High (85/100)
- **Why It Matters:** Confirms preToolUse input fields: toolName, toolArgs, cwd, timestamp. No skill-related fields.
- **Limitations:** Java SDK; CLI implementation may differ slightly.

## Source 22

- **Title:** Copilot CLI Help Output (built-in)
- **URL:** N/A (local CLI output)
- **Source Type:** Primary — CLI self-documentation
- **Trust Level:** High (95/100)
- **Why It Matters:** Lists /instructions command ("View and toggle custom instruction files"), /skills command, /context command ("Show context window token usage and visualization"). Confirms instruction file locations.
- **Limitations:** Help text is brief; does not explain internal behavior.
