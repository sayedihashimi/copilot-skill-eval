---
description: Deep research prompt for GitHub Copilot CLI to investigate the latest best practices for authoring AI skills and to produce reusable implementation guidance.
---

# Deep Research: Latest Best Practices for AI Skills

You are performing **deep research** for me on how to create high-quality AI skills, with a **primary focus on GitHub Copilot CLI / GitHub Copilot skills** and a **secondary focus on transferable patterns from other mature skill ecosystems**.

Your job is not just to summarize articles. Your job is to produce **implementation-grade guidance** that I can reuse later when I ask Copilot to create or review skills.

## Primary outcome

Produce a research-backed set of artifacts that will help me:

1. Learn how to create GitHub Copilot skills following the **latest best practices**.
2. Understand when to use a **skill** versus **custom instructions**, **path/repo instructions**, **prompt files**, or **custom agents/profiles**.
3. Create skills that are **reliable, maintainable, reusable, testable, and easy for Copilot to apply correctly**.
4. Build a reusable **best-practices reference pack** that I can later point Copilot at when I say things like:
   - “Create a new skill following the latest best practices.”
   - “Review this skill against the latest best practices.”
   - “Update this skill to current best practices.”

## Research priorities

Prioritize sources in this order:

1. **Official GitHub Copilot documentation**
2. Official docs or specifications referenced by GitHub
3. High-quality public example repositories
4. Community guidance only when it adds concrete value beyond official docs

Use this repository as **one example pattern to inspect**, not as a source of truth:

- https://github.com/199-biotechnologies/claude-deep-research-skill

Do **not** assume that repo’s conventions are automatically correct for GitHub Copilot skills. Compare any useful ideas from it against official GitHub guidance.

## Important research rules

- Clearly separate:
  - **Officially documented facts**
  - **Reasonable implementation recommendations**
  - **Cross-platform ideas borrowed from other ecosystems**
  - **Repo-specific opinions or experiments**
- Prefer the most current guidance you can find.
- If documentation is unclear, incomplete, or seems to conflict across sources, call that out explicitly.
- Do **not** invent unsupported metadata, undocumented behavior, or unverified capability claims.
- When useful, quote exact metadata field names or exact support details, but keep quotes short and minimal.
- When you make an inference, label it as an inference.

## Research questions to answer

### 1) Core model: what a Copilot skill actually is

Research and explain:

- What a GitHub Copilot skill is
- How Copilot decides when to use a skill
- Supported folder and file layout patterns
- What parts of a skill are injected into model context versus just stored as supporting files
- What metadata fields are currently supported in `SKILL.md`
- Current compatibility considerations, caveats, and limitations
- Any current differences between CLI, coding agent, and editor usage if relevant

### 2) Compare the available customization surfaces

Create a clear comparison table for:

- Skills
- Repo-wide custom instructions
- Path-specific instructions
- Prompt files
- Custom agents / agent profiles

For each, explain:

- Best use cases
- Strengths
- Weaknesses
- Context cost
- Reusability
- Whether it is always-on or just-in-time
- Whether it is task-scoped or repo-scoped
- When a team should choose it instead of a skill

### 3) Latest best practices for skill authoring

Synthesize current best practices for:

- Writing skill descriptions so Copilot invokes the skill reliably
- Deciding what belongs in `SKILL.md` versus supporting files
- Keeping `SKILL.md` lean without making it too vague
- Organizing supporting content into references, examples, templates, and scripts
- Designing small, composable skills instead of giant catch-all skills
- Reducing ambiguity and hallucinations
- Reducing context bloat
- Making skills robust even when the model only partially follows instructions
- Improving repeatability and consistency
- Avoiding brittle tool-specific guidance
- Making skills easier to maintain as the platform evolves

### 4) Repository architecture for a serious skills repo

Recommend a modern repository layout for a production-quality skills repository.

At minimum address the role of:

- `SKILL.md`
- `reference/`
- `examples/`
- `templates/`
- `scripts/`
- `tests/`
- `evaluation/`
- changelog / versioning docs

For each folder or artifact, explain:

- What belongs there
- What should not belong there
- Why it improves quality and maintainability
- How it should interact with the rest of the repo

