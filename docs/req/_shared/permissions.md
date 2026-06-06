# Permission catalog

> Canonical **global** permission names used across all REQs. Full REQ specification: `docs/req/access/req-rol-001-permission-catalog-and-effective-permissions.md`.

The catalog contains exactly these permissions:

| Permission                        | Label (exact)                     | Description                                                                                                          | Group    |
| --------------------------------- | --------------------------------- | -------------------------------------------------------------------------------------------------------------------- | -------- |
| **Users.View**                    | View users                        | View the users list, user details, and read-only role badges on user screens.                                        | Users    |
| **Users.Manage**                  | Manage users                      | Create and edit user profile data (name, email).                                                                     | Users    |
| **Users.Deactivate**              | Deactivate users                  | Deactivate and reactivate user accounts.                                                                             | Users    |
| **Roles.View**                    | View roles                        | View the roles list and role details.                                                                                | Roles    |
| **Roles.Manage**                  | Manage roles                      | Create, edit, and delete custom roles; manage role and user assignments.                                             | Roles    |
| **Sessions.ViewOwn**              | View own sessions                 | View the current user's active sessions on **My account**.                                                           | Sessions |
| **Sessions.ManageOwn**            | Manage own sessions               | Revoke non-current own sessions and use **Sign out everywhere**.                                                     | Sessions |
| **Sessions.ViewAny**              | View user sessions                | View active sessions of any user in **User details**.                                                                | Sessions |
| **Sessions.ManageAny**            | Manage user sessions              | Revoke sessions of any user, including **Revoke all sessions**.                                                      | Sessions |
| **Time.ViewOwn**                  | View own time entries             | Open **My time** and view own time entries.                                                                          | Time     |
| **Time.LogOwn**                   | Log own time                      | Create time entries and use the running timer.                                                                       | Time     |
| **Time.ManageOwn**                | Manage own time entries           | Edit and delete own time entries.                                                                                    | Time     |
| **Time.ViewAny**                  | View all time entries             | View any user's time entries in authorized cross-user contexts.                                                      | Time     |
| **Time.ManageAny**                | Manage all time entries           | Edit and delete any user's time entries globally.                                                                    | Time     |
| **Time.ViewReports**              | View time reports                 | Open **Time reports**, grouped analysis, CSV export, and audit log.                                                  | Time     |
| **Time.LogPastLimit**             | Log time outside backdating limit | Create and edit entries with **Work date** older than the configured limit.                                          | Time     |
| **Billing.ViewOwn**               | View own billing data             | Open **My leave**, **My availability**, and **My billing**; view own leave, availability, and published settlements. | Billing  |
| **Billing.ViewAny**               | View all billing data             | View employment profiles, contracts, leave, and team availability for any user.                                      | Billing  |
| **Billing.ManageEmployment**      | Manage employment data            | Create and edit positions, employment profiles, and contracts.                                                       | Billing  |
| **Billing.ManageLeave**           | Manage leave requests             | Create, edit, and cancel leave requests for any user.                                                                | Billing  |
| **Billing.ApproveLeave**          | Approve leave requests            | Approve or reject submitted leave requests.                                                                          | Billing  |
| **Billing.ViewReports**           | View billing reports              | Open **Billing reports**, analysis, CSV export, and settlement audit log.                                            | Billing  |
| **Billing.ManageSettlements**     | Manage settlements                | Create settlement periods, recalculate settlements, and close periods.                                               | Billing  |
| **Billing.ManageOwnAvailability** | Manage own availability           | Create, edit, and delete own availability entries and weekly recurring pattern.                                      | Billing  |
| **Billing.ManageAvailability**    | Manage user availability          | Create, edit, and delete availability entries and weekly recurring pattern for any user.                             | Billing  |

## Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- Users with **Deactivated** true cannot sign in and have no effective permissions (REQ-USR-005).
- Access denial message: **`You do not have permission to perform this action.`**

## Project-scoped permissions

Project access is **not** controlled by the global catalog above. Each project evaluates membership and project roles per REQ-PRJ-005. Project permission catalog:

| Permission                 | Label (exact)          |
| -------------------------- | ---------------------- |
| **Project.View**           | View project           |
| **Project.Manage**         | Manage project         |
| **Project.Members.View**   | View project members   |
| **Project.Members.Manage** | Manage project members |
| **Project.Issues.View**    | View project issues    |
| **Project.Issues.Manage**  | Manage project issues  |
| **Project.Time.Log**       | Log project time       |
| **Project.Time.View**      | View project time      |
| **Project.Time.Manage**    | Manage project time    |

Access denial on a project: **`You do not have permission to perform this action on this project.`**
