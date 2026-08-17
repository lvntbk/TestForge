using TestForge.Application.Analysis;
using TestForge.Application.Build;
using TestForge.Application.Git;
using TestForge.Application.Repositories;
using TestForge.Application.Testing;
using TestRunEntity = TestForge.Domain.Entities.TestRun;

namespace TestForge.Worker;

public sealed class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IGitRepositoryCloner _cloner;
    private readonly IProjectAnalyzer _analyzer;
    private readonly IDotNetBuildRunner _buildRunner;
    private readonly IDotNetTestRunner _testRunner;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IGitRepositoryCloner cloner,
        IProjectAnalyzer analyzer,
        IDotNetBuildRunner buildRunner,
        IDotNetTestRunner testRunner,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _cloner = cloner;
        _analyzer = analyzer;
        _buildRunner = buildRunner;
        _testRunner = testRunner;
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
        var reportRepository = scope.ServiceProvider
            .GetRequiredService<ITestRunReportRepository>();

        var testingTestRun =
            await repository.GetNextTestingAsync(cancellationToken);

        if (testingTestRun is not null)
        {
            await RunTestsAsync(
                testingTestRun,
                repository,
                reportRepository,
                cancellationToken);

            return;
        }

        var buildingTestRun =
            await repository.GetNextBuildingAsync(cancellationToken);

        if (buildingTestRun is not null)
        {
            await BuildAsync(
                buildingTestRun,
                repository,
                reportRepository,
                cancellationToken);

            return;
        }

        var queuedTestRun =
            await repository.GetNextQueuedAsync(cancellationToken);

        if (queuedTestRun is not null)
        {
            await CloneAndAnalyzeAsync(
                queuedTestRun,
                repository,
                cancellationToken);
        }
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
            await FailAsync(
                testRun,
                repository,
                cloneResult.StandardError,
                cancellationToken);

            return;
        }

        testRun.MarkAsAnalyzing();
        await repository.SaveChangesAsync(cancellationToken);

        var analysis = await _analyzer.AnalyzeAsync(
            cloneResult.WorkspacePath,
            cancellationToken);

        if (!analysis.IsSupported)
        {
            await FailAsync(
                testRun,
                repository,
                "Desteklenen ASP.NET Core Web API projesi bulunamadı.",
                cancellationToken);

            return;
        }

        testRun.MarkAsBuilding();
        await repository.SaveChangesAsync(cancellationToken);
    }

    private async Task BuildAsync(
        TestRunEntity testRun,
        ITestRunRepository repository,
        ITestRunReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        var workspacePath = GetWorkspacePath(testRun.Id);

        var analysis = await _analyzer.AnalyzeAsync(
            workspacePath,
            cancellationToken);

        var targetPath = analysis.WebProjectPaths.First();

        _logger.LogInformation(
            "Building {TargetPath} in Docker for {TestRunId}.",
            targetPath,
            testRun.Id);

        var result = await _buildRunner.BuildAsync(
            testRun.Id,
            workspacePath,
            targetPath,
            cancellationToken);

        var report = await reportRepository.GetOrCreateAsync(
            testRun.Id,
            cancellationToken);

        report.RecordBuild(
            targetPath,
            result.ExitCode,
            result.DurationMilliseconds,
            result.StandardOutput,
            result.StandardError);

        await reportRepository.SaveChangesAsync(cancellationToken);

        if (!result.IsSuccessful)
        {
            await FailAsync(
                testRun,
                repository,
                CombineDiagnostics(
                    result.StandardError,
                    result.StandardOutput),
                cancellationToken);

            return;
        }

        testRun.MarkAsTesting();
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Docker build succeeded for {TestRunId}.",
            testRun.Id);
    }

    private async Task RunTestsAsync(
        TestRunEntity testRun,
        ITestRunRepository repository,
        ITestRunReportRepository reportRepository,
        CancellationToken cancellationToken)
    {
        var workspacePath = GetWorkspacePath(testRun.Id);

        var analysis = await _analyzer.AnalyzeAsync(
            workspacePath,
            cancellationToken);

        var selectedTestProjects =
            TestProjectSelector.Select(
                analysis.TestProjectPaths);

        if (selectedTestProjects.Count == 0)
        {
            _logger.LogWarning(
                "No relevant test projects found for {TestRunId}.",
                testRun.Id);

            testRun.MarkAsCompleted(DateTimeOffset.UtcNow);
            await repository.SaveChangesAsync(cancellationToken);
            return;
        }

        foreach (var testProjectPath in selectedTestProjects)
        {
            _logger.LogInformation(
                "Running {TestProjectPath} in Docker.",
                testProjectPath);

            var result = await _testRunner.RunAsync(
                testRun.Id,
                workspacePath,
                testProjectPath,
                cancellationToken);

            var report = await reportRepository.GetOrCreateAsync(
                testRun.Id,
                cancellationToken);

            report.RecordTest(
                result.TestProjectPath,
                result.ExitCode,
                result.DurationMilliseconds,
                result.StandardOutput,
                result.StandardError);

            await reportRepository.SaveChangesAsync(cancellationToken);

            if (!result.IsSuccessful)
            {
                await FailAsync(
                    testRun,
                    repository,
                    CombineDiagnostics(
                        result.StandardError,
                        result.StandardOutput),
                    cancellationToken);

                return;
            }

            _logger.LogInformation(
                "Tests passed: {TestProjectPath}.",
                testProjectPath);
        }

        testRun.MarkAsCompleted(DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Test run {TestRunId} completed successfully.",
            testRun.Id);
    }


    private static async Task FailAsync(
        TestRunEntity testRun,
        ITestRunRepository repository,
        string error,
        CancellationToken cancellationToken)
    {
        testRun.MarkAsFailed(error, DateTimeOffset.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);
    }

    private static string GetWorkspacePath(Guid testRunId)
    {
        return Path.Combine(
            Path.GetTempPath(),
            "testforge",
            "workspaces",
            testRunId.ToString("N"));
    }

    private static string CombineDiagnostics(
        string standardError,
        string standardOutput)
    {
        var diagnostics = string.Join(
            Environment.NewLine,
            standardError,
            standardOutput);

        return diagnostics.Length > 4000
            ? diagnostics[^4000..]
            : diagnostics;
    }
}
