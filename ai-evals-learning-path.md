# AI Evals — A Build-First Learning Path (concepts general, built in .NET)

Concept first, then a small thing to build, then a checkpoint that tells you it clicked.
The **Learn (general)** line points at the tool-agnostic source for the *idea*. The
**Build** line proves it in .NET. Each stage assumes the one before it.

**How to read the sources (build-first).** Do the Build with just the roadmap's concept
paragraph; open the source only when you hit a wall or a surprising result. A paper read to
answer a question you already have sticks; one read cold to "prepare" evaporates. Each
**Learn** line is tagged with how deep to go and a time-box — if you're over the box, you're
reading too deep for a build-first path:

- *Skim* — abstract, intro's contribution list, headings, figures/tables. Mining for the map, not reading linearly.
- *Core sections* — abstract → intro → method → the results that document a failure mode you'll hit. Skip related-work and appendices. Only the two anchor papers (Zheng et al., RAGAS) get this.
- *Reference-only* — grab the definition + one worked example (Wikipedia / Hugging Face docs are fine); don't "read" the paper.

## Running projects (only two, both real)

- **PatchProse** *(the spine)* — a small tool that takes a git diff and produces a
  conventional-commit message + PR description. It carries almost every stage because one
  tiny tool gives you all three of: checkable structure (deterministic), a classifiable
  commit *type* (ML.NET ground truth), and fuzzy prose (judge). You'll actually use it.
- **CiteFirst** *(a focused expansion, not a parallel track)* — a RAG assistant over a
  library's docs. It enters **only at Stage 7**, purely to unlock the RAG eval family
  (retrieval, groundedness, faithfulness) that PatchProse can't exercise. Do **not** build
  it until you reach Stage 7 — building it early is how it becomes a half-done project.

Your existing **JobSearchBuilder** (LLM-as-judge + Promptfoo) stays as-is — it's your
shipped proof, not a construction site. The learning happens in PatchProse/CiteFirst.

> Resources are named (title + authors) so you can search them, not linked, to avoid stale
> URLs. Verify current package APIs against `dotnet/ai-samples` before relying on them.

Tooling you'll accumulate: C# + an LLM client → `Microsoft.Extensions.AI` →
`Microsoft.Extensions.AI.Evaluation` (Quality, NLP, Safety) → `ML.NET` for the classic-ML
contrast. Exercises run as xUnit/NUnit tests so evals live with your other tests.

---

## Map of eval approaches (keep this in view)

Every stage is really "which of these am I using, and why." No single approach wins; real
suites combine them.

| Approach | What it is | Needs ground truth? | Cost | Catches | Reach for it when |
|----------|-----------|:-------------------:|:----:|---------|-------------------|
| Rule / assertion | Structural checks: valid JSON, required field, length | No | ~0 | Format & contract bugs | Always — cheapest first line |
| Deterministic metric | Compare to reference: exact, regex, BLEU/ROUGE/F1 | Yes | Low | Surface divergence from a known answer | There's one right answer |
| Classification metric | Precision/recall/F1/confusion matrix on labels | Yes | Low | Wrong category on a fixed label set | Task is really classification |
| LLM-as-judge | 2nd model grades against a rubric | No (rubric) | High | *Meaning* bugs, no single right answer | Many answers are all valid |
| Human eval | People judge directly | Is the truth | Highest | Everything — it's the reference | Calibrating everything above |

---

## Stage 0 — The mental model

**Concept.** An eval is a repeatable measurement of output quality against defined
criteria. Three axes: deterministic vs. model-graded; reference-based vs. reference-free;
pointwise vs. pairwise. Four building blocks: dataset → subject → evaluator(s) →
aggregate + threshold.

**Learn (general).** *[Skim · 30 min]* "A Survey on Evaluation of Large Language Models" (Chang et al., 2023);
the Hugging Face `evaluate` library docs for how metrics get packaged.

**Build.** Nothing yet. Write the three axes, four blocks, and the five-row map above on an
index card.

**Checkpoint.** You can place your JobSearchBuilder eval on the grid instantly
(model-graded, reference-free, pointwise) and name which map rows it does *not* use.

---

