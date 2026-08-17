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
- Persistent build and test reports
- Build and test exit codes, durations, project paths and logs
- TRX test result generation and secure XML parsing
- Passed, failed and skipped test count aggregation
- JSON report endpoint (`GET /api/test-runs/{id}/report`)
- Unit and integration tests

## Pipeline

Queued → Cloning → Analyzing → Building → Testing → Completed / Failed

## Verified repositories

- lvntbk/evofit: Completed
- dotnet-architecture/eShopOnWeb: Failed with captured project dependency error
- kubeltd/distkeep: Rejected as unsupported project type

## Current tests

TestForge.Tests: 11 passed, 0 failed

## Next milestone

Harden the execution lifecycle:

- Fail explicitly when no test projects are detected
- Store individual test project results
- Add report retention and log size policies
- Clean up expired workspaces and TRX files

## Later roadmap

- HTML report
- Workspace cleanup
- Retry policy
- Atomic job claiming for multiple workers
- NuGet allowlist proxy
- API endpoint analysis through OpenAPI
- Automatic xUnit test generation
- React and TypeScript frontend
