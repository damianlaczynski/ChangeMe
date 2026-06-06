---
id: REQ-TIM-003
title: Issue Details — Time Tab
domain: time
status: active
depends_on: [REQ-ISS-003, REQ-TIM-001, REQ-TIM-002, REQ-TIM-004, REQ-TIM-007]
---

## Goal

On **Issue details**, authorized users must see logged work time for the issue, who contributed, and actions to log or manage time.

## Features

### Access

- Screen: **Issue details** — **Time** tab.
- Tab is visible when the user has **Project.Time.View** on the issue's project.
- Tab label format per REQ-TIM-007.
- **Log time** and **Start timer** on this tab require **Time.LogOwn** and **Project.Time.Log** on the issue's project.

### Tab layout

Layout order (top to bottom):

1. **Time summary** card.
2. Action bar with **Log time** (primary) and **Start timer** (secondary, outlined) when permitted.
3. Time entries table.
4. **Show more** when more entries exist.

### Time summary card

- Card title: **`Time on this issue`**.
- **Total logged time** using REQ-TIM-007 duration format.
- **Contributors** row: label **`Contributors`**, then up to **5** contributor display names as compact badges sorted alphabetically; when more than **5**, show **`+{n} more`** badge with tooltip listing remaining names alphabetically.
- When no time logged, contributors row shows **`No time logged yet`**.

### Time entries table

| Column          | Content                                                                       |
| --------------- | ----------------------------------------------------------------------------- |
| **Author**      | Display name of entry author.                                                 |
| **Work date**   | Calendar date.                                                                |
| **Duration**    | REQ-TIM-007 format.                                                           |
| **Description** | Truncated to **80** characters with full text in tooltip; **`—`** when empty. |
| **Actions**     | Overflow menu (see below).                                                    |

- Sorted by **work date** descending, then **created at** descending.
- First page **10** entries; **Show more** appends older pages.
- While loading, skeleton rows appear in the table area; the summary card remains visible once loaded.
- Empty state (including zero entries after load): illustration-free message **`No time logged on this issue yet`** with **Log time** (primary) and **Start timer** (secondary) buttons when permitted.

### Row actions

- Overflow menu: **Edit**, **Delete**.
- **Edit** opens **Edit time entry** dialog (REQ-TIM-004) when authorized.
- **Delete** opens delete confirmation (REQ-TIM-004) when authorized.
- Menu items the user lacks permission for are omitted (not disabled).

### Actions

- **Log time** opens **Log time** dialog with **Project** and **Issue** pre-filled and read-only (REQ-TIM-002).
- **Start timer** starts a timer associated with this issue (REQ-TIM-002).

### Permissions and visibility

- Users with **Project.Time.View** only see the summary and table; action buttons are hidden when the user lacks **Time.LogOwn** or **Project.Time.Log**.
- Edit and delete visibility follows REQ-TIM-004 authorization rules.

### Out of scope for this REQ

- Adding time operations to the issue **History** tab (REQ-ISS-003).
- Cross-issue or cross-project reports (REQ-TIM-005).

---
