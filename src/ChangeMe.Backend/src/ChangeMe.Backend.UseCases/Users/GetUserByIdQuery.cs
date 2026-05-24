using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDetailsDto>;

public class GetUserByIdHandler(
  ApplicationDbContext context,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  IOptions<AuthOptions> authOptions,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator) : IQueryHandler<GetUserByIdQuery, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
  {
    var user = await context.Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .ThenInclude(x => x.Role)
      .Include(x => x.ExternalLogins)
      .Include(x => x.Passkeys)
      .Include(x => x.AccountInvitations)
      .FirstOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var roles = user.Roles
      .Select(x => x.Role)
      .OrderBy(role => role.Name)
      .Select(role => new UserRoleSummaryDto(role.Id, role.Name, role.IsSystem))
      .ToList();

    var effectivePermissions = await UsersUtils.GetEffectivePermissionsForUserAsync(
      context,
      user.Id,
      cancellationToken);

    var lastSignInAt = await UsersUtils.GetLastSignInAtAsync(context, user.Id, cancellationToken);

    var auth = authOptions.Value;
    var externalLogins = auth.ExternalProvidersEnabled
      ? user.ExternalLogins
        .OrderBy(x => x.ProviderKey)
        .Select(login => new UserExternalLoginDto(
          login.ProviderKey,
          ExternalAuthUtils.ResolveProviderDisplayName(auth, login.ProviderKey),
          login.LinkedAtUtc))
        .ToList()
      : [];

    var passkeys = passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment()
      ? user.Passkeys
        .OrderBy(x => x.CreatedAtUtc)
        .Select(x => new UserPasskeyDto(
          x.Id,
          x.Name,
          x.CreatedAtUtc,
          x.LastUsedAtUtc,
          x.AuthenticatorType,
          x.BackupEligible,
          x.BackupState))
        .ToList()
      : [];

    return Result.Success(
      user.ToDetailsDto(
        lastSignInAt,
        roles,
        effectivePermissions,
        externalLogins,
        auth.InvitationLinkLifetimeHours,
        passkeys,
        passwordExpirationEvaluator));
  }
}
