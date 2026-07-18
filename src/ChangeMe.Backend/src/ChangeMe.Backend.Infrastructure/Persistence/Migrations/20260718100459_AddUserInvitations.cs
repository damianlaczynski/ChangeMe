using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "invitations",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InvitedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "invitation_roles",
                schema: "changeme_backend",
                columns: table => new
                {
                    InvitationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invitation_roles", x => new { x.InvitationId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_invitation_roles_invitations_InvitationId",
                        column: x => x.InvitationId,
                        principalSchema: "changeme_backend",
                        principalTable: "invitations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_invitations_ExpiresAt",
                schema: "changeme_backend",
                table: "invitations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_Id",
                schema: "changeme_backend",
                table: "invitations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_NormalizedEmail",
                schema: "changeme_backend",
                table: "invitations",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_Status",
                schema: "changeme_backend",
                table: "invitations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_invitations_TokenHash",
                schema: "changeme_backend",
                table: "invitations",
                column: "TokenHash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "invitation_roles",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "invitations",
                schema: "changeme_backend");
        }
    }
}
