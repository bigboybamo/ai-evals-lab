namespace PatchProse;

public sealed record FileMatchResult(
    IReadOnlyCollection<string> MissingFiles,
    IReadOnlyCollection<string> UnexpectedFiles)
{
    public bool IsMatch => MissingFiles.Count == 0 && UnexpectedFiles.Count == 0;
}
