namespace TestForge.Application.Build;

public interface IDotNetBuildRunner
{
    Task<BuildExecutionResult> BuildAsync(
        Guid testRunId,
        string workspacePath,
        string targetPath,
        CancellationToken cancellationToken = default);
}
