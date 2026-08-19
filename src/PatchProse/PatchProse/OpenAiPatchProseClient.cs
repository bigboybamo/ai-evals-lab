using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace PatchProse;

public sealed class OpenAiPatchProseClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly string _model;
    private readonly bool _disposeClient;

    public OpenAiPatchProseClient()
        : this(
            new HttpClient(),
            Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                ?? throw new InvalidOperationException("Set OPENAI_API_KEY before calling the LLM."),
            Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-mini",
            disposeClient: true)
    {
    }

    public OpenAiPatchProseClient(HttpClient httpClient, string apiKey, string model, bool disposeClient = false)
    {
        _httpClient = httpClient;
        _model = model;
        _disposeClient = disposeClient;

        _httpClient.BaseAddress ??= new Uri("https://api.openai.com/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    /// <summary>
    /// Sends a git diff to the LLM and returns the parsed commit message and PR description.
    /// </summary>
    public async Task<PatchProseOutput> GenerateAsync(string diff, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(diff))
        {
            throw new ArgumentException("A git diff is required.", nameof(diff));
        }

        var request = new
        {
            model = _model,
            temperature = 0.2,
            response_format = new { type = "json_object" },
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = """
                    You write concise Git metadata from diffs.
                    Return only JSON with this shape:
                    {
                      "commitMessage": "type(scope): summary",
                      "pullRequestDescription": "Summary...\n\nFiles touched:\n- path/from/diff.cs"
                    }
                    The commit message must be a conventional commit.
                    The pull request description must mention only files present in the diff.
                    Do not invent behavior, files, tests, issues, or implementation details.
                    """
                },
                new
                {
                    role = "user",
                    content = $"Generate a commit message and pull request description for this diff:\n\n{diff}"
                }
            }
        };

        using var response = await _httpClient.PostAsync(
            "v1/chat/completions",
            new StringContent(JsonSerializer.Serialize(request, JsonOptions), Encoding.UTF8, "application/json"),
            cancellationToken);

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"LLM request failed: {(int)response.StatusCode} {responseBody}");
        }

        return ParseChatCompletion(responseBody);
    }

    public void Dispose()
    {
        if (_disposeClient)
        {
            _httpClient.Dispose();
        }
    }

    /// <summary>
    /// Extracts the JSON payload from an OpenAI chat-completion response.
    /// </summary>
    private static PatchProseOutput ParseChatCompletion(string responseBody)
    {
        using var responseJson = JsonDocument.Parse(responseBody);
        var content = responseJson
            .RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("The LLM response did not include content.");
        }

        using var outputJson = JsonDocument.Parse(content);

        return new PatchProseOutput(
            outputJson.RootElement.GetProperty("commitMessage").GetString()
                ?? throw new InvalidOperationException("The LLM response did not include commitMessage."),
            outputJson.RootElement.GetProperty("pullRequestDescription").GetString()
                ?? throw new InvalidOperationException("The LLM response did not include pullRequestDescription."));
    }
}
