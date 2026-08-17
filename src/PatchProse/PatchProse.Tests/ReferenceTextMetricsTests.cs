namespace PatchProse.Tests;

public sealed class ReferenceTextMetricsTests
{
    [Test]
    public void Exact_match_passes_for_identical_text()
    {
        // Arrange
        var generated = "Adds local setup guidance to the README.";
        var reference = "Adds local setup guidance to the README.";

        // Act
        var actual = ReferenceTextMetrics.ExactMatch(generated, reference);

        // Assert
        Assert.That(actual, Is.True);
    }

    [Test]
    public void Exact_match_fails_for_valid_paraphrase()
    {
        // Arrange
        var generated = "Documents local setup steps in the README.";
        var reference = "Adds local setup guidance to the README.";

        // Act
        var actual = ReferenceTextMetrics.ExactMatch(generated, reference);

        // Assert
        Assert.That(actual, Is.False);
    }

    [Test]
    public void Token_overlap_f1_rewards_shared_words_not_meaning()
    {
        // Arrange
        var generated = "Sets location to Starbucks when coffee is mentioned without a named place.";
        var reference = "Sets location to null when coffee is mentioned without a named place.";

        // Act
        var actual = ReferenceTextMetrics.TokenOverlapF1(generated, reference);

        // Assert
        Assert.That(actual.F1, Is.GreaterThan(0.8));
    }
}
