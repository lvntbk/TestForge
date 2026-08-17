namespace TestForge.Application.Testing;

public sealed record TestResultCounts(
    int PassedCount,
    int FailedCount,
    int SkippedCount);
