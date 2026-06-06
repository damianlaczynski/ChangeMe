---
id: REQ-BIL-006
title: My Leave
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-004, REQ-BIL-005]
---

## Goal

The signed-in user must be able to view their leave balance, manage their own leave requests, and submit new requests for approval.

## Features

### My leave screen

- Screen: **My leave**
- Requires **Billing.ViewOwn**.
- Page title: **`My leave`**.

### Leave balance card

At the top of the screen, for the **current calendar year**:

| Metric             | Display                                                                                         |
| ------------------ | ----------------------------------------------------------------------------------------------- |
| **Entitled days**  | From REQ-BIL-004 **Leave balance** rules.                                                       |
| **Used days**      | Sum of approved allowance-consuming leave.                                                      |
| **Remaining days** | **Entitled days** − **Used days**; when negative, show in warning styling with label unchanged. |

- When the user has no active contract, show info message **`No active employment contract. Leave entitlement is not calculated.`** and hide the balance card.

### My requests table

- Section title: **`My requests`**
- Columns: **Leave type**, **Dates**, **Days**, **Status**, **Submitted at**, **Actions**.
- Default filter: current and future requests (**End date** on or after today) plus any **`Draft`** or **`Submitted`** from the past **30** days.
- Toggle **`Show all years`** expands to all own requests.
- Empty state: **`You have no leave requests.`**
- Header action **Request leave** opens **Request leave** dialog.

### Request leave dialog

| Field           | Behavior                                                             |
| --------------- | -------------------------------------------------------------------- |
| **Leave type**  | Required; active types only.                                         |
| **Start date**  | Required.                                                            |
| **End date**    | Required; defaults to **Start date** on single-day entry.            |
| **Day portion** | Shown when **Allow half-day leave** is **true** and dates are equal. |
| **Reason**      | Not required; max **500** characters.                                |

- **Save as draft**: creates **`Draft`**; message **`Leave request saved.`**
- **Submit**: same approval rules as REQ-BIL-005; message **`Leave request submitted.`** or **`Leave request approved.`**
- **Cancel**: close without saving.
- Validation matches REQ-BIL-005.

### Row actions (own requests)

| Status                                          | Actions                          |
| ----------------------------------------------- | -------------------------------- |
| **`Draft`**                                     | **Edit**, **Submit**, **Delete** |
| **`Submitted`**                                 | **View**, **Cancel**             |
| **`Approved`**, **`Rejected`**, **`Cancelled`** | **View** only                    |

- **Delete** on **`Draft`**: confirmation **`Delete this draft leave request?`**; message **`Leave request deleted.`**
- **Edit** opens the same dialog pre-filled ( **`Draft`** only).

### Leave request details (self)

- Opened from **View** or row click.
- Same read-only fields as REQ-BIL-005 **Leave request details**.
- Header actions: **Edit** / **Submit** / **Cancel** / **Delete** per table above.
- **Back** returns to **My leave**.

### Permissions and visibility

- **Billing.ViewOwn**: required for **My leave** and own request actions.
- Users without **Billing.ManageLeave** can act only on their own requests.
- **Billing.ApproveLeave** does not add actions on **My leave**; approval happens on **Leave requests** (REQ-BIL-005).

---
