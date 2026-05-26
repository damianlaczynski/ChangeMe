using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class LinkExternalAccount(IMediator mediator)
  : BaseEndpoint<LinkExternalAccountCommand, ExternalSignInResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/external/link");
    AllowAnonymous();
    Summary(s => s.Summary = "Link external provider to an existing account");
  }
}

public sealed class LinkExternalAccountCommandValidator : Validator<LinkExternalAccountCommand>
{
  public LinkExternalAccountCommandValidator()
  {
    RuleFor(x => x.State).NotEmpty().MaximumLength(128);
    RuleFor(x => x.Password)
      .NotEmpty()
      .MinimumLength(UserConstraints.PASSWORD_MIN_LENGTH)
      .MaximumLength(UserConstraints.PASSWORD_MAX_LENGTH);
  }
}
