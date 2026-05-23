
namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RequiredChangePasswordCommand(string NewPassword) : ICommand<bool>;

public class RequiredChangePasswordHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IUserAccessor userAccessor,
  IAuthEmailService authEmailService) : ICommandHandler<RequiredChangePasswordCommand, bool>
{
  public async Task<Result<bool>> Handle(RequiredChangePasswordCommand command, CancellationToken cancellationToken)
  {
    if (userAccessor.UserId is not Guid userId || userAccessor.SessionId is not Guid sessionId)
      return Result<bool>.Unauthorized();

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);
    if (user is null || !user.IsActive || !user.HasPasswordSet)
      return Result<bool>.Unauthorized();

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

    await LogoutAllSessionsHandler.RevokeAllActiveSessionsExceptAsync(
      context,
      userId,
      sessionId,
      cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    await authEmailService.SendPasswordChangedAsync(user, cancellationToken);

    return Result.Success(true);
  }
}