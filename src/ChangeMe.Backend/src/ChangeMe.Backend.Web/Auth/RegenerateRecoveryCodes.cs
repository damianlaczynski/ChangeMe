using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class RegenerateRecoveryCodes(IMediator mediator)
  : BaseEndpoint<RegenerateRecoveryCodesCommand, TwoFactorSetupCompletedDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/two-factor/recovery-codes/regenerate");
    Summary(s => s.Summary = "Regenerate two-factor recovery codes");
  }
}

public sealed class RegenerateRecoveryCodesCommandValidator : Validator<RegenerateRecoveryCodesCommand>
{
  public RegenerateRecoveryCodesCommandValidator()
  {
    RuleFor(x => x.CurrentPassword)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH)
      .When(x => !string.IsNullOrWhiteSpace(x.CurrentPassword));

    RuleFor(x => x.VerificationCode)
      .MaximumLength(64)
      .When(x => !string.IsNullOrWhiteSpace(x.VerificationCode));
  }
}
