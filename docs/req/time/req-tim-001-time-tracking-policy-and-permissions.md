---
id: REQ-TIM-001
title: Time Tracking Policy and Permissions
domain: time
status: active
depends_on: [REQ-ROL-001, REQ-PRJ-005]
---

## Goal

The system must define **time entries**, global and project-scoped permissions for logging and reviewing work time, and an administrator-configurable **backdating limit** for work dates.

## Features

### Time entry

A **time entry** records work performed by one user with these attributes:

| Attribute       | Rule                                                                                                   |
| --------------- | ------------------------------------------------------------------------------------------------------ |
| **Author**      | The signed-in user who created the entry; immutable after create.                                      |
| **Project**     | Required. The project the work belongs to.                                                             |
| **Issue**       | Not required. When set, the issue must belong to the selected **Project**.                             |
| **Work date**   | Required calendar date when the work was performed.                                                    |
| **Duration**    | Required; whole minutes only; minimum **1** minute; maximum **24 hours** (**1440** minutes) per entry. |
| **Description** | Not required; max **500** characters when provided.                                                    |
| **Created at**  | System timestamp when the entry was saved.                                                             |
| **Updated at**  | System timestamp when the entry was last edited.                                                       |

- Time entries count toward reports immediately after save; no approval workflow.
- **Billable** / **non-billable** classification is **not** supported.

### Backdating limit

- Application setting **Time backdating limit (days)**: maximum number of calendar days before **today** that a user may set **Work date** on create or edit.
- Default value: **30** days.
- **Work date** must not be in the future.
- Users with global permission **Time.LogPastLimit** may create and edit entries with **Work date** older than the configured limit.
- Users without **Time.LogPastLimit** who submit a **Work date** outside the limit see inline error: **`Work date is outside the allowed range.`**

### Backdating limit administration

- **Time backdating limit (days)** is editable on **Time reports** — **`Settings`** tab (REQ-TIM-005).
- Editing **Time settings** requires global permission **Roles.Manage**.
- Field validation: required integer from **0** to **3650**; inline error on invalid value: **`Enter a whole number of days from 0 to 3650.`**
- On save success, show message **`Time settings saved.`**
- Changing the limit does not alter existing entries; it applies to subsequent create and edit actions.

### Global permission catalog

The global catalog from REQ-ROL-001 is extended with exactly these permissions:

| Permission            | Label (exact)                     | Description                                                                                                                           | Group |
| --------------------- | --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- | ----- |
| **Time.ViewOwn**      | View own time entries             | Open **My time** and view own time entries.                                                                                           | Time  |
| **Time.LogOwn**       | Log own time                      | Create time entries and use the running timer (subject to project membership and project permissions).                                | Time  |
| **Time.ManageOwn**    | Manage own time entries           | Edit and delete own time entries (subject to backdating rules unless **Time.LogPastLimit** applies).                                  | Time  |
| **Time.ViewAny**      | View all time entries             | View any user's time entries on **My time** (when viewing as another user) and in cross-user contexts that reference this permission. | Time  |
| **Time.ManageAny**    | Manage all time entries           | Edit and delete any user's time entries globally.                                                                                     | Time  |
| **Time.ViewReports**  | View time reports                 | Open **Time reports**, run grouped reports, export CSV, and read the time entry operation audit log.                                  | Time  |
| **Time.LogPastLimit** | Log time outside backdating limit | Create and edit time entries with **Work date** older than **Time backdating limit (days)**.                                          | Time  |

- New global time permissions are added only by updating requirements and a subsequent release.

### Project permission catalog

The project permission catalog from REQ-PRJ-005 is extended with exactly these permissions:

| Permission              | Label (exact)       | Description                                                            |
| ----------------------- | ------------------- | ---------------------------------------------------------------------- |
| **Project.Time.Log**    | Log project time    | Create time entries and use the running timer for this project.        |
| **Project.Time.View**   | View project time   | View all time entries recorded against this project and its issues.    |
| **Project.Time.Manage** | Manage project time | Edit and delete any user's time entries recorded against this project. |

### Default role assignments

- The seeded **User** system role (REQ-ROL-006) includes **Time.ViewOwn**, **Time.LogOwn**, and **Time.ManageOwn**.
- The seeded **Administrator** role includes all global time permissions from this REQ.
- Project role permission sets are defined in REQ-PRJ-005.

### Authorization rules

- Creating a time entry requires **Time.LogOwn** and **Project.Time.Log** on the entry's **Project**; the acting user must be a project member on that project.
- Viewing time entries for a project (including on **Issue details** — **Time** tab) requires **Project.Time.View** on that project.
- Editing or deleting another user's time entry in a project requires **Project.Time.Manage** on that project **or** global **Time.ManageAny**.
- Editing or deleting own time entry requires **Time.ManageOwn** and, when the entry belongs to a project, **Project.Time.Log** on that project.
- Global **Time.ViewAny** does **not** bypass **Project.Time.View** for project-scoped lists; both are required where this REQ specifies project visibility.
- Access denial uses the standard global or project message from REQ-ROL-001 and REQ-PRJ-005.

### Out of scope for this REQ

- Billable / non-billable flags and billing workflows.
- Time entry approval before reporting.
- Issue **History** tab entries for time operations (audit is REQ-TIM-006).

---
