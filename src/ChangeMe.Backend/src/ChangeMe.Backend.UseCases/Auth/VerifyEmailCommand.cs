using ChangeMe.Backend.Domain.Aggregates.Users.Entities;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record VerifyEmailCommand(string Token) : ICommand<bool>;

public class VerifyEmailHandler(
  ApplicationDbContext context,
  IUserAuthTokenService tokenService) : ICommandHandler<VerifyEmailCommand, bool>
{
  public async Task<Result<bool>> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
  {
    var validateResult = await tokenService.ValidateTokenAsync(
      command.Token,
      UserAuthTokenType.EmailVerification,
      cancellationToken);

    if (!validateResult.IsSuccess)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidEmailVerificationTokenMessage);

    var user = await context.Users.FirstOrDefaultAsync(x => x.Id == validateResult.Value, cancellationToken);
    if (user is null)
      return Result<bool>.NotFound(AuthSessionUtils.InvalidEmailVerificationTokenMessage);

    user.MarkEmailVerified();
    await tokenService.MarkTokenUsedAsync(command.Token, cancellationToken);
    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(true);
  }
}