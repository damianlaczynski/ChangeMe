using ChangeMe.Backend.Domain.Aggregates.Users.Enums;
using ChangeMe.Backend.Domain.Aggregates.Users.Interfaces;
using ChangeMe.Backend.UseCases.Users.Dtos;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Users;

public sealed record CancelUserPendingEmailChangeCommand(Guid Id) : ICommand<UserDetailsDto>;

public class CancelUserPendingEmailChangeHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IAuthEmailService authEmailService,
  IUserAuthTokenService tokenService) : ICommandHandler<CancelUserPendingEmailChangeCommand, UserDetailsDto>
{
  public async Task<Result<UserDetailsDto>> Handle(
    CancelUserPendingEmailChangeCommand command,
    CancellationToken cancellationToken)
  {
    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
    if (user is null)
      return Result<UserDetailsDto>.NotFound();

    if (!user.HasPendingEmailChange)
      return Result<UserDetailsDto>.Error("No email change is pending.");

    user.CancelPendingEmailChange();
    await tokenService.InvalidateUnusedTokensAsync(
      user.Id,
      UserAuthTokenType.EmailChangeConfirmation,
      cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    var emailResult = await authEmailService.SendEmailChangeCancelledAsync(user, cancellationToken);
    if (!emailResult.IsSuccess)
      return emailResult.Map();

    return await mediator.Send(new GetUserByIdQuery(user.Id), cancellationToken);
  }
}
