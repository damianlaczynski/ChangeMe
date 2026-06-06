---
id: REQ-BIL-012
title: Team Availability Calendar
domain: billing
status: active
depends_on: [REQ-BIL-010, REQ-BIL-011, REQ-PRJ-006, REQ-USR-002]
---

## Goal

An authorized viewer must be able to see availability for multiple users on one calendar and, when permitted, submit or edit availability on behalf of others.

## Features

### Availability calendar screen

- Screen: **Availability calendar**
- Requires **Billing.ViewAny**.
- Page title: **`Availability calendar`**.

### Filters

| Filter       | Behavior                                                                                                               |
| ------------ | ---------------------------------------------------------------------------------------------------------------------- |
| **Users**    | Multi-select of users with **Deactivated** false; empty means **all users**; placeholder **`All users`**.              |
| **Projects** | Multi-select; when set, limits **Users** to members of selected projects (REQ-PRJ-006); empty means no project filter. |
| **Status**   | Multi-select of **Availability status** values plus **`Leave`**; empty means all.                                      |

- Filter changes reload the calendar immediately.
- Deep link from **User details** — **View availability** (REQ-BIL-009) opens with **Users** pre-selected to that user.

### Calendar layout

- View toggle: **`Month`** and **`Week`**; default **`Month`**.
- The selected view persists for the signed-in user across sessions until changed.
- Navigation: previous period, **`Today`**, next period.
- Density toggle: **`Compact`** and **`Standard`**; default **`Standard`**; persists across sessions.
- Legend and density toggle appear below the filter bar in both views.

### Week view

- Rows: one per user (full name); sorted alphabetically by **Last name**, then **First name**.
- Columns: seven days of the visible week; **Today** column has a distinct border.
- Each cell shows the effective **Availability status** for that user and day (same priority as REQ-BIL-011).
- **`Standard`** density: status badge plus time range **`{start}–{end}`** when the effective entry is not all-day.
- **`Compact`** density: status badge only; tooltip on hover shows full detail (**Source**, status, time range, **Notes**).
- When multiple partial-day entries exist, the cell shows the highest-priority status badge and a count badge **`+{n}`** when more than one source applies.
- Clicking a cell opens **User day availability** side panel for that user and day.

### Month view (team)

- Matrix layout: one row per user, one column per calendar day of the visible month (same user ordering as week view).
- First column **User** is sticky during horizontal scroll; day columns scroll horizontally when the month exceeds viewport width.
- Weekend column headers (Saturday, Sunday) use muted styling.
- **Today** column has a distinct border across all rows.
- Cell content rules match week view, including **Compact** / **`Standard`** density.
- Row height in **`Standard`** density accommodates badge and one line of time text; **`Compact`** uses a single-line badge.

### Month summary row

- Below the user matrix in **Month** view, a summary row **Team summary** spans all day columns.
- Each day cell shows counts: **`{available count}`** users with effective status **`Available`**, **`Remote`**, or **`On-site`**; **`{away count}`** users with **`Unavailable`** or **`Leave`**.
- Display format: **`{available count} in · {away count} away`**; when **Users** filter is empty and the **50**-user cap applies, the summary counts only the visible **50** users.
- Summary row is read-only.

### Week summary column

- In **Week** view, an extra column **Summary** at the end of each user row shows **`{available days}/7 in`** counting days in that week with effective status **`Available`**, **`Remote`**, or **`On-site`**.

### User day availability side panel

- Panel title: **`{User full name} — {Weekday}, {Month} {Day}, {Year}`**.
- Same entry list and priority rules as REQ-BIL-011 **Day availability** side panel.
- **`Leave`** and **`Recurring`** rows are read-only for all viewers.
- **`Manual`** rows:
  - **Edit** and **Delete** when the viewer has **Billing.ManageAvailability**.
  - **Edit** and **Delete** when the viewer has **Billing.ManageOwnAvailability** **and** the row's **User** is the signed-in user.
  - Read-only otherwise.
- Header actions:
  - **Add exception** — visible with **Billing.ManageAvailability**, or with **Billing.ManageOwnAvailability** when the panel user is the signed-in user.
  - **Edit pattern** — same visibility rules; opens **Edit weekly pattern** for the panel user.

### Add and edit availability (on behalf of another user)

- **Add availability** and **Edit availability** dialogs match REQ-BIL-011 fields and validation.
- **User** is pre-filled from the panel context and read-only.
- Success messages: **`Availability saved.`**, **`Availability deleted.`**, **`Weekly pattern saved.`**

### Bulk visibility

- When **Users** filter is empty and more than **50** users match, show message **`Showing the first 50 users. Narrow the filter to see others.`** and load only the first **50** users by alphabetical order.
- Empty result: **`No users match the selected filters.`**

### Header actions

- **Add availability** (requires **Billing.ManageAvailability**): opens **Add availability** with **User** dropdown (all active users) before date fields.
- No header action when the viewer has only **Billing.ViewAny** without manage permissions.

### Permissions and visibility

| Action                         | Permission                                             |
| ------------------------------ | ------------------------------------------------------ |
| Open **Availability calendar** | **Billing.ViewAny**                                    |
| Add/edit/delete for any user   | **Billing.ManageAvailability**                         |
| Add/edit/delete for self only  | **Billing.ManageOwnAvailability** on own rows in panel |
| Edit another user's pattern    | **Billing.ManageAvailability**                         |

- Users with **Billing.ViewOwn** but not **Billing.ViewAny** use **My availability** (REQ-BIL-011) only; they do not see **Availability calendar** in the sidebar.

---
