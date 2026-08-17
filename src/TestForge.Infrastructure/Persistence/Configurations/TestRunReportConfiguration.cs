using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestForge.Domain.Entities;

namespace TestForge.Infrastructure.Persistence.Configurations;

public sealed class TestRunReportConfiguration :
    IEntityTypeConfiguration<TestRunReport>
{
    public void Configure(EntityTypeBuilder<TestRunReport> builder)
    {
        builder.ToTable("test_run_reports");
        builder.HasKey(x => x.TestRunId);

        builder.Property(x => x.TestRunId)
            .HasColumnName("test_run_id")
            .ValueGeneratedNever();
        builder.Property(x => x.BuildProjectPath)
            .HasColumnName("build_project_path")
            .HasMaxLength(2048);
        builder.Property(x => x.BuildExitCode)
            .HasColumnName("build_exit_code");
        builder.Property(x => x.BuildDurationMilliseconds)
            .HasColumnName("build_duration_ms");
        builder.Property(x => x.BuildStandardOutput)
            .HasColumnName("build_standard_output");
        builder.Property(x => x.BuildStandardError)
            .HasColumnName("build_standard_error");
        builder.Property(x => x.TestProjectPaths)
            .HasColumnName("test_project_paths")
            .IsRequired();
        builder.Property(x => x.TestExitCode)
            .HasColumnName("test_exit_code");
        builder.Property(x => x.TestDurationMilliseconds)
            .HasColumnName("test_duration_ms");
        builder.Property(x => x.TestStandardOutput)
            .HasColumnName("test_standard_output");
        builder.Property(x => x.TestStandardError)
            .HasColumnName("test_standard_error");
        builder.Property(x => x.PassedCount)
            .HasColumnName("passed_count");
        builder.Property(x => x.FailedCount)
            .HasColumnName("failed_count");
        builder.Property(x => x.SkippedCount)
            .HasColumnName("skipped_count");

        builder.HasOne<TestRun>()
            .WithOne()
            .HasForeignKey<TestRunReport>(x => x.TestRunId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
