using System.Text.RegularExpressions;

namespace PatchProse;

public static partial class PatchProseChecks
{
    /// <summary>
    /// Checks whether the commit message follows the conventional commit format.
    /// </summary>
    public static bool IsConventionalCommit(string commitMessage)
    {
        return ConventionalCommitRegex().IsMatch(commitMessage);
    }

    /// <summary>
    /// Checks whether all issue references found in the diff also appear in the PatchProse output.
    /// </summary>
    public static bool ContainsIssueReferencesFromDiff(string diff, PatchProseOutput output)
    {
        return CompareIssueReferences(diff, output).IsMatch;
    }

    /// <summary>
    /// Compares issue references from the diff with issue references in the generated output.
    /// </summary>
    public static IssueReferenceMatchResult CompareIssueReferences(string diff, PatchProseOutput output)
    {
        var expected = GitDiffParser.ExtractIssueReferences(diff);
        var outputText = $"{output.CommitMessage}\n{output.PullRequestDescription}";
        var actual = GitDiffParser.ExtractIssueReferences(outputText);

        return new IssueReferenceMatchResult(
            expected.Except(actual, StringComparer.OrdinalIgnoreCase).ToArray(),
            actual.Except(expected, StringComparer.OrdinalIgnoreCase).ToArray());
    }

    /// <summary>
    /// Compares files touched in the git diff with files listed in the generated PR description.
    /// </summary>
    public static FileMatchResult CompareFilesTouched(string diff, PatchProseOutput output)
    {
        var expected = GitDiffParser.ExtractTouchedFiles(diff);
        var actual = ExtractFilesTouchedFromDescription(output.PullRequestDescription);

        return new FileMatchResult(
            expected.Except(actual, StringComparer.Ordinal).ToArray(),
            actual.Except(expected, StringComparer.Ordinal).ToArray());
    }

    /// <summary>
    /// Extracts file paths from the generated PR description's "Files touched:" section.
    /// </summary>
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
