---
id: REQ-BIL-005
title: Leave Requests and Approval
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-004]
---

## Goal

Users and administrators must be able to create leave requests, submit them for approval when required, and allow approvers to accept or reject submitted requests.

## Features

### Leave requests list (administrative)

- Screen: **Leave requests**
- Requires **Billing.ViewAny**, **Billing.ManageLeave**, or **Billing.ApproveLeave**.
- Page title: **`Leave requests`**.

### List filters

| Filter         | Behavior                                                                                                      |
| -------------- | ------------------------------------------------------------------------------------------------------------- |
| **Status**     | Multi-select: **`Draft`**, **`Submitted`**, **`Approved`**, **`Rejected`**, **`Cancelled`**; empty means all. |
| **Leave type** | Multi-select; empty means all active types.                                                                   |
| **Users**      | Multi-select; empty means all users; requires **Billing.ViewAny** or **Billing.ManageLeave**.                 |
| **Date from**  | Filters requests whose **End date** is on or after this date.                                                 |
| **Date to**    | Filters requests whose **Start date** is on or before this date.                                              |

- Default **Date from**: first day of the current calendar month.
- Default **Date to**: last day of the current calendar month.
- Quick presets: **`This month`**, **`Next month`**, **`This quarter`**.

### List columns

| Column           | Behavior                                                   |
| ---------------- | ---------------------------------------------------------- |
| **User**         | Full name; link to **User details** when permitted.        |
| **Leave type**   | Type **Name**.                                             |
| **Dates**        | **`{start} – {end}`** or single date for one-day requests. |
| **Days**         | Calculated leave days (half-days show **`0.5`**).          |
| **Status**       | Status badge.                                              |
| **Submitted at** | Date and time; **`—`** for **`Draft`**.                    |

- Default sort: **Submitted at** descending ( **`Draft`** rows sort by **Start date** ascending).
- Empty state: **`No leave requests match the filters.`**

### Leave request details

- Screen: **Leave request details**
- Shows read-only: **User**, **Leave type**, **Start date**, **End date**, **Day portion** (single-day only), **Days**, **Status**, **Reason**, **Submitted at**, **Decided at**, **Decided by**, **Reject reason** (when **Rejected**).
- Header actions depend on status and permissions (see below).
- **Back** returns to **Leave requests** or **My leave** depending on entry point.

### Create leave request (administrative)

- Screen: **Create leave request**
- Requires **Billing.ManageLeave**.
- **User** dropdown: all users with **Deactivated** false.
- Fields: **Leave type** (active types), **Start date**, **End date**, **Day portion** (when **Allow half-day leave** is **true** and **Start date** equals **End date**), **Reason**.
- **Save as draft**: status **`Draft`**; message **`Leave request saved.`**
- **Submit**: when the leave type **Requires approval** is **true**, status **`Submitted`** and **Submitted at** set; message **`Leave request submitted.`** When **Requires approval** is **false**, status **`Approved`** immediately with **Decided at** set to submit time; message **`Leave request approved.`**
- **Cancel**: return without saving.

### Edit leave request

- Allowed when status is **`Draft`** (owner or **Billing.ManageLeave**) or **`Submitted`** (only **Billing.ManageLeave** before a decision).
- **`Approved`**, **`Rejected`**, and **`Cancelled`** requests cannot be edited.
- Same fields as create; changing dates recalculates **Days**.

### Approval actions

On **Leave request details** when status is **`Submitted`**:

| Action      | Permission               | Behavior                                                                                                                              |
| ----------- | ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------- |
| **Approve** | **Billing.ApproveLeave** | Sets **`Approved`**, **Decided at**, **Decided by**; message **`Leave request approved.`** Approver cannot approve their own request. |
| **Reject**  | **Billing.ApproveLeave** | Opens dialog with required **Reject reason**; sets **`Rejected`**; message **`Leave request rejected.`**                              |

- Self-approval attempt shows message **`You cannot approve your own leave request.`**

### Cancel request

| Actor         | Status allowed               | Permission                              |
| ------------- | ---------------------------- | --------------------------------------- |
| Request owner | **`Draft`**, **`Submitted`** | **Billing.ViewOwn** (own requests only) |
| Administrator | any except **`Cancelled`**   | **Billing.ManageLeave**                 |

- Confirmation: **`Cancel this leave request?`**
- On confirm: status **`Cancelled`**; message **`Leave request cancelled.`**

### Validation

- **Start date** must not be before the first day of the current calendar month minus **12** months.
- **End date** must be on or after **Start date**.
- Overlap with another **`Approved`** or **`Submitted`** request for the same user: **`Leave dates overlap an existing request.`**
- **Reject reason**: required on reject; max **500** characters.

### Status workflow

```text
Draft → Submitted → Approved
                 ↘ Rejected
Draft → Cancelled
Submitted → Cancelled
```

- **`Approved`** leave counts toward **Used days** (REQ-BIL-004) and settlements (REQ-BIL-001) from the **Start date** onward.

### Permissions and visibility

- **Billing.ManageLeave**: create and edit requests for any user; cancel any non-cancelled request.
- **Billing.ApproveLeave**: **Approve** and **Reject** on **`Submitted`** requests.
- **Billing.ViewAny**: read-only access to **Leave requests** list and details.

---
