---
id: FR-ROL-001
title: Permission Catalog and Effective Permissions
domain: access
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-002, FR-ROL-005, FR-USR-001, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The system must define a fixed set of permissions and determine each user's effective permissions from their assigned roles.

## Functional requirements

### Permission catalog

The catalog contains exactly these permissions:

| Permission             | Label (exact)        | Description                                                                   | Group    |
| ---------------------- | -------------------- | ----------------------------------------------------------------------------- | -------- |
| **Users.View**         | View users           | View the users list, user details, and read-only role badges on user screens. |
| **Users.Manage**       | Manage users         | Create and edit user profile data (name, email).                              | Users    |
| **Users.Deactivate**   | Deactivate users     | Deactivate and reactivate user accounts.                                      | Users    |
| **Roles.View**         | View roles           | View the roles list and role details.                                         | Roles    |
| **Roles.Manage**       | Manage roles         | Create, edit, and delete custom roles; manage role and user assignments.      | Roles    |
| **Sessions.ViewOwn**   | View own sessions    | View the current user's active sessions on **My account**.                    | Sessions |
| **Sessions.ManageOwn** | Manage own sessions  | Revoke non-current own sessions and use **Sign out everywhere**.              | Sessions |
| **Sessions.ViewAny**   | View user sessions   | View active sessions of any user in **User details**.                         | Sessions |
| **Sessions.ManageAny** | Manage user sessions | Revoke sessions of any user, including **Revoke all sessions**.               | Sessions |

- New permissions are added only by updating requirements and a subsequent release; administrators cannot create new permission codes in the UI.
- **Out of scope:** issue-level permissions. Issues remain available to all authenticated users with **Deactivated** false until a separate functional specification introduces issue permissions.

### Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- After sign-in, registration, or credential renewal (FR-AUTH-001, FR-AUTH-002), the user receives their current effective permission set.
- **My account** (FR-USR-001) reflects the current effective permission set when opened.
- After an administrator changes role assignments (FR-ROL-005), the new permissions apply when the affected user next renews credentials or signs in again.

### Access denial

- A signed-in user who lacks a required permission cannot perform the protected action; the system rejects the action with message **`You do not have permission to perform this action.`**
- A guest cannot perform actions that require sign-in.

### States and business rules

- A role with zero permissions cannot be saved; validation error: **`At least one permission is required.`**
- Users with **Deactivated** true cannot sign in and have no effective permissions (FR-USR-005).

### Permissions and visibility

- Permission names in FR-ROL-001 are used across **Users**, **Roles**, **Auth**, and **Sessions** specifications to control screen and action visibility.

---

## Acceptance scenarios

| ID            | Given                                                                                                                | When                                                                                                    | Then                                                                                                                                                                         |
| ------------- | -------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-ROL-001-01 | Administrator with **Roles.Manage** on **Create role**                                                               | User views the **Permissions** field                                                                    | Checkbox list shows only the nine catalog permissions from FR-ROL-001, grouped by **Users**, **Roles**, and **Sessions**; there is no control to add custom permission codes |
| AC-ROL-001-02 | Signed-in user assigned **Role A** with **Users.View** and **Role B** with **Roles.View**                            | User's effective permissions are evaluated                                                              | Effective set is the union of both roles' permissions with no duplicates                                                                                                     |
| AC-ROL-001-03 | Signed-in user with assigned roles completes sign-in, registration, or credential renewal (FR-AUTH-001, FR-AUTH-002) | Session is established                                                                                  | User receives the current effective permission set derived from assigned roles                                                                                               |
| AC-ROL-001-04 | Signed-in user with assigned roles opens **My account** (FR-USR-001)                                                 | Screen loads                                                                                            | Read-only effective permissions reflect the current union of permissions from all assigned roles                                                                             |
| AC-ROL-001-05 | Signed-in user is active; administrator changes that user's role assignments on **Edit user** (FR-ROL-005)           | Affected user attempts a newly granted or revoked protected action before credential renewal or sign-in | Previous effective permissions still apply until the user next renews credentials or signs in again                                                                          |
| AC-ROL-001-06 | Signed-in user without the permission required for a protected action (for example **Roles.Manage**)                 | User triggers that action                                                                               | Action is rejected with **`You do not have permission to perform this action.`**                                                                                             |
| AC-ROL-001-07 | Guest (not signed in)                                                                                                | User attempts an action that requires sign-in (for example opening **Roles list**)                      | Action is denied; user is redirected to **Login** or blocked per FR-AUTH-001                                                                                                 |
| AC-ROL-001-08 | Administrator with **Roles.Manage** on **Create role** or **Edit role**; no permission checkboxes selected           | User submits the form                                                                                   | Save is rejected with form-level error **`At least one permission is required.`**                                                                                            |
| AC-ROL-001-09 | User account with **Deactivated** true (FR-USR-005)                                                                  | User attempts **Login**                                                                                 | Sign-in is rejected; user has no effective permissions                                                                                                                       |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
