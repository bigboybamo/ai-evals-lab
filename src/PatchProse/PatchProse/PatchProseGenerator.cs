namespace PatchProse;

public sealed class PatchProseGenerator
{
    private readonly OpenAiPatchProseClient _client;

    public PatchProseGenerator(OpenAiPatchProseClient client)
    {
        _client = client;
    }

    public async Task<PatchProseOutput> GenerateFromGitDiffAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var diff = GitDiffReader.Read(repositoryPath);

        if (string.IsNullOrWhiteSpace(diff))
        {
            throw new InvalidOperationException("No staged or unstaged git diff was found.");
        }

        return await _client.GenerateAsync(diff, cancellationToken);
    }
}
