---
id: REQ-ISS-001
title: Issue List
domain: issues
status: active
depends_on: [REQ-AUTH-001, REQ-ISS-002, REQ-USR-005]
---
## Goal

The user must be able to browse all issues, search, filter, sort, navigate to details, manage watches, and quickly start creating a new issue.

## Features

### Access

- Screen: **Issues list**
- Available only to authenticated users. Guests are redirected to **Login** (REQ-AUTH-001).

### Search and actions bar

- **Add issue** button opens **Create issue** (REQ-ISS-002).

### Issues table — columns

| Column            | Description                                                                                       |
| ----------------- | ------------------------------------------------------------------------------------------------- |
| **Title**         | Short issue title; clickable link to **Issue details**.                                           |
| **Status**        | Issue status badge (**New**, **In Progress**, **Resolved**, **Closed**).                          |
| **Priority**      | Priority badge (**Low**, **Medium**, **High**, **Critical**).                                     |
| **Assigned to**   | Assignee as **`{name} ({email})`** or **email** when name is missing; **`Unassigned`** when none. |
| **Created at**    | Issue creation date and time.                                                                     |
| **Last activity** | Date and time of the most recent change, comment, or other activity on the issue.                 |
| **Actions**       | Watch control and overflow menu for row actions (see below).                                      |

### Sorting

- **Title**: sortable alphabetically ascending and descending.
- **Created at**: sortable chronologically ascending and descending.
- **Last activity**: sortable chronologically ascending and descending.
- Default sort: **Last activity**, descending (most recent first).

### Row actions and watch control

- **Watch / Unwatch**: compact button shows **watcher count** as label and bell / bell-slash icon for current watch state. Tooltip format: `**Watch this issue ({n} watchers)`** or `**Unwatch this issue ({n} watchers)\*\*`where`{n}` is the count.
- Overflow menu: **Open details**, **Edit issue**, **Delete issue**.
- **Delete issue** confirmation: `**Delete "{issue title}"? This action cannot be undone.`\*\*

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Status** filter: multi-select; empty selection means no restriction.
- **Priority** filter: multi-select; empty selection means no restriction.
- **Assigned to** filter: single-select user list from assignable users (REQ-USR-005); clearable.
- **Watched by me** filter: checkbox; when selected, shows only issues watched by the current user.
- **My issues** filter: checkbox; when selected, shows only issues **created by** or **assigned to** the current user.
- All filters combine with search text using **AND** logic.
- **Apply filters** submits the filter panel with the current search text.
- **Clear filters** resets the filter form and removes all filter constraints (search text included).
- Applied filters list

### Pagination

- The issues table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search, filters, and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area; the screen layout remains visible.

---
