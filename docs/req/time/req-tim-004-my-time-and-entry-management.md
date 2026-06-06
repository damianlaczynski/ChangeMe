---
id: REQ-TIM-004
title: My Time and Entry Management
domain: time
status: active
depends_on: [REQ-TIM-001, REQ-TIM-002, REQ-TIM-006, REQ-TIM-007]
---

## Goal

An authorized user must be able to review their own time entries, filter them, and edit or delete entries they are permitted to manage.

## Features

### My time screen

- Screen: **My time**
- Requires **Time.ViewOwn**.
- Page title: **`My time`**.

### Summary strip

- Below the page title, a summary card shows **`Total in period`** with the sum of **Duration** for entries matching the active filters, using REQ-TIM-007 format.
- Updates when filters change or entries are saved, edited, or deleted.

### Header actions

| Action          | Style              | Visibility                           |
| --------------- | ------------------ | ------------------------------------ |
| **Log time**    | Primary button     | **Time.LogOwn**                      |
| **Start timer** | Outlined secondary | **Time.LogOwn** and no timer running |

- When a timer is running, **Start timer** is replaced by text **`Timer running`** with a link **`View timer`** that scrolls focus to the top bar timer control (REQ-TIM-007).

### Filters

- Toggleable **Filters** panel (collapsed by default), same interaction pattern as **Issues list** (REQ-ISS-001).

| Filter        | Behavior                                                                                                                             |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------ |
| **Date from** | Inclusive start of **work date** range.                                                                                              |
| **Date to**   | Inclusive end of **work date** range.                                                                                                |
| **Project**   | Single-select dropdown; **`All projects`** default; lists projects where the user has **Project.Time.View** or **Project.Time.Log**. |

- Default filter range: first day through last day of the **current calendar month**.
- Quick preset chips above the date fields: **`This week`**, **`This month`**, **`Last month`**. Selecting a preset sets **Date from** and **Date to** and applies filters immediately.
- **Apply filters** refreshes the list.
- **Clear filters** resets to defaults (current month, all projects).
- Invalid range (**Date from** after **Date to**): inline error **`Date from must be on or before Date to.`**

### Entries table

| Column          | Content                                                                   |
| --------------- | ------------------------------------------------------------------------- |
| **Work date**   | Sortable descending (default) and ascending.                              |
| **Project**     | Project name; link to **Project details** when user has **Project.View**. |
| **Issue**       | Issue title as link to **Issue details** when set; **`—`** when none.     |
| **Duration**    | REQ-TIM-007 format.                                                       |
| **Description** | Truncated to **60** characters with tooltip; **`—`** when empty.          |
| **Actions**     | Overflow menu.                                                            |

- Secondary sort: **created at** descending.
- Server-paginated; first page **20** entries; **Show more** appends older pages.
- Empty state with active filters: **`No time entries match the selected filters.`** with **Clear filters** link.
- Empty state with no entries at all: **`No time entries yet`** with **Log time** primary button.

### Edit time entry

- **Edit** opens **Edit time entry** dialog (REQ-TIM-007) with fields **Project**, **Issue**, **Work date**, **Duration**, **Description** (same rules as REQ-TIM-002, including presets).
- **Project** and **Issue** are editable when the user has **Project.Time.Log** on the target project after save.
- On success: toast **`Time entry saved.`**, dialog closes, list and summary refresh.
- Authorization to **Edit**:
  - Own entry: **Time.ManageOwn** and **Project.Time.Log** on the entry's project.
  - Another user's entry: **Project.Time.Manage** on the entry's project **or** **Time.ManageAny** (entry appears only on **Time reports** drill-down, not on **My time**).

### Delete time entry

- **Delete** shows confirmation dialog: **`Delete this time entry? This action cannot be undone.`**
- On confirm: remove entry, write operation audit (REQ-TIM-006), toast **`Time entry deleted.`**, refresh list and summary.
- Authorization to **Delete** matches **Edit** authorization.

### Cross-user viewing

- **My time** always shows only the signed-in user's entries.
- Viewing another user's entries uses **Time reports** person drill-down (REQ-TIM-005).

### Permissions and visibility

- Sidebar **My time** entry requires **Time.ViewOwn** (REQ-TIM-007).
- Row **Edit** and **Delete** appear in the overflow menu only when authorized for that entry.

### Out of scope for this REQ

- Aggregated reports and CSV export (REQ-TIM-005).

---
