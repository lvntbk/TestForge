namespace TestForge.Application.Build;

public sealed record BuildExecutionResult(
    bool IsSuccessful,
    int ExitCode,
    string StandardOutput,
    string StandardError);
