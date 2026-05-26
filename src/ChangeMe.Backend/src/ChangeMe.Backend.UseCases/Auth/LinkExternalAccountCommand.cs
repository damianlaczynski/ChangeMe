using ChangeMe.Backend.Domain.Aggregates.Sessions;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record LinkExternalAccountCommand(
  string State,
  string Password) : ICommand<ExternalSignInResponseDto>;

public class LinkExternalAccountHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IHttpContextAccessor httpContextAccessor,
  IOptions<AuthOptions> authOptions,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator) : ICommandHandler<LinkExternalAccountCommand, ExternalSignInResponseDto>
{
  public async Task<Result<ExternalSignInResponseDto>> Handle(
    LinkExternalAccountCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.External.Enabled)
      return Result<ExternalSignInResponseDto>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var utcNow = DateTime.UtcNow;
    var pending = await context.ExternalAuthPending
      .FirstOrDefaultAsync(x => x.State == command.State, cancellationToken);
    if (pending is null
        || pending.IsExpired(utcNow)
        || pending.Mode != ExternalAuthMode.LinkAccount
        || string.IsNullOrWhiteSpace(pending.ProviderSubject))
    {
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);
    }

    var provider = ExternalAuthUtils.ResolveProvider(auth, pending.ProviderKey);
    if (provider is null)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    if (string.IsNullOrWhiteSpace(pending.ProviderEmail))
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    var normalizedEmail = User.NormalizeEmail(pending.ProviderEmail);
    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);
    if (user is null || !user.HasPasswordSet)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.Password))
      return Result<ExternalSignInResponseDto>.Unauthorized(AuthSessionUtils.InvalidCredentialsMessage);

    var subjectLinkedElsewhere = await context.ExternalLogins.AnyAsync(
      x => x.ProviderKey == provider.ProviderKey
           && x.ProviderSubject == pending.ProviderSubject
           && x.UserId != user.Id,
      cancellationToken);
    if (subjectLinkedElsewhere)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Conflict(ExternalAuthUtils.ExternalAccountAlreadyLinkedMessage);
    }

    var externalLoginResult = ExternalLogin.Create(
      user.Id,
      provider.ProviderKey,
      pending.ProviderSubject);
    if (!externalLoginResult.IsSuccess)
      return externalLoginResult.Map();

    var addLoginResult = user.AddExternalLogin(externalLoginResult.Value);
    if (!addLoginResult.IsSuccess)
      return Result<ExternalSignInResponseDto>.Error(addLoginResult.Errors.First());

    externalLoginResult.Value.RecordStepUp(DateTime.UtcNow);
    await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return await ExternalSignInAuthUtils.IssueSignInResponseAsync(
      context,
      jwtTokenGenerator,
      sessionLifetime,
      passwordExpirationEvaluator,
      passkeyPolicyEvaluator,
      httpContextAccessor,
      auth,
      user,
      pending.IdentityProviderMfaAsserted,
      SignInMethods.ExternalWithProvider(provider.ProviderKey),
      cancellationToken);
  }
}
