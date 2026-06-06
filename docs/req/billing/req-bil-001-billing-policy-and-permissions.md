---
id: REQ-BIL-001
title: Billing Policy and Permissions
domain: billing
status: active
depends_on: [REQ-ROL-001, REQ-TIM-001, REQ-USR-004]
---

## Goal

The system must define employment and billing entities (positions, contracts, leave, settlements), global permissions for managing and reviewing them, and the rules that connect logged work time to monthly settlements.

## Features

### Employment contract

An **employment contract** links one user to one **Position** for a date range and defines how that user's work is measured and settled.

| Attribute              | Rule                                                                                                                                  |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| **User**               | Required. The account the contract applies to.                                                                                        |
| **Position**           | Required. The organizational position held under this contract (REQ-BIL-002).                                                         |
| **Contract type**      | Required. One of **`Employment`**, **`Mandate`**, **`Work contract`**, **`B2B`**.                                                     |
| **Start date**         | Required. First calendar day the contract is effective.                                                                               |
| **End date**           | Not required. When set, last calendar day the contract is effective; must be on or after **Start date**.                              |
| **FTE**                | Required decimal from **0.01** to **1.00** (two decimal places); expresses full-time equivalent (for example **`1.00`** = full time). |
| **Monthly hours norm** | Required; whole minutes; minimum **60**; maximum **10080** (**7** days × **24** hours). Expected paid work time per calendar month.   |
| **Hourly rate**        | Not required. Decimal amount with two fractional digits; minimum **0.01** when provided.                                              |
| **Monthly salary**     | Not required. Decimal amount with two fractional digits; minimum **0.01** when provided.                                              |
| **Notes**              | Not required; max **500** characters when provided.                                                                                   |

- At least one of **Hourly rate** or **Monthly salary** must be set on every contract.
- A user may have **multiple** contracts over time; contract date ranges for the same user must **not overlap**.
- The contract active on a given calendar day is the one whose **Start date** ≤ that day and (**End date** is empty or **End date** ≥ that day).
- When no contract is active for a user on a day, that user has **no monthly hours norm** for settlement on that day.

### Leave request

A **leave request** records planned or taken absence for one user.

| Attribute         | Rule                                                                                          |
| ----------------- | --------------------------------------------------------------------------------------------- |
| **User**          | Required. The account taking leave.                                                           |
| **Leave type**    | Required. One of the configured **Leave types** (REQ-BIL-004).                                |
| **Start date**    | Required. First calendar day of leave.                                                        |
| **End date**      | Required. Last calendar day of leave; must be on or after **Start date**.                     |
| **Day portion**   | Required on single-day requests only: **`Full day`**, **`First half`**, or **`Second half`**. |
| **Status**        | **`Draft`**, **`Submitted`**, **`Approved`**, **`Rejected`**, or **`Cancelled`**.             |
| **Submitted at**  | Set when status becomes **`Submitted`**; omitted while **`Draft`**.                           |
| **Decided at**    | Set when status becomes **`Approved`** or **`Rejected`**; omitted otherwise.                  |
| **Decided by**    | Display name of the approver when **Decided at** is set; omitted otherwise.                   |
| **Reason**        | Not required; max **500** characters when provided.                                           |
| **Reject reason** | Required when status is **`Rejected`**; max **500** characters.                               |

- Multi-day requests always count as full calendar days between **Start date** and **End date** inclusive.
- Single-day requests with **Day portion** **`First half`** or **`Second half`** count as **0.5** leave days toward balances and settlements.
- Only **`Approved`** leave reduces expected working time in settlements.

### Settlement period

A **settlement period** is one calendar month for which the system calculates per-user billing summaries.

| Attribute     | Rule                                                                      |
| ------------- | ------------------------------------------------------------------------- |
| **Year**      | Required calendar year.                                                   |
| **Month**     | Required calendar month (**1–12**).                                       |
| **Status**    | **`Open`** or **`Closed`**.                                               |
| **Closed at** | Timestamp when status becomes **`Closed`**; omitted while **`Open`**.     |
| **Closed by** | Display name of the user who closed the period; omitted while **`Open`**. |

- At most one settlement period exists per **Year** + **Month** pair.
- New periods are created with status **`Open`**.
- While **`Open`**, settlements can be recalculated; while **`Closed`**, recalculation is blocked.

