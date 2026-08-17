using TestForge.Domain.Entities;

namespace TestForge.Api.Contracts.TestRuns;

public sealed record TestRunReportResponse(
    Guid TestRunId,
    string? BuildProjectPath,
    int? BuildExitCode,
    long? BuildDurationMilliseconds,
    string? BuildStandardOutput,
    string? BuildStandardError,
    IReadOnlyList<string> TestProjectPaths,
    int? TestExitCode,
    long? TestDurationMilliseconds,
    string? TestStandardOutput,
    string? TestStandardError,
    int? PassedCount,
    int? FailedCount,
    int? SkippedCount)
{
    public static TestRunReportResponse FromEntity(TestRunReport report)
    {
        var paths = report.TestProjectPaths.Split(
            ["\r\n", "\n"],
            StringSplitOptions.RemoveEmptyEntries);

        return new TestRunReportResponse(
            report.TestRunId,
            report.BuildProjectPath,
            report.BuildExitCode,
            report.BuildDurationMilliseconds,
            report.BuildStandardOutput,
            report.BuildStandardError,
            paths,
            report.TestExitCode,
            report.TestDurationMilliseconds,
            report.TestStandardOutput,
            report.TestStandardError,
            report.PassedCount,
            report.FailedCount,
            report.SkippedCount);
    }
}
