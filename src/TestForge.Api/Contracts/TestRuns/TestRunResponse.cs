using TestForge.Domain.Entities;

namespace TestForge.Api.Contracts.TestRuns;

public sealed record TestRunResponse(
    Guid Id,
    string RepositoryUrl,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? ErrorMessage)
{
    public static TestRunResponse FromEntity(TestRun testRun)
    {
        return new TestRunResponse(
            testRun.Id,
            testRun.RepositoryUrl,
            testRun.Status.ToString(),
            testRun.CreatedAtUtc,
            testRun.StartedAtUtc,
            testRun.CompletedAtUtc,
            testRun.ErrorMessage);
    }
}
