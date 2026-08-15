using Microsoft.EntityFrameworkCore;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;
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
                x => x.Id == id,
                cancellationToken);
    }
}
