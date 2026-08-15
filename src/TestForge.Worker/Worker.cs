using TestForge.Application.Analysis;
using TestForge.Application.Build;
using TestForge.Application.Git;
using TestForge.Application.Repositories;
using TestRunEntity = TestForge.Domain.Entities.TestRun;

namespace TestForge.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGitRepositoryCloner _cloner;
    private readonly IProjectAnalyzer _analyzer;
    private readonly IDotNetBuildRunner _buildRunner;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IGitRepositoryCloner cloner,
        IProjectAnalyzer analyzer,
        IDotNetBuildRunner buildRunner,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _cloner = cloner;
        _analyzer = analyzer;
        _buildRunner = buildRunner;
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

        var buildingTestRun =
            await repository.GetNextBuildingAsync(cancellationToken);

        if (buildingTestRun is not null)
        {
            await BuildAsync(
                buildingTestRun,
                repository,
                cancellationToken);

            return;
        }

        var queuedTestRun =
            await repository.GetNextQueuedAsync(cancellationToken);

        if (queuedTestRun is null)
        {
            return;
        }

        await CloneAndAnalyzeAsync(
            queuedTestRun,
            repository,
            cancellationToken);
    }

    private async Task CloneAndAnalyzeAsync(
        TestRunEntity testRun,
        ITestRunRepository repository,
        CancellationToken cancellationToken)
    {
        testRun.StartCloning(DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

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
            "Analysis: Solutions={Solutions}, Projects={Projects}, Web={Web}, Tests={Tests}",
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
    }

    private async Task BuildAsync(
        TestRunEntity testRun,
        ITestRunRepository repository,
        CancellationToken cancellationToken)
    {
        var workspacePath = Path.Combine(
            Path.GetTempPath(),
            "testforge",
            "workspaces",
            testRun.Id.ToString("N"));

        var analysis = await _analyzer.AnalyzeAsync(
            workspacePath,
            cancellationToken);

        var targetPath =
            analysis.WebProjectPaths.First();

        _logger.LogInformation(
            "Building {TargetPath} in Docker for {TestRunId}.",
            targetPath,
            testRun.Id);

        var result = await _buildRunner.BuildAsync(
            testRun.Id,
            workspacePath,
            targetPath,
            cancellationToken);

        if (!result.IsSuccessful)
        {
            var diagnostics = string.Join(
                Environment.NewLine,
                result.StandardError,
                result.StandardOutput);

            if (diagnostics.Length > 4000)
            {
                diagnostics = diagnostics[^4000..];
            }

            testRun.MarkAsFailed(
                diagnostics,
                DateTimeOffset.UtcNow);

            await repository.SaveChangesAsync(cancellationToken);

            _logger.LogError(
                "Docker build failed with exit code {ExitCode}.",
                result.ExitCode);

            return;
        }

        testRun.MarkAsTesting();
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Docker build succeeded for {TestRunId}.",
            testRun.Id);
    }
}
