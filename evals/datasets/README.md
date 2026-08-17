# Evaluation Datasets

Datasets store reproducible inputs and expected facts for eval runs.

For Stage 1, this folder should hold PatchProse cases that deterministic checks can run against:

- the raw git diff;
- the generated PatchProse output captured from a real run;
- expected issue references from the diff;
- expected files touched;
- expected deterministic check results;
- expected reference metric results;
- known limitation or surprising behavior.

The Stage 1 dataset is `patchprose-stage-01.json`.
