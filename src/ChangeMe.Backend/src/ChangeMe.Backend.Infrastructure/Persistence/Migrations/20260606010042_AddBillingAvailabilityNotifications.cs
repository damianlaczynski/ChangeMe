using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingAvailabilityNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "IssueId",
                schema: "changeme_backend",
                table: "notifications",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "BillingSourceEntityId",
                schema: "changeme_backend",
                table: "notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BillingSourceRevisionAt",
                schema: "changeme_backend",
                table: "notifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientUserId_BillingSourceEntityId_Billing~",
                schema: "changeme_backend",
                table: "notifications",
                columns: new[] { "RecipientUserId", "BillingSourceEntityId", "BillingSourceRevisionAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_notifications_RecipientUserId_BillingSourceEntityId_Billing~",
                schema: "changeme_backend",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "BillingSourceEntityId",
                schema: "changeme_backend",
                table: "notifications");

            migrationBuilder.DropColumn(
                name: "BillingSourceRevisionAt",
                schema: "changeme_backend",
                table: "notifications");

            migrationBuilder.AlterColumn<Guid>(
                name: "IssueId",
                schema: "changeme_backend",
                table: "notifications",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
