---
id: REQ-ROL-001
title: Permission Catalog and Effective Permissions
domain: access
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-002, REQ-ROL-005, REQ-USR-001, REQ-USR-005]
---
## Goal

The system must define a fixed set of permissions and determine each user's effective permissions from their assigned roles.

## Features

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
- **Out of scope for this REQ:** issue-level permissions. Issues remain available to all authenticated users with **Deactivated** false until a separate requirements document introduces issue permissions.

### Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- After sign-in, registration, or credential renewal (REQ-AUTH-001, REQ-AUTH-002), the user receives their current effective permission set.
- **My account** (REQ-USR-001) reflects the current effective permission set when opened.
- After an administrator changes role assignments (REQ-ROL-005), the new permissions apply when the affected user next renews credentials or signs in again.

### Access denial

- A signed-in user who lacks a required permission cannot perform the protected action; the system rejects the action with message **`You do not have permission to perform this action.`**
- A guest cannot perform actions that require sign-in.

### States and business rules

- A role with zero permissions cannot be saved; validation error: **`At least one permission is required.`**
- Users with **Deactivated** true cannot sign in and have no effective permissions (REQ-USR-005).

### Permissions and visibility

- Permission names in this REQ are used across **Users**, **Roles**, **Auth**, and **Sessions** requirements to control screen and action visibility.

---
