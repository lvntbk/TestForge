using TestForge.Application.Repositories;

namespace TestForge.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
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
                    "An error occurred while processing a test run.");

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
            "Test run {TestRunId} moved to Cloning. Repository: {RepositoryUrl}",
            testRun.Id,
            testRun.RepositoryUrl);
    }
}
