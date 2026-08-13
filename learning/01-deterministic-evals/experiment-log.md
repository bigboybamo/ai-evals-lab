# Experiment Log — Stage 1

Raw material for the article. Record what happened, not what should have happened.

---

# Experiment 0 — A red eval caused by the harness, not the model

**Date:** 2026-08-13

## Setup

Three rule-based checks in `PatchProseChecks`, asserted in NUnit against a hand-written
`PatchProseOutput`. The LLM was not involved at all.

## Result

`Files_touched_match_the_diff` failed. `CompareFilesTouched` reported both files as
`UnexpectedFiles` and none as expected.

## Cause

`GitDiffParser.DiffHeaderRegex` was `^diff --git (?<left>\S+) (?<right>\S+)$`.

The test file is saved CRLF, and C# raw string literals preserve the source file's line
endings, so the fixture contained `\r\n`. `\S` does not match `\r`, and `$` in multiline mode
only asserts before `\n`. Verified against the .NET regex engine directly:

```
CRLF matches: 0
LF   matches: 1
```

So `ExtractTouchedFiles` returned an empty set, and every file in the description looked
unexpected.

## Fix

```csharp
[GeneratedRegex(@"^diff --git (?<left>\S+) (?<right>\S+)\r?$", RegexOptions.Multiline)]
```

Suite green: 3 passed, 0 failed.

## Observation

Real `git diff` read through `Process.StandardOutput` emits LF, so production input would
have passed. Only the hand-written fixture was CRLF — the test data did not faithfully
represent the real input.

The failure looked exactly like a model bug and was entirely a harness bug. Nothing was
being evaluated at the time.

## Question

How do I tell a genuine subject failure from a broken evaluator or a bad golden label,
without reading the source every time?

## Article Use

Strong opening example. A red eval is not automatically evidence about the model — Stage 8's
"your dataset is the ceiling" arriving early and by accident.

---

# Experiment 1 — Prompt rule contradiction passes cheap checks

**Date:** 2026-08-13

## Input

A prompt diff changed `prompts/extract_event.txt` by adding this rule:

```text
For this demo, if the message mentions coffee but does not name a location, set location to "Starbucks".
```

The existing prompt already said `location` must be null when no location is named, even if the activity hints at one such as coffee.

Full case saved in `evals/datasets/patchprose-stage-01.json` as `prompt-rule-contradiction-001`.

## Generated Output

```text
docs(prompts): update extract_event.txt with demo-specific location rule

Added a demo-specific rule to the extract_event.txt prompt to set location to "Starbucks" if the message mentions coffee but does not specify a location.

Files touched:
- prompts/extract_event.txt
```

## Reference Description

PatchProse accurately summarized the added rule and listed the right file. A stronger description would also mention that the new demo rule conflicts with the existing instruction that `location` must remain null when no explicit location is named.

## Rule Check Results

- Conventional commit: pass
- Issue number: not applicable; the diff contains no issue reference
- Files touched: pass; `prompts/extract_event.txt` matches the diff

## Observation

The deterministic checks pass even though the diff introduces a contradiction in the prompt behavior. These checks verify structure and traceable facts, but they do not understand whether the change is logically safe or internally consistent.

## Question

How do I evaluate a PR description that is factually accurate but misses the most important risk in the diff?

## Article Use

This is the first real "cheap evals are necessary but not sufficient" example. It shows where deterministic checks stop and why Stage 2 needs an LLM judge or a stronger project-specific evaluator.
