# Plan: Session Tracing — Track Skills/Plugins/Instructions Per Run

## Problem

The skill-eval framework currently configures which skills and plugins should be available
to each Copilot invocation (via staging directories and `skill_manager.py`), but it never
verifies what Copilot **actually loaded**. There is no post-hoc validation, no session ID
tracking, and no way to detect if a run used the wrong set of assets. This undermines
confidence in evaluation results.

The Copilot CLI writes rich session telemetry to `~/.copilot/session-state/{id}/events.jsonl`.
Each JSONL file contains:

- `session.start` — session ID, model, working directory, copilot version
- `skill.invoked` — every skill that was loaded, with name, path, pluginName, description,
  and full content
- `user.message` — the prompt sent (including `transformedContent` with injected metadata)

A reference parser already exists at `C:\data\mycode\prompt-tracer\_main` (the `prompt-tracer`
CLI tool) — we should follow its patterns.

## Proposed Approach

Add a **session tracing** capability that runs after each Copilot invocation to parse the
events.jsonl, record which assets Copilot loaded, store the session ID, and flag mismatches
between expected and actual configurations.

---

## Todos

### 1. Create `session_tracer.py` module

**File:** `src/skill_eval/session_tracer.py`

A self-contained module (no dependency on prompt-tracer) that:

- **Finds the events.jsonl** for a given session. The generate step should set
  `COPILOT_HOME` to an isolated temp directory so the events.jsonl path is predictable:
  `{copilot_home}/session-state/{session_id}/events.jsonl`. If `COPILOT_HOME` is not
  isolated, fall back to finding the most-recently-modified events.jsonl under
  `~/.copilot/session-state/`.

- **Parses these event types** (reference: `prompt-tracer/src/prompt_tracer/providers/copilot.py`):
  - `session.start` → extract `sessionId`, `selectedModel`, `startTime`, `copilotVersion`,
    `context.cwd`
  - `skill.invoked` → extract `name`, `path`, `pluginName`, `description`,
    `len(content)` (do not store full content — it can be huge)
  - `user.message` (first only) → extract `content` (truncated to 200 chars) as the prompt

- **Returns a `SessionTrace` dataclass:**

  ```python
  @dataclass
  class LoadedResource:
      resource_type: str          # "skill"
      name: str
      path: str | None = None
      plugin_name: str | None = None
      description: str | None = None
      content_length: int = 0
      timestamp: str | None = None

  @dataclass
  class SessionTrace:
      session_id: str | None = None
      model: str | None = None
      copilot_version: str | None = None
      start_time: str | None = None
      cwd: str | None = None
      prompt: str | None = None       # first user message, truncated
      resources: list[LoadedResource] = field(default_factory=list)
      total_events: int = 0
      events_file: str | None = None  # path to the events.jsonl parsed
  ```

- **Provides a `trace_session(copilot_home: Path | None = None) -> SessionTrace | None`
  function** that finds the most recent events.jsonl and parses it.

- **Provides a `compare_resources(trace: SessionTrace, expected_skills: list[str],
  expected_plugins: list[str]) -> dict`** that returns:
  ```python
  {
      "expected_skills": ["dotnet-webapi"],
      "actual_skills": ["dotnet-webapi", "dotnet-csharp"],
      "unexpected": ["dotnet-csharp"],       # loaded but not expected
      "missing": [],                          # expected but not loaded
      "expected_plugins": ["dotnet-artisan"],
      "actual_plugins": ["dotnet-artisan"],
      "match": False,                         # True only if exact match
  }
  ```

### 2. Isolate `COPILOT_HOME` during generation

**File:** `src/skill_eval/generate.py`

Currently, each Copilot invocation writes to the user's global `~/.copilot/session-state/`.
This makes it hard to find the right events.jsonl, especially if multiple sessions run
concurrently.

Changes:
- In `_run_copilot()`, create a temporary directory for `COPILOT_HOME` and pass it via
  environment variable: `env["COPILOT_HOME"] = str(temp_copilot_home)`
- After the process completes, find the events.jsonl in the isolated home
- Pass `copilot_home` to the new `trace_session()` function
- **Preserve the events.jsonl**: copy it to the run output directory
  (`output/{config}/run-{N}/events.jsonl`) for future reference

Verify that setting `COPILOT_HOME` doesn't break skill discovery. The staging directory
approach with `cwd` should still work. If `COPILOT_HOME` interferes with skill resolution,
fall back to using the default home and finding the most-recently-modified session.

### 3. Record session trace data in generation output

**File:** `src/skill_eval/generate.py`

After each Copilot invocation, call `trace_session()` and include the results in the
usage dict that gets written to `generation-usage.json`:

