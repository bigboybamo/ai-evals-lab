# Stage 1 — Deterministic & Rule-Based Evals

**Status:** in progress · branch `feature/stage-01-deterministic-evals`

Subject: **PatchProse** — git diff → conventional commit message + PR description.

Written at the *end* of the stage, not the start. Keep it concise.

---

## What is this concept?

Deterministic evals are repeatable checks that do not ask another model to judge the
answer.

In PatchProse, I used two kinds:

- rule-based checks: conventional commit format, issue references, and files touched
- reference metrics: exact match, token-overlap F1, Microsoft F1, BLEU, and GLEU

Rules check whether the output obeys a concrete contract. Reference metrics compare the
generated PR description to a human-written reference description.

## What problem does it solve?

PatchProse generates text from a git diff, so I need a cheap way to catch obvious failures
before reaching for LLM-as-judge.

These checks answer questions like:

- Did the commit message follow conventional commit format?
- Did PatchProse preserve issue references from the diff?
- Did it list the files that actually changed?
- How much does the generated description overlap with my reference description?

The first baseline over 9 cases produced:

```text
Conventional commits: 9/9
Issue references matched: 8/9
Files touched matched: 8/9
Exact matches: 0/9
Average token F1: 0.4479
Average Microsoft F1: 0.4378
Average Microsoft BLEU: 0.1706
Average Microsoft GLEU: 0.2045
```

## When should I use it?

Use deterministic evals when the requirement can be checked from the input and output
without interpretation.

Good fits:

- fixed output formats
- required IDs or issue references
- file lists that must match the diff
- exact labels
- simple reference comparisons
- fast regression checks in tests or CI

They are the right first layer because they are cheap, fast, repeatable, and do not depend
on an API key.

## When should I NOT use it?

Do not treat deterministic evals as proof that generated prose is correct.

They are weak when the question requires meaning, judgment, or context:

- Did the PR description mention the most important risk?
- Did the output understand a contradiction in the diff?
- Is the implementation behavior actually correct?
- Is this a good summary, or just a structurally valid one?
- Are two differently worded descriptions semantically equivalent?

Exact match is especially poor for PatchProse descriptions because good summaries can be
phrased many ways. BLEU and GLEU are stricter than token F1 and can be harsh on valid
paraphrases, but they can still be fooled by near-identical wrong text.

## What failure did I observe?

The strongest failure is `high-overlap-wrong-meaning-009`.

Generated:

```text
Sets location to null when coffee is mentioned without a named location.
```

Reference:

```text
Sets location to Starbucks when coffee is mentioned without a named location.
```

The wording is almost identical, but the meaning is wrong. One token changes the behavior
that matters.

The metrics still scored it highly:

```text
Token F1: 0.9444
Microsoft F1: 0.9583
Microsoft BLEU: 0.8844
Microsoft GLEU: 0.8889
```

That satisfies the checkpoint: BLEU says the text is very similar, but the description is
wrong.

## What did the experiment teach me?

Cheap evals are necessary but not sufficient.

Rules caught concrete failures:

- `missing-issue-reference-007` omitted `ORD-88`
- `hallucinated-file-008` listed `src/Users/RolePermissions.cs`, which was not in the diff

Reference metrics gave useful similarity numbers, but they did not prove correctness. The
Microsoft NLP cross-check showed my hand-rolled F1 and Microsoft's F1 are close but not
identical, which is expected because tokenization details differ.

The real lesson is that every metric answers a narrow question:

- regex answers "does this match the format?"
- issue matching answers "were issue IDs preserved?"
- file matching answers "did the file list match the diff?"
- F1/BLEU/GLEU answer "how much does this overlap with a reference?"

None of them answer "is this semantically correct?" That gap is the reason to move toward
LLM-as-judge in Stage 2.

---

## Stage completion checklist

### Build

- [x] Required feature works — CLI runs end to end against a real repo
- [x] Tests pass — 3/3 green
- [x] Evaluation dataset exists — `evals/datasets/patchprose-stage-01.json` has 9 seed cases with reference descriptions
- [x] Experiment can be reproduced — run `dotnet run --project src/PatchProse/PatchProse.Evals`
- [x] Baseline saved — `evals/baselines/patchprose-stage-01-baseline.json`
- [x] At least one failure case reproduced — CRLF harness bug (Experiment 0)
- [x] Reference metrics implemented — hand-rolled exact match + token F1
- [x] Cross-checked against `Microsoft.Extensions.AI.Evaluation.NLP`

### Learn

- [x] Questions documented
- [x] Relevant sources studied
- [x] I can explain the concept without notes
- [x] I understand when to use it
- [x] I understand when NOT to use it
- [x] I understand why the failure occurred
- [x] **Checkpoint:** I can show a PatchProse example where BLEU says "good" but the
      description is wrong

### Write

- [x] Interesting experiment identified — `high-overlap-wrong-meaning-009`
- [ ] Article scratchpad updated — `articles/01-cheap-evals-first/scratch.md`
- [x] Before/after evidence captured — dataset + baseline + experiment log
- [ ] Article outline written
- [ ] Relevant code snippets selected
- [ ] Draft completed

### Finish

- [ ] Stage branch merged
- [ ] Tagged `stage-01-complete`
- [ ] Next problem/question identified

