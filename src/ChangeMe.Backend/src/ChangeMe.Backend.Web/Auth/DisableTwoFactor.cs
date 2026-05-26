using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.Web.Auth;

public class DisableTwoFactor(IMediator mediator) : BaseEndpoint<DisableTwoFactorCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/two-factor/disable");
    Summary(s => s.Summary = "Disable two-factor authentication");
  }
}

public sealed class DisableTwoFactorCommandValidator : Validator<DisableTwoFactorCommand>
{
  public DisableTwoFactorCommandValidator()
  {
    RuleFor(x => x.CurrentPassword)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.CurrentPassword));

    RuleFor(x => x.VerificationCode)
      .MaximumLength(64)
      .When(x => !string.IsNullOrWhiteSpace(x.VerificationCode));
  }
}
