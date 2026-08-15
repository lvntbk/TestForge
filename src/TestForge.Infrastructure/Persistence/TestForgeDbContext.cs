using Microsoft.EntityFrameworkCore;
using TestForge.Domain.Entities;

namespace TestForge.Infrastructure.Persistence;

public sealed class TestForgeDbContext : DbContext
{
    public TestForgeDbContext(
        DbContextOptions<TestForgeDbContext> options)
        : base(options)
    {
    }

    public DbSet<TestRun> TestRuns => Set<TestRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TestForgeDbContext).Assembly);
    }
}
