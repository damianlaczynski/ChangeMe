namespace ChangeMe.Backend.UseCases.Billing.Enums;

public enum BillingSettlementReportGroupingMode
{
  ByPerson = 0,
  ByPosition = 1,
  ByContractType = 2,
  OvertimeSummary = 3,
  UndertimeSummary = 4,
}

public enum BillingLeaveReportGroupingMode
{
  ByPerson = 0,
  ByLeaveType = 1,
  LeaveCalendar = 2,
}
