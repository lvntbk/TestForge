# TestForge Project Status

## Current milestone

The first end-to-end MVP pipeline is operational.

## Completed

- ASP.NET Core Web API
- Clean Architecture project structure
- PostgreSQL and EF Core
- TestRun persistence and migrations
- GitHub repository URL validation
- Background worker
- Repository cloning
- ASP.NET Core project detection
- Test project detection
- Docker-isolated builds
- Docker-isolated test execution
- CPU, memory, PID, timeout and capability limits
- Persistent state transitions
- Unit and integration tests

## Pipeline

Queued → Cloning → Analyzing → Building → Testing → Completed / Failed

## Verified repositories

- lvntbk/evofit: Completed
- dotnet-architecture/eShopOnWeb: Failed with captured project dependency error
- kubeltd/distkeep: Rejected as unsupported project type

## Current tests

TestForge.Tests: 7 passed, 0 failed

## Next milestone

Persist build and test reports:

- Build exit code
- Test exit code
- Passed, failed and skipped counts
- Build and test durations
- Executed project paths
- Build and test logs
- TestRunReport entity and migration
- GET /api/test-runs/{id}/report endpoint
- JSON report response

## Later roadmap

- HTML report
- TRX parsing
- Workspace cleanup
- Retry policy
- Atomic job claiming for multiple workers
- NuGet allowlist proxy
- API endpoint analysis through OpenAPI
- Automatic xUnit test generation
- React and TypeScript frontend
