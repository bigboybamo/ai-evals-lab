# Evaluation Baselines

Baselines store saved eval results for comparison against future runs.

The Stage 1 baseline is `patchprose-stage-01-baseline.json`.

It includes PatchProse's custom deterministic checks, the hand-rolled token-overlap F1, and
Microsoft.Extensions.AI.Evaluation.NLP scores for F1, BLEU, and GLEU.

Regenerate it from the repository root:

```powershell
dotnet run --project src\PatchProse\PatchProse.Evals\PatchProse.Evals.csproj
```
