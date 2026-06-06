using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettlementOperationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "settlement_operation_log",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    Operation = table.Column<string>(type: "text", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_operation_log", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_settlement_operation_log_Id",
                schema: "changeme_backend",
                table: "settlement_operation_log",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_operation_log_SettlementPeriodId",
                schema: "changeme_backend",
                table: "settlement_operation_log",
                column: "SettlementPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_operation_log_Timestamp",
                schema: "changeme_backend",
                table: "settlement_operation_log",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "settlement_operation_log",
                schema: "changeme_backend");
        }
    }
}
