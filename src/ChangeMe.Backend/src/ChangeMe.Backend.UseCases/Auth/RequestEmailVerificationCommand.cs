using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.Infrastructure.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;
using ChangeMe.Backend.UseCases.Auth.Utils;

namespace ChangeMe.Backend.UseCases.Auth;

public sealed record RequestEmailVerificationCommand(string Email) : ICommand<EmailVerificationAckDto>;

public class RequestEmailVerificationHandler(
  ApplicationDbContext context,
  UserEmailVerificationService emailVerificationService) : ICommandHandler<RequestEmailVerificationCommand, EmailVerificationAckDto>
{
  public async ValueTask<Result<EmailVerificationAckDto>> Handle(
    RequestEmailVerificationCommand command,
    CancellationToken cancellationToken)
  {
    var normalizedEmail = User.NormalizeEmail(command.Email);

    var user = await context.Users.FirstOrDefaultAsync(
      x => x.NormalizedEmail == normalizedEmail && !x.Deactivated && !x.EmailVerified,
      cancellationToken);

    if (user is not null)
    {
      var verificationResult = await emailVerificationService.SendVerificationAsync(user, cancellationToken);
      if (!verificationResult.IsSuccess)
        return verificationResult.Map();

      await context.SaveChangesAsync(cancellationToken);
    }

    return Result.Success(new EmailVerificationAckDto
    {
      Message = AuthSessionUtils.EmailVerificationResendAckMessage
    });
  }
}
