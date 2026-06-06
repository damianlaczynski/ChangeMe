using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpdateEmploymentContractCommand(
  Guid Id,
  Guid ContractId,
  Guid UserId,
  Guid PositionId,
  ContractType ContractType,
  DateOnly StartDate,
  DateOnly? EndDate,
  decimal Fte,
  int MonthlyHoursNormMinutes,
  decimal? HourlyRate,
  decimal? MonthlySalary,
  string? Notes) : ICommand<EmploymentContractDetailsDto>;

public class UpdateEmploymentContractHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpdateEmploymentContractCommand, EmploymentContractDetailsDto>
{
  public async Task<Result<EmploymentContractDetailsDto>> Handle(
    UpdateEmploymentContractCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var contract = await context.EmploymentContracts
      .FirstOrDefaultAsync(c => c.Id == command.ContractId && c.UserId == command.Id, cancellationToken);
    if (contract is null)
      return Result.NotFound();

    if (command.UserId == Guid.Empty)
      return Result.Invalid(new ValidationError(nameof(command.UserId), "User is required."));

    if (command.UserId != contract.UserId)
    {
      var userExistsResult = await EmploymentUtils.EnsureUserExistsAsync(
        context,
        command.UserId,
        cancellationToken);
      if (!userExistsResult.IsSuccess)
        return userExistsResult.Map();
    }

    if (command.PositionId != contract.PositionId)
    {
      var positionResult = await EmploymentUtils.EnsureActivePositionAsync(
        context,
        command.PositionId,
        cancellationToken);
      if (!positionResult.IsSuccess)
        return positionResult.Map();
    }
    else
    {
      var positionExists = await context.Positions
        .AsNoTracking()
        .AnyAsync(p => p.Id == command.PositionId, cancellationToken);
      if (!positionExists)
        return Result.NotFound();
    }

    var overlapResult = await EmploymentUtils.EnsureNoContractOverlapAsync(
      context,
      command.UserId,
      command.StartDate,
      command.EndDate,
      command.ContractId,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var updateResult = contract.Update(
      command.UserId,
      command.PositionId,
      command.ContractType,
      command.StartDate,
      command.EndDate,
      command.Fte,
      command.MonthlyHoursNormMinutes,
      command.HourlyRate,
      command.MonthlySalary,
      command.Notes);
    if (!updateResult.IsSuccess)
      return updateResult.Map();

    await context.SaveChangesAsync(cancellationToken);

    return await mediator.Send(
      new GetEmploymentContractByIdQuery(command.UserId, command.ContractId),
      cancellationToken);
  }
}
