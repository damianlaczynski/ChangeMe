using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class SendInvitation(IMediator mediator) : BaseEndpoint<SendInvitationCommand, UserDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Post("/users/{Id}/send-invitation");
    Summary(s => s.Summary = "Send invitation");
  }
}

public sealed class SendInvitationCommandValidator : Validator<SendInvitationCommand>
{
  public SendInvitationCommandValidator()
  {
    RuleFor(x => x.Id).NotEmpty();
  }
}
