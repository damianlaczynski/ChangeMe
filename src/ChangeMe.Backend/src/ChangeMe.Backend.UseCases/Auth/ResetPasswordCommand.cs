using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.UseCases.Auth.Utils;
using ChangeMe.Backend.UseCases.Users.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record ResetPasswordCommand(string Token, string NewPassword) : ICommand<bool>;

public class ResetPasswordHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService) : ICommandHandler<ResetPasswordCommand, bool>
{
  public async Task<Result<bool>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      command.Token,
      UserAuthTokenType.PasswordReset,
      cancellationToken);

    if (!validateResult.IsSuccess)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidPasswordResetTokenMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == validateResult.Value, cancellationToken);
    if (user is null || !user.HasPasswordSet)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidPasswordResetTokenMessage);

    var passwordHash = passwordHasher.HashPassword(command.NewPassword);
    var updateResult = user.SetPasswordHash(passwordHash);
    if (!updateResult.IsSuccess)
      return Result<bool>.Invalid(updateResult.ValidationErrors);

    await UsersUtils.RevokeAllActiveSessionsAsync(context, user.Id, cancellationToken);
    await tokenService.MarkTokenUsedAsync(command.Token, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await authEmailService.SendPasswordResetCompletedAsync(user, cancellationToken);

    return Result.Success(true);
  }
}
