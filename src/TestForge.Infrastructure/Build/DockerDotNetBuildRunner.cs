using System.Diagnostics;
using TestForge.Application.Build;

namespace TestForge.Infrastructure.Build;

public sealed class DockerDotNetBuildRunner : IDotNetBuildRunner
{
    private static readonly TimeSpan BuildTimeout =
        TimeSpan.FromMinutes(5);

    public async Task<BuildExecutionResult> BuildAsync(
        Guid testRunId,
        string workspacePath,
        string targetPath,
        CancellationToken cancellationToken = default)
    {
        var fullWorkspacePath = Path.GetFullPath(workspacePath);
        var fullTargetPath = Path.GetFullPath(
            Path.Combine(workspacePath, targetPath));

        if (!fullTargetPath.StartsWith(
                fullWorkspacePath + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Build target workspace dışında olamaz.");
        }

        var relativeTarget = Path.GetRelativePath(
            fullWorkspacePath,
            fullTargetPath);

        using var timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeoutSource.CancelAfter(BuildTimeout);

        using var process = new Process
        {
            StartInfo = CreateStartInfo(
                testRunId,
                fullWorkspacePath,
                relativeTarget)
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

            return new BuildExecutionResult(
                process.ExitCode == 0,
                process.ExitCode,
                stopwatch.ElapsedMilliseconds,
                output,
                error);
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            TryKill(process);

            return new BuildExecutionResult(
                false,
                -1,
                stopwatch.ElapsedMilliseconds,
                string.Empty,
                "Docker build operation timed out.");
        }
        catch (Exception exception)
        {
            TryKill(process);

            return new BuildExecutionResult(
                false,
                -1,
                stopwatch.ElapsedMilliseconds,
                string.Empty,
                exception.Message);
        }
    }

    private static ProcessStartInfo CreateStartInfo(
        Guid testRunId,
        string workspacePath,
        string targetPath)
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
            $"testforge-build-{testRunId:N}";

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
            "-v",
            $"{workspacePath}:/workspace",
            "-w",
            "/workspace",
            "mcr.microsoft.com/dotnet/sdk:8.0",
            "dotnet",
            "build",
            targetPath,
            "--nologo"
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