## Stage 1 — Deterministic & rule-based evals (PatchProse)

**Concept.** The cheapest evals need no LLM: rule/assertion checks (valid format, required
fields) and reference metrics (exact, regex, BLEU/GLEU/ROUGE/F1). Fast, zero variance,
blind to meaning.

**Learn (general).** *[Reference-only · 15 min]* Papineni et al., "BLEU" (2002) and Lin, "ROUGE" (2004) — read once.
Hugging Face `evaluate` metric docs for the same in code.

**Build.** Stand up **PatchProse**. Add non-LLM checks on its output: is it valid
conventional-commit format (regex), does it reference the right issue number, do the
"files touched" match the diff. Then score the description against reference descriptions —
hand-roll exact-match + token-overlap F1, then swap in
`Microsoft.Extensions.AI.Evaluation.NLP` (BLEU/GLEU/F1) and confirm the numbers agree.

**Checkpoint.** You can give a PatchProse example where BLEU says "good" but the
description is wrong — i.e. why string-similarity ≠ correctness. That gap justifies Stage 2.

---

## Stage 2 — Model-graded, reference-free evals (LLM-as-judge)

**Concept.** When many outputs are valid, a second model grades against a plain-English
rubric instead of matching one golden string.

**Learn (general).** *[Core sections · 60 min — anchor paper]* The canonical paper: Zheng et al., "Judging LLM-as-a-Judge with
MT-Bench and Chatbot Arena" (2023). Read it properly — Stages 4–5 lean on it directly.

**Build.** Judge PatchProse's *PR description* prose: a golden set of ~8 diffs, and a
hand-rolled judge (`IChatClient` call taking diff+description+rubric → PASS/FAIL+reasoning).
Rubric targets: accurately summarizes the diff, invents no changes, right tone. Assert in
xUnit.

**Checkpoint.** You can flip the same description PASS→FAIL by tightening only the rubric —
e.g. "must not claim changes not present in the diff."

---

## Stage 3 — Formalize with Microsoft.Extensions.AI.Evaluation

**Concept.** Every framework is the same four blocks. Learn to see them in a real library:
built-in quality evaluators (Relevance, Truth, Completeness, Coherence, Groundedness,
Equivalence, Retrieval), reporting, and response caching.

**Learn (general).** *[Skim · 20 min]* For the tool-agnostic anatomy, skim the Promptfoo docs and the OpenAI
`evals` repo README — different tools, identical four blocks. Then the Microsoft Learn
"Microsoft.Extensions.AI.Evaluation libraries" page.

