using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetPositions(IMediator mediator)
  : BaseEndpoint<GetPositionsQuery, PaginationResult<PositionListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/positions");
    Summary(s => s.Summary = "Get positions");
  }
}
