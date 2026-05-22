using ChangeMe.Backend.Domain.Aggregates.Users;
using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.Web.Auth;

public class ChangePassword(IMediator mediator) : BaseEndpoint<ChangePasswordCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/change-password");
    Summary(s =>
    {
      s.Summary = "Change password";
      s.Description = "Change the signed-in user's password and revoke all sessions.";
    });
  }
}

public sealed class ChangePasswordCommandValidator : Validator<ChangePasswordCommand>
{
  public ChangePasswordCommandValidator()
  {
    RuleFor(x => x.CurrentPassword)
      .NotEmpty();

    RuleFor(x => x.NewPassword)
      .NotEmpty()
      .MinimumLength(UserConstraints.PASSWORD_MIN_LENGTH)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH);
  }
}
