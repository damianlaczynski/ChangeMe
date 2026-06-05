---
id: REQ-ROL-005
title: Role and User Assignments
domain: access
status: active
depends_on: [REQ-INV-001, REQ-ROL-004, REQ-USR-003, REQ-USR-004, REQ-USR-005]
---
## Goal

An authorized administrator must be able to assign roles to users from **Invite user** and **Edit user**, and remove a user from a role from **Role details**, using consistent rules in both places.

## Features

### Entry point — Users administration

- Role assignment for a user is performed on **Invite user** (REQ-INV-001) and **Edit user** (REQ-USR-003); there is no separate role-assignment screen under **Users**.
- **Roles** multi-select lists all roles sorted by **name** ascending; each option shows role **name** and **System** badge when applicable.
- At least **one** role must be selected before save.
- Saving **Invite user** or **Edit user** replaces the user's entire role set with the selected set.
- The **Roles** field is visible and editable only with permission **Roles.Manage**.
- Without **Roles.Manage**, **Invite user** is **not available** (every new user must receive role assignment at invite).
- **Permissions** preview on create/edit follows REQ-USR-003.

- **User details** (REQ-USR-004) shows assigned roles and effective permissions read-only; role badges link to **Role details**.
- Role changes are made only through **Edit user**, not from **User details**.

### Entry point — Roles administration

- **Role details** (REQ-ROL-004) shows the **Assigned users** section and **Remove from role** per user.
- Adding a user to a role is done by editing the user (**Edit user**) and selecting the role in the **Roles** multi-select.

### Validation

- **Roles** (on **Invite user** and **Edit user**): at least one role selected; error: **`At least one role is required.`**
- **Remove from role** (on **Role details**): must not leave the user with zero roles; error: **`Each user must have at least one role. Assign another role before removing this one.`**

### Self-service restrictions

- Standard users cannot change their own roles.
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field and **Permissions** preview are **not shown**.
- Self role change from any entry point is rejected with message **`You cannot change your own roles.`**

### States and business rules

- Assignment changes from either entry point use the same replace semantics and validation rules.
- Changes take effect for the affected user after credential renewal or next sign-in.
- Changing role assignments does **not** revoke active sessions.
- A user with **Deactivated** true can remain assigned to a role; deactivation is controlled in REQ-USR-005.

### Permissions and visibility

- **Roles.Manage**: required for the **Roles** field on **Invite user** / **Edit user** and for **Remove from role** on **Role details**.
- **Roles.View**: allows read-only **Roles** and **Permissions** on **User details** and read-only **Assigned users** on **Role details** when the user lacks **Roles.Manage**.

---
