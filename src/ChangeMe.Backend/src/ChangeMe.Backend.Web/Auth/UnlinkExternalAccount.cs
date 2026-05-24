using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.Web.Auth;

public class UnlinkExternalAccount(IMediator mediator)
  : BaseEndpoint<UnlinkExternalAccountCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/external/{ProviderKey}/unlink");
    Summary(s => s.Summary = "Unlink an external provider from the signed-in account");
  }
}

public sealed class UnlinkExternalAccountCommandValidator : Validator<UnlinkExternalAccountCommand>
{
  public UnlinkExternalAccountCommandValidator()
  {
    RuleFor(x => x.ProviderKey)
      .NotEmpty()
      .MaximumLength(TwoFactorConstraints.PROVIDER_KEY_MAX_LENGTH);
  }
}
