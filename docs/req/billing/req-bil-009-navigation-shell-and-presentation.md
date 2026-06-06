---
id: REQ-BIL-009
title: Navigation, Shell, and Presentation
domain: billing
status: active
depends_on: [REQ-BIL-006, REQ-BIL-007, REQ-BIL-008, REQ-BIL-011, REQ-BIL-012]
---

## Goal

Billing and settlement features must be reachable from consistent navigation and use the same visual language as time tracking and user administration screens.

## Features

### Sidebar navigation

| Entry                     | Icon                      | Visibility                                                                | Placement                                         |
| ------------------------- | ------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------- |
| **My leave**              | **`pi pi-sun`**           | **Billing.ViewOwn**                                                       | After **My time**, before administrative entries. |
| **My availability**       | **`pi pi-calendar-plus`** | **Billing.ViewOwn**                                                       | After **My leave**.                               |
| **My billing**            | **`pi pi-wallet`**        | **Billing.ViewOwn**                                                       | After **My availability**.                        |
| **Leave requests**        | **`pi pi-calendar`**      | **Billing.ViewAny**, **Billing.ManageLeave**, or **Billing.ApproveLeave** | After **Users**, before **Roles**.                |
| **Availability calendar** | **`pi pi-users`**         | **Billing.ViewAny**                                                       | After **Leave requests**.                         |
| **Positions**             | **`pi pi-briefcase`**     | **Billing.ManageEmployment** or **Billing.ViewAny**                       | After **Availability calendar**.                  |
| **Settlements**           | **`pi pi-calculator`**    | **Billing.ManageSettlements** or **Billing.ViewReports**                  | After **Positions**.                              |
| **Billing reports**       | **`pi pi-chart-line`**    | **Billing.ViewReports**                                                   | After **Time reports**.                           |

- No duplicate sidebar entry for **Create leave request** or **Create period**; those remain header actions on their parent screens.

### User details integration

- **Employment** section on **User details** per REQ-BIL-003 appears when the viewer has **Billing.ViewAny** or **Billing.ManageEmployment**.
- Quick link **View leave** in **Employment** section header opens **Leave requests** filtered to this user (requires **Billing.ViewAny** or **Billing.ManageLeave**).
- Quick link **View availability** in **Employment** section header opens **Availability calendar** filtered to this user (requires **Billing.ViewAny**).

### Duration and balance display

- Reuse duration format from REQ-TIM-007 for all time columns in billing screens.
- **Balance** display: prefix **`+`** for overtime, **`-`** for undertime, no prefix for zero (for example **`+2h 30m`**, **`-1h 15m`**, **`0m`**).

### Status badges

| Entity            | Status            | Badge color semantic        |
| ----------------- | ----------------- | --------------------------- |
| Leave request     | **`Draft`**       | neutral                     |
| Leave request     | **`Submitted`**   | info                        |
| Leave request     | **`Approved`**    | success                     |
| Leave request     | **`Rejected`**    | danger                      |
| Leave request     | **`Cancelled`**   | neutral                     |
| Settlement period | **`Open`**        | info                        |
| Settlement period | **`Closed`**      | neutral                     |
| Contract          | **`Active`**      | success                     |
| Contract          | **`Future`**      | info                        |
| Contract          | **`Ended`**       | neutral                     |
| Availability      | **`Available`**   | success                     |
| Availability      | **`Unavailable`** | danger                      |
| Availability      | **`Remote`**      | info                        |
| Availability      | **`On-site`**     | primary                     |
| Availability      | **`Leave`**       | danger (with **Leave** tag) |

### Empty and loading states

- List and table screens show a loading indicator while data loads.
- Forms keep the user on the same screen when validation fails.

### Permissions and visibility

- Sidebar entries are hidden entirely when the user lacks the required permission; no disabled ghost entries.

---
