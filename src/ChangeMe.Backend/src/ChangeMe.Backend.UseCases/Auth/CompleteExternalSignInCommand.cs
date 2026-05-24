using ChangeMe.Backend.Domain.Aggregates.Roles;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record CompleteExternalSignInCommand(
  string Code,
  string State) : ICommand<ExternalSignInResponseDto>;

public class CompleteExternalSignInHandler(
  ApplicationDbContext context,
  IOidcExternalAuthService oidcExternalAuthService,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IHttpContextAccessor httpContextAccessor,
  IAuthEmailService authEmailService,
  IUserAuthTokenService userAuthTokenService,
  IOptions<AuthOptions> authOptions,
  TimeProvider timeProvider) : ICommandHandler<CompleteExternalSignInCommand, ExternalSignInResponseDto>
{
  public async Task<Result<ExternalSignInResponseDto>> Handle(
    CompleteExternalSignInCommand command,
    CancellationToken cancellationToken)
  {
    var auth = authOptions.Value;
    if (!auth.ExternalProvidersEnabled)
      return Result<ExternalSignInResponseDto>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var utcNow = DateTime.UtcNow;
    var pending = await context.ExternalAuthPending
      .FirstOrDefaultAsync(x => x.State == command.State, cancellationToken);
    if (pending is null || pending.IsExpired(utcNow))
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    var provider = ExternalAuthUtils.ResolveProvider(auth, pending.ProviderKey);
    if (provider is null)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    var redirectUri = ExternalAuthUtils.BuildRedirectUri(auth);
    var oidcResult = await oidcExternalAuthService.ExchangeAuthorizationCodeAsync(
      provider,
      pending,
      command.Code,
      redirectUri,
      cancellationToken);
    if (!oidcResult.IsSuccess)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    var assertion = oidcResult.Value;
    pending.SetProviderAssertion(
      assertion.ProviderSubject,
      assertion.Email,
      assertion.EmailVerified,
      assertion.FirstName,
      assertion.LastName,
      assertion.IdentityProviderMfaAsserted);

    if (!ExternalAuthUtils.IsEmailDomainAllowed(assertion.Email, provider.AllowedEmailDomains))
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.SignInNotAllowedMessage);
    }

    if (pending.Mode == ExternalAuthMode.StepUp && pending.UserId is Guid stepUpUserId)
      return await CompleteStepUpModeAsync(
        pending,
        provider,
        stepUpUserId,
        assertion,
        cancellationToken);

    if (pending.Mode == ExternalAuthMode.Link && pending.UserId is Guid linkUserId)
      return await CompleteLinkModeAsync(
        pending,
        provider,
        linkUserId,
        assertion,
        auth,
        cancellationToken);

    return await CompleteSignInModeAsync(
      pending,
      provider,
      assertion,
      auth,
      cancellationToken);
  }

  private async Task<Result<ExternalSignInResponseDto>> CompleteSignInModeAsync(
    ExternalAuthPending pending,
    ExternalProviderConfiguration provider,
    OidcSignInResult assertion,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    var existingLogin = await context.ExternalLogins
      .Include(x => x.User)
      .FirstOrDefaultAsync(
        x => x.ProviderKey == provider.ProviderKey && x.ProviderSubject == assertion.ProviderSubject,
        cancellationToken);

    if (existingLogin is not null)
      return await SignInExistingUserAsync(
        pending,
        existingLogin.User,
        assertion.IdentityProviderMfaAsserted,
        auth,
        cancellationToken);

    if (assertion.EmailVerified && !string.IsNullOrWhiteSpace(assertion.Email))
    {
      var normalizedEmail = User.NormalizeEmail(assertion.Email);
      var matchedUser = await context.Users
        .Include(x => x.ExternalLogins)
        .Include(x => x.AccountInvitations)
        .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

      if (matchedUser is not null)
        return await HandleMatchedUserAsync(
          pending,
          provider,
          matchedUser,
          assertion,
          auth,
          cancellationToken);
    }

    if (!auth.PublicRegistrationEnabled)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.NoAccountExistsMessage);
    }

    return await RegisterAndSignInAsync(
      pending,
      provider,
      assertion,
      auth,
      cancellationToken);
  }

  private async Task<Result<ExternalSignInResponseDto>> HandleMatchedUserAsync(
    ExternalAuthPending pending,
    ExternalProviderConfiguration provider,
    User matchedUser,
    OidcSignInResult assertion,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    if (matchedUser.ExternalLogins.FirstOrDefault(x =>
          x.ProviderKey.Equals(provider.ProviderKey, StringComparison.OrdinalIgnoreCase))
        is { } existingLoginForProvider)
    {
      if (!existingLoginForProvider.ProviderSubject.Equals(
            assertion.ProviderSubject,
            StringComparison.Ordinal))
      {
        await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
        return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);
      }

      return await SignInExistingUserAsync(
        pending,
        matchedUser,
        assertion.IdentityProviderMfaAsserted,
        auth,
        cancellationToken);
    }

    var subjectLinkedElsewhere = await context.ExternalLogins.AnyAsync(
      x => x.ProviderKey == provider.ProviderKey && x.ProviderSubject == assertion.ProviderSubject,
      cancellationToken);
    if (subjectLinkedElsewhere)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Conflict(ExternalAuthUtils.ExternalAccountAlreadyLinkedMessage);
    }

    if (ExternalAuthUtils.IsInvitationPending(matchedUser))
    {
      if (!assertion.EmailVerified || string.IsNullOrWhiteSpace(assertion.Email))
      {
        await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
        return Result<ExternalSignInResponseDto>.Unauthorized(AuthSessionUtils.InvitePendingAccountMessage);
      }

      var linkResult = await LinkExternalLoginAsync(matchedUser, provider, assertion, cancellationToken);
      if (!linkResult.IsSuccess)
        return linkResult.Map();

      var invitationResult = matchedUser.CompleteInvitationViaExternalSignIn(
        assertion.FirstName,
        assertion.LastName,
        timeProvider.GetUtcNow().UtcDateTime);
      if (!invitationResult.IsSuccess)
        return invitationResult.Map();

      await userAuthTokenService.InvalidateUnusedTokensAsync(
        matchedUser.Id,
        UserAuthTokenType.Invitation,
        cancellationToken);

      await authEmailService.SendExternalAccountLinkedAsync(
        matchedUser,
        provider.DisplayName,
        cancellationToken);

      return await SignInExistingUserAsync(
        pending,
        matchedUser,
        assertion.IdentityProviderMfaAsserted,
        auth,
        cancellationToken);
    }

    if (!matchedUser.HasPasswordSet)
    {
      var linkResult = await LinkExternalLoginAsync(matchedUser, provider, assertion, cancellationToken);
      if (!linkResult.IsSuccess)
        return linkResult.Map();

      await authEmailService.SendExternalAccountLinkedAsync(
        matchedUser,
        provider.DisplayName,
        cancellationToken);

      return await SignInExistingUserAsync(
        pending,
        matchedUser,
        assertion.IdentityProviderMfaAsserted,
        auth,
        cancellationToken);
    }

    pending.MarkLinkAccountRequired();
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new ExternalSignInResponseDto
    {
      LinkAccountRequired = new ExternalAccountLinkRequiredDto(
        pending.State,
        matchedUser.Email,
        provider.ProviderKey,
        provider.DisplayName)
    });
  }

  private async Task<Result<ExternalSignInResponseDto>> RegisterAndSignInAsync(
    ExternalAuthPending pending,
    ExternalProviderConfiguration provider,
    OidcSignInResult assertion,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    if (!assertion.EmailVerified || string.IsNullOrWhiteSpace(assertion.Email))
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.NoAccountExistsMessage);
    }

    var subjectLinkedElsewhere = await context.ExternalLogins.AnyAsync(
      x => x.ProviderKey == provider.ProviderKey && x.ProviderSubject == assertion.ProviderSubject,
      cancellationToken);
    if (subjectLinkedElsewhere)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Conflict(ExternalAuthUtils.ExternalAccountAlreadyLinkedMessage);
    }

    var defaultRole = await context.Roles
      .FirstOrDefaultAsync(x => x.Name == RoleConstraints.UserRoleName, cancellationToken);
    if (defaultRole is null)
      return Result<ExternalSignInResponseDto>.CriticalError("Default user role is not configured.");

    var emailVerified = !auth.EmailVerificationEnabled || assertion.EmailVerified;
    var createUserResult = User.CreateInvited(
      assertion.Email,
      assertion.FirstName,
      assertion.LastName,
      emailVerified);
    if (!createUserResult.IsSuccess)
      return createUserResult.Map();

    var user = createUserResult.Value;
    user.AssignRole(defaultRole.Id);

    var externalLoginResult = ExternalLogin.Create(
      user.Id,
      provider.ProviderKey,
      assertion.ProviderSubject);
    if (!externalLoginResult.IsSuccess)
      return externalLoginResult.Map();

    var addLoginResult = user.AddExternalLogin(externalLoginResult.Value);
    if (!addLoginResult.IsSuccess)
      return Result<ExternalSignInResponseDto>.Error(addLoginResult.Errors.First());

    await context.Users.AddAsync(user, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);

    return await ExternalSignInAuthUtils.IssueSignInResponseAsync(
      context,
      jwtTokenGenerator,
      sessionLifetime,
      passwordExpirationEvaluator,
      httpContextAccessor,
      auth,
      user,
      assertion.IdentityProviderMfaAsserted,
      cancellationToken);
  }

  private async Task<Result<ExternalSignInResponseDto>> CompleteStepUpModeAsync(
    ExternalAuthPending pending,
    ExternalProviderConfiguration provider,
    Guid userId,
    OidcSignInResult assertion,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    var login = user.ExternalLogins.FirstOrDefault(x =>
      x.ProviderKey.Equals(provider.ProviderKey, StringComparison.OrdinalIgnoreCase)
      && x.ProviderSubject.Equals(assertion.ProviderSubject, StringComparison.Ordinal));
    if (login is null)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);
    }

    login.RecordStepUp(DateTime.UtcNow);
    await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new ExternalSignInResponseDto { ExternalStepUpCompleted = true });
  }

  private async Task<Result<ExternalSignInResponseDto>> CompleteLinkModeAsync(
    ExternalAuthPending pending,
    ExternalProviderConfiguration provider,
    Guid userId,
    OidcSignInResult assertion,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.ExternalSignInFailedMessage);

    if (!ExternalAuthUtils.IsEmailDomainAllowed(assertion.Email, provider.AllowedEmailDomains))
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(ExternalAuthUtils.SignInNotAllowedMessage);
    }

    if (!ExternalAuthUtils.ProviderEmailMatchesUser(user, assertion.Email, assertion.EmailVerified))
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Error(ExternalAuthUtils.ExternalProviderEmailMismatchMessage);
    }

    var subjectLinkedElsewhere = await context.ExternalLogins.AnyAsync(
      x => x.ProviderKey == provider.ProviderKey
           && x.ProviderSubject == assertion.ProviderSubject
           && x.UserId != user.Id,
      cancellationToken);
    if (subjectLinkedElsewhere)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Conflict(ExternalAuthUtils.ExternalAccountAlreadyLinkedMessage);
    }

    var linkResult = await LinkExternalLoginAsync(user, provider, assertion, cancellationToken);
    if (!linkResult.IsSuccess)
      return linkResult.Map();

    var linkedLogin = user.ExternalLogins.First(x =>
      x.ProviderKey.Equals(provider.ProviderKey, StringComparison.OrdinalIgnoreCase));
    linkedLogin.RecordStepUp(DateTime.UtcNow);

    await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendExternalAccountLinkedAsync(
      user,
      provider.DisplayName,
      cancellationToken);

    return Result.Success(new ExternalSignInResponseDto { AccountLinkCompleted = true });
  }

  private async Task<Result<ExternalSignInResponseDto>> SignInExistingUserAsync(
    ExternalAuthPending pending,
    User user,
    bool identityProviderMfaAsserted,
    AuthOptions auth,
    CancellationToken cancellationToken)
  {
    if (!user.IsActive)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(AuthSessionUtils.DeactivatedAccountMessage);
    }

    if (auth.EmailVerificationEnabled && !user.EmailVerified)
    {
      await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);
      return Result<ExternalSignInResponseDto>.Unauthorized(AuthSessionUtils.EmailNotVerifiedMessage);
    }

    await RecordSignInStepUpAsync(user.Id, pending, cancellationToken);
    await ExternalAuthUtils.DeletePendingAsync(context, pending, cancellationToken);

    return await ExternalSignInAuthUtils.IssueSignInResponseAsync(
      context,
      jwtTokenGenerator,
      sessionLifetime,
      passwordExpirationEvaluator,
      httpContextAccessor,
      auth,
      user,
      identityProviderMfaAsserted,
      cancellationToken);
  }

  private async Task<Result> LinkExternalLoginAsync(
    User user,
    ExternalProviderConfiguration provider,
    OidcSignInResult assertion,
    CancellationToken cancellationToken)
  {
    var externalLoginResult = ExternalLogin.Create(
      user.Id,
      provider.ProviderKey,
      assertion.ProviderSubject);
    if (!externalLoginResult.IsSuccess)
      return externalLoginResult.Map();

    var addLoginResult = user.AddExternalLogin(externalLoginResult.Value);
    if (!addLoginResult.IsSuccess)
      return addLoginResult;

    await context.SaveChangesAsync(cancellationToken);
    return Result.Success();
  }

  private async Task RecordSignInStepUpAsync(
    Guid userId,
    ExternalAuthPending pending,
    CancellationToken cancellationToken)
  {
    if (string.IsNullOrWhiteSpace(pending.ProviderSubject))
      return;

    var login = await context.ExternalLogins
      .FirstOrDefaultAsync(
        x => x.UserId == userId
             && x.ProviderKey == pending.ProviderKey
             && x.ProviderSubject == pending.ProviderSubject,
        cancellationToken);
    if (login is null)
      return;

    login.RecordStepUp(DateTime.UtcNow);
    await context.SaveChangesAsync(cancellationToken);
  }
}
