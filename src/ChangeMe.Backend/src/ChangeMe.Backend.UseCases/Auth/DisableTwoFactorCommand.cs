using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record DisableTwoFactorCommand(
  string? CurrentPassword,
  string? VerificationCode) : ICommand<bool>;

public class DisableTwoFactorHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IAuthEmailService authEmailService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<DisableTwoFactorCommand, bool>
{
  public async Task<Result<bool>> Handle(DisableTwoFactorCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    if (!twoFactorPolicyEvaluator.IsTwoFactorEnabledForDeployment())
      return Result<bool>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    if (authOptions.Value.TwoFactor.Required)
      return Result<bool>.Error("Two-factor authentication is required and cannot be disabled.");

    var user = await context.Users
      .Include(x => x.RecoveryCodes)
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<bool>.Unauthorized();

    if (!user.TwoFactorEnabled)
      return Result<bool>.Error("Two-factor authentication is not enabled.");

    var utcNow = DateTime.UtcNow;
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
      authOptions.Value.Passkeys.PasskeysAuthenticationEnabled,
      passkeyCount,
      utcNow,
      out var consumedRecoveryCode);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    if (consumedRecoveryCode is not null)
    {
      consumedRecoveryCode.MarkUsed(utcNow);
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);
    }

    user.DisableTwoFactor();
    await UserRecoveryCodePersistenceUtils.DeleteAllForUserAsync(context, userId, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendTwoFactorDisabledAsync(user, cancellationToken);

    return Result.Success(true);
  }
}
