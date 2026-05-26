using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class CancelInvitation(IMediator mediator) : BaseEndpoint<CancelInvitationCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/cancel-invitation");
    Summary(s => s.Summary = "Cancel invitation");
  }
}

public sealed class CancelInvitationCommandValidator : Validator<CancelInvitationCommand>
{
  public CancelInvitationCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
