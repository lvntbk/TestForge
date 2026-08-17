namespace TestForge.Application.Build;

public sealed record BuildExecutionResult(
    bool IsSuccessful,
    int ExitCode,
    long DurationMilliseconds,
    string StandardOutput,
    string StandardError);
