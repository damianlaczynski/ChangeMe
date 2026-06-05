---
id: REQ-USR-003
title: Edit User (Admin)
domain: users
status: active
depends_on: [REQ-AUTH-007, REQ-AUTH-011, REQ-AUTH-014, REQ-AUTH-015, REQ-INV-001, REQ-ROL-001, REQ-ROL-005]
---
## Goal

An authorized administrator must be able to update an existing user's profile, role assignments, and deactivation state.

**Invite user** (new account + first invitation): REQ-INV-001.

## Features

### Permissions preview (edit)

- Below the **Roles** field, a read-only section **Permissions** shows the union of permissions from the currently selected roles (REQ-ROL-001).
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

### Edit user screen

- Screen: **Edit user**
- Requires permission **Users.Manage**.

| Field           | Behavior                                                                                                                            |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **First name**  | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Last name**   | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                                                          |
| **Roles**       | Same rules as create (REQ-ROL-005); visible and editable only with **Roles.Manage**.                                                |
| **Deactivated** | Checkbox; editable only with **Users.Deactivate**; label **`Deactivated`**. When checked, **Status** becomes **`Deactivated`**.     |

- **Password** fields are **not shown** on **Edit user**.
- When **External providers enabled** is **true** and the edited user has at least one **External login**, show persistent notice: **`External sign-in stays linked. Profile email is used for notifications; provider addresses may differ.`** (REQ-AUTH-014).
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.

### Validation

- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per REQ-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Back** button and **Cancel** button navigate to **User details** without saving.
- **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Permissions** preview is **not shown**.
- An administrator **cannot** set their own **Deactivated** to **true**; save is rejected with message **`You cannot deactivate your own account.`**
- When **Email** is changed on save:
  - cancel any **pending email change** on that user (REQ-AUTH-015);
  - apply the new **Email** immediately as **current email**;
  - set **Email verified** true and **Email verified at** to the current date and time when email verification is enabled (REQ-AUTH-011);
  - revoke **all active sessions** for that user;
  - send **Email changed by admin** to the previous **current email** and to the new **Email** (REQ-AUTH-007).
- When **Email** is unchanged on save, admin email rules above do **not** run.

### Permissions and visibility

- **Users.Manage**: required to open **Edit user** and save profile fields.
- **Roles.Manage**: required to view and edit the **Roles** field.
- **Users.Deactivate**: required to view and edit the **Deactivated** field on **Edit user**.

---
