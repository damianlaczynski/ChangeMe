namespace ChangeMe.Backend.Domain.Aggregates.Billing;

public class UserSettlement : Entity, IAggregateRoot
{
  private UserSettlement() { }

  public Guid SettlementPeriodId { get; private set; }
  public Guid UserId { get; private set; }
  public Guid? ContractId { get; private set; }
  public int ExpectedMinutes { get; private set; }
  public int LoggedMinutes { get; private set; }
  public decimal LeaveDays { get; private set; }
  public int BalanceMinutes { get; private set; }
  public DateTime LastCalculatedAt { get; private set; }

  public static UserSettlement Create(
    Guid settlementPeriodId,
    Guid userId,
    Guid? contractId,
    int expectedMinutes,
    int loggedMinutes,
    decimal leaveDays,
    DateTime calculatedAt) =>
    new()
    {
      SettlementPeriodId = settlementPeriodId,
      UserId = userId,
      ContractId = contractId,
      ExpectedMinutes = expectedMinutes,
      LoggedMinutes = loggedMinutes,
      LeaveDays = leaveDays,
      BalanceMinutes = loggedMinutes - expectedMinutes,
      LastCalculatedAt = calculatedAt,
    };

  public void UpdateCalculation(
    Guid? contractId,
    int expectedMinutes,
    int loggedMinutes,
    decimal leaveDays,
    DateTime calculatedAt)
  {
    ContractId = contractId;
    ExpectedMinutes = expectedMinutes;
    LoggedMinutes = loggedMinutes;
    LeaveDays = leaveDays;
    BalanceMinutes = loggedMinutes - expectedMinutes;
    LastCalculatedAt = calculatedAt;
  }
}
