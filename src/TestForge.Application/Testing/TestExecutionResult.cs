namespace TestForge.Application.Testing;

public sealed record TestExecutionResult(
    bool IsSuccessful,
    int ExitCode,
    string TestProjectPath,
    string StandardOutput,
    string StandardError);