**Build.** Re-implement your Stage 2 PatchProse judge with the library; generate the HTML
report via the `dotnet aieval` console tool; write **one custom evaluator** (a C# class)
for the "no invented changes" rule the built-ins don't cover.

**Checkpoint.** You can point at all four building blocks in your own code.

---

## Stage 4 — Judge calibration

**Concept.** Before trusting a judge, prove it agrees with you: feed it pre-labeled
good/bad answers and measure agreement. An uncalibrated judge is a confident RNG.

**Learn (general).** *[Reference-only · 15 min]* Cohen's kappa / inter-annotator agreement (the Wikipedia page suffices);
the human-agreement methodology in the Zheng et al. paper is the applied version.

**Build.** Hand-label ~20 PatchProse descriptions (10 good, 10 bad), run them through the
judge, compute agreement (accuracy, then Cohen's kappa). Improve the rubric; watch agreement
climb.

**Checkpoint.** You have a number ("agrees 90% of the time") and can name the disagreements.

---

## Stage 5 — Judge biases & pairwise evaluation

**Concept.** Judges tilt: position bias (first answer wins), verbosity bias (longer wins),
self-preference (own style wins). Pairwise grading + randomized order mitigates them, and
"better than X" is an easier call than "good in the abstract."

**Learn (general).** *[Revisit · 15 min — reread only the bias section]* Same Zheng et al. (2023) paper — it names and quantifies position,
verbosity, and self-enhancement bias. Search "LLM-as-a-judge position bias" for follow-ups.

**Build.** Two prompt versions (or two models) for PatchProse; a pairwise judge shown both
descriptions picking a winner. Run each pair twice with order swapped; count verdict flips
from ordering alone; fix by randomizing + averaging. (This is also how you'd decide which
prompt version to ship — benchmarking as a *tool*, not a second specialty.)

**Checkpoint.** You measured your judge's position bias as a %, and your fix reduced it.

---

## Stage 6 — Variance, flakiness & caching

**Concept.** Same input can score differently across runs. Levers: temperature 0, multiple
samples + majority vote, response caching so unchanged (prompt, model) pairs don't re-hit
the API.

**Learn (general).** *[Reference-only · 15 min]* Wang et al., "Self-Consistency Improves Chain-of-Thought Reasoning"
(2022) for majority vote; provider API docs for temperature/sampling.

**Build.** Run one PatchProse diff 10× and record the score spread; drop judge temperature
to 0 and re-measure; enable caching and confirm run 2 is instant/free; add a 3-sample
majority vote to a borderline case.

**Checkpoint.** You can state your suite's variance and have a deliberate plan for flaky cases.

---

## Stage 7 — RAG evals (CiteFirst enters here)

**Concept.** Retrieval-augmented answers need eval families the other stages never touch:
did it retrieve the *right* source (retrieval accuracy), and is the answer *grounded* in
that source with no hallucinated facts (groundedness / faithfulness). This is the one place
a second subject earns its keep — PatchProse has no retrieval step to grade.

**Learn (general).** *[Core sections · 60 min — anchor paper]* Es et al., "RAGAS: Automated Evaluation of Retrieval Augmented
Generation" (2023) for faithfulness/answer-relevance/context-precision. The `ragas` docs
explain the metric definitions clearly regardless of language.

**Build.** Now — not before — stand up **CiteFirst**: a small RAG assistant over the
`Microsoft.Extensions.AI.Evaluation` (or ML.NET) docs. Build a golden set of questions with
known correct source sections. Evaluate with the library's Retrieval and Groundedness
evaluators: did it cite the right section, and did it invent any API. Keep CiteFirst minimal —
it exists to be evaluated, not shipped.

**Checkpoint.** You can produce a case where the answer *sounds* right but fails groundedness
because it cited the wrong doc section or invented a method — and your eval catches it.

---

## Stage 8 — Dataset quality (applies to both projects)

**Concept.** A golden set with wrong/thin labels caps your eval's ceiling — no judge beats
its ground truth. Curation is the highest-leverage, least-flashy skill here.

**Learn (general).** *[Skim · 20 min]* Northcutt et al., "Pervasive Label Errors in Test Sets" (2021); Andrew
Ng's data-centric AI talks for the mindset.

**Build.** Audit *both* golden sets (PatchProse diffs and CiteFirst questions) for duplicates,
ambiguity, missing edges. Plant one mislabeled case in each and watch it produce a fake
"failure." Add boundary coverage (empty diff, adversarial question, no-answer-in-docs case).

**Checkpoint.** You caught a "model bug" that was really a bad label — and can tell them apart.

---

## Stage 9 — Aggregation, thresholds & CI gating

**Concept.** An eval only protects you if it runs automatically and can block a bad change.
Pick the aggregate rule ("95% pass, zero groundedness failures") and enforce it in CI.

**Learn (general).** *[Reference-only · as needed]* Standard test-gating practice applied to evals, plus the library's own
CI/reporting docs (and the Azure DevOps eval plugin if you're on AzDO).

**Build.** Run the PatchProse eval in `dotnet test` in GitHub Actions; set a threshold that
fails the job below it; make a deliberately bad prompt change and watch CI go red; publish
the HTML report as a build artifact.

**Checkpoint.** A regressing prompt change cannot merge without turning the pipeline red.

---

## Stage 10 — Deterministic vs. judgment, side by side (ML.NET contrast)

**Concept.** Not every eval needs a second AI. Tasks with real ground truth should use
classic metrics. Knowing which corner of the grid a task belongs in is a senior skill.

**Learn (general).** *[Reference-only · 20 min]* Precision/recall/F1/confusion-matrix fundamentals — scikit-learn's
"Model evaluation" metrics docs, then the ML.NET evaluation-metrics docs for the API.

**Build.** PatchProse hands you a perfect classification target: the commit **type**
(feat/fix/docs/chore) is a real label. Train a small **ML.NET** classifier to predict it
from the diff; evaluate with precision/recall/F1/confusion matrix. Put those metrics next to
your LLM-judge results on the *same* tool and write down when you'd reach for each.

**Checkpoint.** You can argue deterministic-metric vs. LLM-judge for a given task and defend
it — using two results from one project.

---

## Capstone

Unify both tools into one suite: PatchProse graded by rule + deterministic + classification
(ML.NET) + LLM-judge with a *calibrated*, bias-mitigated judge, caching, and a CI gate; and
CiteFirst graded by retrieval + groundedness. One HTML report, one threshold policy. That's a
production-grade eval harness spanning every row of the map — built from two finished
projects, no fragments.

---

### Suggested article carve-outs (each stage ≈ one post)

- "Evals aren't unit tests — the mental model" (Stage 0)
- "Cheap evals first: rules and metrics before you reach for AI" (Stage 1)
- "LLM-as-judge, natively in C#" (Stages 2–3)
- "Don't trust your judge until you've calibrated it" (Stage 4)
- "Your AI judge is biased — prove it and fix it" (Stage 5)
- "Flaky evals: variance, caching, and majority vote" (Stage 6)
- "Evaluating RAG: retrieval and groundedness" (Stage 7)
- "Your dataset is the ceiling" (Stage 8)
- "Gating deploys on eval results in CI" (Stage 9)
- "When NOT to use an AI judge: ML.NET vs. LLM-as-judge" (Stage 10)

---

## Contribution targets — learn by contributing

Contributing to an eval tool teaches evals regardless of the tool's language. Map a stage to
a repo where a PR reinforces it. For real entry points, open
`github.com/<owner>/<repo>/contribute` and filter `label:"good first issue"`. Verify recent
merged PRs + a specific CONTRIBUTING.md before investing.

| Stage(s) | Repo | Why it reinforces the stage | Barrier |
|----------|------|-----------------------------|---------|
| 1 | `promptfoo/js-rouge` | Standalone ROUGE metric — pure deterministic territory | Low |
| 1, 10 | `dotnet/machinelearning` (ML.NET, .NET Foundation) | Classic metrics: precision/recall/F1 | Med |
| 2–3 | `promptfoo/promptfoo` | The tool from the article; you know its internals. Providers, examples, docs | Low–Med |
| 3–6 | `dotnet/ai-samples` | Add a worked eval example — forces API depth; useful to others | Low |
| 3–6 | `dotnet/extensions` | Home of `Microsoft.Extensions.AI.Evaluation` — the library itself | High |
| 4, 5 | `confident-ai/deepeval` (Python) | Judge patterns, calibration, bias — concept-dense; docs/examples count | Med |
| 7 | `explodinggradients/ragas` (Python) | Retrieval/groundedness/faithfulness metrics — matches Stage 7 exactly | Med |
| adjacent | `microsoft/semantic-kernel` | Teaches what you're evaluating (orchestration layer) | Med |

### Notes on the picks

- **promptfoo/promptfoo** — now part of OpenAI, still MIT/open source, used by OpenAI and
  Anthropic. Maintainers welcome outside PRs and AI-assisted work, judged on result quality;
  they especially want provider contributions. Strongest "you already know it" start. TS.
- **dotnet/ai-samples** — lowest-barrier C# option; an eval sample is wanted and makes you
  learn the library end to end. Best *first* PR for Stage 3.
- **dotnet/extensions** — the library source, but a large Microsoft monorepo with a high
  review bar. Start with a bug repro, doc fix, or custom-evaluator sample, not a core change.
- **dotnet/machinelearning** — genuine .NET Foundation project and your Stage 10 contrast.
- **ragas / deepeval** — Python, so not your build language, but highest concept density for
  Stages 4–5 and 7. Even a docs/example PR walks you through their taxonomy. Read regardless.

### Suggested first move

Pick **one** and go shallow before deep: `dotnet/ai-samples` (a small eval example) or
`promptfoo` (a provider tweak or example). Confirm scope in an issue comment first, keep the
PR tiny and single-purpose, include exact "how to test" steps.
