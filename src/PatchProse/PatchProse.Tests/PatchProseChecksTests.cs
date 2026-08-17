namespace PatchProse.Tests;

public sealed class PatchProseChecksTests
{
    public static IEnumerable<TestCaseData> DatasetCases()
    {
        foreach (var testCase in PatchProseDataset.Load().Cases)
        {
            yield return new TestCaseData(testCase).SetName(testCase.Id);
        }
    }

    [TestCaseSource(nameof(DatasetCases))]
    public void Commit_message_uses_expected_conventional_commit_result(PatchProseDatasetCase testCase)
    {
        // Arrange
        var commitMessage = testCase.GeneratedOutput.CommitMessage;
        var expected = testCase.ExpectedCheckResults.ConventionalCommit;

        // Act
        var actual = PatchProseChecks.IsConventionalCommit(commitMessage);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(DatasetCases))]
    public void Issue_references_match_expected_result(PatchProseDatasetCase testCase)
    {
        // Arrange
        var diff = testCase.Diff;
        var output = testCase.ToOutput();
        var expected = testCase.ExpectedCheckResults.IssueReferencesMatch;

        // Act
        var result = PatchProseChecks.CompareIssueReferences(diff, output);

        // Assert
        Assert.That(result.IsMatch, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(DatasetCases))]
    public void Files_touched_match_expected_result(PatchProseDatasetCase testCase)
    {
        // Arrange
        var diff = testCase.Diff;
        var output = testCase.ToOutput();
        var expected = testCase.ExpectedCheckResults.FilesTouchedMatch;

        // Act
        var result = PatchProseChecks.CompareFilesTouched(diff, output);

        // Assert
        Assert.That(result.IsMatch, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(DatasetCases))]
    public void Pull_request_description_has_expected_exact_match_result(PatchProseDatasetCase testCase)
    {
        // Arrange
        var generated = testCase.GeneratedOutput.PullRequestDescription;
        var reference = testCase.ReferenceDescription;
        var expected = testCase.ExpectedReferenceMetrics.ExactMatch;

        // Act
        var actual = ReferenceTextMetrics.ExactMatch(generated, reference);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }

    [TestCaseSource(nameof(DatasetCases))]
    public void Pull_request_description_has_expected_token_overlap_f1(PatchProseDatasetCase testCase)
    {
        // Arrange
        var generated = testCase.GeneratedOutput.PullRequestDescription;
        var reference = testCase.ReferenceDescription;
        var expected = testCase.ExpectedReferenceMetrics.TokenOverlapF1;

        // Act
        var actual = ReferenceTextMetrics.TokenOverlapF1(generated, reference);

        // Assert
        Assert.That(actual.F1, Is.EqualTo(expected).Within(0.0001));
    }
}
