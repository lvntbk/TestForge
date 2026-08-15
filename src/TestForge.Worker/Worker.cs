using TestForge.Application.Analysis;
using TestForge.Application.Git;
using TestForge.Application.Repositories;

namespace TestForge.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGitRepositoryCloner _cloner;
    private readonly IProjectAnalyzer _analyzer;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IGitRepositoryCloner cloner,
        IProjectAnalyzer analyzer,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _cloner = cloner;
        _analyzer = analyzer;
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
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Worker processing error.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
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

        var cloneResult = await _cloner.CloneAsync(
            testRun.Id,
            testRun.RepositoryUrl,
            cancellationToken);

        if (!cloneResult.IsSuccessful)
        {
            testRun.MarkAsFailed(
                cloneResult.StandardError,
                DateTimeOffset.UtcNow);

            await repository.SaveChangesAsync(cancellationToken);
            return;
        }

        testRun.MarkAsAnalyzing();
        await repository.SaveChangesAsync(cancellationToken);

        var analysis = await _analyzer.AnalyzeAsync(
            cloneResult.WorkspacePath,
            cancellationToken);

        _logger.LogInformation(
            "Analysis completed. Solutions: {SolutionCount}, Projects: {ProjectCount}, Web: {WebCount}, Tests: {TestCount}",
            analysis.SolutionPaths.Count,
            analysis.ProjectPaths.Count,
            analysis.WebProjectPaths.Count,
            analysis.TestProjectPaths.Count);

        if (!analysis.IsSupported)
        {
            testRun.MarkAsFailed(
                "Desteklenen ASP.NET Core Web API projesi bulunamadı.",
                DateTimeOffset.UtcNow);

            await repository.SaveChangesAsync(cancellationToken);
            return;
        }

        testRun.MarkAsBuilding();
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Test run {TestRunId} moved to Building.",
            testRun.Id);
    }
}
