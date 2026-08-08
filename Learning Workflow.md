# Learning Workflow

This document describes how I work through each stage of the AI Evals learning path.

The goal is to combine three activities:

```text
Building
+
Learning
+
Writing
```

Rather than treating them as separate tasks, every stage follows the same loop:

```text
Build
  ↓
Break / Measure
  ↓
Learn
  ↓
Improve
  ↓
Explain
  ↓
Write
```

---

# 1. Start With the Project

Do not begin a stage by trying to learn everything about the topic.

Start with a problem in the project.

For example:

```text
PatchProse generates a PR description.

How do I know whether that description is good?
```

That question becomes the reason to learn the next evaluation technique.

The learning should therefore progress naturally:

```text
I need to test something.
        ↓
Can ordinary code test it?
        ↓
Where does that approach fail?
        ↓
What technique solves that problem?
        ↓
Learn that technique.
```

---

# 2. Use One Branch Per Stage

Each learning stage gets its own Git branch.

For example:

```text
feature/stage-01-deterministic-evals

feature/stage-02-llm-judge

feature/stage-03-ai-evaluation-library

feature/stage-04-judge-calibration

feature/stage-05-judge-bias
```

When the stage is complete:

```text
feature branch
      ↓
merge into main
      ↓
tag the finished stage
```

Suggested tags:

```text
stage-01-complete
stage-02-complete
stage-03-complete
...
```

This preserves the state of the project at each point in the learning journey.

---

# 3. Create a Learning Folder for the Stage

Every stage gets:

```text
learning/
└── 01-deterministic-evals/
    ├── README.md
    ├── questions.md
    ├── source-notes.md
    └── experiment-log.md
```

Each file has a different purpose.

---

## `questions.md`

Write down what I don't understand **before doing deep reading**.

For example:

```markdown
# Questions

- What is the difference between a rule and a metric?
- When is exact match useful?
- What does token F1 actually measure?
- Why can similar sentences have opposite meanings?
- When should I stop using deterministic metrics?
```

These questions guide the reading.

---

## `source-notes.md`

Record only information that helps answer the questions raised by the project or experiment.

Avoid writing huge paper summaries.

Example:

```markdown
## BLEU

Measures overlap between generated and reference text.

Useful for:

- comparing outputs against known references;
- measuring lexical similarity.

Does not directly measure:

- factual correctness;
- semantic equivalence.

Important realization:

Similar words do not necessarily mean similar meaning.
```

---

## `experiment-log.md`

Record what actually happened while building.

Example:

```markdown
# Experiment 1

## Input

Rename `User.Name` to `User.FullName`.

## Expected Description

Renames the Name property to FullName.

## Generated Description

Improves the User model naming.

## Result

Token F1: 0.71

## Observation

The score appears reasonable, but the generated description
loses the important fact that a specific property was renamed.

## Question

How well can text-overlap metrics represent semantic correctness?

## Article Use

Possible example for showing why text similarity is not the
same as correctness.
```

This file becomes the raw material for the eventual article.

---

## `README.md`

This contains my final understanding after completing the stage.

Keep it concise.

It should answer:

```text
What is this concept?

What problem does it solve?

When should I use it?

When should I NOT use it?

What failure did I observe?

What did the experiment teach me?
```

---

# 4. Build the Simplest Version First

Do not start by building the final sophisticated implementation.

For example, when learning deterministic evaluation, start with ordinary assertions:

```text
Does the output follow conventional commit format?

Does it contain the expected issue number?

Does it mention the correct changed files?
```

Then introduce metrics.

Then test where those metrics fail.

Only introduce more sophisticated techniques when the simpler approach exposes a limitation.

---

# 5. Deliberately Try to Break the Evaluation

Implementing an evaluator is not enough.

Every stage should contain at least one failure experiment.

The goal is not:

```text
I implemented BLEU.
```

The goal is:

```text
I understand a situation where BLEU gives me a
misleading result.
```

Examples:

### Deterministic metrics

Create:

```text
Different wording
+
Same meaning
```

and:

```text
Very similar wording
+
Opposite meaning
```

---

### LLM-as-Judge

Keep the generated answer unchanged.

Change only the rubric.

Observe:

```text
PASS
 ↓
FAIL
```

---

### Judge Calibration

Compare:

```text
My label
vs.
Judge label
```

Find the disagreements.

---

### Judge Bias

Run:

```text
A vs B
```

and then:

```text
B vs A
```

Check whether the result changes.

---

### Variance

Evaluate the exact same case several times.

Record whether the verdict changes.

---

### RAG

Create an answer that sounds convincing but:

```text
uses the wrong source
```

or:

```text
contains information not present in the retrieved context
```

---

### Dataset Quality

Insert an intentionally incorrect golden label.

Observe the resulting fake model failure.

---

### CI

Make an intentionally worse prompt change.

Verify that the evaluation gate catches it.

