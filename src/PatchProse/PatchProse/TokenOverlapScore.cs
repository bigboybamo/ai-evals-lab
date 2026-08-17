namespace PatchProse;

/// <summary>
/// Stores token-overlap precision, recall, and F1 scores for a generated/reference text pair.
/// </summary>
public sealed record TokenOverlapScore(
    double Precision,
    double Recall,
    double F1);
