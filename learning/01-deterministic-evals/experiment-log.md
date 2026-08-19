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

---

# Experiment 2 — Reference metrics over PatchProse descriptions

**Date:** 2026-08-17

## Setup

Added hand-rolled reference metrics for PR descriptions:

- exact match: the generated description must exactly match the reference after trimming
- token-overlap F1: lowercase word tokens are compared with duplicate counts

The metrics run against the 8 cases in `evals/datasets/patchprose-stage-01.json`.

## Result

Exact match is false for every current case. This is expected: PatchProse can produce a valid description that is not word-for-word identical to my reference.

Token-overlap F1 gives partial credit when generated and reference descriptions share words. For example, `prompt-rule-contradiction-001` scores `0.4194` even though the generated output misses the contradiction risk.

## Observation

Reference metrics measure similarity to one reference answer. They do not prove the generated description is correct, complete, or safe.

Exact match is too strict for prose. Token-overlap F1 is less brittle, but it rewards shared words rather than meaning.

## Question

What score threshold would be meaningful for PatchProse, if many good descriptions can be phrased differently?

## Article Use

This supports the Stage 1 checkpoint: deterministic metrics can be cheap and repeatable while still failing to capture semantic correctness.

---

# Experiment 3 — First deterministic baseline

**Date:** 2026-08-17

## Setup

Added `PatchProse.Evals`, a small console runner that loads `evals/datasets/patchprose-stage-01.json`, runs the deterministic checks and reference metrics, then writes `evals/baselines/patchprose-stage-01-baseline.json`.

Command:

```powershell
dotnet run --project src\PatchProse\PatchProse.Evals\PatchProse.Evals.csproj
```

## Result

```text
Cases: 8
Conventional commits: 8/8
Issue references matched: 7/8
Files touched matched: 7/8
Exact matches: 0/8
Average token F1: 0.3859
```

## Observation

The baseline gives a fixed point for future changes. If a prompt, parser, metric, or dataset edit changes these numbers, I can compare against this file instead of relying on memory.

The `0/8` exact-match count is useful evidence: exact match is too strict for PatchProse descriptions because good prose can be phrased many ways.

## Question

What baseline movement should count as an improvement, and what movement should count as a regression?

## Article Use

Use this as the first evidence table: cheap checks catch concrete structural failures, while reference metrics expose how brittle text matching is for prose.

---

# Experiment 4 — Cross-check with Microsoft.Extensions.AI.Evaluation.NLP

**Date:** 2026-08-19

## Setup

Added `Microsoft.Extensions.AI.Evaluation.NLP` to `PatchProse.Evals` and calculated Microsoft
F1, BLEU, and GLEU for each generated PR description against its reference description.

The package resolved to prerelease version `10.9.0-preview.1.26411.16`.

## Result

```text
Cases: 8
Average token F1: 0.3859
Average Microsoft F1: 0.3728
Average Microsoft BLEU: 0.0814
Average Microsoft GLEU: 0.1190
```

## Observation

Microsoft's F1 and the hand-rolled token F1 are close but not identical. That is acceptable:
the cross-check shows they are measuring the same general idea, while also proving that exact
implementation details such as tokenization matter.

BLEU and GLEU are much lower on these short PR descriptions. That makes them poor headline
metrics for this stage, but useful evidence that different reference metrics can disagree
even on the same generated/reference pair.

## Question

If reference metrics disagree, which one should drive the baseline threshold, and should any
of them gate PatchProse prose quality?

## Article Use

This is the practical "do not worship the number" example. A metric score is a measurement
choice, not a universal truth about output quality.

---

# Experiment 5 — High-overlap wrong-meaning checkpoint case

**Date:** 2026-08-19

## Setup

Added `high-overlap-wrong-meaning-009`, a synthetic case where the generated description
differs from the reference by one critical token:

```text
Generated: Sets location to null when coffee is mentioned without a named location.
Reference: Sets location to Starbucks when coffee is mentioned without a named location.
```

Both descriptions include the same `Files touched:` section.

## Result

```text
Token F1: 0.9444
Microsoft F1: 0.9583
Microsoft BLEU: 0.8844
Microsoft GLEU: 0.8889
```

All cheap structural checks pass:

```text
Conventional commit: pass
Issue references: pass
Files touched: pass
```

## Observation

This case satisfies the Stage 1 checkpoint. BLEU, GLEU, and token F1 all say the generated
description is highly similar to the reference, but the meaning is wrong. The single changed
token reverses the behavior that matters.

## Question

How much semantic risk can hide behind a high overlap score?

## Article Use

Use this as the core Stage 1 failure example: text overlap can look excellent while the
description is still wrong.
