---
id: FR-USR-003
title: Edit User (Admin)
domain: users
type: functional
status: active
depends_on:
  [
    FR-AUTH-007,
    FR-AUTH-011,
    FR-AUTH-014,
    FR-AUTH-015,
    FR-INV-001,
    FR-ROL-001,
    FR-ROL-005,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to update an existing user's profile, role assignments, and deactivation state.

**Invite user** (new account + first invitation): FR-INV-001.

## Functional requirements

### Permissions preview (edit)

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

### Edit user screen

- Screen: **Edit user**
- Requires permission **Users.Manage**.

| Field           | Behavior                                                                                                                            |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **First name**  | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Last name**   | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                                                          |
| **Roles**       | Same rules as create (FR-ROL-005); visible and editable only with **Roles.Manage**.                                                 |
| **Deactivated** | Checkbox; editable only with **Users.Deactivate**; label **`Deactivated`**. When checked, **Status** becomes **`Deactivated`**.     |

- **Password** fields are **not shown** on **Edit user**.
- When **External providers enabled** is **true** and the edited user has at least one **External login**, show persistent notice: **`External sign-in stays linked. Profile email is used for notifications; provider addresses may differ.`** (FR-AUTH-014).
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation, **Back** / **Cancel** → **User details**, and form-area loading unless stated below.
- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per FR-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Permissions** preview is **not shown**.
- An administrator **cannot** set their own **Deactivated** to **true**; save is rejected with message **`You cannot deactivate your own account.`**
- When **Email** is changed on save:
  - cancel any **pending email change** on that user (FR-AUTH-015);
  - apply the new **Email** immediately as **current email**;
  - set **Email verified** true and **Email verified at** to the current date and time when email verification is enabled (FR-AUTH-011);
  - revoke **all active sessions** for that user;
  - send **Email changed by admin** to the previous **current email** and to the new **Email** (FR-AUTH-007).
- When **Email** is unchanged on save, admin email rules above do **not** run.

### Permissions and visibility

- **Users.Manage**: required to open **Edit user** and save profile fields.
- **Roles.Manage**: required to view and edit the **Roles** field.
- **Users.Deactivate**: required to view and edit the **Deactivated** field on **Edit user**.

---

## Acceptance scenarios

| ID            | Given                                                                                                             | When                             | Then                                                                                                                                                                        |
| ------------- | ----------------------------------------------------------------------------------------------------------------- | -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-USR-003-01 | Signed-in administrator with **Users.Manage** and **Roles.Manage** on **Edit user** for another user              | User changes **Roles** selection | **Permissions** preview updates immediately before save; rows show **From role(s)** labels                                                                                  |
| AC-USR-003-02 | Signed-in administrator with **Users.Manage** but without **Roles.Manage**                                        | User opens **Edit user**         | **Roles** field and **Permissions** preview are **not shown**                                                                                                               |
| AC-USR-003-03 | Signed-in administrator editing **their own** account on **Edit user**                                            | User views the form              | **Roles** field and **Permissions** preview are **not shown**                                                                                                               |
| AC-USR-003-04 | Signed-in administrator on **Edit user** with duplicate **Email**                                                 | User clicks **Save changes**     | Form-level error **`A user with this email already exists.`**; form stays open                                                                                              |
| AC-USR-003-05 | Signed-in administrator removes own **Administrator** role on another user's record (invalid self-rule via roles) | User clicks **Save changes**     | Save rejected with **`You cannot remove your own administrator access.`**                                                                                                   |
| AC-USR-003-06 | Signed-in administrator on **Edit user** for **their own** account with **Deactivated** checked                   | User clicks **Save changes**     | Save rejected with **`You cannot deactivate your own account.`**                                                                                                            |
| AC-USR-003-07 | Signed-in administrator changes **Email** on save for a target user                                               | Save succeeds                    | Pending email change cleared; new email applied; **Email verified** set when verification enabled; all user sessions revoked; **Email changed by admin** sent (FR-AUTH-007) |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
