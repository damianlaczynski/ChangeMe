using ChangeMe.Backend.UseCases.Auth;
using ChangeMe.Backend.UseCases.Auth.Dtos;

namespace ChangeMe.Backend.Web.Auth;

public class GetInvitationPreview(IMediator mediator)
  : BaseEndpoint<GetInvitationPreviewQuery, InvitationPreviewDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/auth/invitation");
    AllowAnonymous();
    Summary(s => s.Summary = "Get invitation preview");
  }
}

public sealed class GetInvitationPreviewQueryValidator : Validator<GetInvitationPreviewQuery>
{
  public GetInvitationPreviewQueryValidator()
  {
    RuleFor(x => x.Token).NotEmpty();
  }
}
