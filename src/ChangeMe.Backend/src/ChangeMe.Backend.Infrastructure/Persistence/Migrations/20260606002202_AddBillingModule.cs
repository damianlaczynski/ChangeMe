using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "availability_entries",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AllDay = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_availability_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "billing_settings",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultAnnualLeaveDays = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: false),
                    AllowHalfDayLeave = table.Column<bool>(type: "boolean", nullable: false),
                    DefaultWorkdayStart = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DefaultWorkdayEnd = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HalfDaySplitTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    DefaultWorkdaysCsv = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DefaultAvailabilityStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_billing_settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employment_contracts",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractType = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Fte = table.Column<decimal>(type: "numeric(3,2)", precision: 3, scale: 2, nullable: false),
                    MonthlyHoursNormMinutes = table.Column<int>(type: "integer", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    MonthlySalary = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_contracts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employment_profiles",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NormalizedEmployeeId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NationalId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BankAccount = table.Column<string>(type: "character varying(34)", maxLength: 34, nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employment_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeaveTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DayPortion = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RejectReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_requests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "leave_types",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NormalizedCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CountsAsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    UsesAllowance = table.Column<bool>(type: "boolean", nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsSeeded = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "positions",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "settlement_periods",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settlement_periods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_settlements",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpectedMinutes = table.Column<int>(type: "integer", nullable: false),
                    LoggedMinutes = table.Column<int>(type: "integer", nullable: false),
                    LeaveDays = table.Column<decimal>(type: "numeric(5,1)", precision: 5, scale: 1, nullable: false),
                    BalanceMinutes = table.Column<int>(type: "integer", nullable: false),
                    LastCalculatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_settlements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_recurring_patterns",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_recurring_patterns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "weekly_recurring_pattern_days",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatternId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    Enabled = table.Column<bool>(type: "boolean", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_weekly_recurring_pattern_days", x => x.Id);
                    table.ForeignKey(
                        name: "FK_weekly_recurring_pattern_days_weekly_recurring_patterns_Pat~",
                        column: x => x.PatternId,
                        principalSchema: "changeme_backend",
                        principalTable: "weekly_recurring_patterns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_availability_entries_Id",
                schema: "changeme_backend",
                table: "availability_entries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_availability_entries_LeaveRequestId",
                schema: "changeme_backend",
                table: "availability_entries",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_availability_entries_UserId",
                schema: "changeme_backend",
                table: "availability_entries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_billing_settings_Id",
                schema: "changeme_backend",
                table: "billing_settings",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_contracts_Id",
                schema: "changeme_backend",
                table: "employment_contracts",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_contracts_PositionId",
                schema: "changeme_backend",
                table: "employment_contracts",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_employment_contracts_UserId",
                schema: "changeme_backend",
                table: "employment_contracts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_employment_profiles_Id",
                schema: "changeme_backend",
                table: "employment_profiles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_employment_profiles_UserId",
                schema: "changeme_backend",
                table: "employment_profiles",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_Id",
                schema: "changeme_backend",
                table: "leave_requests",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_LeaveTypeId",
                schema: "changeme_backend",
                table: "leave_requests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_UserId",
                schema: "changeme_backend",
                table: "leave_requests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_leave_types_Id",
                schema: "changeme_backend",
                table: "leave_types",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_types_NormalizedCode",
                schema: "changeme_backend",
                table: "leave_types",
                column: "NormalizedCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_leave_types_NormalizedName",
                schema: "changeme_backend",
                table: "leave_types",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_positions_Id",
                schema: "changeme_backend",
                table: "positions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_positions_NormalizedName",
                schema: "changeme_backend",
                table: "positions",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_settlement_periods_Id",
                schema: "changeme_backend",
                table: "settlement_periods",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_settlement_periods_Year_Month",
                schema: "changeme_backend",
                table: "settlement_periods",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_settlements_Id",
                schema: "changeme_backend",
                table: "user_settlements",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_settlements_SettlementPeriodId_UserId",
                schema: "changeme_backend",
                table: "user_settlements",
                columns: new[] { "SettlementPeriodId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_recurring_pattern_days_Id",
                schema: "changeme_backend",
                table: "weekly_recurring_pattern_days",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_recurring_pattern_days_PatternId_DayOfWeek",
                schema: "changeme_backend",
                table: "weekly_recurring_pattern_days",
                columns: new[] { "PatternId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_weekly_recurring_patterns_Id",
                schema: "changeme_backend",
                table: "weekly_recurring_patterns",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_weekly_recurring_patterns_UserId",
                schema: "changeme_backend",
                table: "weekly_recurring_patterns",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "availability_entries",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "billing_settings",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "employment_contracts",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "employment_profiles",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "leave_requests",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "leave_types",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "positions",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "settlement_periods",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_settlements",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "weekly_recurring_pattern_days",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "weekly_recurring_patterns",
                schema: "changeme_backend");
        }
    }
}
