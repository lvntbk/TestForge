using Microsoft.EntityFrameworkCore;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;
using TestForge.Infrastructure.Persistence;

namespace TestForge.Infrastructure.Repositories;

public sealed class PostgresTestRunReportRepository :
    ITestRunReportRepository
{
    private readonly TestForgeDbContext _dbContext;

    public PostgresTestRunReportRepository(TestForgeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TestRunReport> GetOrCreateAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        var report = await _dbContext.TestRunReports
            .SingleOrDefaultAsync(
                item => item.TestRunId == testRunId,
                cancellationToken);

        if (report is not null)
        {
            return report;
        }

        report = TestRunReport.Create(testRunId);
        await _dbContext.TestRunReports.AddAsync(report, cancellationToken);
        return report;
    }

    public Task<TestRunReport?> GetByTestRunIdAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.TestRunReports
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.TestRunId == testRunId,
                cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
