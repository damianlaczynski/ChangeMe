# Permission catalog

> Canonical permission names used across all functional specifications. Full catalog: `docs/requirements/functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md` (`FR-ROL-001`).

The catalog contains exactly these permissions:

| Permission             | Label (exact)        | Description                                                                   | Group    |
| ---------------------- | -------------------- | ----------------------------------------------------------------------------- | -------- |
| **Users.View**         | View users           | View the users list, user details, and read-only role badges on user screens. | Users    |
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

## Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- Users with **Deactivated** true cannot sign in and have no effective permissions (FR-USR-005).
- Access denial message: **`You do not have permission to perform this action.`**

## Out of scope

Issue-level permissions are not defined yet. Issue actions inside a project workspace remain available to authenticated users who can access the project (FR-PRJ-001) until a separate REQ introduces issue permissions.
