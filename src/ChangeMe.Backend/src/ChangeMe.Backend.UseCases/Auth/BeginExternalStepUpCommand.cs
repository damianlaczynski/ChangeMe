using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record BeginExternalStepUpCommand(string ProviderKey) : ICommand<BeginExternalSignInResponseDto>;

public class BeginExternalStepUpHandler(
  ApplicationDbContext context,
  IOidcExternalAuthService oidcExternalAuthService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginExternalStepUpCommand, BeginExternalSignInResponseDto>
{
  public async ValueTask<Result<BeginExternalSignInResponseDto>> Handle(
    BeginExternalStepUpCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<BeginExternalSignInResponseDto>.Unauthorized();

    var auth = authOptions.Value;
    if (!auth.External.Enabled)
      return Result<BeginExternalSignInResponseDto>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var provider = ExternalAuthUtils.ResolveProvider(auth, command.ProviderKey);
    if (provider is null)
      return Result<BeginExternalSignInResponseDto>.NotFound();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<BeginExternalSignInResponseDto>.Unauthorized();

    if (!user.ExternalLogins.Any(x =>
          x.ProviderKey.Equals(provider.ProviderKey, StringComparison.OrdinalIgnoreCase)))
    {
      return Result<BeginExternalSignInResponseDto>.Error(
        "Sign in with a provider that is already linked to your account.");
    }

    var utcNow = DateTime.UtcNow;
    var (codeVerifier, codeChallenge, state, nonce) = ExternalAuthPkceUtils.CreateAuthorizationParameters();
    var expiresAtUtc = utcNow.AddMinutes(auth.External.PendingLifetimeMinutes);

    var pendingResult = ExternalAuthPending.CreateStepUp(
      provider.ProviderKey,
      state,
      nonce,
      codeChallenge,
      codeVerifier,
      userId,
      expiresAtUtc);
    if (!pendingResult.IsSuccess)
      return pendingResult.Map();

    await context.ExternalAuthPending.AddAsync(pendingResult.Value, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var redirectUri = ExternalAuthUtils.BuildRedirectUri(auth);
    var authorizationUrl = oidcExternalAuthService.BuildAuthorizationUrl(
      provider,
      pendingResult.Value,
      redirectUri);

    return Result.Success(new BeginExternalSignInResponseDto(authorizationUrl));
  }
}
