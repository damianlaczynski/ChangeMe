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
| **Projects.View**      | View projects        | View **Projects list**, open project workspaces, and browse project issues.   | Projects |
| **Projects.Manage**    | Manage projects      | Create new projects.                                                          | Projects |

- New permissions are added only by updating requirements and a subsequent release; administrators cannot create new permission codes in the UI.
- **Out of scope:** issue-level permissions. Issue actions inside a project workspace remain available to authenticated users who can access the project (FR-PRJ-001) until a separate functional specification introduces issue permissions.

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

- Permission names in FR-ROL-001 are used across **Users**, **Roles**, **Auth**, **Sessions**, and **Projects** specifications to control screen and action visibility.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
