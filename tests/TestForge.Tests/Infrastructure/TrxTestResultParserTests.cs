using TestForge.Infrastructure.Testing;

namespace TestForge.Tests.Infrastructure;

public sealed class TrxTestResultParserTests
{
    [Fact]
    public void Parse_WithValidCounters_ReturnsTestCounts()
    {
        // Arrange
        const string trxContent =
            """
            <?xml version="1.0" encoding="utf-8"?>
            <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
              <ResultSummary outcome="Failed">
                <Counters
                    total="10"
                    executed="9"
                    passed="6"
                    failed="2"
                    notExecuted="1" />
              </ResultSummary>
            </TestRun>
            """;

        var trxFilePath = Path.GetTempFileName();

        try
        {
            File.WriteAllText(trxFilePath, trxContent);

            var parser = new TrxTestResultParser();

            // Act
            var result = parser.Parse(trxFilePath);

            // Assert
            Assert.Equal(6, result.PassedCount);
            Assert.Equal(2, result.FailedCount);
            Assert.Equal(1, result.SkippedCount);
        }
        finally
        {
            File.Delete(trxFilePath);
        }
    }
}
