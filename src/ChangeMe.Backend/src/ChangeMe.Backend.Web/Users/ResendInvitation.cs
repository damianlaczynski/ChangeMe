using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class ResendInvitation(IMediator mediator) : BaseEndpoint<ResendInvitationCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/resend-invitation");
    Summary(s => s.Summary = "Resend invitation");
  }
}

public sealed class ResendInvitationCommandValidator : Validator<ResendInvitationCommand>
{
  public ResendInvitationCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
