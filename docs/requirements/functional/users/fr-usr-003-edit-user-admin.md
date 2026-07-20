---
id: FR-USR-003
title: Create and Edit User (Admin)
domain: users
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-008, FR-ROL-001, FR-ROL-005]
inherits_conventions: [STD-ACC-001, STD-FRM-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to create a new user with a password and role assignments, and update an existing user's profile, role assignments, and deactivation state.

## Functional requirements

### Authorization

- **Users.Manage**: required to create and edit user profile fields.
- **Roles.Manage**: required to view and edit role assignments and to create users.
- **Users.Deactivate**: required to view and edit the **Deactivated** field on edit.

### Data

**Create user:**

| Field                | Constraints                                                                       |
| -------------------- | --------------------------------------------------------------------------------- |
| **First name**       | **Required**; max **100** characters.                                             |
| **Last name**        | **Required**; max **100** characters.                                             |
| **Email**            | **Required**; valid email; max **320** characters; unique.                        |
| **Password**         | **Required**; follows password policy (FR-AUTH-008).                              |
| **Confirm password** | **Required**; must match **Password**.                                            |
| **Roles**            | Per FR-ROL-005; visible and editable only with **Roles.Manage**.                  |

- New users are created with **Deactivated** false; **Deactivated** is **not** set on create.

**Edit user:**

| Field           | Constraints                                                                               |
| --------------- | ----------------------------------------------------------------------------------------- |
| **First name**  | **Required**; max **100** characters.                                                     |
| **Last name**   | **Required**; max **100** characters.                                                     |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                |
| **Roles**       | Per FR-ROL-005; visible and editable only with **Roles.Manage**.                          |
| **Deactivated** | Editable only with **Users.Deactivate**; when true, account status is **Deactivated**.    |

- **Password** is set only on create, not on edit.

**Permissions preview** (when **Roles** field is visible):

- Shows the union of permissions from currently selected roles (FR-ROL-001), grouped by **Users**, **Roles**, **Sessions**, with the granting role names per permission.
- Updates immediately when the **Roles** selection changes, before save.

### Operations

- Create a user with profile, password, and role assignments.
- Edit profile, role assignments, and deactivation state.
- Role assignment rules per FR-ROL-005.

### Validation

- Duplicate email: rejection message **`A user with this email already exists.`**
- **Roles**: per FR-ROL-005.
- Other field errors follow field constraints above.

### Business rules

- An administrator **cannot** remove their own **Administrator** role assignment; rejection message: **`You cannot remove your own administrator access.`**
- When an administrator edits their own account, the **Roles** field and permissions preview are hidden.
- An administrator **cannot** set their own **Deactivated** to **true**; rejection message: **`You cannot deactivate your own account.`**
- When **Email** is changed on save, revoke **all active sessions** for that user.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
