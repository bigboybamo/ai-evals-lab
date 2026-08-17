using System.Text.Json;
using PatchProse;

// Default paths are resolved from the repo root so the runner works from any subfolder.
var repositoryRoot = FindRepositoryRoot(Directory.GetCurrentDirectory());
var datasetPath = args.Length > 0
    ? Path.GetFullPath(args[0])
    : Path.Combine(repositoryRoot, "evals", "datasets", "patchprose-stage-01.json");
var baselinePath = args.Length > 1
    ? Path.GetFullPath(args[1])
    : Path.Combine(repositoryRoot, "evals", "baselines", "patchprose-stage-01-baseline.json");

var dataset = StageOneDataset.Load(datasetPath);
var caseResults = dataset.Cases.Select(Evaluate).ToArray();
var baseline = StageOneBaseline.Create(dataset, caseResults);

// Create the output folder on demand so a clean checkout can regenerate baselines.
Directory.CreateDirectory(Path.GetDirectoryName(baselinePath)!);

File.WriteAllText(
    baselinePath,
    JsonSerializer.Serialize(baseline, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    }));

Console.WriteLine($"Wrote baseline: {baselinePath}");
Console.WriteLine($"Cases: {baseline.Summary.CaseCount}");
Console.WriteLine($"Conventional commits: {baseline.Summary.ConventionalCommitPassCount}/{baseline.Summary.CaseCount}");
Console.WriteLine($"Issue references matched: {baseline.Summary.IssueReferencesMatchCount}/{baseline.Summary.CaseCount}");
Console.WriteLine($"Files touched matched: {baseline.Summary.FilesTouchedMatchCount}/{baseline.Summary.CaseCount}");
Console.WriteLine($"Exact matches: {baseline.Summary.ExactMatchCount}/{baseline.Summary.CaseCount}");
Console.WriteLine($"Average token F1: {baseline.Summary.AverageTokenOverlapF1:0.0000}");

// Runs every deterministic check and reference metric for one saved PatchProse output.
static StageOneCaseResult Evaluate(StageOneDatasetCase testCase)
{
    var output = new PatchProseOutput(
        testCase.GeneratedOutput.CommitMessage,
        testCase.GeneratedOutput.PullRequestDescription);

    var issueReferences = PatchProseChecks.CompareIssueReferences(testCase.Diff, output);
    var filesTouched = PatchProseChecks.CompareFilesTouched(testCase.Diff, output);
    var tokenOverlap = ReferenceTextMetrics.TokenOverlapF1(
        testCase.GeneratedOutput.PullRequestDescription,
        testCase.ReferenceDescription);

    return new StageOneCaseResult(
        testCase.Id,
        testCase.Title,
        testCase.Category,
        PatchProseChecks.IsConventionalCommit(testCase.GeneratedOutput.CommitMessage),
        issueReferences.IsMatch,
        filesTouched.IsMatch,
        ReferenceTextMetrics.ExactMatch(testCase.GeneratedOutput.PullRequestDescription, testCase.ReferenceDescription),
        new StageOneTokenOverlapResult(tokenOverlap.Precision, tokenOverlap.Recall, tokenOverlap.F1),
        issueReferences.MissingIssues,
        issueReferences.UnexpectedIssues,
        filesTouched.MissingFiles,
        filesTouched.UnexpectedFiles,
        testCase.KnownLimitation);
}

// Walks upward until it finds .git, which anchors dataset and baseline paths.
static string FindRepositoryRoot(string startDirectory)
{
    var current = new DirectoryInfo(startDirectory);

    while (current is not null)
    {
        if (Directory.Exists(Path.Combine(current.FullName, ".git")))
        {
            return current.FullName;
        }

        current = current.Parent;
    }

    throw new InvalidOperationException("Could not find repository root.");
}

internal sealed record StageOneDataset(
    int SchemaVersion,
    string Project,
    string Stage,
    IReadOnlyList<StageOneDatasetCase> Cases)
{
    // Loads the Stage 1 dataset JSON into typed records used by the eval runner.
    public static StageOneDataset Load(string path)
    {
        var json = File.ReadAllText(path);

        return JsonSerializer.Deserialize<StageOneDataset>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException($"Unable to load dataset from {path}.");
    }
}

internal sealed record StageOneDatasetCase(
    string Id,
    string Title,
    string Category,
    string Diff,
    StageOneGeneratedOutput GeneratedOutput,
    string ReferenceDescription,
    StageOneExpectedCheckResults ExpectedCheckResults,
    StageOneExpectedReferenceMetrics ExpectedReferenceMetrics,
    StageOneExpectedFacts ExpectedFacts,
    string KnownLimitation);

internal sealed record StageOneGeneratedOutput(
    string CommitMessage,
    string PullRequestDescription);

internal sealed record StageOneExpectedCheckResults(
    bool ConventionalCommit,
    bool IssueReferencesMatch,
    bool FilesTouchedMatch);

internal sealed record StageOneExpectedReferenceMetrics(
    bool ExactMatch,
    double TokenOverlapF1);

internal sealed record StageOneExpectedFacts(
    IReadOnlyList<string> IssueReferences,
    IReadOnlyList<string> FilesTouched);

internal sealed record StageOneBaseline(
    int SchemaVersion,
    string Project,
    string Stage,
    string Dataset,
    StageOneBaselineSummary Summary,
    IReadOnlyList<StageOneCaseResult> Cases)
{
    // Builds the persisted baseline document from the evaluated per-case results.
    public static StageOneBaseline Create(StageOneDataset dataset, IReadOnlyList<StageOneCaseResult> results)
    {
        return new StageOneBaseline(
            1,
            dataset.Project,
            dataset.Stage,
            "evals/datasets/patchprose-stage-01.json",
            StageOneBaselineSummary.Create(results),
            results);
    }
}

internal sealed record StageOneBaselineSummary(
    int CaseCount,
    int ConventionalCommitPassCount,
    int IssueReferencesMatchCount,
    int FilesTouchedMatchCount,
    int ExactMatchCount,
    double AverageTokenOverlapF1)
{
    // Aggregates pass counts and average token F1 across the full dataset.
    public static StageOneBaselineSummary Create(IReadOnlyCollection<StageOneCaseResult> results)
    {
        return new StageOneBaselineSummary(
            results.Count,
            results.Count(result => result.ConventionalCommit),
            results.Count(result => result.IssueReferencesMatch),
            results.Count(result => result.FilesTouchedMatch),
            results.Count(result => result.ExactMatch),
            Math.Round(results.Average(result => result.TokenOverlap.F1), 4));
    }
}

internal sealed record StageOneCaseResult(
    string Id,
    string Title,
    string Category,
    bool ConventionalCommit,
    bool IssueReferencesMatch,
    bool FilesTouchedMatch,
    bool ExactMatch,
    StageOneTokenOverlapResult TokenOverlap,
    IReadOnlyCollection<string> MissingIssues,
    IReadOnlyCollection<string> UnexpectedIssues,
    IReadOnlyCollection<string> MissingFiles,
    IReadOnlyCollection<string> UnexpectedFiles,
    string KnownLimitation);

internal sealed record StageOneTokenOverlapResult(
    double Precision,
    double Recall,
    double F1);
