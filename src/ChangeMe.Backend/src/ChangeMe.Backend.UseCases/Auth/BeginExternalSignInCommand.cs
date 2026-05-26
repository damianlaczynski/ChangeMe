using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record BeginExternalSignInCommand(string ProviderKey) : ICommand<BeginExternalSignInResponseDto>;

public class BeginExternalSignInHandler(
  ApplicationDbContext context,
  IOidcExternalAuthService oidcExternalAuthService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginExternalSignInCommand, BeginExternalSignInResponseDto>
{
  public async Task<Result<BeginExternalSignInResponseDto>> Handle(
    BeginExternalSignInCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.External.Enabled)
      return Result<BeginExternalSignInResponseDto>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var provider = ExternalAuthUtils.ResolveProvider(auth, command.ProviderKey);
    if (provider is null)
      return Result<BeginExternalSignInResponseDto>.NotFound();

    var utcNow = DateTime.UtcNow;
    var (codeVerifier, codeChallenge, state, nonce) = ExternalAuthPkceUtils.CreateAuthorizationParameters();
    var expiresAtUtc = utcNow.AddMinutes(auth.External.PendingLifetimeMinutes);

    Result<ExternalAuthPending> pendingResult;
    if (userAccessor.UserId is Guid userId)
    {
      var user = await context.Users
        .Include(x => x.ExternalLogins)
        .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
      if (user is null || !user.IsActive)
        return Result<BeginExternalSignInResponseDto>.Unauthorized();

      if (user.ExternalLogins.Any(x =>
            x.ProviderKey.Equals(provider.ProviderKey, StringComparison.OrdinalIgnoreCase)))
      {
        return Result<BeginExternalSignInResponseDto>.Error("This provider is already linked to your account.");
      }

      pendingResult = ExternalAuthPending.CreateLink(
        provider.ProviderKey,
        state,
        nonce,
        codeChallenge,
        codeVerifier,
        userId,
        expiresAtUtc);
    }
    else
    {
      pendingResult = ExternalAuthPending.CreateSignIn(
        provider.ProviderKey,
        state,
        nonce,
        codeChallenge,
        codeVerifier,
        expiresAtUtc);
    }

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
