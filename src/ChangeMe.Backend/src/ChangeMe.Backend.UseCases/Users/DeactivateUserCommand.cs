using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record DeactivateUserCommand(Guid Id) : ICommand<UserDetailsDto>;

public class DeactivateUserHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<DeactivateUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(DeactivateUserCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId == command.Id)
      return Result<UserDetailsDto>.Error(UsersSupport.CannotDeactivateOwnAccountMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (!user.IsActive)
    {
      return await new GetUserByIdHandler(context)
        .Handle(new GetUserByIdQuery(user.Id), cancellationToken);
    }

    user.Deactivate();
    await UsersSupport.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return await new GetUserByIdHandler(context)
      .Handle(new GetUserByIdQuery(user.Id), cancellationToken);
  }
}
