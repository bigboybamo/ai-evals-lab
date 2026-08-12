namespace PatchProse;

public sealed record PatchProseOutput(
    string CommitMessage,
    string PullRequestDescription);
