namespace PatchProse;

/// <summary>
/// Stores missing and unexpected issue references found when comparing diff facts to generated output.
/// </summary>
public sealed record IssueReferenceMatchResult(
    IReadOnlyCollection<string> MissingIssues,
    IReadOnlyCollection<string> UnexpectedIssues)
{
    public bool IsMatch => MissingIssues.Count == 0 && UnexpectedIssues.Count == 0;
}
