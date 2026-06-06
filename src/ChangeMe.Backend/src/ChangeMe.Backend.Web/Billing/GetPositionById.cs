using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetPositionById(IMediator mediator)
  : BaseEndpoint<GetPositionByIdQuery, PositionDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/positions/{id}");
    Summary(s => s.Summary = "Get position by id");
  }
}
