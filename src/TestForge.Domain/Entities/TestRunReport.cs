namespace TestForge.Domain.Entities;

public sealed class TestRunReport
{
    private TestRunReport()
    {
    }

    private TestRunReport(Guid testRunId)
    {
        TestRunId = testRunId;
    }

    public Guid TestRunId { get; private set; }
    public string? BuildProjectPath { get; private set; }
    public int? BuildExitCode { get; private set; }
    public long? BuildDurationMilliseconds { get; private set; }
    public string? BuildStandardOutput { get; private set; }
    public string? BuildStandardError { get; private set; }
    public string TestProjectPaths { get; private set; } = string.Empty;
    public int? TestExitCode { get; private set; }
    public long? TestDurationMilliseconds { get; private set; }
    public string? TestStandardOutput { get; private set; }
    public string? TestStandardError { get; private set; }
    public int? PassedCount { get; private set; }
    public int? FailedCount { get; private set; }
    public int? SkippedCount { get; private set; }

    public static TestRunReport Create(Guid testRunId)
    {
        if (testRunId == Guid.Empty)
        {
            throw new ArgumentException(
                "Test run kimliği boş olamaz.",
                nameof(testRunId));
        }

        return new TestRunReport(testRunId);
    }

    public void RecordBuild(
        string projectPath,
        int exitCode,
        long durationMilliseconds,
        string standardOutput,
        string standardError)
    {
        BuildProjectPath = projectPath;
        BuildExitCode = exitCode;
        BuildDurationMilliseconds = durationMilliseconds;
        BuildStandardOutput = standardOutput;
        BuildStandardError = standardError;
    }

    public void RecordTest(
        string projectPath,
        int exitCode,
        long durationMilliseconds,
        string standardOutput,
        string standardError)
    {
        TestProjectPaths = AppendLine(TestProjectPaths, projectPath);
        TestExitCode = exitCode;
        TestDurationMilliseconds =
            (TestDurationMilliseconds ?? 0) + durationMilliseconds;
        TestStandardOutput = AppendLine(TestStandardOutput, standardOutput);
        TestStandardError = AppendLine(TestStandardError, standardError);
    }

    private static string AppendLine(string? current, string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return current ?? string.Empty;
        }

        return string.IsNullOrEmpty(current)
            ? value
            : string.Join(Environment.NewLine, current, value);
    }
}
