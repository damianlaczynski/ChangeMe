using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class GetUserForm(IMediator mediator) : BaseEndpoint<GetUserFormQuery, UserFormDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersManage);
    Get("/users/{Id}/form");
    Summary(s => s.Summary = "Get user form data");
  }
}
