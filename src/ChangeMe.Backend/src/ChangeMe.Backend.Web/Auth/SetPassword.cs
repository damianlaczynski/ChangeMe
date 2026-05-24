using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.Web.Validation;

namespace ChangeMe.Backend.Web.Auth;

public class SetPassword(IMediator mediator) : BaseEndpoint<SetPasswordCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/set-password");
    Summary(s => s.Summary = "Set a local password for an external-only account");
  }
}

public sealed class SetPasswordCommandValidator : Validator<SetPasswordCommand>
{
  public SetPasswordCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.NewPassword)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}
