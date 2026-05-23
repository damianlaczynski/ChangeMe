using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record ConfirmUserEmailCommand(Guid Id) : ICommand<UserDetailsDto>;

public class ConfirmUserEmailHandler(
  ApplicationDbContext context,
  IMediator mediator) : ICommandHandler<ConfirmUserEmailCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    ConfirmUserEmailCommand command,
    CancellationToken cancellationToken)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (user.EmailVerified)
      return Result<UserDetailsDto>.Conflict(UsersUtils.EmailAlreadyVerifiedMessage);

    user.MarkEmailVerified();
    await context.SaveChangesAsync(cancellationToken);

    var detailsResult = await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
    return detailsResult.IsSuccess
      ? Result.Success(detailsResult.Value)
      : detailsResult.Map();
  }
}
