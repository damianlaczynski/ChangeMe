using System.Text.Json;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.Infrastructure.Auth.Passkey;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record BeginPasskeySignInCommand(string? Email) : ICommand<PasskeyCeremonyBeginResponseDto>;

public class BeginPasskeySignInHandler(
  ApplicationDbContext context,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginPasskeySignInCommand, PasskeyCeremonyBeginResponseDto>
{
  public async ValueTask<Result<PasskeyCeremonyBeginResponseDto>> Handle(
    BeginPasskeySignInCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<PasskeyCeremonyBeginResponseDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var auth = authOptions.Value;
    var discoverable = auth.Passkeys.DiscoverablePasskeySignInOnLogin
      && string.IsNullOrWhiteSpace(command.Email);

    IReadOnlyList<byte[]>? allowIds = null;
    string? normalizedEmail = null;

    if (!discoverable)
    {
      if (string.IsNullOrWhiteSpace(command.Email))
        return Result<PasskeyCeremonyBeginResponseDto>.Invalid(new ValidationError(
          nameof(command.Email),
          "Enter your email to use a passkey."));

      normalizedEmail = User.NormalizeEmail(command.Email);
      var user = await context.Users
        .AsNoTracking()
        .Include(x => x.ExternalLogins)
        .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail, cancellationToken);

      if (user is null)
        return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized(PasskeyAuthUtils.NoPasskeyForAccountMessage);

      var count = await context.PasskeyCredentials.CountAsync(x => x.UserId == user.Id, cancellationToken);
      if (count == 0)
        return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized(PasskeyAuthUtils.NoPasskeyForAccountMessage);

      if (!PasskeyAuthUtils.CanUsePasskeySignIn(user, count, auth))
        return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized(PasskeyAuthUtils.PasskeyOnlyNotAllowedMessage);

      allowIds = await context.PasskeyCredentials
        .AsNoTracking()
        .Where(x => x.UserId == user.Id)
        .Select(x => x.CredentialId)
        .ToListAsync(cancellationToken);
    }

    var options = passkeyFido2.BeginAuthentication(allowIds, discoverable);
    var utcNow = DateTime.UtcNow;
    var expiresAt = utcNow.AddMinutes(auth.Passkeys.ChallengeLifetimeMinutes);
    var ceremonyResult = await PasskeyCeremonyUtils.StoreCeremonyAsync(
      context,
      WebAuthnCeremonyType.Authentication,
      PasskeyCeremonyUtils.SerializeAssertionOptions(options),
      expiresAt,
      cancellationToken,
      normalizedEmail: normalizedEmail);

    if (!ceremonyResult.IsSuccess)
      return ceremonyResult.Map();

    return Result.Success(new PasskeyCeremonyBeginResponseDto(
      ceremonyResult.Value.Id,
      JsonSerializer.Deserialize<object>(PasskeyCeremonyUtils.SerializeAssertionOptions(options))!));
  }
}

public sealed record CompletePasskeySignInCommand(
  Guid CeremonyId,
  AuthenticatorAssertionRawResponse AssertionResponse) : ICommand<LoginResponseDto>;

