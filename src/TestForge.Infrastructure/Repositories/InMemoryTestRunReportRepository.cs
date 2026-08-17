using System.Collections.Concurrent;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;

namespace TestForge.Infrastructure.Repositories;

public sealed class InMemoryTestRunReportRepository :
    ITestRunReportRepository
{
    private readonly ConcurrentDictionary<Guid, TestRunReport> _reports = new();

    public Task<TestRunReport> GetOrCreateAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        var report = _reports.GetOrAdd(
            testRunId,
            TestRunReport.Create);

        return Task.FromResult(report);
    }

    public Task<TestRunReport?> GetByTestRunIdAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        _reports.TryGetValue(testRunId, out var report);
        return Task.FromResult(report);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
