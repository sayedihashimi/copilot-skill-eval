# Competitive Analysis: copilot-skill-eval vs dotnet/skills skill-validator

> **Date:** April 2026
> **Repos:** [sayedihashimi/copilot-skill-eval](https://github.com/sayedihashimi/copilot-skill-eval) · [dotnet/skills/eng/skill-validator](https://github.com/dotnet/skills/tree/main/eng/skill-validator)

---

## Executive Summary

**copilot-skill-eval** and the **dotnet/skills skill-validator** are both tools for evaluating whether AI coding skills actually improve agent output quality. They represent the two closest competitors in this space. However, they differ significantly in scope, methodology, and target audience.

| | copilot-skill-eval | skill-validator |
|---|---|---|
| **One-liner** | Evaluate how custom skills impact Copilot code generation quality across any tech stack | Validate that agent skills meaningfully improve agent performance with A/B testing |
| **Category** | Comparative evaluation framework | A/B testing + static analysis tool |
| **Primary question** | "Do my skills produce better *generated applications*?" | "Does my skill make the *agent* perform better on specific tasks?" |
| **Owner** | sayedihashimi (individual) | dotnet team (Microsoft) |

---

## 1. Purpose & Evaluation Philosophy

### copilot-skill-eval

Evaluates skills by **generating entire applications** (e.g., a Fitness Studio API, a Property Management app) with and without skills, then scoring the generated code across user-defined quality dimensions (architecture, error handling, security, etc.). The focus is on **end-to-end output quality** of complete, buildable projects.

**Mental model:** "Generate two full apps — one with skills, one without — then compare them across 20+ quality dimensions."

### skill-validator

Evaluates skills by running **targeted task scenarios** (e.g., "diagnose this build failure", "optimize this LINQ query") with and without a specific skill, then using **pairwise LLM judging** to determine which output is better. The focus is on **per-task performance** with statistical rigor.

**Mental model:** "Run the same task 5 times with and without the skill, have an LLM judge compare outputs side-by-side, and compute a statistically significant improvement score."

---

## 2. Target Audience

| Audience | copilot-skill-eval | skill-validator |
|---|---|---|
| Skill authors (any platform) | ✅ Primary | ⚠️ .NET-focused tooling, but generic eval format |
| Plugin maintainers (dotnet/skills) | ⚠️ Could be used | ✅ Primary — built for this repo |
| Teams evaluating skill ROI | ✅ Primary | ✅ Supported |
| CI/CD pipeline operators | ✅ Built-in Actions workflow | ✅ Deep CI integration (matrix jobs, consolidate) |
| Non-.NET developers | ✅ Tech-stack agnostic | ⚠️ Tool is .NET-native; eval format is generic |

---

## 3. Methodology Comparison

This is the most important difference between the two tools.

### 3.1 What Gets Evaluated

| Aspect | copilot-skill-eval | skill-validator |
|---|---|---|
| **Unit of evaluation** | Full application generation | Individual task completion |
| **Baseline** | Generate app with no skills (or a different skill set) | Run agent on task with no skills loaded |
| **Treatment** | Generate same app with skills enabled | Run agent on same task with skill loaded |
| **Comparison level** | Configurable skill *sets* (combinations) | Individual skills (isolated + full-plugin) |
| **Scenario scope** | Entire application (Dashboard, Blog, API, etc.) | Focused task (diagnose build failure, fix bug, etc.) |

### 3.2 How Quality Is Measured

| Aspect | copilot-skill-eval | skill-validator |
|---|---|---|
| **Quality dimensions** | User-defined (e.g., "Component Architecture", "Error Handling") with tier weights (critical 3×, high 2×, medium 1×, low 0.5×) | Fixed weighted formula: quality rubric (0.40) + overall judgment (0.30) + task completion (0.15) + efficiency metrics (0.15) |
| **Scoring method** | LLM analysis per dimension, then weighted aggregation | Pairwise LLM judging (both outputs shown side-by-side) with position-swap bias mitigation |
| **Rubric** | Dimensions with `what_to_look_for` and `why_it_matters` | Per-scenario rubric items (free-text criteria) |
| **Verification** | Build, format-check, security scan, health-check the generated app | Hard assertions (file_exists, output_contains, output_matches, exit_success, run_command_and_assert) |
| **Statistical approach** | Multiple runs with aggregation | Bootstrap confidence intervals, coefficient of variation, normalized gain (Hake 1998), Wilson score intervals |
| **Significance testing** | Average across N runs | 95% CI excludes zero = statistically significant |

### 3.3 Judging Approach

| Aspect | copilot-skill-eval | skill-validator |
|---|---|---|
| **Judge type** | LLM analyzes each config's code independently against dimensions | Pairwise: LLM sees both baseline and skilled output together |
| **Bias mitigation** | None (independent scoring) | Position-swap: runs comparison twice with swapped order, checks consistency |
| **Judge modes** | Single mode (independent LLM analysis) | Three modes: `pairwise` (default), `independent`, `both` |
| **Overfitting detection** | ❌ | ✅ Classifies rubric items as outcome/technique/vocabulary; flags eval criteria that just test the skill's phrasing |

**Key insight:** skill-validator's pairwise judging with position-swap is methodologically stronger for direct A/B comparison. copilot-skill-eval's dimension-based analysis provides richer qualitative insight into *what specifically* improved.

---

## 4. Feature Comparison

### 4.1 Evaluation Pipeline

| Feature | copilot-skill-eval | skill-validator |
|---|---|---|
| **Code generation** | ✅ Generates full apps via Copilot CLI | ✅ Runs agent on tasks via Copilot SDK |
| **Multi-run support** | ✅ `--runs N` | ✅ `--runs N` (default: 5) |
| **Parallel execution** | ✅ Parallel across configs | ✅ Fine-grained: `--parallel-skills`, `--parallel-scenarios`, `--parallel-runs` |
| **Watchdog timeouts** | ✅ With network health checks | ✅ Per-scenario `timeout` |
| **Token tracking** | ✅ `generation-usage.json` | ✅ Per-run token estimates in metrics |
| **Session tracing** | ✅ Verifies which skills were loaded (path-based) | ✅ Skill activation detection via agent events |
| **Skill isolation** | ✅ Staging directory with symlinks/junctions | ✅ Isolated runs (skill only) + plugin runs (all skills) |
| **Rejudge capability** | ❌ Must re-run | ✅ `rejudge` command re-runs judging on saved sessions |
| **Consolidate results** | ❌ | ✅ `consolidate` merges results from CI matrix jobs |

### 4.2 Static Analysis

| Feature | copilot-skill-eval | skill-validator |
|---|---|---|
| **Config validation** | ✅ `validate-config` checks YAML + source resolution | ✅ `check` command — full static analysis |
| **Skill profiling** | ❌ | ✅ BPE token count, complexity tier, section/code-block counts |
| **Spec compliance** | ❌ | ✅ Validates against [agentskills.io](https://agentskills.io) specification |
| **Name validation** | ❌ | ✅ Format, length, directory match |
| **Description validation** | ❌ | ✅ Length limits, minimum quality |
| **External dependency scanning** | ❌ | ✅ `--allowed-external-deps` allowlist |
| **Reference scanning** | ❌ | ✅ `--known-domains` domain validation |
| **Asset size limits** | ❌ | ✅ 5 MB per bundled asset |
| **Research-backed thresholds** | ❌ | ✅ Based on [SkillsBench](https://arxiv.org/abs/2602.12670) (84 tasks, 7,308 trajectories) |

### 4.3 Assertions & Constraints

copilot-skill-eval verifies generated code by running build/format/security/run commands. skill-validator has a rich assertion system:

| Assertion Type | copilot-skill-eval | skill-validator |
|---|---|---|
| Build succeeds | ✅ Configurable build command | ⚠️ Via `run_command_and_assert` or setup commands |
| Output contains/matches | ❌ | ✅ `output_contains`, `output_matches`, negations |
| File exists/contains | ❌ | ✅ `file_exists`, `file_contains`, negations |
| Tool usage constraints | ❌ | ✅ `expect_tools`, `reject_tools` |
| Turn/token limits | ❌ | ✅ `max_turns`, `max_tokens` |
| Custom command assertions | ❌ | ✅ `run_command_and_assert` with exit code + output checks |
| Format checking | ✅ Dedicated format step | ❌ |
| Security scanning | ✅ Vulnerability scan step | ❌ |
| Health endpoint check | ✅ HTTP health check | ❌ |

### 4.4 Reporting & Output

| Output | copilot-skill-eval | skill-validator |
|---|---|---|
| **Console** | ✅ | ✅ Color-coded with ANSI |
| **Markdown report** | ✅ `reports/analysis.md` | ✅ `summary.md` + per-skill scenario reports |
| **JSON results** | ✅ `scores-data.json` | ✅ `results.json` with full schema |
| **JUnit XML** | ❌ | ✅ `results.xml` for CI integration |
| **Build notes** | ✅ `build-notes.md` (Roslyn warnings, etc.) | ❌ |
| **Aggregated scores** | ✅ Cross-run aggregation | ✅ Confidence intervals in results |
| **Investigation guide** | ❌ | ✅ `InvestigatingResults.md` designed for AI agents |
| **Per-run analysis** | ✅ `analysis-run-{N}.md` | ✅ Per-run metrics in `results.json` |

### 4.5 CI/CD Integration

| Feature | copilot-skill-eval | skill-validator |
|---|---|---|
| **Workflow generation** | ✅ `ci-setup` generates complete `.yml` | ❌ (but deeply integrated into dotnet/skills CI) |
| **Matrix job support** | ❌ | ✅ `consolidate` merges across matrix legs |
| **Nightly builds** | ❌ | ✅ Nightly releases as NuGet + tar.gz |
| **PR evaluation comments** | ❌ | ✅ Posts eval results as PR comments |
| **Verdict-as-gate** | ❌ | ✅ `--verdict-warn-only` for soft failures |
| **AOT-compiled binary** | ❌ | ✅ No .NET runtime needed for distribution |
| **dnx support** | ❌ | ✅ Run without installing via `dnx` |

### 4.6 Configuration

| Aspect | copilot-skill-eval | skill-validator |
|---|---|---|
| **Eval definition** | `eval.yaml` (project-wide) + scenario prompts | `eval.yaml` (per-skill in `tests/`) |
| **Skill sources** | `skill-sources.yaml` (git repos + local paths) | Skills discovered from directory structure |
| **Interactive setup** | ✅ `skill-eval init` wizard | ❌ |
| **Per-eval parallelism override** | ❌ | ✅ `config.max_parallel_scenarios/runs` |
| **Model selection** | ✅ `generation_model`, `analysis_model` | ✅ `--model`, `--judge-model` (can differ) |
| **Minimum threshold** | ❌ (relative scoring) | ✅ `--min-improvement` (default 10%) |

---

## 5. Architecture & Technology

| Aspect | copilot-skill-eval | skill-validator |
|---|---|---|
| **Language** | Python 3.10+ | C# (.NET 10) |
| **Dependencies** | PyYAML, Click, Jinja2, Pydantic | System.CommandLine, GitHub.Copilot.SDK, YamlDotNet, ML.Tokenizers, SQLite |
| **AI integration** | Copilot CLI (subprocess) | GitHub Copilot SDK (in-process) |
| **Install** | `pipx install` from git | NuGet tool package, AOT binary, or `dnx` |
| **Distribution** | Git clone + pip | NuGet, tar.gz, GitHub Release |
| **Tokenizer** | ❌ None | ✅ `cl100k_base` BPE tokenizer (Microsoft.ML.Tokenizers) |
| **Database** | ❌ | ✅ SQLite for session persistence |
| **Size** | ~15 Python source files | ~30 C# source files + test suite |
| **AOT compilation** | ❌ | ✅ `PublishAot` for all major platforms |

---

## 6. Evaluation Scenarios: Structure Comparison

### copilot-skill-eval scenario (app generation)

```yaml
scenarios:
  - name: Dashboard
    prompt: prompts/scenarios/dashboard.prompt.md   # Full app specification
    description: "Admin dashboard with charts and data tables"
```

The prompt is a detailed app spec (overview, tech stack, entities, business rules, endpoints). The agent generates an entire buildable application.

### skill-validator scenario (task evaluation)

```yaml
scenarios:
  - name: "Diagnose build failure"
    prompt: "Why does this project fail to build?"
    setup:
      copy_test_files: true
    assertions:
      - type: "output_matches"
        pattern: "CS\\d{4}"
    rubric:
      - "The agent correctly identified the root cause"
      - "The explanation was clear and actionable"
    timeout: 120
```

The prompt is a focused task. The agent works on pre-staged files and its output is checked against hard assertions + LLM rubric judging.

**Key difference:** copilot-skill-eval evaluates *what the skill helps build*. skill-validator evaluates *how the skill helps the agent work*.

---

## 7. Strengths & Weaknesses

### copilot-skill-eval

| Strengths | Weaknesses |
|---|---|
| Evaluates real-world output (full applications) | No pairwise judging (methodologically weaker for A/B) |
| Rich, user-defined quality dimensions with tiered weighting | No static analysis of skill content |
| Tech-stack agnostic (any language/framework) | No overfitting detection |
| Build/format/security/run verification pipeline | No hard assertion system for agent behavior |
| Interactive setup wizard | No bootstrap confidence intervals |
| Self-contained workflow generation for CI | Newer, less mature tooling |
| Comparison across skill *combinations* (not just single skills) | Python-only (no compiled distribution) |

### skill-validator

| Strengths | Weaknesses |
|---|---|
| Pairwise judging with position-swap bias mitigation | Evaluates task outputs, not full applications |
| Bootstrap confidence intervals + normalized gain | Configuration format tightly coupled to skill directory structure |
| Rich assertion system (file, output, tool, token, turn) | No interactive setup |
| Overfitting detection and auto-fix | No build/format/security verification pipeline |
| Static analysis with SkillsBench-backed thresholds | .NET-native tooling (barrier for non-.NET users) |
| BPE tokenization for accurate skill profiling | No user-defined quality dimensions |
| Deep CI integration (matrix consolidate, JUnit, PR comments) | No cross-skill-set comparison |
| AOT-compiled distribution (no runtime needed) | Fixed scoring weights (not user-configurable) |
| Session persistence (SQLite) + rejudge capability | Focused on individual skill value, not combinations |
| `InvestigatingResults.md` designed for AI agent consumption | Part of a larger repo, not independently installable |

---

## 8. Overlap & Differentiation

### Where they overlap

Both tools:
- Run AI agents with and without skills to measure impact
- Support multiple runs for reliability
- Produce structured reports (JSON + Markdown)
- Track token usage and execution metrics
- Integrate with CI/CD pipelines
- Use LLM-based quality judging

### Where they diverge

| Dimension | copilot-skill-eval | skill-validator |
|---|---|---|
| **Evaluation granularity** | Whole application | Individual task |
| **Scoring philosophy** | Multi-dimensional (user-defined) | Single weighted score (fixed formula) |
| **Judging approach** | Independent analysis per config | Pairwise comparison (A vs B) |
| **Statistical rigor** | Average across runs | Bootstrap CI + significance testing + normalized gain |
| **Skill analysis** | None (skills are opaque inputs) | Static profiling + spec validation + overfitting detection |
| **Skill combinations** | ✅ Compare arbitrary skill sets | ❌ Evaluate individual skills only |
| **Verification** | Build + format + security + health check | Assertions + tool/turn/token constraints |
| **Distribution** | Python package from git | NuGet + AOT binary + dnx |

---

## 9. What Each Tool Could Learn From the Other

### copilot-skill-eval could adopt from skill-validator

1. **Pairwise judging with position-swap bias mitigation** — more reliable than independent scoring for A/B comparison
2. **Overfitting detection** — catch eval criteria that merely test skill phrasing rather than genuine improvement
3. **Static skill profiling** — BPE token counts, complexity tiers, SkillsBench thresholds would help skill authors optimize before running expensive evaluations
4. **Bootstrap confidence intervals** — replace simple averaging with proper significance testing
5. **Hard assertions** — complement LLM judging with deterministic checks (output_contains, file_exists, etc.)
6. **Rejudge capability** — avoid re-running expensive generation when only the analysis criteria change
7. **JUnit output** — standard CI test format for broader integration
8. **Normalized gain** — controls for ceiling effects (improving an already-good baseline is harder)

### skill-validator could adopt from copilot-skill-eval

1. **Full application generation + verification** — evaluate skills at the application level, not just task level
2. **User-defined quality dimensions with tiers** — let users define what "better" means for their domain
3. **Build/format/security verification pipeline** — verify that the agent's output actually compiles and passes checks
4. **Interactive setup wizard** — lower the barrier to writing eval files
5. **Skill combination testing** — evaluate how skills interact when used together
6. **Tech-stack agnostic verification** — configurable build/run commands instead of hardcoded patterns
7. **Workflow generation** — `ci-setup` command for easy GitHub Actions integration

---

## 10. Key Takeaways

1. **Different evaluation granularity.** copilot-skill-eval evaluates at the *application* level ("did the skill produce a better app?"); skill-validator evaluates at the *task* level ("did the skill help the agent complete this task better?"). Both are valid but answer different questions.

2. **skill-validator is methodologically stronger** for A/B testing, with pairwise judging, position-swap bias mitigation, bootstrap confidence intervals, overfitting detection, and normalized gain. These are well-established evaluation techniques backed by academic research (SkillsBench).

3. **copilot-skill-eval provides richer qualitative insight** through user-defined multi-dimensional analysis. You learn not just *if* a skill helps, but *what specifically* it improves (architecture, error handling, security, etc.).

4. **copilot-skill-eval offers broader reach** — it works with any tech stack and any skill combination. skill-validator is deeply integrated into the dotnet/skills ecosystem and .NET toolchain.

5. **Complementary approaches.** The ideal evaluation combines both: skill-validator's rigorous A/B methodology for pass/fail gating in CI, plus copilot-skill-eval's dimensional analysis for understanding *why* a skill helps and guiding iterative improvement.

6. **Maturity gap.** skill-validator is a production tool used in the official dotnet/skills CI pipeline with AOT distribution, NuGet packaging, nightly builds, and an extensive test suite. copilot-skill-eval is earlier-stage with more room for adoption and ecosystem growth.
