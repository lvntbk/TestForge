using TestForge.Domain.Enums;

namespace TestForge.Domain.Entities;

public sealed class TestRun
{
    private TestRun()
    {
    }

    private TestRun(Guid id, string repositoryUrl, DateTimeOffset createdAtUtc)
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
