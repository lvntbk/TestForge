using Microsoft.EntityFrameworkCore;
using TestForge.Application.Analysis;
using TestForge.Application.Build;
using TestForge.Application.Git;
using TestForge.Application.Repositories;
using TestForge.Infrastructure.Analysis;
using TestForge.Infrastructure.Build;
using TestForge.Infrastructure.Git;
using TestForge.Infrastructure.Persistence;
using TestForge.Infrastructure.Repositories;
using TestForge.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Postgres connection string bulunamadı.");

builder.Services.AddDbContext<TestForgeDbContext>(
    options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<
    ITestRunRepository,
    PostgresTestRunRepository>();

builder.Services.AddSingleton<
    IGitRepositoryCloner,
    GitRepositoryCloner>();

builder.Services.AddSingleton<
    IProjectAnalyzer,
    DotNetProjectAnalyzer>();

builder.Services.AddSingleton<
    IDotNetBuildRunner,
    DockerDotNetBuildRunner>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
