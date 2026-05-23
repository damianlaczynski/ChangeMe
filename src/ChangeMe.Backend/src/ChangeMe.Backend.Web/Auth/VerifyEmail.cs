using ChangeMe.Backend.UseCases.Auth;

namespace ChangeMe.Backend.Web.Auth;

public class VerifyEmail(IMediator mediator) : BaseEndpoint<VerifyEmailCommand, bool>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/verify-email");
    AllowAnonymous();
    Summary(s => s.Summary = "Verify email address using token from link");
  }
}

public sealed class VerifyEmailCommandValidator : Validator<VerifyEmailCommand>
{
  public VerifyEmailCommandValidator()
  {
    RuleFor(x => x.Token).NotEmpty();
  }
}
