using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RegenerateRecoveryCodesCommand(
  string? CurrentPassword,
  string? VerificationCode) : ICommand<TwoFactorSetupCompletedDto>;

public class RegenerateRecoveryCodesHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IAuthEmailService authEmailService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<RegenerateRecoveryCodesCommand, TwoFactorSetupCompletedDto>
{
  public async Task<Result<TwoFactorSetupCompletedDto>> Handle(
    RegenerateRecoveryCodesCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<TwoFactorSetupCompletedDto>.Unauthorized();

    if (!twoFactorPolicyEvaluator.IsTwoFactorEnabledForDeployment())
      return Result<TwoFactorSetupCompletedDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users
      .Include(x => x.RecoveryCodes)
      .Include(x => x.ExternalLogins)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<TwoFactorSetupCompletedDto>.Unauthorized();

    if (!user.TwoFactorEnabled)
      return Result<TwoFactorSetupCompletedDto>.Error("Two-factor authentication is not enabled.");

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
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);

    var plainCodes = RecoveryCodeGenerator.GenerateCodes(
      authOptions.Value.TwoFactor.RecoveryCodeCount,
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
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(new TwoFactorSetupCompletedDto(plainCodes.ToList()));
  }
}
