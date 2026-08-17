namespace PatchProse;

public sealed record IssueReferenceMatchResult(
    IReadOnlyCollection<string> MissingIssues,
    IReadOnlyCollection<string> UnexpectedIssues)
{
    public bool IsMatch => MissingIssues.Count == 0 && UnexpectedIssues.Count == 0;
}
