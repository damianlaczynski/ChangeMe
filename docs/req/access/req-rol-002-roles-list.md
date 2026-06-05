---
id: REQ-ROL-002
title: Roles List
domain: access
status: active
depends_on: [REQ-ROL-003]
---
## Goal

An authorized administrator must be able to browse roles, search and sort them, and open role administration flows.

## Features

### Search and actions bar

- Screen: **Roles list**
- Sidebar entry **Roles** is visible only with permission **Roles.View**.
- Search field placeholder: **`Search roles...`**
- Search matches **name** or **description** fragment (case-insensitive).
- **Search** button and form submit apply the current search text.
- **Add role** button opens **Create role** (REQ-ROL-003); visible only with permission **Roles.Manage**.

### Roles table

| Column          | Description                                                            |
| --------------- | ---------------------------------------------------------------------- |
| **Name**        | Role name; link to **Role details**.                                   |
| **Description** | Role description text, or em dash (**`—`**) when empty.                |
| **Permissions** | Exact format: **`{n} permissions`** where `{n}` is the assigned count. |
| **Users**       | Exact format: **`{n} users`** where `{n}` is the assignment count.     |
| **System**      | Badge **`System`** for seeded roles; blank for custom roles.           |
| **Actions**     | Overflow menu (see below).                                             |

### Sorting

- Sortable columns: **Name**, **Users**, **Permissions**.
- Default sort: **Name**, ascending.

### Row overflow menu

| Action           | Permission required | Behavior                                                               |
| ---------------- | ------------------- | ---------------------------------------------------------------------- |
| **Open details** | **Roles.View**      | Opens **Role details**.                                                |
| **Edit role**    | **Roles.Manage**    | Opens **Edit role** for custom roles only.                             |
| **Delete role**  | **Roles.Manage**    | Shown for custom roles only; confirmation and behavior in REQ-ROL-003. |

- Menu actions the current user lacks permission for are **not shown**.
- **Edit role** and **Delete role** are **not shown** for system roles (**Administrator**, **User**); system roles are opened via **Open details** only.

### Pagination

- The roles table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area.

### Permissions and visibility

- **Roles.View**: required for **Roles list** and **Open details**.
- **Roles.Manage**: required for **Add role**, **Edit role**, and **Delete role**.

---
