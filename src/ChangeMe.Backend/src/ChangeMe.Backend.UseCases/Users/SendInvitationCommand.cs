using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record SendInvitationCommand(Guid Id) : ICommand<UserDetailsDto>;

public class SendInvitationHandler(
  IMediator mediator,
  ApplicationDbContext context,
  UserInvitationService invitationService) : ICommandHandler<SendInvitationCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    SendInvitationCommand command,
    CancellationToken cancellationToken)
  {
    var user = await context.Users
      .Include(x => x.AccountInvitations)
      .FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (user.HasPasswordSet)
      return Result<UserDetailsDto>.Error(UsersUtils.InvitationAlreadyAcceptedMessage);

    if (user.Deactivated)
      return Result<UserDetailsDto>.Error(UsersUtils.CannotManageInvitationForDeactivatedMessage);

    if (user.HasPendingInvitation)
      return Result<UserDetailsDto>.Error(UsersUtils.PendingInvitationAlreadyExistsMessage);

    var invitationResult = await invitationService.SendInvitationAsync(user, cancellationToken);
    if (!invitationResult.IsSuccess)
      return invitationResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
  }
}
