namespace PatchProse.Tests;

public sealed class PatchProseChecksTests
{
    private const string Diff = """
        diff --git a/src/CheckoutService.cs b/src/CheckoutService.cs
        index 1111111..2222222 100644
        --- a/src/CheckoutService.cs
        +++ b/src/CheckoutService.cs
        @@ -1,5 +1,6 @@
         public sealed class CheckoutService
         {
        +    // Fixes CART-42
             public bool CanCheckout(Cart cart) => cart.Items.Count > 0;
         }
        diff --git a/tests/CheckoutServiceTests.cs b/tests/CheckoutServiceTests.cs
        index 3333333..4444444 100644
        --- a/tests/CheckoutServiceTests.cs
        +++ b/tests/CheckoutServiceTests.cs
        @@ -1,4 +1,5 @@
         public sealed class CheckoutServiceTests
         {
        +    // Covers CART-42
         }
        """;

    private static readonly PatchProseOutput Output = new(
        "fix(checkout): handle empty carts for CART-42",
        """
        Summary:
        Handles the CART-42 checkout edge case without changing unrelated behavior.

        Files touched:
        - src/CheckoutService.cs
        - tests/CheckoutServiceTests.cs
        """);

    [Test]
    public void Commit_message_uses_conventional_commit_format()
    {
        Assert.That(PatchProseChecks.IsConventionalCommit(Output.CommitMessage), Is.True);
    }

    [Test]
    public void Output_contains_issue_number_from_diff()
    {
        Assert.That(PatchProseChecks.ContainsIssueReferencesFromDiff(Diff, Output), Is.True);
    }

    [Test]
    public void Files_touched_match_the_diff()
    {
        var result = PatchProseChecks.CompareFilesTouched(Diff, Output);

        Assert.That(result.IsMatch, Is.True);
        Assert.That(result.MissingFiles, Is.Empty);
        Assert.That(result.UnexpectedFiles, Is.Empty);
    }
}
