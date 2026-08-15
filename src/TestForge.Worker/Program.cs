using Microsoft.EntityFrameworkCore;
using TestForge.Application.Repositories;
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

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
