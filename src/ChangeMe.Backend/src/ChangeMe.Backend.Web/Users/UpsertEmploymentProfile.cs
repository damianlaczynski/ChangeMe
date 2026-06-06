using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Users;

public class UpsertEmploymentProfile(IMediator mediator)
  : BaseEndpoint<UpsertEmploymentProfileCommand, EmploymentProfileDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Put("/users/{Id}/employment/profile");
    RequirePermission(PermissionCodes.BillingManageEmployment);
    Summary(s => s.Summary = "Create or update user employment profile");
  }
}