### 5) Quality assurance, testing, and validation

Research how strong skill authors validate quality.

Include guidance for:

- Manual review patterns
- Golden examples
- Prompt-based regression testing
- Review checklists
- Linting or validation of skill metadata
- Script verification
- Freshness checks for external references
- Evaluation rubrics for correctness, completeness, maintainability, and context efficiency

If current GitHub tooling for formal validation is limited, say so clearly and recommend practical alternatives.

### 6) Research-to-implementation workflow

I want to use this research later when I ask Copilot to build skills.

Design a practical workflow for:

- Maintaining a “skills best practices” reference pack inside a repository
- Updating it over time
- Recording source provenance
- Distilling research into concise authoring rules
- Turning the research into implementation checklists and review rubrics
- Feeding that research back into Copilot when I ask it to generate or update a skill

### 7) Anti-patterns and failure modes

Identify common mistakes and anti-patterns such as:

- Putting too much content into `SKILL.md`
- Making the description too vague or too broad
- Encoding unstable facts directly into the skill
- Mixing repo policy with narrow task instructions
- Using unsupported metadata
- Having no examples or tests
- Creating one giant skill instead of focused skills
- Including outdated implementation assumptions
- Failing to separate canonical guidance from convenience notes

For each anti-pattern:

- Explain why it is harmful
- Show a better alternative
- Explain how to detect it during review

## Required outputs

Create the following files as separate Markdown files.

### 1. `research/skills-best-practices-report.md`
A comprehensive report.

Requirements:
- Organize it into clear sections
- Use citations inline or as footnotes
- Clearly label official guidance vs inferred guidance
- Include a “what changed recently” section if the sources support it
- Include a concise final recommendations section

### 2. `research/skills-best-practices-summary.md`
A concise executive summary.

Requirements:
- Short and high signal
- Focus on the most actionable takeaways
- Suitable for quick reading before authoring a skill

### 3. `research/copilot-skill-checklist.md`
A practical checklist for creating or reviewing a skill.

Requirements:
- Use checkbox format
- Organize by phases: design, authoring, validation, maintenance
- Include both required and recommended checks

### 4. `research/copilot-skill-repo-template.md`
A recommended repository structure.

Requirements:
- Show a sample directory tree
- Explain why each part exists
- Include notes on optional vs recommended folders

### 5. `research/copilot-skill-authoring-rules.md`
A compact rules file I can later paste into prompts when asking Copilot to generate a skill.

Requirements:
- Keep it short enough to be reusable in prompts
- Prefer imperative authoring rules
- Focus on durable guidance rather than temporary platform quirks

### 6. `research/copilot-skill-evaluation-rubric.md`
A scoring rubric for reviewing a skill.

Requirements:
- Include scoring dimensions
- Include what “excellent / acceptable / poor” looks like
- Include at least one overall pass/fail recommendation model

### 7. `research/sources.md`
A source inventory.

For each source include:
- Title
- URL
- Source type
- Date accessed
- Trust level
- Why it matters
- Notes on limitations or ambiguity, if any

## Method requirements

Follow this workflow:

1. Start with official GitHub Copilot documentation.
2. Extract the current documented facts about skills and adjacent customization surfaces.
3. Inspect high-quality public examples and compare them against the official guidance.
4. Separate hard facts from recommendations.
5. Turn the research into implementation-ready guidance and reusable artifacts.

## Output style requirements

- Be explicit and practical.
- Prefer concrete guidance over abstract advice.
- Avoid filler.
- Use tables where they improve clarity.
- Use examples when they make the recommendation easier to apply.
- Do not overquote sources.
- If something is unknown, undocumented, or likely to change, say so directly.

## Final synthesis section

At the end of `research/skills-best-practices-report.md`, include a section titled exactly:

## How I should prompt Copilot in the future

That section must provide:

1. A short reusable prompt for:
   - **Create a skill**
2. A short reusable prompt for:
   - **Review this skill**
3. A short reusable prompt for:
   - **Update this skill to the latest best practices**

These prompts should be concise and directly usable.

## Final constraint

This is **not** just a literature review.

I want a **research-backed implementation pack** that I can reuse to help Copilot author skills correctly and consistently in the future.