public class CompletePasskeySignInHandler(
  ApplicationDbContext context,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IJwtTokenGenerator jwtTokenGenerator,
  ISessionLifetimeService sessionLifetime,
  IPasswordExpirationEvaluator passwordExpirationEvaluator,
  IHttpContextAccessor httpContextAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<CompletePasskeySignInCommand, LoginResponseDto>
{
  public async ValueTask<Result<LoginResponseDto>> Handle(
    CompletePasskeySignInCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<LoginResponseDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var auth = authOptions.Value;
    var utcNow = DateTime.UtcNow;
    var maxAttempts = auth.Passkeys.MaxFailedPasskeyAttempts;

    var ceremony = await PasskeyCeremonyUtils.LoadCeremonyAsync(
      context,
      command.CeremonyId,
      WebAuthnCeremonyType.Authentication,
      cancellationToken);

    if (ceremony is null || ceremony.IsExpired(utcNow))
      return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.TimedOutMessage);

    if (ceremony.FailedAttemptCount >= maxAttempts)
      return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

    var options = PasskeyCeremonyUtils.DeserializeAssertionOptions(ceremony.OptionsJson);
    var credentialId = command.AssertionResponse.RawId;

    var stored = await context.PasskeyCredentials
      .Include(x => x.User)
      .ThenInclude(x => x.ExternalLogins)
      .Include(x => x.User)
      .ThenInclude(x => x.AccountInvitations)
      .FirstOrDefaultAsync(x => x.CredentialId == credentialId, cancellationToken);

    if (stored is null)
    {
      await PasskeyCeremonyUtils.RecordFailedAttemptAsync(context, ceremony, cancellationToken);
      if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
        return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

      return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.NoMatchMessage);
    }

    var user = stored.User;

    if (!PasskeyAuthUtils.DoesCeremonyEmailMatchUser(ceremony, user))
    {
      await PasskeyCeremonyUtils.RecordFailedAttemptAsync(context, ceremony, cancellationToken);
      if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
        return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

      return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.NoMatchMessage);
    }

    try
    {
      var verify = await passkeyFido2.CompleteAuthenticationAsync(
        command.AssertionResponse,
        options,
        stored.PublicKey,
        stored.SignCount,
        stored.CredentialId,
        cancellationToken);

      stored.RecordUse(verify.SignCount, utcNow);

      if (!user.IsActive)
        return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.DeactivatedAccountMessage);

      if (ExternalAuthUtils.IsInvitationPending(user))
        return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.InvitePendingAccountMessage);

      if (auth.EmailVerification.Enabled && !user.EmailVerified)
        return Result<LoginResponseDto>.Unauthorized(AuthSessionUtils.EmailNotVerifiedMessage);

      var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == user.Id, cancellationToken);
      if (!PasskeyAuthUtils.CanUsePasskeySignIn(user, passkeyCount, auth))
        return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.PasskeyOnlyNotAllowedMessage);

      context.WebAuthnCeremonyPending.Remove(ceremony);
      await context.SaveChangesAsync(cancellationToken);

      var uvPresent = PasskeyWebAuthnUtils.HasUserVerification(command.AssertionResponse);
      return await PasskeyAuthUtils.IssuePasskeySignInResponseAsync(
        context,
        jwtTokenGenerator,
        sessionLifetime,
        passwordExpirationEvaluator,
        passkeyPolicy,
        httpContextAccessor,
        authOptions,
        user,
        uvPresent,
        cancellationToken);
    }
    catch (Fido2VerificationException)
    {
      await PasskeyCeremonyUtils.RecordFailedAttemptAsync(context, ceremony, cancellationToken);
      if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
        return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

      return Result<LoginResponseDto>.Unauthorized(PasskeyAuthUtils.NoMatchMessage);
    }
  }
}

public sealed record BeginPasskeyRegistrationCommand : ICommand<PasskeyCeremonyBeginResponseDto>
{
  /// <summary>Not read; required for FastEndpoints request binding (POST <c>{{}}</c>).</summary>
  public object? Unused { get; init; }
  public string? CurrentPassword { get; init; }
  public string? VerificationCode { get; init; }
}

public class BeginPasskeyRegistrationHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginPasskeyRegistrationCommand, PasskeyCeremonyBeginResponseDto>
{
  public async ValueTask<Result<PasskeyCeremonyBeginResponseDto>> Handle(
    BeginPasskeyRegistrationCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<PasskeyCeremonyBeginResponseDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized();

    var auth = authOptions.Value;
    var count = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    if (!passkeyPolicy.IsPasskeySetupRequired(user, count))
    {
      var (stepUpResult, consumedRecoveryCode) = await TwoFactorStepUpUtils.ValidateSignedInStepUpAsync(
        context,
        user,
        userId,
        command.CurrentPassword,
        command.VerificationCode,
        passwordHasher,
        totpService,
        secretProtector,
        recoveryCodeHasher,
        authOptions,
        cancellationToken);
      if (!stepUpResult.IsSuccess)
        return stepUpResult.Map();

      if (consumedRecoveryCode is not null)
      {
        consumedRecoveryCode.MarkUsed(DateTime.UtcNow);
        await context.SaveChangesAsync(cancellationToken);
      }
    }

    if (count >= auth.Passkeys.MaximumPasskeysPerUser)
      return Result<PasskeyCeremonyBeginResponseDto>.Error(PasskeyAuthUtils.MaximumPasskeysMessage);

    var existingIds = await context.PasskeyCredentials
      .Where(x => x.UserId == userId)
      .Select(x => x.CredentialId)
      .ToListAsync(cancellationToken);

    var options = passkeyFido2.BeginRegistration(user, existingIds, discoverable: true);
    var utcNow = DateTime.UtcNow;
    var ceremonyResult = await PasskeyCeremonyUtils.StoreCeremonyAsync(
      context,
      WebAuthnCeremonyType.Registration,
      PasskeyCeremonyUtils.SerializeCreateOptions(options),
      utcNow.AddMinutes(auth.Passkeys.ChallengeLifetimeMinutes),
      cancellationToken,
      userId: userId);

    if (!ceremonyResult.IsSuccess)
      return ceremonyResult.Map();

    return Result.Success(new PasskeyCeremonyBeginResponseDto(
      ceremonyResult.Value.Id,
      JsonSerializer.Deserialize<object>(PasskeyCeremonyUtils.SerializeCreateOptions(options))!));
  }
}