```python
usage = _run_copilot(prompt, cfg, cwd=staging_dir, ...)
trace = trace_session(copilot_home=temp_copilot_home)
if trace:
    usage["session_id"] = trace.session_id
    usage["model"] = trace.model
    usage["copilot_version"] = trace.copilot_version
    usage["loaded_resources"] = [asdict(r) for r in trace.resources]
    usage["resource_comparison"] = compare_resources(
        trace,
        expected_skills=[Path(s).name for s in cfg.skills],
        expected_plugins=[Path(p).name for p in cfg.plugins],
    )
```

Also print a summary to the console during generation:
```
    📋 Session: 05594136-1e08-41d5-9f02-e7fb825e168a
    📋 Skills loaded: dotnet-webapi, dotnet-csharp (2)
    ⚠️  Unexpected skill: dotnet-csharp
```

### 4. Add asset usage summary to build-notes.md

**File:** `src/skill_eval/verify.py`

Add a new section **"Asset Usage Per Run"** to the build notes report. Read the
`generation-usage.json` (if it exists) and render:

```markdown
## Asset Usage Per Run

| Configuration | Run | Session ID | Model | Skills Loaded | Plugins | Match? |
|---|---|---|---|---|---|---|
| dotnet-webapi | 1 | 0559...168a | claude-sonnet-4.6 | dotnet-webapi | — | ✅ |
| dotnet-artisan | 1 | c794...4452 | claude-sonnet-4.6 | using-dotnet, dotnet-advisor, dotnet-webapi | dotnet-artisan | ✅ |
| no-skills | 1 | a100...da07 | claude-sonnet-4.6 | — | — | ✅ |
```

If any run has a mismatch, add a **"⚠️ Asset Mismatches"** subsection:

```markdown
### ⚠️ Asset Mismatches

| Configuration | Run | Issue |
|---|---|---|
| no-skills | 2 | Unexpected skill loaded: dotnet-csharp |
```

### 5. Add asset usage section to analysis.md

**File:** `src/skill_eval/aggregator.py`

Add an **"Asset Usage Summary"** section to the aggregated analysis report (after the
Verification Summary). This section should:

- List which skills/plugins each configuration actually loaded across runs
- Flag any inconsistencies (e.g., a skill loaded in run 1 but not run 2)
- Highlight any runs where the wrong assets were used
- Show session IDs for each run (clickable reference for debugging)

### 6. Preserve events.jsonl in run output

**File:** `src/skill_eval/generate.py`

After parsing, copy the events.jsonl to the run output directory:
`output/{config}/run-{N}/events.jsonl`

This allows re-analysis later without needing the original session state directory
(which may be cleaned up or overwritten).

### 7. Add session trace data to verification-data.json

**File:** `src/skill_eval/verify.py`

Extend the verification JSON output to include session trace data from
`generation-usage.json`:

```json
{
  "config": "dotnet-artisan",
  "run_id": 1,
  "scenario": "FitnessStudioApi",
  "build_success": true,
  "session_id": "05594136-1e08-41d5-9f02-e7fb825e168a",
  "model": "claude-sonnet-4.6",
  "loaded_skills": ["using-dotnet", "dotnet-advisor", "dotnet-webapi"],
  "loaded_plugins": ["dotnet-artisan"],
  "asset_match": true
}
```

---

## Implementation Order

1. **`session_tracer.py`** — new module, no existing code changes (lowest risk)
2. **`generate.py`** — integrate tracing after Copilot invocation, isolate COPILOT_HOME
3. **`verify.py`** — add asset usage section to build-notes.md, extend verification JSON
4. **`aggregator.py`** — add asset usage summary to analysis.md

## Key Design Decisions

- **Self-contained**: No dependency on prompt-tracer package. The parsing logic is simple
  enough (~60 lines) to inline.
- **Graceful degradation**: If events.jsonl is missing or unparseable, tracing returns
  `None` and the pipeline continues normally — all tracing is optional/additive.
- **COPILOT_HOME isolation**: Preferred approach for reliable session identification.
  If it breaks skill resolution, fall back to timestamp-based most-recent-session detection.
- **Events.jsonl preservation**: Copying to output dir ensures reproducibility even after
  session state cleanup.

## Reference: prompt-tracer event parsing

The key patterns from `C:\data\mycode\prompt-tracer\_main\src\prompt_tracer\providers\copilot.py`:

```python
_RESOURCE_EVENT_TYPES = frozenset({"skill.invoked"})
_META_EVENT_TYPES = frozenset({
    "session.start", "session.model_change", "user.message",
})

# Detection: first event must be session.start with producer=copilot-agent
# skill.invoked fields: name, path, pluginName, description, content
# session.start fields: sessionId, selectedModel, startTime, copilotVersion, context
```
