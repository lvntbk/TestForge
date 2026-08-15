using TestForge.Domain.Entities;
using TestForge.Domain.Enums;

namespace TestForge.Tests.Domain;

public sealed class TestRunTests
{
    [Fact]
    public void Create_WithValidRepositoryUrl_CreatesQueuedTestRun()
    {
        // Arrange
        const string repositoryUrl =
            "https://github.com/example/sample-api";

        var createdAtUtc =
            new DateTimeOffset(2026, 8, 15, 9, 0, 0, TimeSpan.Zero);

        // Act
        var testRun = TestRun.Create(repositoryUrl, createdAtUtc);

        // Assert
        Assert.NotEqual(Guid.Empty, testRun.Id);
        Assert.Equal(repositoryUrl, testRun.RepositoryUrl);
        Assert.Equal(TestRunStatus.Queued, testRun.Status);
        Assert.Equal(createdAtUtc, testRun.CreatedAtUtc);
        Assert.Null(testRun.StartedAtUtc);
        Assert.Null(testRun.CompletedAtUtc);
        Assert.Null(testRun.ErrorMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyRepositoryUrl_ThrowsArgumentException(
        string repositoryUrl)
    {
        // Act
        var action = () =>
            TestRun.Create(repositoryUrl, DateTimeOffset.UtcNow);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }
}
