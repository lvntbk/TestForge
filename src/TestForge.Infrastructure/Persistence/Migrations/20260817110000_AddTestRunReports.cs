using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TestForge.Infrastructure.Persistence;

#nullable disable

namespace TestForge.Infrastructure.Persistence.Migrations;

[DbContext(typeof(TestForgeDbContext))]
[Migration("20260817110000_AddTestRunReports")]
public partial class AddTestRunReports : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "test_run_reports",
            columns: table => new
            {
                test_run_id = table.Column<Guid>(type: "uuid", nullable: false),
                build_project_path = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                build_exit_code = table.Column<int>(type: "integer", nullable: true),
                build_duration_ms = table.Column<long>(type: "bigint", nullable: true),
                build_standard_output = table.Column<string>(type: "text", nullable: true),
                build_standard_error = table.Column<string>(type: "text", nullable: true),
                test_project_paths = table.Column<string>(type: "text", nullable: false),
                test_exit_code = table.Column<int>(type: "integer", nullable: true),
                test_duration_ms = table.Column<long>(type: "bigint", nullable: true),
                test_standard_output = table.Column<string>(type: "text", nullable: true),
                test_standard_error = table.Column<string>(type: "text", nullable: true),
                passed_count = table.Column<int>(type: "integer", nullable: true),
                failed_count = table.Column<int>(type: "integer", nullable: true),
                skipped_count = table.Column<int>(type: "integer", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_test_run_reports", x => x.test_run_id);
                table.ForeignKey(
                    name: "FK_test_run_reports_test_runs_test_run_id",
                    column: x => x.test_run_id,
                    principalTable: "test_runs",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "test_run_reports");
    }
}
