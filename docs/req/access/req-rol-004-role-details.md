---
id: REQ-ROL-004
title: Role Details
domain: access
status: active
depends_on: [REQ-ROL-001, REQ-ROL-003, REQ-USR-004]
---
## Goal

An authorized administrator must be able to review a role's metadata, permissions, and assigned users, and navigate to related administration flows.

## Features

### Role details screen

- Screen: **Role details**
- Requires permission **Roles.View**.
- Opened from **Roles list** (**Name** link or **Open details**).

### Role summary

| Field              | Behavior                                                                   |
| ------------------ | -------------------------------------------------------------------------- |
| **Name**           | Read-only role name.                                                       |
| **Description**    | Read-only description, or em dash (**`—`**) when empty.                    |
| **System**         | Read-only badge **`System`** for seeded roles; not shown for custom roles. |
| **Permissions**    | Read-only count in format **`{n} permissions`**.                           |
| **Assigned users** | Read-only count in format **`{n} users`**.                                 |

### Permissions section

- Section title: **`Permissions`**
- Lists every permission assigned to the role.
- Each row shows permission **label** and **description** from REQ-ROL-001, grouped by **Users**, **Roles**, **Sessions**.
- Permissions are read-only on this screen; editing happens on **Edit role** (REQ-ROL-003).

### Assigned users section

- Section title: **`Assigned users`**
- Visible with permission **Roles.View**.
- Table columns:

| Column      | Description                                                                                                 |
| ----------- | ----------------------------------------------------------------------------------------------------------- |
| **Name**    | **First name** and **Last name** when set; **`—`** when both empty; link to **User details** (REQ-USR-004). |
| **Email**   | User email address.                                                                                         |
| **Account** | Badge **`Active`** or **`Deactivated`** (from **Deactivated**).                                             |
| **Actions** | **Remove from role** when the user has **Roles.Manage**.                                                    |

- Default sort within section: **Name**, ascending.
- Search field placeholder within section: **`Search assigned users...`**; filters **name** and **email** (case-insensitive).
- Empty state: **`No users are assigned to this role.`**
- While loading, a loading indicator is shown in the section.
- The assigned users table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search resets to **page 1**.

### Header actions

| Action          | Permission required | Behavior                                     |
| --------------- | ------------------- | -------------------------------------------- |
| **Edit role**   | **Roles.Manage**    | Opens **Edit role** (custom roles only).     |
| **Delete role** | **Roles.Manage**    | Custom roles only; behavior per REQ-ROL-003. |

- Actions the current user lacks permission for are **not shown**.
- **Edit role** and **Delete role** are **not shown** for system roles.

### Row action — Remove from role

- **Remove from role** opens confirmation: **`Remove "{full name}" from role "{role name}"? The user will lose permissions granted only through this role.`**
- On confirm: remove the role from that user; show message **`User removed from role.`**; refresh the assigned users list in place.
- Removal is rejected when the user would have zero roles; show message **`Each user must have at least one role. Assign another role before removing this one.`**

### Actions and navigation

- **Back** returns to **Roles list**.
- Clicking a user **Name** in **Assigned users** opens **User details** for that user.

### Permissions and visibility

- **Roles.View**: required for **Role details**, **Permissions** section, and read-only **Assigned users** list.
- **Roles.Manage**: required for **Edit role**, **Delete role**, and **Remove from role**.

---
