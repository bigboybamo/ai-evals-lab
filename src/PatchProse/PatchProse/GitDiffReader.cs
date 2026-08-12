using System.Diagnostics;

namespace PatchProse;

public static class GitDiffReader
{
    public static string Read(string repositoryPath)
    {
        if (string.IsNullOrWhiteSpace(repositoryPath))
        {
            throw new ArgumentException("A repository path is required.", nameof(repositoryPath));
        }

        var staged = RunGitDiff(repositoryPath, "--cached");
        var unstaged = RunGitDiff(repositoryPath);

        return string.Join(
            Environment.NewLine,
            new[] { staged, unstaged }.Where(diff => !string.IsNullOrWhiteSpace(diff)));
    }

    private static string RunGitDiff(string repositoryPath, string? argument = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            WorkingDirectory = repositoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        startInfo.ArgumentList.Add("diff");

        if (!string.IsNullOrWhiteSpace(argument))
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start git.");

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"git diff failed: {error}");
        }

        return output.Trim();
    }
}
