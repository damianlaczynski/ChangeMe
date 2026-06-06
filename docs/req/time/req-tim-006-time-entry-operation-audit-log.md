---
id: REQ-TIM-006
title: Time Entry Operation Audit Log
domain: time
status: active
depends_on: [REQ-TIM-001, REQ-TIM-002, REQ-TIM-004, REQ-TIM-005]
---

## Goal

Every create, update, and delete action on a time entry must be recorded in a dedicated audit log so administrators can review changes, including mistakes corrected by administrators.

## Features

### Audited operations

The system appends an audit record when:

- a time entry is **created**,
- a time entry is **updated**,
- a time entry is **deleted**.

- Audit records are written for all acting users, including users with **Time.ManageAny** or **Project.Time.Manage**.
- Deleted entries remain represented in the audit log; the entry no longer appears in operational lists or reports.

### Audit record content

Each audit record includes:

| Field            | Content                                                                              |
| ---------------- | ------------------------------------------------------------------------------------ |
| **Operation**    | Badge: **`Created`** (green), **`Updated`** (blue), or **`Deleted`** (red).          |
| **Acting user**  | Display name of the user who performed the operation.                                |
| **Occurred at**  | Date and time of the operation.                                                      |
| **Entry author** | Display name of the time entry **Author**.                                           |
| **Project**      | Project name at the time of the operation.                                           |
| **Issue**        | Issue title when the entry had an issue; **`—`** when none.                          |
| **Work date**    | **Work date** after the operation (**before** value on delete).                      |
| **Duration**     | Duration after the operation in REQ-TIM-007 format (**before** value on delete).     |
| **Description**  | Description after the operation, or **`—`** when empty (**before** value on delete). |

- For **Updated** operations, expanded detail shows **Before** / **After** pairs for **Work date**, **Duration**, **Description**, **Project**, and **Issue** when any changed.
- For **Created** operations, **Before** values are omitted.
- For **Deleted** operations, expanded detail shows **Before** values only.

### Audit log tab

- Location: **Time reports** — **`Audit log`** tab (REQ-TIM-005).
- Requires **Time.ViewReports**.

### Filters

- Toggleable **Filters** panel (expanded by default on **`Audit log`** tab).

| Filter           | Behavior                                                              |
| ---------------- | --------------------------------------------------------------------- |
| **Date from**    | Default: first day of current calendar month.                         |
| **Date to**      | Default: last day of current calendar month.                          |
| **Acting user**  | Single-select; clearable; all users when empty.                       |
| **Entry author** | Single-select; clearable; all users when empty.                       |
| **Project**      | Single-select; clearable; all projects when empty.                    |
| **Operation**    | Multi-select: **Created**, **Updated**, **Deleted**; empty means all. |

- **Apply filters** refreshes the list.
- **Clear filters** resets to tab defaults.

### Audit list

- Table columns: **Operation**, **Occurred at**, **Acting user**, **Entry author**, **Project**, **Duration** (after/before value per rules above).
- Rows expand on click to show full **Before** / **After** detail panel.
- Sorted by **Occurred at** descending.
- Server-paginated; first page **20** records; **Show more** appends older pages.
- Empty state: **`No audit records match the selected filters.`**

### Separation from issue history

- Time entry operations do **not** appear on **Issue details** — **History** tab (REQ-ISS-003).
- The audit log is the sole user-facing history for time entry changes.

### Out of scope for this REQ

- Immutable tamper-evident storage or external SIEM export.
- Restoring deleted entries from the audit log.

---
