---
id: FR-ROL-003
title: Create and Edit Role
domain: access
type: functional
status: active
depends_on: [FR-ROL-001, FR-ROL-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to create custom roles and edit their name, description, and permissions.

## Functional requirements

### Create role screen

- Screen: **Create role**
- Requires permission **Roles.Manage**.

| Field           | Behavior                                                                                                          |
| --------------- | ----------------------------------------------------------------------------------------------------------------- |
| **Name**        | Text field, **required**; **2–100** characters; unique case-insensitive.                                          |
| **Description** | Multiline text area, **not required**; max **500** characters; empty when omitted.                                |
| **Permissions** | Checkbox list grouped by **Users**, **Roles**, **Sessions** (FR-ROL-001); at least **one** checkbox **required**. |

- Each checkbox shows permission **label** and **description** from FR-ROL-001.

### Edit role screen

- Screen: **Edit role**
- Requires permission **Roles.Manage**.
- Available only for **custom** roles.
- Same fields and rules as **Create role**, pre-filled with current role data.

### System role edit restriction

- **Administrator** and **User** system roles **cannot** be edited.
- Navigating to **Edit role** for a system role opens **Role details** in read-only mode with message **`System roles cannot be modified.`** and **Back** to **Roles list**.

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation, **Back** / **Cancel** defaults, and form-area loading unless stated below.
- **Back** / **Cancel**: **Roles list** when creating; **Role details** when editing.
- **Name**: required; **2–100** characters; unique case-insensitive; inline error on duplicate: **`A role with this name already exists.`**
- **Description**: max **500** characters when not empty; inline error: **`Description cannot exceed 500 characters.`**
- **Permissions**: at least one selected; form-level error: **`At least one permission is required.`**

### Form actions

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

- Creating or editing a role does **not** change user assignments; assignments are managed on **Invite user** / **Edit user** and **Remove from role** on **Role details** (FR-ROL-005).
- Permission changes on a role take effect for assigned users after their next credential renewal or sign-in.

### Permissions and visibility

- **Roles.Manage**: required to open **Create role**, **Edit role**, and **Delete role**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
