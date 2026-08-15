using Microsoft.EntityFrameworkCore;
using TestForge.Application.Repositories;
using TestForge.Infrastructure.Persistence;
using TestForge.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddSingleton<
        ITestRunRepository,
        InMemoryTestRunRepository>();
}
else
{
    var connectionString = builder.Configuration
        .GetConnectionString("Postgres")
        ?? throw new InvalidOperationException(
            "Postgres connection string bulunamadı.");

    builder.Services.AddDbContext<TestForgeDbContext>(
        options => options.UseNpgsql(connectionString));

    builder.Services.AddScoped<
        ITestRunRepository,
        PostgresTestRunRepository>();
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

public partial class Program;
