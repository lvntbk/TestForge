namespace TestForge.Application.Git;

public interface IGitRepositoryCloner
{
    Task<GitCloneResult> CloneAsync(
        Guid testRunId,
        string repositoryUrl,
        CancellationToken cancellationToken = default);
}
