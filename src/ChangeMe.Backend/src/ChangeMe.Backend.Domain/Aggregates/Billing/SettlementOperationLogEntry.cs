using ChangeMe.Backend.Domain.Aggregates.Billing.Enums;

namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class SettlementOperationLogEntry : Entity
{
  private SettlementOperationLogEntry() { }

  public Guid SettlementPeriodId { get; private set; }
  public int PeriodYear { get; private set; }
  public int PeriodMonth { get; private set; }
  public SettlementOperationType Operation { get; private set; }
  public Guid ActorUserId { get; private set; }
  public Guid? TargetUserId { get; private set; }
  public DateTime Timestamp { get; private set; }

  public static SettlementOperationLogEntry Create(
    Guid settlementPeriodId,
    int periodYear,
    int periodMonth,
    SettlementOperationType operation,
    Guid actorUserId,
    DateTime timestamp,
    Guid? targetUserId = null) =>
    new()
    {
      SettlementPeriodId = settlementPeriodId,
      PeriodYear = periodYear,
      PeriodMonth = periodMonth,
      Operation = operation,
      ActorUserId = actorUserId,
      TargetUserId = targetUserId,
      Timestamp = timestamp,
    };
}
