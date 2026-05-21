using ChangeMe.Backend.UseCases.Users.Dtos;

using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record DeactivateUserCommand(Guid Id) : ICommand<UserDetailsDto>;

public class DeactivateUserHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeactivateUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId == command.Id)
      return Result<UserDetailsDto>.Error(UsersUtils.CannotDeactivateOwnAccountMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (user.IsActive)
    {
      user.Deactivate();
      await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
      await context.SaveChangesAsync(cancellationToken);
    }

    var userResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    if (!userResult.IsSuccess)
      return userResult.Map();

    return userResult;
  }
}
