# Evaluation Datasets

Datasets store reproducible inputs and expected facts for eval runs.

For Stage 1, this folder should hold PatchProse cases that deterministic checks can run against:

- the raw git diff;
- the generated PatchProse output captured from a real run;
- expected issue references from the diff;
- expected files touched;
- observed pass/fail results;
- known limitation or surprising behavior.

The first seed dataset is `patchprose-stage-01.json`.
