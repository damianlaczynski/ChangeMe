---
id: REQ-PRJ-005
title: Project RBAC and Effective Permissions
domain: projects
status: active
depends_on: [REQ-ROL-001]
---

## Goal

The system must define a fixed set of **resource-scoped** project permissions, assign each project member exactly one **project role**, and evaluate access to project screens and actions against the target project.

## Features

### Resource-scoped evaluation

- Project permissions are **not** global application permissions from REQ-ROL-001.
- Every project action is authorized against the **target project** and the acting user's **project role** on that project.
- A user without membership on a project has **no** project permissions on that project.
- Access denial message: **`You do not have permission to perform this action on this project.`**

### Project permission catalog

The project permission catalog contains exactly these permissions:

| Permission                 | Label (exact)          | Description                                                                   |
| -------------------------- | ---------------------- | ----------------------------------------------------------------------------- |
| **Project.View**           | View project           | View **Project details**, project summaries, and project history.             |
| **Project.Manage**         | Manage project         | Edit project **name** and **description**; delete the project.                |
| **Project.Members.View**   | View project members   | View the members list and **Membership history**.                             |
| **Project.Members.Manage** | Manage project members | Add members, remove members, and change member roles.                         |
| **Project.Issues.View**    | View project issues    | View issues that belong to the project.                                       |
| **Project.Issues.Manage**  | Manage project issues  | Create, edit, and delete issues in the project.                               |
| **Project.Time.Log**       | Log project time       | Create time entries and use the running timer for this project (REQ-TIM-002). |
| **Project.Time.View**      | View project time      | View all time entries for this project and its issues (REQ-TIM-003).          |
| **Project.Time.Manage**    | Manage project time    | Edit and delete any user's time entries for this project (REQ-TIM-004).       |

- New project permissions are added only by updating requirements and a subsequent release; administrators cannot create new project permission codes in the UI.

### Project roles

Each project member has exactly one project role on that project:

| Project role | Permissions                                                                                                                                  |
| ------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Owner**    | All permissions from the project permission catalog.                                                                                         |
| **Member**   | **Project.View**, **Project.Members.View**, **Project.Issues.View**, **Project.Issues.Manage**, **Project.Time.Log**, **Project.Time.View**. |
| **Viewer**   | **Project.View**, **Project.Members.View**, **Project.Issues.View**, **Project.Time.View**.                                                  |

- Project roles are fixed; custom project roles are **not** supported.
- A user can hold different project roles on different projects.

### Effective project permissions

- A user's effective permissions on a project are determined solely by their current **project role** on that project.
- After sign-in or credential renewal (REQ-AUTH-001, REQ-AUTH-002), project membership and roles are available for authorization checks.
- Changing a member's project role takes effect immediately for subsequent actions on that project.

### Project creation membership

- When a user creates a custom project (REQ-PRJ-003), the system adds the creator as an **Owner** on that project automatically.
- The **Default** system project (REQ-PRJ-001) includes every active user (**Deactivated** false) as a **Member** automatically when:
  - the project is first seeded, or
  - the user account becomes active (registration, invitation acceptance, or reactivation).

### States and business rules

- Deactivated users (**Deactivated** true) are not added to the **Default** project and lose effective project permissions because they cannot sign in (REQ-USR-005).
- A project must have at least one **Owner** at all times.
- Global permissions from REQ-ROL-001 do **not** grant project permissions unless the user is also a project member with a role that includes the required permission.

### Out of scope for this REQ

- Issue-level permissions separate from project membership.
- Custom per-project permission sets outside the three fixed project roles.
