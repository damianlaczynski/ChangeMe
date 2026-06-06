---
id: REQ-BIL-008
title: Billing Reports and Export
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-004, REQ-BIL-005, REQ-BIL-007]
---

## Goal

An authorized administrator must be able to analyze employment, leave, and settlement data with flexible filters, grouping, and CSV export.

## Features

### Access

- Screen: **Billing reports**
- Requires **Billing.ViewReports**.
- Page title: **`Billing reports`**.

### Screen tabs

- Four tabs: **`Reports`**, **`Leave`**, **`Audit log`**, **`Settings`**.
- Default tab on open: **`Reports`**.
- **`Audit log`** shows settlement operation history from REQ-BIL-007.
- **`Settings`** shows **Billing settings** and **Leave types** from REQ-BIL-004.

### Reports tab — filters

| Filter                | Behavior                                                                                                                            |
| --------------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **Settlement period** | Required single select; defaults to the most recent **`Closed`** period, or the most recent **`Open`** period when none are closed. |
| **Users**             | Multi-select; empty means all users with a settlement row in the period.                                                            |
| **Contract type**     | Multi-select: **`Employment`**, **`Mandate`**, **`Work contract`**, **`B2B`**; empty means all.                                     |

- **Run report** applies filters and refreshes results.

### Reports tab — grouping modes

| Mode                  | Label (exact)     | Result                                                                                               |
| --------------------- | ----------------- | ---------------------------------------------------------------------------------------------------- |
| **By person**         | By person         | One row per user: **User**, **Expected time**, **Logged time**, **Leave days**, **Balance**.         |
| **By position**       | By position       | One row per position: **Position**, **User count**, **Total logged time**, **Total balance**.        |
| **By contract type**  | By contract type  | One row per type: **Contract type**, **User count**, **Total expected time**, **Total logged time**. |
| **Overtime summary**  | Overtime summary  | Users with **Balance** > **0** only; sorted by balance descending.                                   |
| **Undertime summary** | Undertime summary | Users with **Balance** < **0** only; sorted by balance ascending.                                    |

- Default grouping: **By person**.
- KPI card above the table when a report has run: **Users**, **Total expected time**, **Total logged time**, **Net balance** (sum of row balances).

### Reports tab — presentation

- Empty state before first run: **`Select filters and run a report.`**
- Empty result: **`No data matches the selected filters.`**
- Duration columns use REQ-TIM-007 format.
- **Balance** column uses the same styling rules as REQ-BIL-007.

### CSV export

- Button **Export CSV** on **Reports** tab after a successful run.
- Filename: **`billing-report-{period-year}-{period-month}-{grouping}.csv`** using the selected grouping mode slug.
- UTF-8 with header row matching visible table columns.
- Success message: **`Report exported.`**

### Leave tab

- Analyzes leave independent of settlement periods.

| Filter         | Behavior                                               |
| -------------- | ------------------------------------------------------ |
| **Year**       | Required; defaults to current calendar year.           |
| **Leave type** | Multi-select; empty means all.                         |
| **Users**      | Multi-select; empty means all.                         |
| **Status**     | Defaults to **`Approved`** only; multi-select allowed. |

### Leave tab — grouping modes

| Mode               | Label (exact)  | Result                                                                           |
| ------------------ | -------------- | -------------------------------------------------------------------------------- |
| **By person**      | By person      | **User**, **Entitled days**, **Used days**, **Remaining days**.                  |
| **By leave type**  | By leave type  | **Leave type**, **Total days**, **Request count**.                               |
| **Leave calendar** | Leave calendar | One row per **approved** request: **User**, **Leave type**, **Dates**, **Days**. |

- **Export CSV** uses filename **`leave-report-{year}-{grouping}.csv`**.

### Audit log tab

- Table columns: **Timestamp**, **Actor**, **Operation**, **Period**, **User** (when applicable).
- Default sort: **Timestamp** descending.
- Filter: **Settlement period** (optional; empty means all).
- Server-paginated; **20** rows per page.
- Empty state: **`No settlement operations recorded.`**

### Settings tab

- Two sections: **`Billing settings`** (leave allowance and **Default work hours** per REQ-BIL-004) and **`Leave types`** administration.
- Visible to users with **Billing.ViewReports**; editable only with **Billing.ManageSettlements**.

### Permissions and visibility

- **Billing.ViewReports**: required for all tabs; read-only **Settings** without **Billing.ManageSettlements**.
- **Billing.ManageSettlements**: edit actions on **Settings** tab.

---
