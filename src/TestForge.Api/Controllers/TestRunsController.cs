using Microsoft.AspNetCore.Mvc;
using TestForge.Api.Contracts.TestRuns;
using TestForge.Application.Repositories;
using TestForge.Domain.Entities;

namespace TestForge.Api.Controllers;

[ApiController]
[Route("api/test-runs")]
public sealed class TestRunsController : ControllerBase
{
    private readonly ITestRunRepository _repository;

    public TestRunsController(ITestRunRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    [ProducesResponseType<TestRunResponse>(
        StatusCodes.Status202Accepted)]
    [ProducesResponseType<ValidationProblemDetails>(
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        CreateTestRunRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsValidGitHubRepositoryUrl(request.RepositoryUrl))
        {
            ModelState.AddModelError(
                nameof(request.RepositoryUrl),
                "Geçerli bir HTTPS GitHub repository URL'si girilmelidir.");

            return ValidationProblem(ModelState);
        }

        var testRun = TestRun.Create(
            request.RepositoryUrl,
            DateTimeOffset.UtcNow);

        await _repository.AddAsync(testRun, cancellationToken);

        var response = TestRunResponse.FromEntity(testRun);

        return AcceptedAtAction(
            nameof(GetById),
            new { id = testRun.Id },
            response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<TestRunResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var testRun = await _repository.GetByIdAsync(
            id,
            cancellationToken);

        if (testRun is null)
        {
            return NotFound();
        }

        return Ok(TestRunResponse.FromEntity(testRun));
    }

    private static bool IsValidGitHubRepositoryUrl(string? repositoryUrl)
    {
        if (!Uri.TryCreate(
                repositoryUrl,
                UriKind.Absolute,
                out var uri))
        {
            return false;
        }

        if (uri.Scheme != Uri.UriSchemeHttps ||
            !uri.Host.Equals(
                "github.com",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segments = uri.AbsolutePath.Split(
            '/',
            StringSplitOptions.RemoveEmptyEntries);

        return segments.Length == 2;
    }
}
