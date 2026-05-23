using ChangeMe.Backend.Domain.Interfaces;
using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.Web.Validation;

namespace ChangeMe.Backend.Web.Auth;

public class ResetPassword(IMediator mediator) : BaseEndpoint<ResetPasswordCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/reset-password");
    AllowAnonymous();
    Summary(s => s.Summary = "Reset password");
  }
}

public sealed class ResetPasswordCommandValidator : Validator<ResetPasswordCommand>
{
  public ResetPasswordCommandValidator(IPasswordPolicyValidator passwordPolicyValidator)
  {
    RuleFor(x => x.Token).NotEmpty();

    RuleFor(x => x.NewPassword)
      .NotEmpty()
      .MustSatisfyPasswordPolicy(passwordPolicyValidator);
  }
}
