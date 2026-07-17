---
id: FR-ROL-001
title: Permission Catalog and Effective Permissions
domain: access
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-002, FR-ROL-005, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The system must define a fixed set of permissions and determine each user's effective permissions from their assigned roles.

## Functional requirements

### Data

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

### Operations

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- After sign-in or credential renewal (FR-AUTH-001, FR-AUTH-002), the user receives their current effective permission set.
- After an administrator changes role assignments (FR-ROL-005), the new permissions apply when the affected user next renews credentials or signs in again.
- A signed-in user who lacks a required permission cannot perform the protected action; rejection message: **`You do not have permission to perform this action.`**
- A guest cannot perform actions that require sign-in.

### Validation

- A role with zero permissions cannot be saved; rejection message: **`At least one permission is required.`**

### Business rules

- Users with **Deactivated** true cannot sign in and have no effective permissions (FR-USR-005).
- Permission names in FR-ROL-001 control capability access across **Users**, **Roles**, **Auth**, and **Sessions** specifications.

## Out of scope

- Issue-level permissions. Issues remain available to all authenticated users with **Deactivated** false until a separate functional specification introduces issue permissions.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
