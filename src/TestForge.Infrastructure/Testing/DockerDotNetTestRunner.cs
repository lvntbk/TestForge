using System.Diagnostics;
using TestForge.Application.Testing;

namespace TestForge.Infrastructure.Testing;

public sealed class DockerDotNetTestRunner : IDotNetTestRunner
{
    private static readonly TimeSpan TestTimeout =
        TimeSpan.FromMinutes(5);

    public async Task<TestExecutionResult> RunAsync(
        Guid testRunId,
        string workspacePath,
        string testProjectPath,
        CancellationToken cancellationToken = default)
    {
        var fullWorkspacePath = Path.GetFullPath(workspacePath);

        var fullTestProjectPath = Path.GetFullPath(
            Path.Combine(workspacePath, testProjectPath));

        if (!fullTestProjectPath.StartsWith(
                fullWorkspacePath + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Test projesi workspace dışında olamaz.");
        }

        var relativeTestProjectPath = Path.GetRelativePath(
            fullWorkspacePath,
            fullTestProjectPath);

        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutSource.CancelAfter(TestTimeout);

        using var process = new Process
        {
            StartInfo = CreateStartInfo(
                testRunId,
                fullWorkspacePath,
                relativeTestProjectPath)
        };

        var stopwatch = Stopwatch.StartNew();

        try
        {
            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync(timeoutSource.Token);

            var output = await outputTask;
            var error = await errorTask;

            return new TestExecutionResult(
                process.ExitCode == 0,
                process.ExitCode,
                relativeTestProjectPath,
                stopwatch.ElapsedMilliseconds,
                output,
                error);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);

            return new TestExecutionResult(
                false,
                -1,
                relativeTestProjectPath,
                stopwatch.ElapsedMilliseconds,
                string.Empty,
                "Docker test operation timed out.");
        }
        catch (Exception exception)
        {
            TryKill(process);

            return new TestExecutionResult(
                false,
                -1,
                relativeTestProjectPath,
                stopwatch.ElapsedMilliseconds,
                string.Empty,
                exception.Message);
        }
    }

    private static ProcessStartInfo CreateStartInfo(
        Guid testRunId,
        string workspacePath,
        string testProjectPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var containerName =
            $"testforge-test-{testRunId:N}";

        string[] arguments =
        [
            "run",
            "--rm",
            "--name",
            containerName,
            "--cpus",
            "1",
            "--memory",
            "1g",
            "--pids-limit",
            "256",
            "--cap-drop",
            "ALL",
            "--security-opt",
            "no-new-privileges",
            "--user",
            "1000:1000",
            "-e",
            "DOTNET_CLI_HOME=/workspace/.testforge/dotnet",
            "-e",
            "NUGET_PACKAGES=/workspace/.testforge/nuget",
            "-e",
            "NUGET_HTTP_CACHE_PATH=/workspace/.testforge/http-cache",
            "-v",
            $"{workspacePath}:/workspace",
            "-w",
            "/workspace",
            "mcr.microsoft.com/dotnet/sdk:8.0",
            "dotnet",
            "test",
            testProjectPath,
            "--nologo",
            "--logger",
            "console;verbosity=normal"
        ];

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

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
