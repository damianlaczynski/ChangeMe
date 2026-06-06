---
id: REQ-BIL-011
title: My Availability Calendar
domain: billing
status: active
depends_on: [REQ-BIL-004, REQ-BIL-010, REQ-BIL-006]
---

## Goal

The signed-in user must be able to view their availability on a calendar, maintain a weekly recurring pattern, and add or edit manual availability exceptions.

## Features

### My availability screen

- Screen: **My availability**
- Requires **Billing.ViewOwn**.
- Page title: **`My availability`**.

### Calendar layout

- View toggle: **`Month`** and **`Week`**; default **`Month`**.
- Navigation: previous period, **`Today`**, next period.
- **Month** view: one cell per calendar day in the visible month; each cell shows the effective **Availability status** using display priority from REQ-BIL-010 (**`Leave`** > **`Manual`** > **`Recurring`**).
- **Week** view: seven day columns with a time grid from one hour before **Default workday start** through one hour after **Default workday end** (REQ-BIL-004), in **30**-minute rows; when settings are unavailable, use **06:00**–**20:00**; entries render as blocks spanning their time range; all-day entries appear in a band above the grid.
- **Today** column or cell uses a distinct border in both views.
- Legend below the calendar: **`Available`**, **`Unavailable`**, **`Remote`**, **`On-site`**, **`Leave`** ( **`Leave`** uses the **`Unavailable`** color with a **`Leave`** tag).

### Day cell presentation (month view)

| Effective source | Cell display                                                                                                      |
| ---------------- | ----------------------------------------------------------------------------------------------------------------- |
| **`Leave`**      | Status badge **`Leave`** plus leave type name (truncated to **20** characters).                                   |
| **`Manual`**     | **Availability status** badge.                                                                                    |
| **`Recurring`**  | **Availability status** badge, recurring indicator, and time range **`{start}–{end}`** when not all-day.          |
| No data          | Empty cell with neutral background; weekend days (Saturday, Sunday) use a muted background when no entry applies. |

- Clicking a day opens **Day availability** side panel.

### Day availability side panel

Read-only when the user lacks **Billing.ManageOwnAvailability**; editable otherwise.

- Panel title: **`{Weekday}, {Month} {Day}, {Year}`**.
- Lists all entries for that day sorted by priority: **`Leave`** first, then **`Manual`**, then **`Recurring`**.
- Each row shows: **Source** badge (**`Leave`**, **`Manual`**, **`Recurring`**), **Availability status**, time range (**`All day`** or **`{start}–{end}`**), **Notes** when present.
- **`Leave`** rows have no edit or delete actions.
- **`Manual`** rows: **Edit**, **Delete** (requires **Billing.ManageOwnAvailability**).
- **`Recurring`** rows are read-only with hint **`From weekly pattern.`**
- Header action **Add exception** (requires **Billing.ManageOwnAvailability**) opens **Add availability** dialog with **Start date** and **End date** pre-filled to the selected day.

### Add availability dialog

| Field                   | Behavior                                                                   |
| ----------------------- | -------------------------------------------------------------------------- |
| **Start date**          | Required; defaults from selected day.                                      |
| **End date**            | Required; defaults to **Start date**.                                      |
| **All day**             | Toggle; default **true**.                                                  |
| **Start time**          | Shown when **All day** is **false** and dates are equal.                   |
| **End time**            | Shown when **Start time** is shown.                                        |
| **Availability status** | Required; **`Available`**, **`Unavailable`**, **`Remote`**, **`On-site`**. |
| **Notes**               | Not required; max **500** characters.                                      |

- **Save**: creates **`Manual`** entry; message **`Availability saved.`**; refreshes calendar and panel.
- **Cancel**: close without saving.
- Validation per REQ-BIL-010 including overlap error **`Availability overlaps an existing entry.`**

### Edit availability dialog

- Same fields as **Add availability**, pre-filled from the selected **`Manual`** entry.
- **Save changes**: message **`Availability saved.`**
- **Delete** button: confirmation **`Delete this availability entry?`**; message **`Availability deleted.`**

### Weekly pattern section

- Collapsible section **Weekly pattern** below the calendar; default **collapsed**.
- Requires **Billing.ManageOwnAvailability** to edit; read-only table with **Billing.ViewOwn** only.
- Info line above the table: **`Organization defaults: {default workdays as comma-separated short names} {default workday start}–{default workday end}, {default availability status}.`** Values come from billing settings (REQ-BIL-004).
- Table: **Day**, **Enabled**, **Hours**, **Status**.
- **Hours** shows **`{start}–{end}`** when **Enabled** is **true**; **`—`** when **false**.
- Header action **Edit pattern** opens **Edit weekly pattern** dialog.
- Header action **Reset to organization defaults** (requires **Billing.ManageOwnAvailability**): confirmation **`Replace your weekly pattern with organization defaults scaled to your current contract FTE?`**; on confirm, overwrite the pattern per REQ-BIL-004 scaling rules and regenerate **`Recurring`** entries; message **`Weekly pattern reset to organization defaults.`**

### Edit weekly pattern dialog

- Seven rows (**Monday**–**Sunday**) with **Enabled** toggle, **Start time**, **End time**, **Availability status** per REQ-BIL-010.
- **Save pattern**: regenerate **`Recurring`** entries per REQ-BIL-010; message **`Weekly pattern saved.`**; refresh calendar.
- **Cancel**: close without saving.
- Validation: when **Enabled** is **true**, **Start time** and **End time** are required; inline error **`Enter start and end times.`**

### Empty and edge states

- When no pattern, no manual entries, and no leave exist in the visible range, month cells remain empty (no special empty-state banner).
- Loading indicator while calendar data loads.

### Permissions and visibility

- **Billing.ViewOwn**: view calendar and read-only panel and pattern.
- **Billing.ManageOwnAvailability**: all edit actions on own data.

---
