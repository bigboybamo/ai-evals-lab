# Questions

Written before deep reading. Expected to be shallow and partly wrong — the point is to
have something specific to answer, not to sound informed. Correct them after the
experiments rather than rewriting history.

## Seeded from the code (2026-08-13)

These came out of building PatchProse, not from reading. Edit freely.

- `ExtractFilesTouchedFromDescription` only works because the system prompt promises a
  literal `Files touched:` section. If the model writes `Changed files:` instead, the check
  fails. Is that check measuring correctness, or measuring whether the model obeyed a format
  I invented?
- `GitDiffReader.Read` concatenates the staged and unstaged diffs. If one file has both, the
  model sees it twice. Does that change the output? Would any metric notice?
- An empty diff currently throws. Should "no input" be an error, or a case with an expected
  empty result?
- If I write the reference description myself, isn't mine just one of many valid answers?
  What does a low score mean when the output is fine but worded differently?

## From running it

- How can a PatchProse output pass format, issue, and file checks while missing the most important risk in the diff?
- Can deterministic evals detect contradictions inside a prompt?
- Should I add project-specific rules, like "do not add examples that conflict with existing instructions"?
- Where is the boundary between a cheap rule check and needing an LLM judge?
- If the output accurately summarizes a bad change, is PatchProse good or bad for that case?

## Stage 1 questions I expect to need answered

- What is the difference between a rule and a metric?
- When is exact match actually useful?
- What does token-overlap F1 measure, precisely?
- What do BLEU and ROUGE add over token F1?
- When should I stop trusting deterministic metrics?