public sealed record CompletePasskeyRegistrationCommand(
  Guid CeremonyId,
  AuthenticatorAttestationRawResponse AttestationResponse,
  string Name,
  string? CurrentPassword,
  string? VerificationCode) : ICommand<MyAccountPasskeyDto>;

public class CompletePasskeyRegistrationHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<CompletePasskeyRegistrationCommand, MyAccountPasskeyDto>
{
  public async ValueTask<Result<MyAccountPasskeyDto>> Handle(
    CompletePasskeyRegistrationCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<MyAccountPasskeyDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountPasskeyDto>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountPasskeyDto>.Unauthorized();

    var existingCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    if (!passkeyPolicy.IsPasskeySetupRequired(user, existingCount))
    {
      var (stepUpResult, consumedRecoveryCode) = await TwoFactorStepUpUtils.ValidateSignedInStepUpAsync(
        context,
        user,
        userId,
        command.CurrentPassword,
        command.VerificationCode,
        passwordHasher,
        totpService,
        secretProtector,
        recoveryCodeHasher,
        authOptions,
        cancellationToken);
      if (!stepUpResult.IsSuccess)
        return stepUpResult.Map();

      if (consumedRecoveryCode is not null)
        consumedRecoveryCode.MarkUsed(DateTime.UtcNow);
    }

    var utcNow = DateTime.UtcNow;
    var ceremony = await PasskeyCeremonyUtils.LoadCeremonyAsync(
      context,
      command.CeremonyId,
      WebAuthnCeremonyType.Registration,
      cancellationToken);

    if (ceremony is null || ceremony.UserId != userId || ceremony.IsExpired(utcNow))
      return Result<MyAccountPasskeyDto>.Unauthorized(PasskeyAuthUtils.TimedOutMessage);

    var options = PasskeyCeremonyUtils.DeserializeCreateOptions(ceremony.OptionsJson);

    try
    {
      var registered = await passkeyFido2.CompleteRegistrationAsync(
        command.AttestationResponse,
        options,
        cancellationToken);

      var name = string.IsNullOrWhiteSpace(command.Name)
        ? $"Passkey {(await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken)) + 1}"
        : command.Name.Trim();

      var credential = PasskeyCredential.Create(
        userId,
        name,
        registered.Id,
        registered.PublicKey,
        registered.SignCount,
        registered.AaGuid,
        MapAuthenticatorType(registered.Transports),
        registered.IsBackupEligible,
        registered.IsBackedUp,
        utcNow);

      if (!credential.IsSuccess)
        return Result<MyAccountPasskeyDto>.Invalid(credential.ValidationErrors);

      await context.PasskeyCredentials.AddAsync(credential.Value, cancellationToken);
      context.WebAuthnCeremonyPending.Remove(ceremony);
      await context.SaveChangesAsync(cancellationToken);

      await authEmailService.SendPasskeyAddedAsync(user, name, cancellationToken);

      return Result.Success(Map(credential.Value));
    }
    catch (Fido2VerificationException)
    {
      return Result<MyAccountPasskeyDto>.Unauthorized(PasskeyAuthUtils.VerificationFailedMessage);
    }
  }

