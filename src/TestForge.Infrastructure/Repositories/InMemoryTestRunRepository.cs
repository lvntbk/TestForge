using System.Collections.Concurrent;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;

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
}
