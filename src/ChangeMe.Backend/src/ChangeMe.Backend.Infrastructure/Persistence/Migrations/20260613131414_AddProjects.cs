using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProjects : Migration
    {
        private static readonly Guid DefaultProjectId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projects",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Visibility = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_project_members_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalSchema: "changeme_backend",
                        principalTable: "projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "changeme_backend",
                table: "projects",
                columns: new[] { "Id", "Name", "Key", "Description", "Status", "Visibility", "Color", "CreatedBy", "UpdatedBy", "CreatedAt", "UpdatedAt", "IsDeleted" },
                values: new object[] { DefaultProjectId, "General", "GEN", "Default workspace for existing issues.", "ACTIVE", "INTERNAL", "#3B82F6", Guid.Empty, null, DateTime.UtcNow, null, false });

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                schema: "changeme_backend",
                table: "issues",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(
                $"UPDATE changeme_backend.issues SET \"ProjectId\" = '{DefaultProjectId}' WHERE \"ProjectId\" IS NULL;");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProjectId",
                schema: "changeme_backend",
                table: "issues",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_issues_ProjectId",
                schema: "changeme_backend",
                table: "issues",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_Id",
                schema: "changeme_backend",
                table: "project_members",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_ProjectId_UserId",
                schema: "changeme_backend",
                table: "project_members",
                columns: new[] { "ProjectId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projects_Id",
                schema: "changeme_backend",
                table: "projects",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_projects_Key",
                schema: "changeme_backend",
                table: "projects",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_issues_projects_ProjectId",
                schema: "changeme_backend",
                table: "issues",
                column: "ProjectId",
                principalSchema: "changeme_backend",
                principalTable: "projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_issues_projects_ProjectId",
                schema: "changeme_backend",
                table: "issues");

            migrationBuilder.DropTable(
                name: "project_members",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "projects",
                schema: "changeme_backend");

            migrationBuilder.DropIndex(
                name: "IX_issues_ProjectId",
                schema: "changeme_backend",
                table: "issues");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                schema: "changeme_backend",
                table: "issues");
        }
    }
}
