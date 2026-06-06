using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public class GetPositionsQuery : PaginationQuery<PositionListItemDto>
{
  public string? SearchText { get; set; }
}

public class GetPositionsHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : IQueryHandler<GetPositionsQuery, PaginationResult<PositionListItemDto>>
{
  public async Task<Result<PaginationResult<PositionListItemDto>>> Handle(
    GetPositionsQuery query,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequireAnyPermission(
      userAccessor,
      PermissionCodes.BillingViewAny,
      PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var positionsQuery = context.Positions.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(query.SearchText))
    {
      var searchText = query.SearchText.Trim();
#if PostgreSQL
      positionsQuery = positionsQuery.Where(p =>
        EF.Functions.ILike(p.Name, $"%{searchText}%")
        || EF.Functions.ILike(p.Department, $"%{searchText}%"));
#else
      positionsQuery = positionsQuery.Where(p =>
        EF.Functions.Like(p.Name, $"%{searchText}%")
        || EF.Functions.Like(p.Department, $"%{searchText}%"));
#endif
    }

    var canManage = BillingUtils.CanManageEmployment(userAccessor);

    var projected = positionsQuery.Select(p => new PositionListItemDto
    {
      Id = p.Id,
      Name = p.Name,
      Department = string.IsNullOrEmpty(p.Department) ? null : p.Department,
      IsActive = p.IsActive,
      ContractCount = context.EmploymentContracts.Count(c => c.PositionId == p.Id),
      CanManage = canManage,
    });

    query.PaginationParameters.SortField = query.PaginationParameters.SortField switch
    {
      "Department" => nameof(PositionListItemDto.Department),
      "Contracts" => nameof(PositionListItemDto.ContractCount),
      "Active" => nameof(PositionListItemDto.IsActive),
      _ => nameof(PositionListItemDto.Name),
    };

    var paged = await projected.ToPaginationResultAsync(x => x, query.PaginationParameters, cancellationToken);
    return Result.Success(paged);
  }
}
