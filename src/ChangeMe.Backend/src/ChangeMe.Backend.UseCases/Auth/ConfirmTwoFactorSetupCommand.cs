using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record ConfirmTwoFactorSetupCommand(string VerificationCode)
  : ICommand<TwoFactorSetupCompletedDto>;

public class ConfirmTwoFactorSetupHandler(
  ApplicationDbContext context,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IAuthEmailService authEmailService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<ConfirmTwoFactorSetupCommand, TwoFactorSetupCompletedDto>
{
  public async Task<Result<TwoFactorSetupCompletedDto>> Handle(
    ConfirmTwoFactorSetupCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<TwoFactorSetupCompletedDto>.Unauthorized();

    if (!twoFactorPolicyEvaluator.IsTwoFactorEnabledForDeployment())
      return Result<TwoFactorSetupCompletedDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<TwoFactorSetupCompletedDto>.Unauthorized();

    if (user.TwoFactorEnabled)
      return Result<TwoFactorSetupCompletedDto>.Error("Two-factor authentication is already enabled.");

    var pending = await context.TwoFactorEnrollmentPending
      .AsNoTracking()
      .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    var utcNow = DateTime.UtcNow;
    if (pending is null || pending.IsExpired(utcNow))
      return Result<TwoFactorSetupCompletedDto>.Unauthorized("Two-factor setup timed out. Start again.");

    var sharedSecret = secretProtector.Unprotect(pending.SecretCiphertext);
    if (!totpService.ValidateCode(sharedSecret, command.VerificationCode, utcNow))
    {
      return Result<TwoFactorSetupCompletedDto>.Invalid(new ValidationError(
        nameof(command.VerificationCode),
        TwoFactorAuthUtils.InvalidVerificationCodeMessage));
    }

    var protectedSecret = secretProtector.Protect(sharedSecret);
    var enableResult = user.EnableTwoFactor(protectedSecret, utcNow);
    if (!enableResult.IsSuccess)
      return Result<TwoFactorSetupCompletedDto>.Invalid(enableResult.ValidationErrors);

    var twoFactorOptions = authOptions.Value.TwoFactor;
    var plainCodes = RecoveryCodeGenerator.GenerateCodes(
      twoFactorOptions.RecoveryCodeCount,
      TwoFactorConstraints.RECOVERY_CODE_LENGTH);
    var recoveryEntities = new List<UserRecoveryCode>();
    foreach (var plainCode in plainCodes)
    {
      var codeResult = UserRecoveryCode.Create(userId, recoveryCodeHasher.Hash(plainCode));
      if (!codeResult.IsSuccess)
        return Result<TwoFactorSetupCompletedDto>.Invalid(codeResult.ValidationErrors);

      recoveryEntities.Add(codeResult.Value);
    }

    await UserRecoveryCodePersistenceUtils.ReplaceAllForUserAsync(
      context,
      userId,
      recoveryEntities,
      cancellationToken);

    var deletedPendingCount = await context.TwoFactorEnrollmentPending
      .Where(x => x.UserId == userId)
      .ExecuteDeleteAsync(cancellationToken);
    if (deletedPendingCount == 0)
      return Result<TwoFactorSetupCompletedDto>.Unauthorized("Two-factor setup timed out. Start again.");

    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendTwoFactorEnabledAsync(user, cancellationToken);

    return Result.Success(new TwoFactorSetupCompletedDto(plainCodes.ToList()));
  }
}
