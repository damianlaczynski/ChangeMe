using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.Web.Billing;

public class GetSettlementPeriods(IMediator mediator)
  : BaseEndpointWithoutRequest<GetSettlementPeriodsQuery, IReadOnlyList<SettlementPeriodListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/settlement-periods");
    RequirePermissions(
      PermissionCodes.BillingViewReports,
      PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "List settlement periods");
  }
}

public class CreateSettlementPeriod(IMediator mediator)
  : BaseEndpoint<CreateSettlementPeriodCommand, SettlementPeriodDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/settlement-periods");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Create settlement period");
  }
}

public class GetSettlementPeriodById(IMediator mediator)
  : BaseEndpoint<GetSettlementPeriodByIdQuery, SettlementPeriodDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/settlement-periods/{Id}");
    RequirePermissions(
      PermissionCodes.BillingViewReports,
      PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Get settlement period details");
  }
}

public class RecalculateAllSettlements(IMediator mediator)
  : BaseEndpoint<RecalculateAllSettlementsCommand, SettlementPeriodDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/settlement-periods/{SettlementPeriodId}/recalculate-all");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Recalculate all user settlements in a period");
  }
}

public class CloseSettlementPeriod(IMediator mediator)
  : BaseEndpoint<CloseSettlementPeriodCommand, SettlementPeriodDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/settlement-periods/{SettlementPeriodId}/close");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Close settlement period");
  }
}

public class GetUserSettlementById(IMediator mediator)
  : BaseEndpoint<GetUserSettlementByIdQuery, UserSettlementDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/user-settlements/{Id}");
    RequirePermissions(
      PermissionCodes.BillingViewReports,
      PermissionCodes.BillingManageSettlements,
      PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "Get user settlement details");
  }
}

public class RecalculateUserSettlement(IMediator mediator)
  : BaseEndpoint<RecalculateUserSettlementCommand, UserSettlementDetailsDto>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Post("/billing/user-settlements/{UserSettlementId}/recalculate");
    RequirePermission(PermissionCodes.BillingManageSettlements);
    Summary(s => s.Summary = "Recalculate a single user settlement");
  }
}

public class GetMySettlements(IMediator mediator)
  : BaseEndpointWithoutRequest<GetMySettlementsQuery, IReadOnlyList<MySettlementListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/my/settlements");
    RequirePermission(PermissionCodes.BillingViewOwn);
    Summary(s => s.Summary = "List published settlements for the signed-in user");
  }
}

public class GetSettlementOperationHistory(IMediator mediator)
  : BaseEndpoint<GetSettlementOperationHistoryQuery, PaginationResult<SettlementOperationLogListItemDto>>(mediator)
{
  protected override void ConfigureEndpoint()
  {
    Get("/billing/settlement-operation-history");
    RequirePermission(PermissionCodes.BillingViewReports);
    Summary(s => s.Summary = "Get settlement operation history");
  }
}
