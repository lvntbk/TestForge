using TestForge.Domain.Entities;

namespace TestForge.Application.Repositories;

public interface ITestRunReportRepository
{
    Task<TestRunReport> GetOrCreateAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default);

    Task<TestRunReport?> GetByTestRunIdAsync(
        Guid testRunId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
