using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record SendPasswordResetCommand(Guid Id) : ICommand<UserDetailsDto>;

public class SendPasswordResetHandler(
  IMediator mediator,
  ApplicationDbContext context,
  UserPasswordResetService passwordResetService) : ICommandHandler<SendPasswordResetCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    SendPasswordResetCommand command,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .Include(x => x.AccountInvitations)
      .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (!user.IsActive)
      return Result<UserDetailsDto>.Error(UsersUtils.CannotSendPasswordResetToDeactivatedMessage);

    if (user.HasPendingInvitation)
      return Result<UserDetailsDto>.Error(UsersUtils.CannotSendPasswordResetToInvitePendingMessage);

    if (!user.HasPasswordSet)
      return Result<UserDetailsDto>.Error(UsersUtils.CannotSendPasswordResetWithoutLocalPasswordMessage);

    var resetResult = await passwordResetService.SendPasswordResetAsync(user, cancellationToken);
    if (!resetResult.IsSuccess)
      return resetResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
  }
}
