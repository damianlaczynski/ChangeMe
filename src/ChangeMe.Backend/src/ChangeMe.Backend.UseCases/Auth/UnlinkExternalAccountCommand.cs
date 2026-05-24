using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using Microsoft.Extensions.Options;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record UnlinkExternalAccountCommand(
  string ProviderKey,
  string? CurrentPassword,
  string? VerificationCode) : ICommand<bool>;

public class UnlinkExternalAccountHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  ITotpService totpService,
  ITwoFactorSecretProtector secretProtector,
  IRecoveryCodeHasher recoveryCodeHasher,
  IAuthEmailService authEmailService,
  IUserAccessor userAccessor,
  IOptions<AuthOptions> authOptions) : ICommandHandler<UnlinkExternalAccountCommand, bool>
{
  public async Task<Result<bool>> Handle(
    UnlinkExternalAccountCommand command,
    CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    var auth = authOptions.Value;
    if (!auth.ExternalProvidersEnabled)
      return Result<bool>.Forbidden(ExternalAuthUtils.ExternalProvidersDisabledMessage);

    var user = await context.Users
      .Include(x => x.ExternalLogins)
      .Include(x => x.RecoveryCodes)
      .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<bool>.Unauthorized();

    var canUnlinkResult = ExternalAuthUtils.ValidateCanUnlinkExternalLogin(user, command.ProviderKey);
    if (!canUnlinkResult.IsSuccess)
      return canUnlinkResult.Map();

    var utcNow = DateTime.UtcNow;
    var stepUpResult = TwoFactorStepUpUtils.ValidateStepUp(
      user,
      command.CurrentPassword,
      command.VerificationCode,
      passwordHasher,
      totpService,
      secretProtector,
      recoveryCodeHasher,
      authOptions,
      utcNow,
      out var consumedRecoveryCode);
    if (!stepUpResult.IsSuccess)
      return stepUpResult.Map();

    if (consumedRecoveryCode is not null)
    {
      consumedRecoveryCode.MarkUsed(utcNow);
      await authEmailService.SendRecoveryCodeUsedAsync(user, cancellationToken);
    }

    var displayName = ExternalAuthUtils.ResolveProviderDisplayName(auth, command.ProviderKey);
    var removeResult = user.RemoveExternalLogin(command.ProviderKey);
    if (!removeResult.IsSuccess)
      return removeResult.Map();

    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendExternalAccountUnlinkedAsync(user, displayName, cancellationToken);

    return Result.Success(true);
  }
}
