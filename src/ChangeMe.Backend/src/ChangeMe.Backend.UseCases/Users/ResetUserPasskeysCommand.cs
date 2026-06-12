using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record ResetUserPasskeysCommand(Guid Id) : ICommand<UserDetailsDto>;

public class ResetUserPasskeysHandler(
  ApplicationDbContext context,
  IPasskeyPolicyEvaluator passkeyPolicyEvaluator,
  IAuthEmailService authEmailService,
  IMediator mediator) : ICommandHandler<ResetUserPasskeysCommand, UserDetailsDto>
{
  public async ValueTask<Result<UserDetailsDto>> Handle(
    ResetUserPasskeysCommand command,
    CancellationToken cancellationToken)
  {
    if (!passkeyPolicyEvaluator.IsPasskeysEnabledForDeployment())
      return Result<UserDetailsDto>.Forbidden(AuthSessionUtils.PermissionDeniedMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    var hasPasskeys = await context.PasskeyCredentials.AnyAsync(x => x.UserId == command.Id, cancellationToken);
    if (!hasPasskeys)
      return Result<UserDetailsDto>.Error("This user has no passkeys registered.");

    await context.PasskeyCredentials
      .Where(x => x.UserId == command.Id)
      .ExecuteDeleteAsync(cancellationToken);

    await LogoutAllSessionsHandler.RevokeAllActiveSessionsAsync(context, command.Id, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    await authEmailService.SendPasskeysResetByAdminAsync(user, cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(command.Id), cancellationToken);
  }
}
