---
id: FR-USR-003
title: Create and Edit User (Admin)
domain: users
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-008, FR-ROL-001, FR-ROL-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to create a new user with a password and role assignments, and update an existing user's profile, role assignments, and deactivation state.

## Functional requirements

### Permissions preview (create and edit)

- Below the **Roles** field, a read-only section **Permissions** shows the union of permissions from the currently selected roles (FR-ROL-001).
- Each permission row shows:
  - **Label** and **description** from the catalog;
  - **From roles** — comma-separated list of selected role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one selected role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**.
- The preview updates immediately when the **Roles** selection changes, before save.
- When no role is selected, the section shows: **`Select at least one role to preview permissions.`**
- When the selected roles grant no permissions, the section shows: **`No permissions.`**
- The section is read-only; it does not replace the **Roles** field for assignment.
- The preview is shown only when the **Roles** field is visible (requires **Roles.Manage**).

### Create user screen

- Screen: **Create user**
- Requires permission **Users.Manage** and **Roles.Manage** (FR-ROL-005).

| Field                | Behavior                                                                          |
| -------------------- | --------------------------------------------------------------------------------- |
| **First name**       | Text field, **required**; max **100** characters.                                 |
| **Last name**        | Text field, **required**; max **100** characters.                                 |
| **Email**            | Text field, **required**; valid email; max **320** characters; must be unique.    |
| **Password**         | Password field, **required**; **8–128** characters.                               |
| **Confirm password** | **Required**; must match **Password**.                                            |
| **Roles**            | Same rules as edit (FR-ROL-005); visible and editable only with **Roles.Manage**. |

- Password rules follow **Password policy** (FR-AUTH-008).
- **Deactivated** is **not** shown on **Create user**; new users are created with **Deactivated** false.

### Edit user screen

- Screen: **Edit user**
- Requires permission **Users.Manage**.

| Field           | Behavior                                                                                                                        |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **First name**  | **Required**; max **100** characters.                                                                                           |
| **Last name**   | **Required**; max **100** characters.                                                                                           |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                                                      |
| **Roles**       | Same rules as create (FR-ROL-005); visible and editable only with **Roles.Manage**.                                             |
| **Deactivated** | Checkbox; editable only with **Users.Deactivate**; label **`Deactivated`**. When checked, **Status** becomes **`Deactivated`**. |

- **Password** fields are **not shown** on **Edit user**; password is set only on **Create user**.
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation, **Back** / **Cancel** → **User details** (edit) or **Users list** (create), and form-area loading unless stated below.
- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per FR-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Create user** — **Create user** button: on success show message **`User created.`** and open **User details** for the new user.
- **Edit user** — **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Permissions** preview is **not shown**.
- An administrator **cannot** set their own **Deactivated** to **true**; save is rejected with message **`You cannot deactivate your own account.`**
- When **Email** is changed on save, revoke **all active sessions** for that user.

### Permissions and visibility

- **Users.Manage**: required to open **Create user** / **Edit user** and save profile fields.
- **Roles.Manage**: required to view and edit the **Roles** field and to open **Create user**.
- **Users.Deactivate**: required to view and edit the **Deactivated** field on **Edit user**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
