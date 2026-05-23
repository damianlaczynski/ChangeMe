using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.Web.Validation;

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
  public ChangePasswordCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.CurrentPassword)
      .NotEmpty();

    RuleFor(x => x.NewPassword)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}