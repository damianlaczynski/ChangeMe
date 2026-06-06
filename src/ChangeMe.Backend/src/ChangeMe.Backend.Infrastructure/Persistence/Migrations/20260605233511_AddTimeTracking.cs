using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "time_entries",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_time_entries_issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "changeme_backend",
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_time_entries_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "changeme_backend",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "time_entry_audit_log",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TimeEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Operation = table.Column<int>(type: "integer", nullable: false),
                    ActingUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryAuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssueTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    WorkDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreviousWorkDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreviousDurationMinutes = table.Column<int>(type: "integer", nullable: true),
                    PreviousDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreviousProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousProjectName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PreviousIssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    PreviousIssueTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_entry_audit_log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "time_tracking_settings",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BackdatingLimitDays = table.Column<int>(type: "integer", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_time_tracking_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_running_timers",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_running_timers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_AuthorUserId",
                schema: "changeme_backend",
                table: "time_entries",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_Id",
                schema: "changeme_backend",
                table: "time_entries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_IssueId",
                schema: "changeme_backend",
                table: "time_entries",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_ProjectId",
                schema: "changeme_backend",
                table: "time_entries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entries_WorkDate",
                schema: "changeme_backend",
                table: "time_entries",
                column: "WorkDate");

            migrationBuilder.CreateIndex(
                name: "IX_time_entry_audit_log_ActingUserId",
                schema: "changeme_backend",
                table: "time_entry_audit_log",
                column: "ActingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entry_audit_log_CreatedAt",
                schema: "changeme_backend",
                table: "time_entry_audit_log",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_time_entry_audit_log_EntryAuthorUserId",
                schema: "changeme_backend",
                table: "time_entry_audit_log",
                column: "EntryAuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_time_entry_audit_log_Id",
                schema: "changeme_backend",
                table: "time_entry_audit_log",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_time_entry_audit_log_ProjectId",
                schema: "changeme_backend",
                table: "time_entry_audit_log",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_time_tracking_settings_Id",
                schema: "changeme_backend",
                table: "time_tracking_settings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_running_timers_Id",
                schema: "changeme_backend",
                table: "user_running_timers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_running_timers_UserId",
                schema: "changeme_backend",
                table: "user_running_timers",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "time_entries",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "time_entry_audit_log",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "time_tracking_settings",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_running_timers",
                schema: "changeme_backend");
        }
    }
}
