---
id: REQ-BIL-007
title: Settlement Periods and Calculation
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-003, REQ-BIL-004, REQ-TIM-001]
---

## Goal

An authorized administrator must be able to manage monthly **settlement periods**, recalculate per-user settlements from contracts, logged time, and approved leave, and close periods when review is complete.

## Features

### Settlements screen

- Screen: **Settlements**
- Requires **Billing.ManageSettlements** or **Billing.ViewReports** (read-only without recalculate or close actions).
- Page title: **`Settlements`**.

### Period selector

- Dropdown lists all settlement periods sorted by **Year** and **Month** descending.
- Label format: **`{Month name} {Year}`** with status badge **`Open`** or **`Closed`**.
- Header action **Create period** (requires **Billing.ManageSettlements**).

### Create settlement period

- Dialog fields: **Year** (required integer, current year ± **2**), **Month** (required **1–12**).
- Duplicate period: **`A settlement period for this month already exists.`**
- On success: create period with status **`Open`**, run initial calculation for all users who had an active contract or logged time in that month, show message **`Settlement period created.`**, select the new period.

### Period summary

When a period is selected, show:

| Field                  | Behavior                                               |
| ---------------------- | ------------------------------------------------------ |
| **Status**             | **`Open`** or **`Closed`**                             |
| **Closed at**          | Shown when **Closed**; omitted when **Open**.          |
| **Closed by**          | Shown when **Closed**; omitted when **Open**.          |
| **Last calculated at** | Most recent recalculation across all user settlements. |

- Header actions when **`Open`** and user has **Billing.ManageSettlements**:
  - **Recalculate all** — recomputes every **user settlement** in the period; message **`Settlements recalculated.`**
  - **Close period** — see below.
- When **`Closed`**, **Recalculate all** and **Close period** are not shown.

### User settlements table

- Section title: **`User settlements`**
- Columns: **User**, **Position**, **Contract type**, **Expected time**, **Logged time**, **Leave days**, **Balance**, **Last calculated at**.
- Time columns use duration format from REQ-TIM-007 (**`{h}h {m}m`**).
- **Balance** uses warning styling when negative, success styling when positive, neutral when zero.
- Row click opens **User settlement details** (read-only breakdown).
- Empty state: **`No settlements for this period.`**
- Per-row **Recalculate** ( **`Open`** periods only): message **`Settlement recalculated.`**

### User settlement details

Read-only breakdown for one user in one period:

| Block             | Content                                                                                                           |
| ----------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Contract**      | Active contract summary or **`No active contract`**.                                                              |
| **Expected time** | **Monthly hours norm**, proration note when the contract did not cover the full month, minus approved paid leave. |
| **Logged time**   | Total **Duration** from time entries in the period.                                                               |
| **Leave**         | List of **approved** leave requests in the period with **Days** per row.                                          |
| **Balance**       | **Logged minutes** − **Expected minutes** with undertime/overtime label.                                          |

- **Back** returns to **Settlements**.

### Calculation rules

When recalculating a **user settlement**:

1. Determine the active contract per REQ-BIL-001 (majority of days in the month).
2. **Expected minutes** starts from **Monthly hours norm** × (days contract active in month ÷ days in month), rounded to whole minutes.
3. Subtract minutes for each **approved** leave day where the leave type **Counts as paid** is **true**; use **8** hours (**480** minutes) per full day and **4** hours (**240** minutes) per half-day unless the contract's daily norm differs (daily norm = **Monthly hours norm** ÷ **22**, rounded to whole minutes).
4. **Logged minutes** = sum of time entry **Duration** for that user and month.
5. **Leave days** = sum of approved leave days in the period.
6. **Balance minutes** = **Logged minutes** − **Expected minutes**.
7. Set **Last calculated at** to the calculation timestamp.

### Close period

- Action **Close period** on **`Open`** periods only.
- Confirmation: **`Close {Month name} {Year}? Settlements cannot be recalculated after closing.`**
- On confirm: status **`Closed`**, **Closed at** and **Closed by** set; message **`Settlement period closed.`**
- **`Closed`** periods remain visible on **My billing** as read-only published summaries.

### My billing (employee view)

- Screen: **My billing**
- Requires **Billing.ViewOwn**.
- Shows a table of **user settlements** for the signed-in user for **`Closed`** periods only, columns: **Period**, **Expected time**, **Logged time**, **Leave days**, **Balance**.
- **`Open`** periods are not shown to employees.
- Empty state: **`No published settlements yet.`**
- Row opens read-only **User settlement details** for that period.

### Settlement operation history

- Append-only log entries when: period created, recalculate all, single recalculate, period closed.
- Each entry: **Timestamp**, **Actor**, **Operation** (**`Created`**, **`Recalculated`**, **`Closed`**), **Period**, optional **User** for single recalculate.
- Visible on **Billing reports** — **`Audit log`** tab (REQ-BIL-008).

### Permissions and visibility

- **Billing.ManageSettlements**: create periods, recalculate, close.
- **Billing.ViewReports**: read-only **Settlements** and operation history.
- **Billing.ViewOwn**: **My billing** for **`Closed`** periods only.

---
