# Stage 1 — Deterministic & Rule-Based Evals

**Status:** in progress · branch `feature/stage-01-deterministic-evals`

Subject: **PatchProse** — git diff → conventional commit message + PR description.

Written at the *end* of the stage, not the start. Keep it concise.

---

## What is this concept?

<!-- Rules vs. reference metrics, in my own words. -->

## What problem does it solve?

## When should I use it?

## When should I NOT use it?

## What failure did I observe?

<!-- The checkpoint case: high metric score, wrong description. -->

## What did the experiment teach me?

---

## Stage completion checklist

### Build

- [x] Required feature works — CLI runs end to end against a real repo
- [x] Tests pass — 3/3 green
- [x] Evaluation dataset exists — `evals/datasets/patchprose-stage-01.json` has 8 seed cases with reference descriptions
- [ ] Experiment can be reproduced
- [ ] Baseline saved — `evals/baselines/`
- [x] At least one failure case reproduced — CRLF harness bug (Experiment 0)
- [ ] Reference metrics implemented — hand-rolled exact match + token F1
- [ ] Cross-checked against `Microsoft.Extensions.AI.Evaluation.NLP`

### Learn

- [ ] Questions documented
- [ ] Relevant sources studied
- [ ] I can explain the concept without notes
- [ ] I understand when to use it
- [ ] I understand when NOT to use it
- [ ] I understand why the failure occurred
- [ ] **Checkpoint:** I can show a PatchProse example where BLEU says "good" but the
      description is wrong

### Write

- [ ] Interesting experiment identified
- [ ] Article scratchpad updated — `articles/01-cheap-evals-first/scratch.md`
- [ ] Before/after evidence captured
- [ ] Article outline written
- [ ] Relevant code snippets selected
- [ ] Draft completed

### Finish

- [ ] Stage branch merged
- [ ] Tagged `stage-01-complete`
- [ ] Next problem/question identified

