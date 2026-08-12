using System.Text.RegularExpressions;

namespace PatchProse;

public static partial class GitDiffParser
{
    public static IReadOnlySet<string> ExtractTouchedFiles(string diff)
    {
        var files = new SortedSet<string>(StringComparer.Ordinal);

        foreach (Match match in DiffHeaderRegex().Matches(diff))
        {
            AddPath(files, match.Groups["left"].Value);
            AddPath(files, match.Groups["right"].Value);
        }

        return files;
    }

    public static IReadOnlySet<string> ExtractIssueReferences(string text)
    {
        var issues = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in IssueRegex().Matches(text))
        {
            issues.Add(match.Value);
        }

        return issues;
    }

    private static void AddPath(ISet<string> files, string path)
    {
        if (path == "/dev/null")
        {
            return;
        }

        var normalized = path.Replace('\\', '/');

        if (normalized.StartsWith("a/", StringComparison.Ordinal)
            || normalized.StartsWith("b/", StringComparison.Ordinal))
        {
            normalized = normalized[2..];
        }

        files.Add(normalized);
    }

    [GeneratedRegex(@"^diff --git (?<left>\S+) (?<right>\S+)$", RegexOptions.Multiline)]
    private static partial Regex DiffHeaderRegex();

    [GeneratedRegex(@"(?<![A-Za-z0-9])(?:#[0-9]+|[A-Z][A-Z0-9]+-[0-9]+)(?![A-Za-z0-9])")]
    private static partial Regex IssueRegex();
}
