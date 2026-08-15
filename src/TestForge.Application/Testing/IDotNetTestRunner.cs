namespace TestForge.Application.Testing;

public interface IDotNetTestRunner
{
    Task<TestExecutionResult> RunAsync(
        Guid testRunId,
        string workspacePath,
        string testProjectPath,
        CancellationToken cancellationToken = default);
}
