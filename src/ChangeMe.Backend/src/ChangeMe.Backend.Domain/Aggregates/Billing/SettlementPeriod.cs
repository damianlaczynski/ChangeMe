using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class SettlementPeriod : Entity, IAggregateRoot
{
  private SettlementPeriod() { }

  public int Year { get; private set; }
  public int Month { get; private set; }
  public SettlementPeriodStatus Status { get; private set; } = SettlementPeriodStatus.Open;
  public DateTime? ClosedAt { get; private set; }
  public Guid? ClosedByUserId { get; private set; }

  public static Result<SettlementPeriod> Create(int year, int month)
  {
    if (month is < 1 or > 12)
      return Result.Invalid(new ValidationError(nameof(Month), "Month must be between 1 and 12."));

    return Result.Success(new SettlementPeriod
    {
      Year = year,
      Month = month,
      Status = SettlementPeriodStatus.Open,
    });
  }

  public Result Close(Guid closedByUserId, DateTime closedAt)
  {
    if (Status == SettlementPeriodStatus.Closed)
      return Result.Conflict(BillingConstraints.SettlementPeriodClosedMessage);

    Status = SettlementPeriodStatus.Closed;
    ClosedAt = closedAt;
    ClosedByUserId = closedByUserId;
    return Result.Success();
  }
}
