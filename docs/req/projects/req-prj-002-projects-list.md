---
id: REQ-PRJ-002
title: Projects List
domain: projects
status: active
depends_on: [REQ-PRJ-003, REQ-PRJ-005]
---

## Goal

An authenticated user must be able to browse projects they belong to, search and sort them, and open project administration flows.

## Features

### Access

- Screen: **Projects list**
- Available only to authenticated users.
- Sidebar entry **Projects** is visible to every authenticated user.
- The list shows only projects where the current user is a member with **Project.View**.

### Search and actions bar

- Search field placeholder: **`Search projects...`**
- Search matches **name** or **description** fragment (case-insensitive) within the visible project set.
- **Search** button and form submit apply the current search text.
- **Add project** button opens **Create project** (REQ-PRJ-003); visible to every authenticated user.

### Projects table

| Column          | Description                                                                       |
| --------------- | --------------------------------------------------------------------------------- |
| **Name**        | Project name; link to **Project details**.                                        |
| **Description** | Project description text, or em dash (**`—`**) when empty.                        |
| **Issues**      | Exact format: **`{n} issues`** where `{n}` is the issue count.                    |
| **My role**     | Current user's project role badge: **Owner**, **Member**, or **Viewer**.          |
| **System**      | Badge **`System`** for the seeded **Default** project; blank for custom projects. |
| **Actions**     | Overflow menu (see below).                                                        |

### Sorting

- Sortable columns: **Name**, **Issues**.
- Default sort: **Name**, ascending.

### Row overflow menu

| Action             | Permission required on project | Behavior                                                                  |
| ------------------ | ------------------------------ | ------------------------------------------------------------------------- |
| **Open details**   | **Project.View**               | Opens **Project details**.                                                |
| **Edit project**   | **Project.Manage**             | Opens **Edit project** for custom projects only.                          |
| **Delete project** | **Project.Manage**             | Shown for custom projects only; confirmation and behavior in REQ-PRJ-003. |

- Menu actions the current user lacks permission for on that project are **not shown**.
- **Edit project** and **Delete project** are **not shown** for the **Default** system project; it is opened via **Open details** only.

### Pagination

- The projects table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area.

### Empty state

- When the user is not a member of any project: **`You are not a member of any project.`**

### Permissions and visibility

- **Project.View** on a project: required to see that project in the list and use **Open details**.
- **Project.Manage** on a project: required for **Edit project** and **Delete project** on that project.
