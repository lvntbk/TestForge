using Microsoft.EntityFrameworkCore;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;
using TestForge.Domain.Enums;
using TestForge.Infrastructure.Persistence;

namespace TestForge.Infrastructure.Repositories;

public sealed class PostgresTestRunRepository : ITestRunRepository
{
    private readonly TestForgeDbContext _dbContext;

    public PostgresTestRunRepository(TestForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        TestRun testRun,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testRun);

        await _dbContext.TestRuns.AddAsync(
            testRun,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<TestRun?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TestRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(
                testRun => testRun.Id == id,
                cancellationToken);
    }

    public Task<TestRun?> GetNextQueuedAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TestRuns
            .Where(testRun =>
                testRun.Status == TestRunStatus.Queued)
            .OrderBy(testRun => testRun.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TestRun?> GetNextBuildingAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TestRuns
            .Where(testRun =>
                testRun.Status == TestRunStatus.Building)
            .OrderBy(testRun => testRun.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<TestRun?> GetNextTestingAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TestRuns
            .Where(testRun =>
                testRun.Status == TestRunStatus.Testing)
            .OrderBy(testRun => testRun.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
