using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record ActivateUserCommand(Guid Id) : ICommand<UserDetailsDto>;

public class ActivateUserHandler(
  ApplicationDbContext context) : ICommandHandler<ActivateUserCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(ActivateUserCommand command, CancellationToken cancellationToken)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (user.IsActive)
    {
      return await new GetUserByIdHandler(context)
        .Handle(new GetUserByIdQuery(user.Id), cancellationToken);
    }

    user.Activate();
    await context.SaveChangesAsync(cancellationToken);

    return await new GetUserByIdHandler(context)
      .Handle(new GetUserByIdQuery(user.Id), cancellationToken);
  }
}