### User settlement

A **user settlement** is the billing summary for one user within one **settlement period**.

| Attribute              | Rule                                                                                                                                                   |
| ---------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Settlement period**  | Required parent period.                                                                                                                                |
| **User**               | Required.                                                                                                                                              |
| **Contract**           | The employment contract active for the majority of days in the period; **`—`** when no contract was active.                                            |
| **Expected minutes**   | Whole minutes. **Monthly hours norm** of the active contract prorated for partial months and reduced by **approved leave** days (including half-days). |
| **Logged minutes**     | Sum of **Duration** from **time entries** (REQ-TIM-001) whose **Work date** falls in the period and whose **Author** is this user.                     |
| **Leave days**         | Decimal with one fractional digit. Total **approved** leave days in the period.                                                                        |
| **Balance minutes**    | **Logged minutes** minus **Expected minutes**; negative means undertime, positive means overtime.                                                      |
| **Last calculated at** | Timestamp of the most recent calculation.                                                                                                              |

- Calculations use **time entries** as recorded; no approval workflow on time data.
- Users without an active contract during the period still receive a row with **Expected minutes** **`0`** and **Contract** **`—`**.

### Global permission catalog

The global catalog from REQ-ROL-001 is extended with exactly these permissions:

| Permission                        | Label (exact)            | Description                                                                                                                            | Group   |
| --------------------------------- | ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------- | ------- |
| **Billing.ViewOwn**               | View own billing data    | Open **My leave**, **My availability**, and **My billing**; view own leave balance, requests, availability, and published settlements. | Billing |
| **Billing.ViewAny**               | View all billing data    | View employment profiles, contracts, leave, and team availability for any user in admin contexts.                                      | Billing |
| **Billing.ManageEmployment**      | Manage employment data   | Create and edit positions, employment profiles, and contracts.                                                                         | Billing |
| **Billing.ManageLeave**           | Manage leave requests    | Create, edit, and cancel leave requests for any user.                                                                                  | Billing |
| **Billing.ApproveLeave**          | Approve leave requests   | Approve or reject **Submitted** leave requests.                                                                                        | Billing |
| **Billing.ViewReports**           | View billing reports     | Open **Billing reports**, run grouped analysis, export CSV, and read settlement operation history.                                     | Billing |
| **Billing.ManageSettlements**     | Manage settlements       | Create settlement periods, recalculate **user settlements**, and close periods.                                                        | Billing |
| **Billing.ManageOwnAvailability** | Manage own availability  | Create, edit, and delete own **Manual** availability entries and **weekly recurring pattern** (REQ-BIL-010).                           | Billing |
| **Billing.ManageAvailability**    | Manage user availability | Create, edit, and delete availability entries and **weekly recurring pattern** for any user (REQ-BIL-010).                             | Billing |

### Default role assignments

- The seeded **User** system role (REQ-ROL-006) includes **Billing.ViewOwn** and **Billing.ManageOwnAvailability**.
- The seeded **Administrator** role includes all global billing permissions from this REQ, including **Billing.ManageAvailability**.

### Authorization rules

- Viewing own leave, availability, and settlements requires **Billing.ViewOwn**.
- Viewing another user's employment, leave, or team availability requires **Billing.ViewAny**.
- Managing own availability requires **Billing.ManageOwnAvailability** (REQ-BIL-011).
- Managing any user's availability requires **Billing.ManageAvailability** (REQ-BIL-012).
- Managing positions, employment profiles, and contracts requires **Billing.ManageEmployment**.
- Creating or editing leave on behalf of another user requires **Billing.ManageLeave**.
- Approving or rejecting leave requires **Billing.ApproveLeave** on the action; approvers must not approve their own **Submitted** requests.
- Settlement period and recalculation actions require **Billing.ManageSettlements**.
- **Billing reports** requires **Billing.ViewReports**.
- Access denial uses the standard global message from REQ-ROL-001.

### Out of scope for this REQ

- Payslip PDF generation and bank payment files.
- Tax, ZUS, and statutory payroll filing.
- Billable / non-billable flags on time entries (REQ-TIM-001).
- Automatic email notifications for leave decisions.

---
