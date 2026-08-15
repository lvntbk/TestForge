using System.Net;
using System.Net.Http.Json;
using TestForge.Api.Contracts.TestRuns;

namespace TestForge.Tests.Integration;

public sealed class TestRunsEndpointTests :
    IClassFixture<TestForgeApiFactory>
{
    private readonly HttpClient _client;

    public TestRunsEndpointTests(
        TestForgeApiFactory factory)
    {
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
}
