# Source Notes — Stage 1

Only what answers a question already raised in `questions.md` or `experiment-log.md`.
No paper summaries. If a note does not connect to something observed, it does not belong here.

Stage 1 reading is **reference-only, ~15 min total** — grab the definition and one worked
example. Do not read these linearly.

- Papineni et al., "BLEU: a Method for Automatic Evaluation of Machine Translation" (2002)
- Lin, "ROUGE: A Package for Automatic Evaluation of Summaries" (2004)
- Hugging Face `evaluate` metric docs — the same metrics in code
- `Microsoft.Extensions.AI.Evaluation.NLP` docs — BLEU / GLEU / F1 in .NET

---

## Token-overlap F1

<!-- Definition + one worked example. Which question does this answer? -->

## BLEU

Microsoft.Extensions.AI.Evaluation.NLP includes a `BLEUEvaluator`. Microsoft describes BLEU
as comparing a response to one or more references using the bilingual evaluation understudy
algorithm. The score is a text-similarity signal, not proof of correctness.

## ROUGE

<!-- Definition + one worked example. How does it differ from BLEU? -->

## Microsoft.Extensions.AI.Evaluation.NLP

The NLP package contains non-LLM evaluators. Microsoft's docs list `F1Evaluator`,
`BLEUEvaluator`, and `GLEUEvaluator`; these compare generated response text to reference
responses using tokenization or n-gram overlap rather than another model.

In this repo, the package resolved to `Microsoft.Extensions.AI.Evaluation.NLP`
`10.9.0-preview.1.26411.16`, so the API is still prerelease.

The useful lesson from the first cross-check: my hand-rolled token F1 and Microsoft's F1 are
close but not identical. That is expected because implementations can tokenize and normalize
text differently. The important thing is understanding what the metric measures, not forcing
two libraries to produce the exact same decimal.