---

# 6. Read After the Experiment Raises Questions

Reading should answer a problem I have already encountered.

The sequence should be:

```text
Build
   ↓
Observe something strange
   ↓
Write down the question
   ↓
Read
   ↓
Understand why it happened
```

Not:

```text
Read several papers
   ↓
Hope the information becomes useful later
```

When reading papers or documentation, focus on:

```text
Definition
+
Relevant method
+
Relevant failure mode
+
Anything directly explaining my experiment
```

Skip material that does not help answer the current question.

---

# 7. Improve the Implementation

After understanding the concept, return to the code.

Examples:

```text
Improve the rubric

Add another evaluator

Randomize pairwise answer order

Add multiple judge samples

Introduce caching

Correct dataset labels

Add missing edge cases

Introduce a CI threshold
```

The learning should produce an actual improvement in the project.

---

# 8. Save Before and After Results

Never overwrite the original experiment.

Keep the baseline.

For example:

```text
evals/
└── baselines/
    ├── stage-02-v1.json
    └── stage-02-v2.json
```

Then I can compare:

```text
Before

5 / 8 passed
62.5%
```

with:

```text
After

7 / 8 passed
87.5%
```

This makes improvements measurable and gives the article real evidence.

---

# 9. Capture Evidence While Building

Useful evidence includes:

- inputs;
- outputs;
- evaluator results;
- pass rates;
- disagreement counts;
- variance;
- screenshots;
- before/after results;
- surprising failures.

Store evaluation results under:

```text
evals/
├── datasets/
├── rubrics/
├── baselines/
└── reports/
```

Do not rely on memory when writing the article later.

---

# 10. Keep an Article Scratchpad During Development

Each planned article gets:

```text
articles/
└── 01-cheap-evals-first/
    ├── scratch.md
    ├── outline.md
    ├── draft.md
    ├── snippets/
    └── assets/
```

During development, only worry about `scratch.md`.

Write down interesting observations immediately.

Example:

```markdown
- F1 gave this result a surprisingly high score despite
  one word changing the meaning completely.

- This should probably be the central example in the article.

- Screenshot the result.

- Introduce semantic evaluation immediately after showing
  this failure.
```

Do not try to write polished prose while still experimenting.

---

# 11. Explain the Concept Before Writing

Before drafting the article, I should be able to answer these questions without looking at my notes:

```text
What is it?

Why does it exist?

What problem did I encounter that required it?

How does it work?

What does it do well?

Where does it fail?

When should I use it?

When should I use something else?
```

If I cannot answer these clearly, I am not finished learning the stage.

---

# 12. Build the Article Around the Experiment

The article should generally follow:

```text
Problem
   ↓
Initial Approach
   ↓
Implementation
   ↓
Experiment
   ↓
Unexpected Result
   ↓
Why It Happened
   ↓
Concept
   ↓
Improved Implementation
   ↓
New Result
   ↓
Lesson
```

This is preferable to:

```text
Definition
Theory
History
Code
Conclusion
```

The reader should encounter the concept because there is an engineering problem that requires it.

---

# 13. Stage Completion Checklist

A stage is complete only when all three areas are done.

```markdown
## Build

- [ ] Required feature works
- [ ] Tests pass
- [ ] Evaluation dataset exists
- [ ] Experiment can be reproduced
- [ ] Baseline saved
- [ ] At least one failure case reproduced

## Learn

- [ ] Questions documented
- [ ] Relevant sources studied
- [ ] I can explain the concept without notes
- [ ] I understand when to use it
- [ ] I understand when NOT to use it
- [ ] I understand why the failure occurred
- [ ] Learning-path checkpoint satisfied

## Write

- [ ] Interesting experiment identified
- [ ] Article scratchpad updated
- [ ] Before/after evidence captured
- [ ] Article outline written
- [ ] Relevant code snippets selected
- [ ] Draft completed

## Finish

- [ ] Stage branch merged
- [ ] Stage tagged
- [ ] Next problem/question identified
```

---

# 14. Repeat the Loop

The overall progression should feel like one continuous engineering problem:

```text
I built an AI application.
        ↓
How do I evaluate it?
        ↓
Rules
        ↓
Where do rules fail?
        ↓
Metrics
        ↓
Where do metrics fail?
        ↓
LLM-as-Judge
        ↓
Can I trust the judge?
        ↓
Calibration
        ↓
Is the judge biased?
        ↓
Bias testing
        ↓
Is it stable?
        ↓
Variance testing
        ↓
What about retrieval?
        ↓
RAG evaluation
        ↓
Can I trust the dataset?
        ↓
Dataset auditing
        ↓
How does this protect production?
        ↓
CI gates
        ↓
Do I need an LLM for every eval?
        ↓
No — choose the evaluator that matches the problem.
```

The objective is for the **project, learning, experiments, and articles to all be outputs of the same process**.