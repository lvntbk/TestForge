using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TestForge.Api.Contracts.TestRuns;
using TestForge.Application.Repositories;

namespace TestForge.Tests.Integration;

public sealed class TestRunsEndpointTests :
    IClassFixture<TestForgeApiFactory>
{
    private readonly HttpClient _client;
    private readonly TestForgeApiFactory _factory;

    public TestRunsEndpointTests(
        TestForgeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_WithValidGitHubUrl_ReturnsAcceptedTestRun()
    {
        // Arrange
        var request = new
        {
            RepositoryUrl = "https://github.com/kubeltd/distkeep"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/test-runs",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var testRun = await response.Content
            .ReadFromJsonAsync<TestRunResponse>();

        Assert.NotNull(testRun);
        Assert.NotEqual(Guid.Empty, testRun.Id);
        Assert.Equal(request.RepositoryUrl, testRun.RepositoryUrl);
        Assert.Equal("Queued", testRun.Status);
        Assert.Null(testRun.StartedAtUtc);
        Assert.Null(testRun.CompletedAtUtc);
        Assert.Null(testRun.ErrorMessage);
    }

    [Fact]
    public async Task Create_WithInvalidGitHubUrl_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            RepositoryUrl = "https://google.com/test"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/test-runs",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_AfterCreate_ReturnsCreatedTestRun()
    {
        // Arrange
        var request = new
        {
            RepositoryUrl = "https://github.com/kubeltd/distkeep"
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/test-runs",
            request);

        var createdTestRun = await createResponse.Content
            .ReadFromJsonAsync<TestRunResponse>();

        Assert.NotNull(createdTestRun);

        // Act
        var getResponse = await _client.GetAsync(
            $"/api/test-runs/{createdTestRun.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedTestRun = await getResponse.Content
            .ReadFromJsonAsync<TestRunResponse>();

        Assert.NotNull(fetchedTestRun);
        Assert.Equal(createdTestRun.Id, fetchedTestRun.Id);
        Assert.Equal(request.RepositoryUrl, fetchedTestRun.RepositoryUrl);
        Assert.Equal("Queued", fetchedTestRun.Status);
    }

    [Fact]
    public async Task GetById_WithUnknownId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync(
            $"/api/test-runs/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReport_AfterReportIsRecorded_ReturnsReport()
    {
        var createResponse = await _client.PostAsJsonAsync(
            "/api/test-runs",
            new { RepositoryUrl = "https://github.com/kubeltd/distkeep" });

        var testRun = await createResponse.Content
            .ReadFromJsonAsync<TestRunResponse>();

        Assert.NotNull(testRun);

        var reports = _factory.Services
            .GetRequiredService<ITestRunReportRepository>();
        var report = await reports.GetOrCreateAsync(testRun.Id);
        report.RecordBuild("src/Sample.Api.csproj", 0, 1250, "ok", "");
        report.RecordTest("tests/Sample.Tests.csproj", 0, 800, "passed", "");
        await reports.SaveChangesAsync();

        var response = await _client.GetAsync(
            $"/api/test-runs/{testRun.Id}/report");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<TestRunReportResponse>();

        Assert.NotNull(result);
        Assert.Equal(testRun.Id, result.TestRunId);
        Assert.Equal(0, result.BuildExitCode);
        Assert.Equal(1250, result.BuildDurationMilliseconds);
        Assert.Single(result.TestProjectPaths);
        Assert.Equal("tests/Sample.Tests.csproj", result.TestProjectPaths[0]);
    }

    [Fact]
    public async Task GetReport_BeforeProcessing_ReturnsNotFound()
    {
        var response = await _client.GetAsync(
            $"/api/test-runs/{Guid.NewGuid()}/report");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
