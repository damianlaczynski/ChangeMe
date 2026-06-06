import {
  BillingLeaveReportGroupingMode,
  BillingLeaveReportResultDto,
  BillingSettlementReportGroupingMode,
  BillingSettlementReportResultDto,
  SettlementOperationType
} from '@features/billing/models/billing-report.model';
import {
  SettlementPeriodListItemDto,
  SettlementPeriodStatus
} from '@features/billing/models/settlement.model';
import {
  formatMonthlyHoursNorm,
  getContractTypeLabel
} from '@features/billing/utils/billing.utils';

export const billingSettlementGroupingModes = [
  { label: 'By person', value: BillingSettlementReportGroupingMode.ByPerson },
  { label: 'By position', value: BillingSettlementReportGroupingMode.ByPosition },
  {
    label: 'By contract type',
    value: BillingSettlementReportGroupingMode.ByContractType
  },
  {
    label: 'Overtime summary',
    value: BillingSettlementReportGroupingMode.OvertimeSummary
  },
  {
    label: 'Undertime summary',
    value: BillingSettlementReportGroupingMode.UndertimeSummary
  }
];

export const billingLeaveGroupingModes = [
  { label: 'By person', value: BillingLeaveReportGroupingMode.ByPerson },
  { label: 'By leave type', value: BillingLeaveReportGroupingMode.ByLeaveType },
  { label: 'Leave calendar', value: BillingLeaveReportGroupingMode.LeaveCalendar }
];

export function pickDefaultSettlementPeriodId(
  periods: SettlementPeriodListItemDto[]
): string | null {
  const closed = periods.find(
    (period) => period.status === SettlementPeriodStatus.Closed
  );
  return closed?.id ?? periods[0]?.id ?? null;
}

export function getSettlementGroupingSlug(
  mode: BillingSettlementReportGroupingMode
): string {
  switch (mode) {
    case BillingSettlementReportGroupingMode.ByPosition:
      return 'by-position';
    case BillingSettlementReportGroupingMode.ByContractType:
      return 'by-contract-type';
    case BillingSettlementReportGroupingMode.OvertimeSummary:
      return 'overtime-summary';
    case BillingSettlementReportGroupingMode.UndertimeSummary:
      return 'undertime-summary';
    case BillingSettlementReportGroupingMode.ByPerson:
    default:
      return 'by-person';
  }
}

export function getLeaveGroupingSlug(mode: BillingLeaveReportGroupingMode): string {
  switch (mode) {
    case BillingLeaveReportGroupingMode.ByLeaveType:
      return 'by-leave-type';
    case BillingLeaveReportGroupingMode.LeaveCalendar:
      return 'leave-calendar';
    case BillingLeaveReportGroupingMode.ByPerson:
    default:
      return 'by-person';
  }
}

export function getSettlementOperationLabel(
  operation: SettlementOperationType
): string {
  switch (operation) {
    case SettlementOperationType.Created:
      return 'Created';
    case SettlementOperationType.Recalculated:
      return 'Recalculated';
    case SettlementOperationType.Closed:
      return 'Closed';
    default:
      return operation;
  }
}

function escapeCsv(value: string): string {
  if (value.includes('"') || value.includes(',') || value.includes('\n')) {
    return `"${value.replace(/"/g, '""')}"`;
  }
  return value;
}

function downloadCsv(fileName: string, headers: string[], rows: string[][]): void {
  const lines = [
    headers.map(escapeCsv).join(','),
    ...rows.map((row) => row.map(escapeCsv).join(','))
  ];
  const blob = new Blob(['\uFEFF' + lines.join('\n')], {
    type: 'text/csv;charset=utf-8;'
  });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = fileName;
  anchor.click();
  URL.revokeObjectURL(url);
}

export function exportSettlementReportCsv(
  report: BillingSettlementReportResultDto
): void {
  const slug = getSettlementGroupingSlug(report.groupingMode);
  const fileName = `billing-report-${report.periodYear}-${report.periodMonth}-${slug}.csv`;

  switch (report.groupingMode) {
    case BillingSettlementReportGroupingMode.ByPosition:
      downloadCsv(
        fileName,
        ['Position', 'User count', 'Total logged time', 'Total balance'],
        report.rows.map((row) => [
          row.positionName ?? row.label,
          String(row.userCount ?? 0),
          formatMonthlyHoursNorm(row.loggedMinutes),
          formatMonthlyHoursNorm(row.balanceMinutes)
        ])
      );
      return;
    case BillingSettlementReportGroupingMode.ByContractType:
      downloadCsv(
        fileName,
        ['Contract type', 'User count', 'Total expected time', 'Total logged time'],
        report.rows.map((row) => [
          row.contractType ? getContractTypeLabel(row.contractType) : row.label,
          String(row.userCount ?? 0),
          formatMonthlyHoursNorm(row.expectedMinutes),
          formatMonthlyHoursNorm(row.loggedMinutes)
        ])
      );
      return;
    case BillingSettlementReportGroupingMode.OvertimeSummary:
    case BillingSettlementReportGroupingMode.UndertimeSummary:
    case BillingSettlementReportGroupingMode.ByPerson:
    default:
      downloadCsv(
        fileName,
        ['User', 'Expected time', 'Logged time', 'Leave days', 'Balance'],
        report.rows.map((row) => [
          row.userDisplayName ?? row.label,
          formatMonthlyHoursNorm(row.expectedMinutes),
          formatMonthlyHoursNorm(row.loggedMinutes),
          String(row.leaveDays),
          formatMonthlyHoursNorm(row.balanceMinutes)
        ])
      );
  }
}

export function exportLeaveReportCsv(report: BillingLeaveReportResultDto): void {
  const slug = getLeaveGroupingSlug(report.groupingMode);
  const fileName = `leave-report-${report.year}-${slug}.csv`;

  switch (report.groupingMode) {
    case BillingLeaveReportGroupingMode.ByLeaveType:
      downloadCsv(
        fileName,
        ['Leave type', 'Total days', 'Request count'],
        report.rows.map((row) => [
          row.leaveTypeName ?? '',
          String(row.totalDays ?? 0),
          String(row.requestCount ?? 0)
        ])
      );
      return;
    case BillingLeaveReportGroupingMode.LeaveCalendar:
      downloadCsv(
        fileName,
        ['User', 'Leave type', 'Dates', 'Days'],
        report.rows.map((row) => [
          row.userDisplayName ?? '',
          row.leaveTypeName ?? '',
          row.datesDisplay ?? '',
          String(row.days ?? 0)
        ])
      );
      return;
    case BillingLeaveReportGroupingMode.ByPerson:
    default:
      downloadCsv(
        fileName,
        ['User', 'Entitled days', 'Used days', 'Remaining days'],
        report.rows.map((row) => [
          row.userDisplayName ?? '',
          String(row.entitledDays ?? 0),
          String(row.usedDays ?? 0),
          String(row.remainingDays ?? 0)
        ])
      );
  }
}
