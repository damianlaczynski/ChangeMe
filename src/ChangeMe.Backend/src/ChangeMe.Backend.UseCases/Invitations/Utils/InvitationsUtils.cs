using ChangeMe.Backend.Domain.Aggregates.Invitations;
using ChangeMe.Backend.Domain.Aggregates.Invitations.Enums;
using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Email;
using ChangeMe.Backend.UseCases.Invitations.Dtos;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Invitations.Utils;

public static class InvitationsUtils
{
  public static DateTime CalculateExpiresAt(DateTime createdAt) =>
    createdAt.Add(InvitationConstraints.DefaultLifetime);

  public static async Task<bool> HasActiveUserWithEmailAsync(
    ApplicationDbContext context,
    string normalizedEmail,
    CancellationToken cancellationToken) =>
    await context.Users
      .AsNoTracking()
      .AnyAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken);

  public static async Task<bool> HasPendingInvitationForEmailAsync(
    ApplicationDbContext context,
    string normalizedEmail,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    return await context.Invitations
      .AsNoTracking()
      .AnyAsync(
        i => i.NormalizedEmail == normalizedEmail
          && i.Status == InvitationStatus.PENDING
          && i.ExpiresAt > utcNow,
        cancellationToken);
  }

  public static async Task<IReadOnlyList<Guid>> ResolveRoleIdsForCreateAsync(
    ApplicationDbContext context,
    IUserAccessor userAccessor,
    IReadOnlyList<Guid>? requestedRoleIds,
    CancellationToken cancellationToken)
  {
    if (userAccessor.HasPermission(PermissionCodes.RolesManage))
      return requestedRoleIds?.Distinct().ToList() ?? [];

    var userRoleId = await context.Roles
      .AsNoTracking()
      .Where(r => r.Name == RoleConstraints.UserRoleName)
      .Select(r => r.Id)
      .SingleAsync(cancellationToken);

    return [userRoleId];
  }

  public static async Task ApplyExpiryTransitionsAsync(
    ApplicationDbContext context,
    IEnumerable<Guid>? invitationIds,
    CancellationToken cancellationToken)
  {
    var utcNow = DateTime.UtcNow;
    var query = context.Invitations
      .Where(i => i.Status == InvitationStatus.PENDING && i.ExpiresAt <= utcNow);

    if (invitationIds is not null)
    {
      var idList = invitationIds.Distinct().ToList();
      if (idList.Count == 0)
        return;

      query = query.Where(i => idList.Contains(i.Id));
    }

    var expiredInvitations = await query.ToListAsync(cancellationToken);
    foreach (var invitation in expiredInvitations)
      invitation.ApplyExpiryIfNeeded(utcNow);

    if (expiredInvitations.Count > 0)
      await context.SaveChangesAsync(cancellationToken);
  }

  public static async Task<Invitation?> FindByTokenHashAsync(
    ApplicationDbContext context,
    string tokenHash,
    CancellationToken cancellationToken) =>
    await context.Invitations
      .Include(i => i.Roles)
      .FirstOrDefaultAsync(i => i.TokenHash == tokenHash, cancellationToken);

  public static Task<Result> SendInvitationEmailAsync(
    IEmailService emailService,
    IOptions<AuthOptions> authOptions,
    string email,
    string token,
    DateTime expiresAt)
  {
    var acceptanceUrl = $"{authOptions.Value.FrontendBaseUrl.TrimEnd('/')}/invitations/accept/{token}";
    var detail = $"This invitation expires on {expiresAt:yyyy-MM-dd HH:mm} UTC.";
    var body = BrandedEmailTemplates.BuildActionEmail(
      "You are invited to ChangeMe",
      $"You have been invited to join ChangeMe as {email}.",
      detail,
      acceptanceUrl,
      "Accept invitation");

    return emailService.SendEmailAsync(
      email,
      "You are invited to ChangeMe",
      body);
  }
}
