using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record GetMyAccountQuery(bool doNothing = false) : IQuery<MyAccountDto>;

public class GetMyAccountHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : IQueryHandler<GetMyAccountQuery, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(GetMyAccountQuery query, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users
      .AsNoTracking()
      .Include(x => x.Roles)
      .ThenInclude(x => x.Role)
      .Include(x => x.ExternalLogins)
      .Include(x => x.Passkeys)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    var auth = authOptions.Value;
    var roles = user.Roles
      .Select(x => x.Role)
      .OrderBy(role => role.Name)
      .Select(role => new UserRoleSummaryDto(role.Id, role.Name, role.IsSystem))
      .ToList();

    var effectivePermissions = await UsersUtils.GetEffectivePermissionsForUserAsync(
      context,
      userId,
      cancellationToken);

    var externalLogins = auth.External.Enabled
      ? user.ExternalLogins
        .OrderBy(x => x.ProviderKey)
        .Select(login => new MyAccountExternalLoginDto(
          login.ProviderKey,
          ExternalAuthUtils.ResolveProviderDisplayName(auth, login.ProviderKey),
          login.LinkedAtUtc))
        .ToList()
      : [];

    var linkedProviderKeys = user.ExternalLogins
      .Select(x => x.ProviderKey)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);

    var linkableProviders = auth.External.Enabled
      ? auth.External.Providers
        .Where(x => x.IsConfigured && !linkedProviderKeys.Contains(x.ProviderKey))
        .Select(x => new ExternalProviderSettingsDto
        {
          ProviderKey = x.ProviderKey,
          DisplayName = x.DisplayName
        })
        .ToList()
      : [];

    var passkeys = auth.Passkeys.PasskeysAuthenticationEnabled
      ? user.Passkeys
        .OrderBy(x => x.CreatedAtUtc)
        .Select(CompletePasskeyRegistrationHandler.Map)
        .ToList()
      : [];

    var passkeyStepUpFresh = auth.Passkeys.PasskeysAuthenticationEnabled
      && user.IsPasskeyStepUpFresh(DateTime.UtcNow, auth.Passkeys.PasskeyStepUpValidityMinutes);

    return Result.Success(new MyAccountDto(
      user.Id,
      user.FirstName,
      user.LastName,
      user.Email,
      user.CreatedAt,
      user.HasPasswordSet,
      user.TwoFactorEnabled,
      user.TwoFactorEnabledAt,
      ExternalAuthUtils.IsExternalStepUpFresh(user, auth, DateTime.UtcNow),
      roles,
      effectivePermissions,
      externalLogins,
      linkableProviders,
      passkeys,
      passkeyStepUpFresh));
  }
}
