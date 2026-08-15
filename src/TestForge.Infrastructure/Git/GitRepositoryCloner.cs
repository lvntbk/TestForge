using System.Diagnostics;
using TestForge.Application.Git;

namespace TestForge.Infrastructure.Git;

public sealed class GitRepositoryCloner : IGitRepositoryCloner
{
    private static readonly TimeSpan CloneTimeout =
        TimeSpan.FromMinutes(2);

    private readonly string _workspaceRoot;

    public GitRepositoryCloner()
    {
        _workspaceRoot = Path.Combine(
            Path.GetTempPath(),
            "testforge",
            "workspaces");
    }

    public async Task<GitCloneResult> CloneAsync(
        Guid testRunId,
        string repositoryUrl,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_workspaceRoot);

        var workspacePath = Path.Combine(
            _workspaceRoot,
            testRunId.ToString("N"));

        if (Directory.Exists(workspacePath))
        {
            Directory.Delete(workspacePath, recursive: true);
        }

        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutSource.CancelAfter(CloneTimeout);

        using var process = new Process
        {
            StartInfo = CreateStartInfo(
                repositoryUrl,
                workspacePath)
        };

        try
        {
            process.Start();

            var standardOutputTask =
                process.StandardOutput.ReadToEndAsync();

            var standardErrorTask =
                process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(timeoutSource.Token);

            var standardOutput = await standardOutputTask;
            var standardError = await standardErrorTask;

            return new GitCloneResult(
                process.ExitCode == 0,
                workspacePath,
                standardOutput,
                standardError);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);

            return new GitCloneResult(
                false,
                workspacePath,
                string.Empty,
                "Repository clone operation timed out.");
        }
        catch (Exception exception)
        {
            TryKill(process);

            return new GitCloneResult(
                false,
                workspacePath,
                string.Empty,
                exception.Message);
        }
    }

    private static ProcessStartInfo CreateStartInfo(
        string repositoryUrl,
        string workspacePath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("clone");
        startInfo.ArgumentList.Add("--depth");
        startInfo.ArgumentList.Add("1");
        startInfo.ArgumentList.Add("--no-tags");
        startInfo.ArgumentList.Add("--");
        startInfo.ArgumentList.Add(repositoryUrl);
        startInfo.ArgumentList.Add(workspacePath);

        return startInfo;
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Process already stopped.
        }
    }
}
