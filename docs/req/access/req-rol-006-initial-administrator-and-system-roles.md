---
id: REQ-ROL-006
title: Initial Administrator and System Roles
domain: access
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-009, REQ-AUTH-011, REQ-AUTH-012, REQ-AUTH-013, REQ-ROL-001, REQ-ROL-003, REQ-ROL-004, REQ-ROL-005]
---
## Goal

When the application is first deployed, the system must provide seeded system roles and a first administrator account so user and role administration can begin without manual data preparation.

## Features

### Initial administrator

The deployment supplies these values for the first administrator:

| Value          | Required |
| -------------- | -------- |
| **Email**      | Yes      |
| **Password**   | Yes      |
| **First name** | Yes      |
| **Last name**  | Yes      |

- On first startup, if no administrator account exists for the configured **Email**, the system creates an administrator user with **Deactivated** false, the supplied profile, and password.
- If an administrator with that **Email** already exists, the system does **not** recreate the account or reset the password.
- The first administrator is assigned the **Administrator** role.
- The first administrator signs in through **Login** (REQ-AUTH-001) and can access **Users list**, **Roles list**, and session administration per their permissions.

### Seeded system roles

On first startup, the system ensures these roles exist:

| Role              | **System** badge | Permissions                                        |
| ----------------- | ---------------- | -------------------------------------------------- |
| **Administrator** | Yes              | All permissions from REQ-ROL-001.                  |
| **User**          | Yes              | **Sessions.ViewOwn**, **Sessions.ManageOwn** only. |

- If the **Administrator** role already exists, the system adds any newly defined catalog permissions that role does not yet have.
- System roles follow edit, delete, and assignment rules from REQ-ROL-003, REQ-ROL-004, and REQ-ROL-005.

### Registration default role

- Public registration (REQ-AUTH-001), when **Public registration enabled** is **true** (REQ-AUTH-012), assigns the **User** role automatically.
- Registration does **not** assign **Administrator**.

### States and business rules

- Initial administrator **Password** values must not appear in user-visible logs or messages.
- Production deployments must use a strong, unique password for the initial administrator; password expiration (REQ-AUTH-009) and two-factor authentication (REQ-AUTH-013) apply under the same rules as for other accounts when enabled.
- The initial administrator account is created with **Email verified** true so sign-in is possible when email verification is enabled (REQ-AUTH-011).

### Out of scope

- **Out of scope for this REQ:** forced password change on first sign-in (including the seeded administrator).

### Permissions and visibility

- The seeded **Administrator** role grants all permissions from REQ-ROL-001, including **Roles.Manage** and **Users.Manage**.
