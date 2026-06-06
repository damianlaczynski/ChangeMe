using System.Globalization;
using ChangeMe.Backend.Domain.Aggregates.Billing;
using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;
using ChangeMe.Backend.UseCases.Billing.Dtos;

namespace ChangeMe.Backend.UseCases.Billing.Utils;

internal static class SettlementsUtils
{
  public static bool CanViewSettlements(IUserAccessor userAccessor) =>
    BillingUtils.CanViewBillingReports(userAccessor)
    || BillingUtils.CanManageSettlements(userAccessor);

  public static string FormatPeriodLabel(int year, int month)
  {
    var monthName = CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
    return $"{monthName} {year}";
  }

  public static string FormatBalanceLabel(int balanceMinutes) =>
    balanceMinutes switch
    {
      > 0 => "Overtime",
      < 0 => "Undertime",
      _ => "Balanced",
    };

  public static async Task<string?> GetClosedByDisplayNameAsync(
    ApplicationDbContext context,
    Guid? closedByUserId,
    CancellationToken cancellationToken)
  {
    if (closedByUserId is null)
      return null;

    return await EmploymentUtils.GetUserDisplayNameAsync(context, closedByUserId.Value, cancellationToken);
  }

  public static UserSettlementListItemDto MapListItem(
    UserSettlement settlement,
    string userDisplayName,
    string positionName,
    ContractType? contractType,
    bool canRecalculate) =>
    new()
    {
      Id = settlement.Id,
      UserId = settlement.UserId,
      UserDisplayName = userDisplayName,
      PositionName = positionName,
      ContractType = contractType,
      ExpectedMinutes = settlement.ExpectedMinutes,
      LoggedMinutes = settlement.LoggedMinutes,
      LeaveDays = settlement.LeaveDays,
      BalanceMinutes = settlement.BalanceMinutes,
      LastCalculatedAt = settlement.LastCalculatedAt,
      CanRecalculate = canRecalculate,
    };
}