  private static string MapAuthenticatorType(IReadOnlyCollection<AuthenticatorTransport>? transports) =>
    transports is not null && transports.Contains(AuthenticatorTransport.Usb)
      ? "Security key"
      : "Platform";

  internal static MyAccountPasskeyDto Map(PasskeyCredential c) =>
    new(c.Id, c.Name, c.CreatedAtUtc, c.LastUsedAtUtc, c.AuthenticatorType, c.BackupEligible, c.BackupState);
}

public sealed record RenamePasskeyCommand : ICommand<MyAccountPasskeyDto>
{
  public Guid PasskeyId { get; init; }
  public string Name { get; init; } = string.Empty;
  public string? CurrentPassword { get; init; }
  public string? VerificationCode { get; init; }
}

public class RenamePasskeyHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<RenamePasskeyCommand, MyAccountPasskeyDto>
{
  public async ValueTask<Result<MyAccountPasskeyDto>> Handle(
    RenamePasskeyCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<MyAccountPasskeyDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountPasskeyDto>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountPasskeyDto>.Unauthorized();

    var (stepUpResult, consumedRecoveryCode) = await TwoFactorStepUpUtils.ValidateSignedInStepUpAsync(
      context,
      user,
      userId,
      command.CurrentPassword,
      command.VerificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      cancellationToken);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    if (consumedRecoveryCode is not null)
    {
      consumedRecoveryCode.MarkUsed(DateTime.UtcNow);
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);
    }

    var credential = await context.PasskeyCredentials
      .FirstOrDefaultAsync(x => x.Id == command.PasskeyId && x.UserId == userId, cancellationToken);

    if (credential is null)
      return Result<MyAccountPasskeyDto>.NotFound();

    credential.Rename(command.Name);
    await context.SaveChangesAsync(cancellationToken);
    return Result.Success(CompletePasskeyRegistrationHandler.Map(credential));
  }
}

public sealed record RemovePasskeyCommand : ICommand<bool>
{
  public Guid PasskeyId { get; init; }
  public string? CurrentPassword { get; init; }
  public string? VerificationCode { get; init; }
}

public class RemovePasskeyHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IOptions<AuthOptions> authOptions) : ICommandHandler<RemovePasskeyCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(RemovePasskeyCommand command, CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<bool>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null)
      return Result<bool>.Unauthorized();

    var (stepUpResult, consumedRecoveryCode) = await TwoFactorStepUpUtils.ValidateSignedInStepUpAsync(
      context,
      user,
      userId,
      command.CurrentPassword,
      command.VerificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      cancellationToken);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    if (consumedRecoveryCode is not null)
    {
      consumedRecoveryCode.MarkUsed(DateTime.UtcNow);
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);
    }

    var credential = await context.PasskeyCredentials
      .FirstOrDefaultAsync(x => x.Id == command.PasskeyId && x.UserId == userId, cancellationToken);
    if (credential is null)
      return Result<bool>.NotFound();

    var count = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    var auth = authOptions.Value;

    if (count == 1 && !user.HasPasswordSet && user.ExternalLogins.Count == 0)
      return Result<bool>.Error(PasskeyAuthUtils.RemoveOnlySignInMethodMessage);

    if (count == 1 && auth.Passkeys.PasskeysAuthenticationRequired)
      return Result<bool>.Error(PasskeyAuthUtils.RemoveRequiredPasskeyMessage);

    var name = credential.Name;
    context.PasskeyCredentials.Remove(credential);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendPasskeyRemovedAsync(user, name, cancellationToken);
    return Result.Success(true);
  }
}

public sealed record BeginPasskeyStepUpCommand : ICommand<PasskeyCeremonyBeginResponseDto>
{
  /// <summary>Not read; required for FastEndpoints request binding (POST <c>{{}}</c>).</summary>
  public object? Unused { get; init; }
}

public class BeginPasskeyStepUpHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IOptions<AuthOptions> authOptions) : ICommandHandler<BeginPasskeyStepUpCommand, PasskeyCeremonyBeginResponseDto>
{
  public async ValueTask<Result<PasskeyCeremonyBeginResponseDto>> Handle(
    BeginPasskeyStepUpCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<PasskeyCeremonyBeginResponseDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<PasskeyCeremonyBeginResponseDto>.Unauthorized();

    var allowIds = await context.PasskeyCredentials
      .Where(x => x.UserId == userId)
      .Select(x => x.CredentialId)
      .ToListAsync(cancellationToken);

    if (allowIds.Count == 0)
      return Result<PasskeyCeremonyBeginResponseDto>.Error("No passkeys registered.");

    var options = passkeyFido2.BeginAuthentication(allowIds, discoverable: false);
    var auth = authOptions.Value;
    var ceremonyResult = await PasskeyCeremonyUtils.StoreCeremonyAsync(
      context,
      WebAuthnCeremonyType.StepUp,
      PasskeyCeremonyUtils.SerializeAssertionOptions(options),
      DateTime.UtcNow.AddMinutes(auth.Passkeys.ChallengeLifetimeMinutes),
      cancellationToken,
      userId: userId);

    if (!ceremonyResult.IsSuccess)
      return ceremonyResult.Map();

    return Result.Success(new PasskeyCeremonyBeginResponseDto(
      ceremonyResult.Value.Id,
      JsonSerializer.Deserialize<object>(PasskeyCeremonyUtils.SerializeAssertionOptions(options))!));
  }
}

public sealed record CompletePasskeyStepUpCommand(
  Guid CeremonyId,
  AuthenticatorAssertionRawResponse AssertionResponse) : ICommand<bool>;

public class CompletePasskeyStepUpHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  IPasskeyFido2Service passkeyFido2,
  IPasskeyPolicyEvaluator passkeyPolicy,
  IOptions<AuthOptions> authOptions) : ICommandHandler<CompletePasskeyStepUpCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(CompletePasskeyStepUpCommand command, CancellationToken cancellationToken)
  {
    if (!passkeyPolicy.IsPasskeysEnabledForDeployment())
      return Result<bool>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    var auth = authOptions.Value;
    var maxAttempts = auth.Passkeys.MaxFailedPasskeyAttempts;
    var utcNow = DateTime.UtcNow;
    var ceremony = await PasskeyCeremonyUtils.LoadCeremonyAsync(
      context,
      command.CeremonyId,
      WebAuthnCeremonyType.StepUp,
      cancellationToken);

    if (ceremony is null || ceremony.UserId != userId || ceremony.IsExpired(utcNow))
      return Result<bool>.Unauthorized(PasskeyAuthUtils.TimedOutMessage);

    if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
      return Result<bool>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

    var options = PasskeyCeremonyUtils.DeserializeAssertionOptions(ceremony.OptionsJson);
    var credentialId = command.AssertionResponse.RawId;

    var stored = await context.PasskeyCredentials
      .FirstOrDefaultAsync(x => x.UserId == userId && x.CredentialId == credentialId, cancellationToken);

    if (stored is null)
    {
      await PasskeyCeremonyUtils.RecordFailedAttemptAsync(context, ceremony, cancellationToken);
      if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
        return Result<bool>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

      return Result<bool>.Unauthorized(PasskeyAuthUtils.VerificationFailedMessage);
    }

    try
    {
      var verify = await passkeyFido2.CompleteAuthenticationAsync(
        command.AssertionResponse,
        options,
        stored.PublicKey,
        stored.SignCount,
        stored.CredentialId,
        cancellationToken);

      stored.RecordUse(verify.SignCount, utcNow);
      var user = await context.Users.FirstAsync(x => x.Id == userId, cancellationToken);
      user.RecordPasskeyStepUp(utcNow);
      context.WebAuthnCeremonyPending.Remove(ceremony);
      await context.SaveChangesAsync(cancellationToken);
      return Result.Success(true);
    }
    catch (Fido2VerificationException)
    {
      await PasskeyCeremonyUtils.RecordFailedAttemptAsync(context, ceremony, cancellationToken);
      if (PasskeyCeremonyUtils.IsAttemptLimitReached(ceremony, maxAttempts))
        return Result<bool>.Unauthorized(PasskeyAuthUtils.TooManyAttemptsMessage);

      return Result<bool>.Unauthorized(PasskeyAuthUtils.VerificationFailedMessage);
    }
  }
}
