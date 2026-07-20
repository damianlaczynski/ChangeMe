---
id: FR-USR-004
title: User Details and Session Administration
domain: users
type: functional
status: active
depends_on: [FR-AUTH-004, FR-ROL-001, FR-ROL-004, FR-USR-003, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-LST-002, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to inspect a user's account, roles, effective permissions, and active sessions and revoke sessions when needed.

## Functional requirements

### Authorization

- **Users.View**: required to view user profile, roles, and permissions.
- **Users.Manage**: required to edit the user.
- **Users.Deactivate**: required to deactivate or activate the user.
- **Sessions.ViewAny**: required to view active sessions.
- **Sessions.ManageAny**: required to revoke sessions.

### Data

| Field                          | Notes                                                                            |
| ------------------------------ | -------------------------------------------------------------------------------- |
| **First name** / **Last name** | Read-only.                                                                       |
| **Email**                      | Account email address.                                                           |
| **Status**                     | **Active** or **Deactivated**.                                                   |
| **Member since**               | Account creation date and time.                                                  |
| **Last sign-in**               | Most recent session **signed in at**; **`Never`** when the user has no sessions. |
| **Password last changed at**   | Date and time; **`—`** when not yet recorded.                                    |
| **Deactivated at**             | Set when **Deactivated** is **true**; cleared when **false**.                    |

- **Roles**: assigned role names; each links to role details (FR-ROL-004) when permitted.
- **Permissions**: read-only effective permission set (union of assigned roles, FR-ROL-001), with granting role names per permission.
- **Active sessions**: same attributes as FR-AUTH-004 except the **current session** indicator is not shown in the administrator view.
- Inherits STD-LST-002 unless stated below.

### Operations

- View user profile, roles, effective permissions, and active sessions.
- Edit user profile, roles, and deactivation (FR-USR-003).
- Deactivate or activate the user (FR-USR-005).
- Revoke a single session or all active sessions for the user after confirmation.
- Open role details from an assigned role.

### Validation

- **Revoke session**: confirmation message **`Revoke this session? That device will be signed out.`**
- **Revoke all sessions**: confirmation message **`Revoke all active sessions for this user? They will be signed out on every device.`**

### Business rules

- Role changes are made only through user edit (FR-USR-003), not from user details.
- Users with **Deactivated** true display status **Deactivated**; the active sessions list is empty.
- Revoking a session signs out that device on next activity.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
