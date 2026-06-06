using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Entities;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record CreateEmploymentContractCommand(
  Guid Id,
  Guid PositionId,
  ContractType ContractType,
  DateOnly StartDate,
  DateOnly? EndDate,
  decimal Fte,
  int MonthlyHoursNormMinutes,
  decimal? HourlyRate,
  decimal? MonthlySalary,
  string? Notes) : ICommand<EmploymentContractDetailsDto>;

public class CreateEmploymentContractHandler(
  IMediator mediator,
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<CreateEmploymentContractCommand, EmploymentContractDetailsDto>
{
  public async Task<Result<EmploymentContractDetailsDto>> Handle(
    CreateEmploymentContractCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var userExistsResult = await EmploymentUtils.EnsureUserExistsAsync(context, command.Id, cancellationToken);
    if (!userExistsResult.IsSuccess)
      return userExistsResult.Map();

    var positionResult = await EmploymentUtils.EnsureActivePositionAsync(
      context,
      command.PositionId,
      cancellationToken);
    if (!positionResult.IsSuccess)
      return positionResult.Map();

    var overlapResult = await EmploymentUtils.EnsureNoContractOverlapAsync(
      context,
      command.Id,
      command.StartDate,
      command.EndDate,
      excludeContractId: null,
      cancellationToken);
    if (!overlapResult.IsSuccess)
      return overlapResult.Map();

    var contractResult = EmploymentContract.Create(
      command.Id,
      command.PositionId,
      command.ContractType,
      command.StartDate,
      command.EndDate,
      command.Fte,
      command.MonthlyHoursNormMinutes,
      command.HourlyRate,
      command.MonthlySalary,
      command.Notes);
    if (!contractResult.IsSuccess)
      return contractResult.Map();

    var hadContracts = await context.EmploymentContracts
      .AnyAsync(c => c.UserId == command.Id, cancellationToken);

    await context.EmploymentContracts.AddAsync(contractResult.Value, cancellationToken);

    if (!hadContracts)
    {
      var hasPattern = await context.WeeklyRecurringPatterns
        .AnyAsync(p => p.UserId == command.Id, cancellationToken);
      if (!hasPattern)
      {
        var settings = await context.BillingSettings
          .FirstOrDefaultAsync(s => s.Id == BillingSettings.SingletonId, cancellationToken);
        if (settings is not null)
        {
          var pattern = WeeklyRecurringPattern.CreateDefault(
            command.Id,
            settings,
            contractResult.Value.Fte);
          await context.WeeklyRecurringPatterns.AddAsync(pattern, cancellationToken);
        }
      }
    }

    await context.SaveChangesAsync(cancellationToken);

    var detailsResult = await mediator.Send(
      new GetEmploymentContractByIdQuery(command.Id, contractResult.Value.Id),
      cancellationToken);
    if (!detailsResult.IsSuccess)
      return detailsResult.Map();

    return Result.Created(
      detailsResult.Value,
      $"/users/{command.Id}/employment/contracts/{contractResult.Value.Id}");
  }
}
