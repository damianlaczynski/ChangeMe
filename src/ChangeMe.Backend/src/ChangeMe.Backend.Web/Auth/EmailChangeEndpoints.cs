using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class RequestEmailChange(IMediator mediator)
  : BaseEndpoint<RequestEmailChangeCommand, MyAccountDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/email-change");
    Summary(s => s.Summary = "Start self-service email change after step-up");
  }
}

public sealed class RequestEmailChangeCommandValidator : Validator<RequestEmailChangeCommand>
{
  public RequestEmailChangeCommandValidator()
  {
    RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(320);
  }
}

public class CancelEmailChange(IMediator mediator)
  : BaseEndpoint<CancelEmailChangeCommand, MyAccountDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/email-change/cancel");
    Summary(s => s.Summary = "Cancel pending self-service email change after step-up");
  }
}

public class ResendEmailChangeConfirmation(IMediator mediator)
  : BaseEndpointWithoutRequest<ResendEmailChangeConfirmationCommand, MyAccountDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/email-change/resend");
    Summary(s => s.Summary = "Resend email change confirmation link to the new address");
  }
}

public class ConfirmEmailChange(IMediator mediator)
  : BaseEndpoint<ConfirmEmailChangeCommand, ConfirmEmailChangeResponseDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/auth/email-change/confirm");
    AllowAnonymous();
    Summary(s => s.Summary = "Confirm self-service email change using token from link");
  }
}

public sealed class ConfirmEmailChangeCommandValidator : Validator<ConfirmEmailChangeCommand>
{
  public ConfirmEmailChangeCommandValidator()
  {
    RuleFor(x => x.Token).NotEmpty();
  }
}
