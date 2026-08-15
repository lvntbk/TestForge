using TestForge.Domain.Entities;

namespace TestForge.Application.Repositories;

public interface ITestRunRepository
{
    Task AddAsync(
        TestRun testRun,
        CancellationToken cancellationToken = default);

    Task<TestRun?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<TestRun?> GetNextQueuedAsync(
        CancellationToken cancellationToken = default);

    Task<TestRun?> GetNextBuildingAsync(
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
