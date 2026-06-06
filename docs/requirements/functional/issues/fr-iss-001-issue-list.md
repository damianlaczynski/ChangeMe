---
id: FR-ISS-001
title: Issue List
domain: issues
type: functional
status: active
depends_on: [FR-AUTH-001, FR-ISS-002, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to browse all issues, search, filter, sort, navigate to details, manage watches, and quickly start creating a new issue.

## Functional requirements

### Access

- Screen: **Issues list**
- Available only to authenticated users. Guests are redirected to **Login** (FR-AUTH-001).

### Search and actions bar

- **Add issue** button opens **Create issue** (FR-ISS-002).

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

- Inherits `FR-UI-001` (**Administrative list screens**) for filters panel, applied filter chips, pagination, loading, and overflow menu visibility unless stated below.
- **Status** filter: multi-select; empty selection means no restriction.
- **Priority** filter: multi-select; empty selection means no restriction.
- **Assigned to** filter: single-select user list from assignable users (FR-USR-005); clearable.
- **Watched by me** filter: checkbox; when selected, shows only issues watched by the current user.
- **My issues** filter: checkbox; when selected, shows only issues **created by** or **assigned to** the current user.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
