---
id: REQ-ROL-001
title: Permission Catalog and Effective Permissions
domain: access
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-002, REQ-ROL-005, REQ-USR-001, REQ-USR-005]
---

## Goal

The system must define a fixed set of permissions and determine each user's effective permissions from their assigned roles.

## Features

### Permission catalog

The catalog contains exactly these permissions:

| Permission                        | Label (exact)                     | Description                                                                                                                                                  | Group    |
| --------------------------------- | --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------ | -------- |
| **Users.View**                    | View users                        | View the users list, user details, and read-only role badges on user screens.                                                                                |
| **Users.Manage**                  | Manage users                      | Create and edit user profile data (name, email).                                                                                                             | Users    |
| **Users.Deactivate**              | Deactivate users                  | Deactivate and reactivate user accounts.                                                                                                                     | Users    |
| **Roles.View**                    | View roles                        | View the roles list and role details.                                                                                                                        | Roles    |
| **Roles.Manage**                  | Manage roles                      | Create, edit, and delete custom roles; manage role and user assignments.                                                                                     | Roles    |
| **Sessions.ViewOwn**              | View own sessions                 | View the current user's active sessions on **My account**.                                                                                                   | Sessions |
| **Sessions.ManageOwn**            | Manage own sessions               | Revoke non-current own sessions and use **Sign out everywhere**.                                                                                             | Sessions |
| **Sessions.ViewAny**              | View user sessions                | View active sessions of any user in **User details**.                                                                                                        | Sessions |
| **Sessions.ManageAny**            | Manage user sessions              | Revoke sessions of any user, including **Revoke all sessions**.                                                                                              | Sessions |
| **Time.ViewOwn**                  | View own time entries             | Open **My time** and view own time entries.                                                                                                                  | Time     |
| **Time.LogOwn**                   | Log own time                      | Create time entries and use the running timer (REQ-TIM-002).                                                                                                 | Time     |
| **Time.ManageOwn**                | Manage own time entries           | Edit and delete own time entries (REQ-TIM-004).                                                                                                              | Time     |
| **Time.ViewAny**                  | View all time entries             | View any user's time entries in authorized cross-user contexts (REQ-TIM-005).                                                                                | Time     |
| **Time.ManageAny**                | Manage all time entries           | Edit and delete any user's time entries globally (REQ-TIM-004).                                                                                              | Time     |
| **Time.ViewReports**              | View time reports                 | Open **Time reports**, grouped analysis, CSV export, and audit log (REQ-TIM-005, REQ-TIM-006).                                                               | Time     |
| **Time.LogPastLimit**             | Log time outside backdating limit | Create and edit entries with **Work date** older than **Time backdating limit (days)** (REQ-TIM-001).                                                        | Time     |
| **Billing.ViewOwn**               | View own billing data             | Open **My leave**, **My availability**, and **My billing**; view own leave, availability, and published settlements (REQ-BIL-006, REQ-BIL-007, REQ-BIL-011). | Billing  |
| **Billing.ViewAny**               | View all billing data             | View employment profiles, contracts, leave, and team availability for any user (REQ-BIL-003, REQ-BIL-005, REQ-BIL-012).                                      | Billing  |
| **Billing.ManageEmployment**      | Manage employment data            | Create and edit positions, employment profiles, and contracts (REQ-BIL-002, REQ-BIL-003).                                                                    | Billing  |
| **Billing.ManageLeave**           | Manage leave requests             | Create, edit, and cancel leave requests for any user (REQ-BIL-005).                                                                                          | Billing  |
| **Billing.ApproveLeave**          | Approve leave requests            | Approve or reject submitted leave requests (REQ-BIL-005).                                                                                                    | Billing  |
| **Billing.ViewReports**           | View billing reports              | Open **Billing reports**, analysis, CSV export, and settlement audit log (REQ-BIL-008).                                                                      | Billing  |
| **Billing.ManageSettlements**     | Manage settlements                | Create settlement periods, recalculate settlements, and close periods (REQ-BIL-007).                                                                         | Billing  |
| **Billing.ManageOwnAvailability** | Manage own availability           | Create, edit, and delete own availability entries and weekly recurring pattern (REQ-BIL-010, REQ-BIL-011).                                                   | Billing  |
| **Billing.ManageAvailability**    | Manage user availability          | Create, edit, and delete availability entries and weekly recurring pattern for any user (REQ-BIL-010, REQ-BIL-012).                                          | Billing  |

- New permissions are added only by updating requirements and a subsequent release; administrators cannot create new permission codes in the UI.
- **Out of scope for this REQ:** project-scoped permissions (REQ-PRJ-005, including project time permissions) and issue-level permissions beyond project membership.

### Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- After sign-in, registration, or credential renewal (REQ-AUTH-001, REQ-AUTH-002), the user receives their current effective permission set.
- **My account** (REQ-USR-001) reflects the current effective permission set when opened.
- After an administrator changes role assignments (REQ-ROL-005), the new permissions apply when the affected user next renews credentials or signs in again.

### Access denial

- A signed-in user who lacks a required permission cannot perform the protected action; the system rejects the action with message **`You do not have permission to perform this action.`**
- A guest cannot perform actions that require sign-in.

### States and business rules

- A role with zero permissions cannot be saved; validation error: **`At least one permission is required.`**
- Users with **Deactivated** true cannot sign in and have no effective permissions (REQ-USR-005).

### Permissions and visibility

- Permission names in this REQ are used across **Users**, **Roles**, **Auth**, and **Sessions** requirements to control screen and action visibility.

---
