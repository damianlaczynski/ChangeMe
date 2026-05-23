using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class RequestEmailVerification(IMediator mediator)
  : BaseEndpoint<RequestEmailVerificationCommand, EmailVerificationAckDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/request-email-verification");
    AllowAnonymous();
    Summary(s => s.Summary = "Request email verification link");
  }
}

public sealed class RequestEmailVerificationCommandValidator : Validator<RequestEmailVerificationCommand>
{
  public RequestEmailVerificationCommandValidator()
  {
    RuleFor(x => x.Email)
      .NotEmpty()
      .EmailAddress()
      .MaximumLength(UserConstraints.EMAIL_MAX_LENGTH);
  }
}
