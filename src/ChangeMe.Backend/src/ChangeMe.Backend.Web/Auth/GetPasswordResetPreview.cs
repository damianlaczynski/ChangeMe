using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class GetPasswordResetPreview(IMediator mediator)
  : BaseEndpoint<GetPasswordResetPreviewQuery, PasswordResetPreviewDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/auth/password-reset");
    AllowAnonymous();
    Summary(s => s.Summary = "Validate password reset token");
  }
}

public sealed class GetPasswordResetPreviewQueryValidator : Validator<GetPasswordResetPreviewQuery>
{
  public GetPasswordResetPreviewQueryValidator()
  {
    RuleFor(x => x.Token).NotEmpty();
  }
}
