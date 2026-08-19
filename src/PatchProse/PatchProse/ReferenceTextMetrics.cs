using System.Text.RegularExpressions;

namespace PatchProse;

public static partial class ReferenceTextMetrics
{
    /// <summary>
    /// Checks whether the generated text exactly matches the reference text after trimming.
    /// </summary>
    public static bool ExactMatch(string generated, string reference)
    {
        return string.Equals(generated.Trim(), reference.Trim(), StringComparison.Ordinal);
    }

    /// <summary>
    /// Scores generated text against reference text using token-overlap precision, recall, and F1.
    /// </summary>
    public static TokenOverlapScore TokenOverlapF1(string generated, string reference)
    {
        var generatedTokens = Tokenize(generated);
        var referenceTokens = Tokenize(reference);

        if (generatedTokens.Count == 0 && referenceTokens.Count == 0)
        {
            return new TokenOverlapScore(1, 1, 1);
        }

        if (generatedTokens.Count == 0 || referenceTokens.Count == 0)
        {
            return new TokenOverlapScore(0, 0, 0);
        }

        var generatedCounts = CountTokens(generatedTokens);
        var referenceCounts = CountTokens(referenceTokens);
        var overlap = 0;

        foreach (var (token, generatedCount) in generatedCounts)
        {
            if (referenceCounts.TryGetValue(token, out var referenceCount))
            {
                overlap += Math.Min(generatedCount, referenceCount);
            }
        }

        var precision = (double)overlap / generatedTokens.Count;
        var recall = (double)overlap / referenceTokens.Count;
        var f1 = precision + recall == 0 ? 0 : 2 * precision * recall / (precision + recall);

        return new TokenOverlapScore(precision, recall, f1);
    }

    /// <summary>
    /// Splits text into lowercase word-like tokens used by the overlap metric.
    /// </summary>
    private static IReadOnlyList<string> Tokenize(string text)
    {
        return WordRegex()
            .Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .ToArray();
    }

    /// <summary>
    /// Counts duplicate token occurrences so repeated words affect overlap correctly.
    /// </summary>
    private static Dictionary<string, int> CountTokens(IEnumerable<string> tokens)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        foreach (var token in tokens)
        {
            counts[token] = counts.GetValueOrDefault(token) + 1;
        }

        return counts;
    }

    [GeneratedRegex(@"[a-z0-9]+")]
    private static partial Regex WordRegex();
}
