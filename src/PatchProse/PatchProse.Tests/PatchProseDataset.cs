using System.Text.Json;

namespace PatchProse.Tests;

public sealed record PatchProseDataset(
    int SchemaVersion,
    string Project,
    string Stage,
    IReadOnlyList<PatchProseDatasetCase> Cases)
{
    public static PatchProseDataset Load()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "patchprose-stage-01.json");
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<PatchProseDataset>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"Unable to load dataset from {path}.");
    }
}

public sealed record PatchProseDatasetCase(
    string Id,
    string Title,
    string Category,
    string Diff,
    PatchProseDatasetOutput GeneratedOutput,
    string ReferenceDescription,
    PatchProseExpectedCheckResults ExpectedCheckResults,
    PatchProseExpectedReferenceMetrics ExpectedReferenceMetrics,
    PatchProseExpectedFacts ExpectedFacts,
    string KnownLimitation)
{
    public PatchProseOutput ToOutput()
    {
        return new PatchProseOutput(
            GeneratedOutput.CommitMessage,
            GeneratedOutput.PullRequestDescription);
    }
}

public sealed record PatchProseDatasetOutput(
    string CommitMessage,
    string PullRequestDescription);

public sealed record PatchProseExpectedCheckResults(
    bool ConventionalCommit,
    bool IssueReferencesMatch,
    bool FilesTouchedMatch);

public sealed record PatchProseExpectedReferenceMetrics(
    bool ExactMatch,
    double TokenOverlapF1);

public sealed record PatchProseExpectedFacts(
    IReadOnlyList<string> IssueReferences,
    IReadOnlyList<string> FilesTouched);
