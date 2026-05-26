using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record ResetUserTwoFactorCommand(Guid Id) : ICommand<UserDetailsDto>;

public class ResetUserTwoFactorHandler(
  ApplicationDbContext context,
  ITwoFactorPolicyEvaluator twoFactorPolicyEvaluator,
  IAuthEmailService authEmailService,
  IMediator mediator) : ICommandHandler<ResetUserTwoFactorCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    ResetUserTwoFactorCommand command,
    CancellationToken cancellationToken)
  {
    if (!twoFactorPolicyEvaluator.IsTwoFactorEnabledForDeployment())
      return Result<UserDetailsDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (!user.TwoFactorEnabled)
      return Result<UserDetailsDto>.Error("Two-factor authentication is not enabled for this user.");

    user.DisableTwoFactor();
    await UserRecoveryCodePersistenceUtils.DeleteAllForUserAsync(context, user.Id, cancellationToken);
    await LogoutAllSessionsHandler.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendTwoFactorResetByAdminAsync(user, cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(command.Id), cancellationToken);
  }
}
