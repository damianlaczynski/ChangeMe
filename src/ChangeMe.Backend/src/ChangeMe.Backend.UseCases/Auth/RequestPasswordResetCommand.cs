using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RequestPasswordResetCommand(string Email) : ICommand<PasswordResetAckDto>;

public class RequestPasswordResetHandler(
  ApplicationDbContext context,
  UserPasswordResetService passwordResetService) : ICommandHandler<RequestPasswordResetCommand, PasswordResetAckDto>
{
  public async Task<Result<PasswordResetAckDto>> Handle(
    RequestPasswordResetCommand command,
    CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);

    var user = await context.Users
      .FirstOrDefaultAsync(
        x => x.NormalizedEmail == normalizedEmail && !x.Deactivated && x.HasPasswordSet,
        cancellationToken);

    if (user is not null)
      await passwordResetService.SendPasswordResetAsync(user, cancellationToken);

    return Result.Success(new PasswordResetAckDto
    {
      Message = AuthSessionUtils.ForgotPasswordSuccessMessage
    });
  }
}
