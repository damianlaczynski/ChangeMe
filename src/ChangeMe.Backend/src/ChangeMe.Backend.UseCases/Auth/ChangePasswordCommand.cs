namespace ChangeMe.Backend.UseCases.Auth;

public sealed record ChangePasswordCommand(
  string CurrentPassword,
  string NewPassword) : ICommand<bool>;

public class ChangePasswordHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IUserAccessor userAccessor,
  IAuthEmailService authEmailService) : ICommandHandler<ChangePasswordCommand, bool>
{
  public async ValueTask<Result<bool>> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId)
      return Result<bool>.Unauthorized();

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive)
      return Result<bool>.Unauthorized();

    if (!passwordHasher.VerifyPassword(user.PasswordHash, command.CurrentPassword))
    {
      return Result.Invalid(new ValidationError(
        nameof(command.CurrentPassword),
        "Current password is incorrect."));
    }

    if (passwordHasher.VerifyPassword(user.PasswordHash, command.NewPassword))
    {
      return Result.Invalid(new ValidationError(
        nameof(command.NewPassword),
        "New password must differ from the current password."));
    }

    var newPasswordHash = passwordHasher.HashPassword(command.NewPassword);
    var updateResult = user.SetPasswordHash(newPasswordHash);
    if (!updateResult.IsSuccess)
      return Result<bool>.Invalid(updateResult.ValidationErrors);

    await LogoutAllSessionsHandler.RevokeAllActiveSessionsAsync(context, userId, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await authEmailService.SendPasswordChangedAsync(user, cancellationToken);

    return Result.Success(true);
  }
}
