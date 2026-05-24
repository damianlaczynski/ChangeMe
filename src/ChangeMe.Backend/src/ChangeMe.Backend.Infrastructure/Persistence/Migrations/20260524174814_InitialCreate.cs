using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChangeMe.Backend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "changeme_backend");

            migrationBuilder.CreateTable(
                name: "external_auth_pending",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    State = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Nonce = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeChallenge = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    CodeVerifier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderSubject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ProviderEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    ProviderEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    ProviderFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderLastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IdentityProviderMfaAsserted = table.Column<bool>(type: "boolean", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_auth_pending", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "issues",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Priority = table.Column<string>(type: "text", nullable: false),
                    AssignedToUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueHistoryEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    IssueTitle = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Link = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EmailSentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsSystem = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_auth_tokens",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_auth_tokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_sessions",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivityAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeviceBrowserLabel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    RefreshTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    RefreshTokenExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Deactivated = table.Column<bool>(type: "boolean", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    HasPasswordSet = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordLastChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TwoFactorSecretCiphertext = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "issue_acceptance_criteria",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_acceptance_criteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_acceptance_criteria_issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "changeme_backend",
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_comments",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_comments_issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "changeme_backend",
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_history_entries",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    PreviousValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentValue = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RelatedCommentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_history_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_history_entries_issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "changeme_backend",
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "issue_watchers",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_issue_watchers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_issue_watchers_issues_IssueId",
                        column: x => x.IssueId,
                        principalSchema: "changeme_backend",
                        principalTable: "issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                schema: "changeme_backend",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => new { x.RoleId, x.PermissionCode });
                    table.ForeignKey(
                        name: "FK_role_permissions_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "changeme_backend",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_invitations",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AcceptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_invitations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_account_invitations_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "external_logins",
                schema: "changeme_backend",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ProviderSubject = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastStepUpAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_external_logins", x => new { x.UserId, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_external_logins_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sign_in_challenges",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FailedAttemptCount = table.Column<int>(type: "integer", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sign_in_challenges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sign_in_challenges_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "two_factor_enrollment_pending",
                schema: "changeme_backend",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecretCiphertext = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_two_factor_enrollment_pending", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_two_factor_enrollment_pending_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_recovery_codes",
                schema: "changeme_backend",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodeHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    UsedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_recovery_codes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_recovery_codes_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_roles",
                schema: "changeme_backend",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "changeme_backend",
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalSchema: "changeme_backend",
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_invitations_UserId_SentAtUtc",
                schema: "changeme_backend",
                table: "account_invitations",
                columns: new[] { "UserId", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_external_auth_pending_ExpiresAtUtc",
                schema: "changeme_backend",
                table: "external_auth_pending",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_external_auth_pending_State",
                schema: "changeme_backend",
                table: "external_auth_pending",
                column: "State",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_external_logins_ProviderKey_ProviderSubject",
                schema: "changeme_backend",
                table: "external_logins",
                columns: new[] { "ProviderKey", "ProviderSubject" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_issue_acceptance_criteria_Id",
                schema: "changeme_backend",
                table: "issue_acceptance_criteria",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_acceptance_criteria_IssueId",
                schema: "changeme_backend",
                table: "issue_acceptance_criteria",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_Id",
                schema: "changeme_backend",
                table: "issue_comments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_comments_IssueId",
                schema: "changeme_backend",
                table: "issue_comments",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_entries_Id",
                schema: "changeme_backend",
                table: "issue_history_entries",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_history_entries_IssueId",
                schema: "changeme_backend",
                table: "issue_history_entries",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_issue_watchers_Id",
                schema: "changeme_backend",
                table: "issue_watchers",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_issue_watchers_IssueId_UserId",
                schema: "changeme_backend",
                table: "issue_watchers",
                columns: new[] { "IssueId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_issues_Id",
                schema: "changeme_backend",
                table: "issues",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_Id",
                schema: "changeme_backend",
                table: "notifications",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientUserId_IsRead",
                schema: "changeme_backend",
                table: "notifications",
                columns: new[] { "RecipientUserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_notifications_RecipientUserId_IssueHistoryEntryId",
                schema: "changeme_backend",
                table: "notifications",
                columns: new[] { "RecipientUserId", "IssueHistoryEntryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_Id",
                schema: "changeme_backend",
                table: "roles",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                schema: "changeme_backend",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sign_in_challenges_ExpiresAtUtc",
                schema: "changeme_backend",
                table: "sign_in_challenges",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_sign_in_challenges_UserId",
                schema: "changeme_backend",
                table: "sign_in_challenges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_tokens_TokenHash",
                schema: "changeme_backend",
                table: "user_auth_tokens",
                column: "TokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_tokens_UserId",
                schema: "changeme_backend",
                table: "user_auth_tokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_auth_tokens_UserId_Type_UsedAtUtc",
                schema: "changeme_backend",
                table: "user_auth_tokens",
                columns: new[] { "UserId", "Type", "UsedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_user_recovery_codes_UserId",
                schema: "changeme_backend",
                table: "user_recovery_codes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_recovery_codes_UserId_CodeHash",
                schema: "changeme_backend",
                table: "user_recovery_codes",
                columns: new[] { "UserId", "CodeHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                schema: "changeme_backend",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_Id",
                schema: "changeme_backend",
                table: "user_sessions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId",
                schema: "changeme_backend",
                table: "user_sessions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_sessions_UserId_RevokedAt",
                schema: "changeme_backend",
                table: "user_sessions",
                columns: new[] { "UserId", "RevokedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_users_Id",
                schema: "changeme_backend",
                table: "users",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedEmail",
                schema: "changeme_backend",
                table: "users",
                column: "NormalizedEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_invitations",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "external_auth_pending",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "external_logins",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "issue_acceptance_criteria",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "issue_comments",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "issue_history_entries",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "issue_watchers",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "notifications",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "role_permissions",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "sign_in_challenges",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "two_factor_enrollment_pending",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_auth_tokens",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_recovery_codes",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_roles",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "user_sessions",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "issues",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "roles",
                schema: "changeme_backend");

            migrationBuilder.DropTable(
                name: "users",
                schema: "changeme_backend");
        }
    }
}
