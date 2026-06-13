using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrlMonitor.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                table: "HealthCheckResults",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "SchedulerJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TriggerType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchedulerJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HealthCheckResult_JobId",
                table: "HealthCheckResults",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_HealthCheckResults_SchedulerJobs_JobId",
                table: "HealthCheckResults",
                column: "JobId",
                principalTable: "SchedulerJobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HealthCheckResults_SchedulerJobs_JobId",
                table: "HealthCheckResults");

            migrationBuilder.DropTable(
                name: "SchedulerJobs");

            migrationBuilder.DropIndex(
                name: "IX_HealthCheckResult_JobId",
                table: "HealthCheckResults");

            migrationBuilder.DropColumn(
                name: "JobId",
                table: "HealthCheckResults");
        }
    }
}
