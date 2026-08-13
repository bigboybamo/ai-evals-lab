namespace PatchProse;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var repositoryPath = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

        try
        {
            using var client = new OpenAiPatchProseClient();
            var generator = new PatchProseGenerator(client);

            var output = await generator.GenerateFromGitDiffAsync(repositoryPath);

            Console.WriteLine(output.CommitMessage);
            Console.WriteLine();
            Console.WriteLine(output.PullRequestDescription);

            return 0;
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            Console.Error.WriteLine(ex.Message);
            return 1;
        }
    }
}
