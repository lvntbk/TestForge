namespace TestForge.Application.Testing;

public interface ITestResultParser
{
    TestResultCounts Parse(string resultFilePath);
}
