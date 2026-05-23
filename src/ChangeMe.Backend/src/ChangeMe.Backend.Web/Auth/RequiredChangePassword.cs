using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.Web.Validation;

namespace ChangeMe.Backend.Web.Auth;

public class RequiredChangePassword(IMediator mediator)
  : BaseEndpoint<RequiredChangePasswordCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/required-change-password");
    Summary(s =>
    {
      s.Summary = "Required password change";
      s.Description =
        "Set a new password when the current password has expired. Revokes other sessions and keeps the current one.";
    });
  }
}

public sealed class RequiredChangePasswordCommandValidator : Validator<RequiredChangePasswordCommand>
{
  public RequiredChangePasswordCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.NewPassword)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}
