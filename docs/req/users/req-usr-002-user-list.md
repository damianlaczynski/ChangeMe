---
id: REQ-USR-002
title: User List
domain: users
status: active
depends_on: [REQ-AUTH-011, REQ-INV-001, REQ-INV-005, REQ-USR-005]
---
## Goal

An authorized administrator must be able to browse users, search and filter them, and open administration flows.

## Features

### Search and actions bar

- Screen: **Users list**
- Sidebar entry **Users** is visible only with permission **Users.View**.
- **Invite user** button opens **Invite user** (REQ-INV-001); visible only with permission **Users.Manage**.

### Users table

| Column             | Description                                                                                                                                         |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**           | **First name** and **Last name** when set; **`—`** when both empty; link to **User details**.                                                       |
| **Email**          | User email address.                                                                                                                                 |
| **Status**         | **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** (REQ-INV-005). Replaces **Account**; **Account state** column removed. |
| **Email verified** | Badge **`Verified`** or **`Unverified`** when email verification is enabled (REQ-AUTH-011); omitted when verification is disabled.                  |
| **Roles**          | One status badge per assigned role showing the role name.                                                                                           |
| **Last sign-in**   | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions.                                                |
| **Created at**     | Account creation date and time.                                                                                                                     |
| **Actions**        | Overflow menu (see below).                                                                                                                          |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Status** multi-select: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`** (REQ-INV-005). Empty selection means no restriction. Replaces the former **Account** filter.
- **Email verified** multi-select: **`Verified`**, **`Unverified`**. Shown only when email verification is enabled (REQ-AUTH-011). Empty selection means no restriction.
- Default filter state: **no status restriction**; **no email verified restriction** when that filter is shown.
- Filters combine with search text using **AND** logic.
- **Apply filters** submits filters with the current search text.
- **Clear filters** resets the filter form and removes all filter constraints from the active query.
- Applied filters list

### Row overflow menu

| Action           | Permission required  | Behavior                                                    |
| ---------------- | -------------------- | ----------------------------------------------------------- |
| **Open details** | **Users.View**       | Opens **User details**.                                     |
| **Edit**         | **Users.Manage**     | Opens **Edit user**.                                        |
| **Deactivate**   | **Users.Deactivate** | Shown only when **Deactivated** is **false** (REQ-USR-005). |
| **Activate**     | **Users.Deactivate** | Shown only when **Deactivated** is **true** (REQ-USR-005).  |

- Menu actions the current user lacks permission for are **not shown**.

### Pagination

- The users table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search, filters, and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area.

### Permissions and visibility

- **Users.View**: required for **Users list** and **Open details**.
- **Users.Manage**: required for **Invite user** and **Edit**.
- **Users.Deactivate**: required for **Deactivate** and **Activate**.

---
