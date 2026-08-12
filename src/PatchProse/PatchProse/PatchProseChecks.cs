using System.Text.RegularExpressions;

namespace PatchProse;

public static partial class PatchProseChecks
{
    public static bool IsConventionalCommit(string commitMessage)
    {
        return ConventionalCommitRegex().IsMatch(commitMessage);
    }

    public static bool ContainsIssueReferencesFromDiff(string diff, PatchProseOutput output)
    {
        var expectedIssues = GitDiffParser.ExtractIssueReferences(diff);

        if (expectedIssues.Count == 0)
        {
            return true;
        }

        var outputText = $"{output.CommitMessage}\n{output.PullRequestDescription}";
        var actualIssues = GitDiffParser.ExtractIssueReferences(outputText);

        return expectedIssues.IsSubsetOf(actualIssues);
    }

    public static FileMatchResult CompareFilesTouched(string diff, PatchProseOutput output)
    {
        var expected = GitDiffParser.ExtractTouchedFiles(diff);
        var actual = ExtractFilesTouchedFromDescription(output.PullRequestDescription);

        return new FileMatchResult(
            expected.Except(actual, StringComparer.Ordinal).ToArray(),
            actual.Except(expected, StringComparer.Ordinal).ToArray());
    }

    public static IReadOnlySet<string> ExtractFilesTouchedFromDescription(string pullRequestDescription)
    {
        var files = new SortedSet<string>(StringComparer.Ordinal);
        var inFilesTouchedSection = false;

        foreach (var rawLine in pullRequestDescription.Split('\n'))
        {
            var line = rawLine.Trim();

            if (line.Equals("Files touched:", StringComparison.OrdinalIgnoreCase))
            {
                inFilesTouchedSection = true;
                continue;
            }

            if (!inFilesTouchedSection)
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            if (!line.StartsWith("- ", StringComparison.Ordinal))
            {
                break;
            }

            files.Add(line[2..].Trim().Replace('\\', '/'));
        }

        return files;
    }

    [GeneratedRegex(@"^(feat|fix|docs|style|refactor|perf|test|build|ci|chore|revert)(\([a-z0-9._-]+\))?!?: .{1,72}$")]
    private static partial Regex ConventionalCommitRegex();
}
