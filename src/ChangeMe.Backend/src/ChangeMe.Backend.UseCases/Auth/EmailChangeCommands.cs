using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RequestEmailChangeCommand(
  string NewEmail,
  string? CurrentPassword,
  string? VerificationCode) : ICommand<MyAccountDto>;

public class RequestEmailChangeHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  TimeProvider timeProvider) : ICommandHandler<RequestEmailChangeCommand, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(
    RequestEmailChangeCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var auth = authOptions.Value;
    if (!auth.EmailChange.Enabled)
      return Result<MyAccountDto>.Forbidden("Self-service email change is not available.");

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    if (user.HasPendingInvitation)
      return Result<MyAccountDto>.Error("Complete your invitation before changing your email.");

    if (user.HasPendingEmailChange)
      return Result<MyAccountDto>.Error("An email change is already pending.");

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    var stepUpResult = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      command.CurrentPassword,
      command.VerificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment(),
      passkeyCount,
      utcNow,
      out var consumedRecoveryCode);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    var normalizedNew = User.NormalizeEmail(command.NewEmail);
    if (await UsersUtils.IsProfileEmailTakenAsync(context, normalizedNew, userId, cancellationToken))
      return Result<MyAccountDto>.Conflict(AuthSessionUtils.DuplicateEmailMessage);

    var beginResult = user.BeginPendingEmailChange(command.NewEmail, utcNow);
    if (!beginResult.IsSuccess)
      return beginResult.Map();

    if (consumedRecoveryCode is not null)
      consumedRecoveryCode.MarkUsed(utcNow);

    var tokenResult = await tokenService.IssueTokenAsync(
      userId,
      UserAuthTokenType.EmailChangeConfirmation,
      utcNow,
      cancellationToken);
    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    var confirmEmailResult = await authEmailService.SendConfirmEmailChangeAsync(
      user.PendingNewEmail!,
      tokenResult.Value,
      cancellationToken);
    if (!confirmEmailResult.IsSuccess)
      return confirmEmailResult.Map();

    var requestedResult = await authEmailService.SendEmailChangeRequestedAsync(user, cancellationToken);
    if (!requestedResult.IsSuccess)
      return requestedResult.Map();

    return await mediator.Send(new GetMyAccountQuery(), cancellationToken);
  }
}

public sealed record CancelEmailChangeCommand(
  string? CurrentPassword,
  string? VerificationCode) : ICommand<MyAccountDto>;

public class CancelEmailChangeHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  TimeProvider timeProvider) : ICommandHandler<CancelEmailChangeCommand, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(
    CancelEmailChangeCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    if (!user.HasPendingEmailChange)
      return Result<MyAccountDto>.Error("No email change is pending.");

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var passkeyCount = await context.PasskeyCredentials.CountAsync(x => x.UserId == userId, cancellationToken);
    var stepUpResult = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      command.CurrentPassword,
      command.VerificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment(),
      passkeyCount,
      utcNow,
      out var consumedRecoveryCode);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    if (consumedRecoveryCode is not null)
      consumedRecoveryCode.MarkUsed(utcNow);

    user.CancelPendingEmailChange();
    await tokenService.InvalidateUnusedTokensAsync(
      userId,
      UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var emailResult = await authEmailService.SendEmailChangeCancelledAsync(user, cancellationToken);
    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return await mediator.Send(new GetMyAccountQuery(), cancellationToken);
  }
}

public sealed record ResendEmailChangeConfirmationCommand() : ICommand<MyAccountDto>;

public class ResendEmailChangeConfirmationHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<ResendEmailChangeConfirmationCommand, MyAccountDto>
{
  public async Task<Result<MyAccountDto>> Handle(
    ResendEmailChangeConfirmationCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<MyAccountDto>.Unauthorized();

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<MyAccountDto>.Unauthorized();

    if (!user.HasPendingEmailChange || user.PendingNewEmail is null)
      return Result<MyAccountDto>.Error("No email change is pending.");

    var utcNow = timeProvider.GetUtcNow().UtcDateTime;
    var tokenResult = await tokenService.IssueTokenAsync(
      userId,
      UserAuthTokenType.EmailChangeConfirmation,
      utcNow,
      cancellationToken);
    if (!tokenResult.IsSuccess)
      return tokenResult.Map();

    var emailResult = await authEmailService.SendConfirmEmailChangeAsync(
      user.PendingNewEmail,
      tokenResult.Value,
      cancellationToken);
    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return await mediator.Send(new GetMyAccountQuery(), cancellationToken);
  }
}

public sealed record ConfirmEmailChangeCommand(string Token) : ICommand<ConfirmEmailChangeResponseDto>;

public sealed record ConfirmEmailChangeResponseDto(
  bool Succeeded,
  string? Message,
  bool WrongSignedInAccount);

public class ConfirmEmailChangeHandler(
  ApplicationDbContext context,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : ICommandHandler<ConfirmEmailChangeCommand, ConfirmEmailChangeResponseDto>
{
  public const string InvalidLinkMessage = "This confirmation link is invalid or has expired.";
  public const string TargetEmailTakenMessage =
    "An account with this email already exists. Cancel the pending email change on My account.";
  public const string SuccessMessage = "Email changed. Sign in with your new email address.";
  public const string WrongAccountMessage =
    "This confirmation link belongs to another account. Sign out and open the link again, or sign in as the account that requested the change.";

  public async Task<Result<ConfirmEmailChangeResponseDto>> Handle(
    ConfirmEmailChangeCommand command,
    CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      command.Token,
      UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    if (!validateResult.IsSuccess)
    {
      return Result.Success(new ConfirmEmailChangeResponseDto(
        false,
        InvalidLinkMessage,
        false));
    }

    var targetUserId = validateResult.Value;
    if (userAccessor.UserId is Guid signedInUserId && signedInUserId != targetUserId)
    {
      return Result.Success(new ConfirmEmailChangeResponseDto(
        false,
        WrongAccountMessage,
        true));
    }

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == targetUserId, cancellationToken);
    if (user is null || !user.HasPendingEmailChange || user.PendingNewEmail is null)
    {
      return Result.Success(new ConfirmEmailChangeResponseDto(
        false,
        InvalidLinkMessage,
        false));
    }

    var previousEmail = user.Email;
    var newEmail = user.PendingNewEmail;
    var normalizedNew = user.PendingNewEmailNormalized!;
    var utcNow = timeProvider.GetUtcNow().UtcDateTime;

    if (await UsersUtils.IsProfileEmailTakenAsync(context, normalizedNew, targetUserId, cancellationToken))
    {
      return Result.Success(new ConfirmEmailChangeResponseDto(
        false,
        TargetEmailTakenMessage,
        false));
    }

    var confirmResult = user.ConfirmPendingEmailChange(utcNow);
    if (!confirmResult.IsSuccess)
      return confirmResult.Map();

    await tokenService.MarkTokenUsedAsync(command.Token, cancellationToken);
    await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var emailResult = await authEmailService.SendEmailChangeCompletedAsync(
      previousEmail,
      newEmail,
      cancellationToken);
    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return Result.Success(new ConfirmEmailChangeResponseDto(
      true,
      SuccessMessage,
      false));
  }
}
