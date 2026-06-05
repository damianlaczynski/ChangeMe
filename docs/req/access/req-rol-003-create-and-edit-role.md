---
id: REQ-ROL-003
title: Create and Edit Role
domain: access
status: active
depends_on: [REQ-ROL-001, REQ-ROL-005]
---
## Goal

An authorized administrator must be able to create custom roles and edit their name, description, and permissions.

## Features

### Create role screen

- Screen: **Create role**
- Requires permission **Roles.Manage**.

| Field           | Behavior                                                                                                           |
| --------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Name**        | Text field, **required**; **2–100** characters; unique case-insensitive.                                           |
| **Description** | Multiline text area, **not required**; max **500** characters; empty when omitted.                                 |
| **Permissions** | Checkbox list grouped by **Users**, **Roles**, **Sessions** (REQ-ROL-001); at least **one** checkbox **required**. |

- Each checkbox shows permission **label** and **description** from REQ-ROL-001.

### Edit role screen

- Screen: **Edit role**
- Requires permission **Roles.Manage**.
- Available only for **custom** roles.
- Same fields and rules as **Create role**, pre-filled with current role data.

### System role edit restriction

- **Administrator** and **User** system roles **cannot** be edited.
- Navigating to **Edit role** for a system role opens **Role details** in read-only mode with message **`System roles cannot be modified.`** and **Back** to **Roles list**.

### Validation

- **Name**: required; **2–100** characters; unique case-insensitive; inline error on duplicate: **`A role with this name already exists.`**
- **Description**: max **500** characters when not empty; inline error: **`Description cannot exceed 500 characters.`**
- **Permissions**: at least one selected; form-level error: **`At least one permission is required.`**
- Validation errors are inline on the relevant field or form-level for **Permissions**; the form stays open on failure.

### Form actions

- **Back** button and **Cancel** button navigate to **Roles list** when creating, or to **Role details** when editing, without saving.
- **Create role** button: on success show message **`Role created.`** and open **Role details** for the new role.
- **Save changes** button: on success show message **`Role saved.`** and open **Role details** for the edited role.

### Delete role

- **Delete role** is available from **Role details** and **Roles list** overflow menu (custom roles only).
- Confirmation dialog: **`Delete role "{role name}"? Users will lose permissions granted only through this role.`**
- On confirm: show message **`Role deleted.`** and navigate to **Roles list**.
- **System roles cannot be deleted**; **Delete role** is not shown for system roles.
- A role assigned to **one or more users** cannot be deleted; show message **`Role is assigned to one or more users. Remove all user assignments before deleting this role.`**

### System role rules

| Role              | **System** badge | Editable | Deletable            | Permissions                                               |
| ----------------- | ---------------- | -------- | -------------------- | --------------------------------------------------------- |
| **Administrator** | Yes              | No       | No                   | All catalog permissions; fixed.                           |
| **User**          | Yes              | No       | No                   | **Sessions.ViewOwn**, **Sessions.ManageOwn** only; fixed. |
| Custom roles      | No               | Yes      | Yes, when unassigned | Selected from catalog at create/edit time.                |

### States and business rules

- Creating or editing a role does **not** change user assignments; assignments are managed on **Invite user** / **Edit user** and **Remove from role** on **Role details** (REQ-ROL-005).
- Permission changes on a role take effect for assigned users after their next credential renewal or sign-in.

### Permissions and visibility

- **Roles.Manage**: required to open **Create role**, **Edit role**, and **Delete role**.

---
