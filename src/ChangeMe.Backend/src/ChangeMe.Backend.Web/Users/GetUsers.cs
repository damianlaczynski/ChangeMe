using ChangeMe.Backend.UseCases.Users;
using ChangeMe.Backend.UseCases.Users.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class GetUsers(IMediator mediator) : BaseEndpoint<GetUsersQuery, PaginationResult<UserListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    RequirePermission(PermissionCodes.UsersView);
    Get("/users");
    Summary(s => s.Summary = "Get users");
  }
}
