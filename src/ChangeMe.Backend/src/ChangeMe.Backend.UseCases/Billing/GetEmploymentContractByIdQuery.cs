using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record GetEmploymentContractByIdQuery(Guid Id, Guid ContractId) : IQuery<EmploymentContractDetailsDto>;

public class GetEmploymentContractByIdHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor,
  TimeProvider timeProvider) : IQueryHandler<GetEmploymentContractByIdQuery, EmploymentContractDetailsDto>
{
  public async Task<Result<EmploymentContractDetailsDto>> Handle(
    GetEmploymentContractByIdQuery query,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequireAnyPermission(
      userAccessor,
      PermissionCodes.BillingViewAny,
      PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var contract = await context.EmploymentContracts
      .AsNoTracking()
      .FirstOrDefaultAsync(c => c.Id == query.ContractId && c.UserId == query.Id, cancellationToken);
    if (contract is null)
      return Result.NotFound();

    var position = await context.Positions
      .AsNoTracking()
      .FirstOrDefaultAsync(p => p.Id == contract.PositionId, cancellationToken);
    if (position is null)
      return Result.NotFound();

    var today = DateOnly.FromDateTime(timeProvider.GetUtcNow().UtcDateTime);
    var userDisplayName = await EmploymentUtils.GetUserDisplayNameAsync(context, query.Id, cancellationToken);
    var canManage = BillingUtils.CanManageEmployment(userAccessor);

    return Result.Success(new EmploymentContractDetailsDto
    {
      Id = contract.Id,
      UserId = contract.UserId,
      UserDisplayName = userDisplayName,
      PositionId = contract.PositionId,
      PositionName = position.Name,
      ContractType = contract.ContractType,
      StartDate = contract.StartDate,
      EndDate = contract.EndDate,
      Fte = contract.Fte,
      MonthlyHoursNormMinutes = contract.MonthlyHoursNormMinutes,
      HourlyRate = contract.HourlyRate,
      MonthlySalary = contract.MonthlySalary,
      Notes = string.IsNullOrWhiteSpace(contract.Notes) ? null : contract.Notes,
      Status = EmploymentUtils.GetContractStatus(contract.StartDate, contract.EndDate, today),
      CanManage = canManage,
    });
  }
}
