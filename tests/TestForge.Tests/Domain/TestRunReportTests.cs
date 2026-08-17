using TestForge.Domain.Entities;

namespace TestForge.Tests.Domain;

public sealed class TestRunReportTests
{
    [Fact]
    public void RecordBuild_StoresBuildResult()
    {
        var report = TestRunReport.Create(Guid.NewGuid());

        report.RecordBuild("src/Api.csproj", 1, 1500, "output", "error");

        Assert.Equal("src/Api.csproj", report.BuildProjectPath);
        Assert.Equal(1, report.BuildExitCode);
        Assert.Equal(1500, report.BuildDurationMilliseconds);
        Assert.Equal("output", report.BuildStandardOutput);
        Assert.Equal("error", report.BuildStandardError);
    }

    [Fact]
    public void RecordTest_MultipleResults_AccumulatesDurationAndLogs()
    {
        var report = TestRunReport.Create(Guid.NewGuid());

        report.RecordTest("tests/A.csproj", 0, 500, "A output", "");
        report.RecordTest("tests/B.csproj", 1, 750, "B output", "B error");

        Assert.Equal(1250, report.TestDurationMilliseconds);
        Assert.Equal(1, report.TestExitCode);
        Assert.Contains("tests/A.csproj", report.TestProjectPaths);
        Assert.Contains("tests/B.csproj", report.TestProjectPaths);
        Assert.Contains("A output", report.TestStandardOutput);
        Assert.Contains("B output", report.TestStandardOutput);
        Assert.Equal("B error", report.TestStandardError);
    }
}
