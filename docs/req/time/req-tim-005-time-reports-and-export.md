---
id: REQ-TIM-005
title: Time Reports and CSV Export
domain: time
status: active
depends_on: [REQ-TIM-001, REQ-TIM-004, REQ-TIM-006, REQ-TIM-007]
---

## Goal

An authorized administrator must be able to analyze logged work time across users, projects, and issues with flexible grouping, date filters, and CSV export.

## Features

### Access

- Screen: **Time reports**
- Requires **Time.ViewReports**.
- Page title: **`Time reports`**.

### Screen tabs

- Three tabs: **`Reports`**, **`Audit log`**, **`Settings`**.
- Default tab on open: **`Reports`**.
- **`Audit log`** content per REQ-TIM-006.
- **`Settings`** content per **Time settings** below.

### Reports tab layout

Layout order (top to bottom):

1. Filter bar (always visible).
2. Grouping selector and **Run report** action.
3. Results area (KPI card + table) after a successful run.

### Report filters

| Filter        | Behavior                                                                                                |
| ------------- | ------------------------------------------------------------------------------------------------------- |
| **Date from** | Required; inclusive start of **work date** range. Default: first day of the **current calendar month**. |
| **Date to**   | Required; inclusive end of **work date** range. Default: last day of the **current calendar month**.    |
| **Projects**  | Multi-select; empty means **all projects**; placeholder **`All projects`**.                             |
| **Users**     | Multi-select of active users; empty means **all users**; placeholder **`All users`**.                   |

- Quick preset chips: **`This week`**, **`This month`**, **`Last month`**, **`Last 30 days`**. Selecting a preset updates **Date from** and **Date to** immediately.
- **Date from** must be on or before **Date to**; inline error: **`Date from must be on or before Date to.`**
- Deep link from **Project details** **View time report** (REQ-TIM-007) opens **Reports** tab with **Projects** pre-selected to that project.

### Grouping modes

Segmented control (single selection) with labels:

| Mode                   | Label (exact)      | Result                                                                                                    |
| ---------------------- | ------------------ | --------------------------------------------------------------------------------------------------------- |
| **By person**          | By person          | One row per user; columns **User**, **Total time**.                                                       |
| **By project**         | By project         | One row per project; columns **Project**, **Total time**.                                                 |
| **By issue**           | By issue           | One row per issue; columns **Issue**, **Project**, **Total time**. Entries without an issue are excluded. |
| **Person and project** | Person and project | Matrix: rows **User**, columns **Project**, cells **Total time**; footer row and column totals.           |
| **Overall**            | Overall            | KPI only (see below); no table rows beyond the summary.                                                   |

- **Run report** loads results; while loading, the results area shows a loading indicator and prior results are hidden.

### Results presentation

- KPI card above the table: label **`Total time`**, value in REQ-TIM-007 format for all entries matching filters (always shown after successful run).
- Table rows sorted alphabetically by primary label (**User**, **Project**, or **Issue**) except **Overall**, which shows KPI card only.
- Empty result set: message **`No time entries match the selected filters.`** in the results area (KPI shows **`0m`**).
- **Export CSV** button appears top-right of the results area after a successful run; outlined secondary styling.

### User drill-down

- In **By person** grouping, each **User** row is clickable (chevron icon indicates expand).
- Clicking toggles an inline expandable section **Person time details** below the row with entry table columns: **Work date**, **Project**, **Issue**, **Duration**, **Description**.
- Only one person section expanded at a time; expanding another collapses the previous.
- Entry list paginated: first page **20**, **Show more** appends.
- **Issue** column links to **Issue details** when the issue still exists; plain text when deleted.

### CSV export

- **Export CSV** downloads the current report view (filters + grouping) as UTF-8 CSV with comma separator.
- File name: **`time-report-{YYYY-MM-DD}.csv`** using today's date in the user's locale calendar.
- **Person and project** matrix export includes header row, data rows, and total row/column.

### Settings tab

- Section title: **`Time settings`**.
- Field **Time backdating limit (days)** (REQ-TIM-001): integer input with description **`Maximum days before today that users can set as work date.`**
- Users with **Time.ViewReports** only: field is read-only.
- Users with **Roles.Manage**: field is editable; **Save** shows toast **`Time settings saved.`** on success.
- Field validation: integer **0–3650**; inline error: **`Enter a whole number of days from 0 to 3650.`**

### Permissions and visibility

- Sidebar **Time reports** requires **Time.ViewReports** (REQ-TIM-007).
- **Settings** tab is visible to all users with **Time.ViewReports**; save requires **Roles.Manage**.

### Out of scope for this REQ

- Billable / non-billable breakdown.
- PDF or Excel export.
- Scheduled or emailed reports.

---
