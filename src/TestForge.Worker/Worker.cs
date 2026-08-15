using TestForge.Application.Git;
using TestForge.Application.Repositories;

namespace TestForge.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGitRepositoryCloner _cloner;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IGitRepositoryCloner cloner,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _cloner = cloner;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation("TestForge worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessNextTestRunAsync(stoppingToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Worker processing error.");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }

        _logger.LogInformation("TestForge worker stopped.");
    }

    private async Task ProcessNextTestRunAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<ITestRunRepository>();

        var testRun = await repository.GetNextQueuedAsync(
            cancellationToken);

        if (testRun is null)
        {
            return;
        }

        testRun.StartCloning(DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cloning repository for test run {TestRunId}.",
            testRun.Id);

        var result = await _cloner.CloneAsync(
            testRun.Id,
            testRun.RepositoryUrl,
            cancellationToken);

        if (!result.IsSuccessful)
        {
            testRun.MarkAsFailed(
                result.StandardError,
                DateTimeOffset.UtcNow);

            await repository.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                "Clone failed for {TestRunId}: {Error}",
                testRun.Id,
                result.StandardError);

            return;
        }

        testRun.MarkAsAnalyzing();
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Repository cloned to {WorkspacePath}.",
            result.WorkspacePath);
    }
}
