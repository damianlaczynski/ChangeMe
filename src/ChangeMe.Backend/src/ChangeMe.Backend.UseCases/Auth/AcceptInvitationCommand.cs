using ChangeMe.Backend.Domain.Aggregates.Auth;
using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record AcceptInvitationCommand(
  string Token,
  string FirstName,
  string LastName,
  string Password) : ICommand<bool>;

public class AcceptInvitationHandler(
  ApplicationDbContext context,
  IPasswordHasher passwordHasher,
  IUserAuthTokenService tokenService,
  IAuthEmailService authEmailService) : ICommandHandler<AcceptInvitationCommand, bool>
{
  public async Task<Result<bool>> Handle(AcceptInvitationCommand command, CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      command.Token,
      UserAuthTokenType.Invitation,
      cancellationToken);

    if (!validateResult.IsSuccess)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidInvitationTokenMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == validateResult.Value, cancellationToken);
    if (user is null)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidInvitationTokenMessage);

    if (user.HasPasswordSet)
      return Result<bool>.Conflict(AuthSessionUtils.InvitationAlreadyAcceptedMessage);

    var profileResult = user.UpdateProfile(command.FirstName, command.LastName);
    if (!profileResult.IsSuccess)
      return Result<bool>.Invalid(profileResult.ValidationErrors);

    var passwordHash = passwordHasher.HashPassword(command.Password);
    var passwordResult = user.SetPasswordHash(passwordHash);
    if (!passwordResult.IsSuccess)
      return Result<bool>.Invalid(passwordResult.ValidationErrors);

    user.MarkEmailVerified();

    await tokenService.MarkTokenUsedAsync(command.Token, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    await authEmailService.SendPasswordResetCompletedAsync(user, cancellationToken);

    return Result.Success(true);
  }
}
