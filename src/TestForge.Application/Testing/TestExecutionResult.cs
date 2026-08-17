namespace TestForge.Application.Testing;

public sealed record TestExecutionResult(
    bool IsSuccessful,
    int ExitCode,
    string TestProjectPath,
    long DurationMilliseconds,
    string StandardOutput,
    string StandardError,
    int PassedCount,
    int FailedCount,
    int SkippedCount);
