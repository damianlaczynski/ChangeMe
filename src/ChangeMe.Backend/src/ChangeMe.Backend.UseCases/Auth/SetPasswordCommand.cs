using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record SetPasswordCommand(
  string NewPassword,
  string? CurrentPassword,
  string? VerificationCode) : ICommand<bool>;

public class SetPasswordHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<SetPasswordCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(SetPasswordCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<bool>.Unauthorized();

    if (user.HasPasswordSet)
      return Result<bool>.Error("A password is already set. Use change password instead.");

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

    var passwordHash = passwordHasher.HashPassword(command.NewPassword);
    var updateResult = user.SetPasswordHash(passwordHash);
    if (!updateResult.IsSuccess)
      return Result<bool>.Invalid(updateResult.ValidationErrors);

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}
