using System.Collections.Concurrent;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;
using TestForge.Domain.Enums;

namespace TestForge.Infrastructure.Repositories;

public sealed class InMemoryTestRunRepository : ITestRunRepository
{
    private readonly ConcurrentDictionary<Guid, TestRun> _testRuns = new();

    public Task AddAsync(
        TestRun testRun,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testRun);

        if (!_testRuns.TryAdd(testRun.Id, testRun))
        {
            throw new InvalidOperationException(
                $"Test run already exists: {testRun.Id}");
        }

        return Task.CompletedTask;
    }

    public Task<TestRun?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        _testRuns.TryGetValue(id, out var testRun);

        return Task.FromResult(testRun);
    }

    public Task<TestRun?> GetNextQueuedAsync(
        CancellationToken cancellationToken = default)
    {
        var testRun = _testRuns.Values
            .Where(item => item.Status == TestRunStatus.Queued)
            .OrderBy(item => item.CreatedAtUtc)
            .FirstOrDefault();

        return Task.FromResult(testRun);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
