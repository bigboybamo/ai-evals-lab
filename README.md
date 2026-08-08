# AI Evals Lab

A build-first exploration of **AI/LLM evaluation**, implemented primarily in **C#/.NET**.

The goal of this repository is to learn how different evaluation approaches work by applying them to real projects, measuring their limitations, and progressively building a more complete evaluation system.

## What I'm Exploring

The learning path covers:

- Rule-based and deterministic evals
- Reference-based metrics
- LLM-as-Judge
- `Microsoft.Extensions.AI.Evaluation`
- Judge calibration
- Judge bias and pairwise evaluation
- Variance and flaky evals
- RAG evaluation
- Dataset quality
- CI evaluation gates
- ML.NET and traditional classification metrics

The main question throughout the project is:

> **What should I evaluate, how should I evaluate it, and why is that evaluation method appropriate?**

---

## Projects

### PatchProse

The main project used throughout most of the learning path.

PatchProse takes a Git diff and generates:

- a conventional commit message;
- a pull request description.

It provides several different things to evaluate:

```text
Git Diff
   ↓
PatchProse
   ↓
Commit Message + PR Description
```

For example:

- commit format can be checked with rules;
- commit type can be treated as classification;
- PR-description quality can be evaluated with an LLM judge;
- hallucinated changes can be checked with a custom evaluator.

---

### CiteFirst

A small RAG application introduced later in the learning path.

```text
Question
   ↓
Retrieve Documentation
   ↓
LLM
   ↓
Answer + Citation
```

It exists specifically for exploring:

- retrieval quality;
- groundedness;
- faithfulness;
- source/citation correctness.

---

## Repository Structure

```text
ai-evals-lab/
│
├── README.md
├── LEARNING_WORKFLOW.md
├── ai-evals-learning-path.md
│
├── src/
│   ├── PatchProse/
│   └── CiteFirst/
│
├── tests/
│   ├── PatchProse.Tests/
│   └── CiteFirst.Tests/
│
├── evals/
│   ├── datasets/
│   ├── rubrics/
│   ├── baselines/
│   └── reports/
│
├── learning/
│   ├── 00-mental-model/
│   ├── 01-deterministic-evals/
│   ├── 02-llm-as-judge/
│   ├── 03-ai-evaluation-library/
│   ├── 04-judge-calibration/
│   ├── 05-judge-bias/
│   ├── 06-variance/
│   ├── 07-rag-evals/
│   ├── 08-dataset-quality/
│   ├── 09-ci-gating/
│   └── 10-mlnet/
│
├── scripts/
│
└── articles/
```

### `scripts/`

Helper scripts that support the eval loop but are not part of either application:

- running an eval suite for a single stage;
- generating the HTML report via the `dotnet aieval` console tool;
- comparing a run against a saved baseline in `evals/baselines/`;
- one-off dataset maintenance (auditing golden sets, spotting duplicates).

Anything here should be re-runnable from a clean checkout, so an experiment can be
reproduced later rather than remembered.

> `articles/` is intentionally excluded from version control (see `.gitignore`).

## Approach

Each stage combines three things:

**Build → Learn → Write**

I first apply the concept to the project, experiment with its limitations, study the underlying idea, and then document what I learned through a technical article.

See [`LEARNING_WORKFLOW.md`](./LEARNING_WORKFLOW.md) for the full process.

---

## End Goal

By the end of the learning path, the repository should contain an evaluation system combining:

```text
Rules
+
Deterministic Metrics
+
Classification Metrics
+
LLM-as-Judge
+
Judge Calibration
+
Bias Mitigation
+
RAG Evaluation
+
Dataset Quality Checks
+
CI Gates
```

The objective is not to use an LLM for every evaluation.

It is to understand **which evaluation approach fits which problem**.