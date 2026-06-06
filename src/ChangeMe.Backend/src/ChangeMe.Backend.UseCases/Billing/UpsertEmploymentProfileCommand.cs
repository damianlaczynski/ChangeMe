using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.Domain.Authorization;
using ChangeMe.Backend.UseCases.Billing.Dtos;
using ChangeMe.Backend.UseCases.Billing.Utils;

namespace ChangeMe.Backend.UseCases.Billing;

public record UpsertEmploymentProfileCommand(
  Guid Id,
  string? EmployeeId,
  string? NationalId,
  string? TaxId,
  string? BankAccount,
  string? Notes) : ICommand<EmploymentProfileDto>;

public class UpsertEmploymentProfileHandler(
  ApplicationDbContext context,
  IUserAccessor userAccessor) : ICommandHandler<UpsertEmploymentProfileCommand, EmploymentProfileDto>
{
  public async Task<Result<EmploymentProfileDto>> Handle(
    UpsertEmploymentProfileCommand command,
    CancellationToken cancellationToken)
  {
    var permissionResult = BillingUtils.RequirePermission(userAccessor, PermissionCodes.BillingManageEmployment);
    if (!permissionResult.IsSuccess)
      return permissionResult.Map();

    var userExistsResult = await EmploymentUtils.EnsureUserExistsAsync(context, command.Id, cancellationToken);
    if (!userExistsResult.IsSuccess)
      return userExistsResult.Map();

    var uniqueEmployeeIdResult = await EmploymentUtils.EnsureUniqueEmployeeIdAsync(
      context,
      command.EmployeeId,
      command.Id,
      cancellationToken);
    if (!uniqueEmployeeIdResult.IsSuccess)
      return uniqueEmployeeIdResult.Map();

    var profile = await context.EmploymentProfiles
      .FirstOrDefaultAsync(p => p.UserId == command.Id, cancellationToken);

    if (profile is null)
    {
      var createResult = EmploymentProfile.Create(
        command.Id,
        command.EmployeeId,
        command.NationalId,
        command.TaxId,
        command.BankAccount,
        command.Notes);
      if (!createResult.IsSuccess)
        return createResult.Map();

      await context.EmploymentProfiles.AddAsync(createResult.Value, cancellationToken);
      profile = createResult.Value;
    }
    else
    {
      var updateResult = profile.Update(
        command.EmployeeId,
        command.NationalId,
        command.TaxId,
        command.BankAccount,
        command.Notes);
      if (!updateResult.IsSuccess)
        return updateResult.Map();
    }

    await context.SaveChangesAsync(cancellationToken);

    return Result.Success(EmploymentUtils.MapProfile(profile, canManage: true));
  }
}
