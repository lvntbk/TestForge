using TestForge.Domain.Enums;

namespace TestForge.Domain.Entities;

public sealed class TestRun
{
    private TestRun()
    {
    }

    private TestRun(
        Guid id,
        string repositoryUrl,
        DateTimeOffset createdAtUtc)
    {
        Id = id;
        RepositoryUrl = repositoryUrl;
        Status = TestRunStatus.Queued;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid Id { get; private set; }

    public string RepositoryUrl { get; private set; } = string.Empty;

    public TestRunStatus Status { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? StartedAtUtc { get; private set; }

    public DateTimeOffset? CompletedAtUtc { get; private set; }

    public string? ErrorMessage { get; private set; }

    public void StartCloning(DateTimeOffset startedAtUtc)
    {
        if (Status != TestRunStatus.Queued)
        {
            throw new InvalidOperationException(
                $"Test run cannot start cloning from status {Status}.");
        }

        Status = TestRunStatus.Cloning;
        StartedAtUtc = startedAtUtc;
        ErrorMessage = null;
    }

    public void MarkAsAnalyzing()
    {
        if (Status != TestRunStatus.Cloning)
        {
            throw new InvalidOperationException(
                $"Test run cannot start analysis from status {Status}.");
        }

        Status = TestRunStatus.Analyzing;
    }

    public void MarkAsBuilding()
    {
        if (Status != TestRunStatus.Analyzing)
        {
            throw new InvalidOperationException(
                $"Test run cannot start building from status {Status}.");
        }

        Status = TestRunStatus.Building;
    }

    public void MarkAsFailed(
        string errorMessage,
        DateTimeOffset completedAtUtc)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            errorMessage = "Unknown processing error.";
        }

        Status = TestRunStatus.Failed;

        ErrorMessage = errorMessage.Length > 4000
            ? errorMessage[..4000]
            : errorMessage;

        CompletedAtUtc = completedAtUtc;
    }

    public static TestRun Create(
        string repositoryUrl,
        DateTimeOffset createdAtUtc)
    {
        if (string.IsNullOrWhiteSpace(repositoryUrl))
        {
            throw new ArgumentException(
                "Repository URL boş olamaz.",
                nameof(repositoryUrl));
        }

        return new TestRun(
            Guid.NewGuid(),
            repositoryUrl.Trim(),
            createdAtUtc);
    }
}
