---
id: FR-ROL-004
title: Role Details
domain: access
type: functional
status: active
depends_on: [FR-ROL-001, FR-ROL-003, FR-USR-004]
inherits_conventions:
  [STD-ACC-001, STD-DTL-001, STD-LST-002, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to review a role's metadata, permissions, and assigned users, and manage assignments.

## Functional requirements

### Authorization

- **Roles.View**: required to view role metadata, permissions, and assigned users.
- **Roles.Manage**: required to edit, delete, and remove users from the role.

### Data

- Role summary: **name**, **description**, **system** flag, permission count, assigned user count.
- **Permissions**: full list of assigned permissions with label and description from FR-ROL-001.
- **Assigned users**: users currently assigned to the role; each entry shows name, email, and account status (**Active** / **Deactivated**).
- Inherits STD-LST-002 unless stated below.
- Assigned-users search matches **name** and **email**; default sort: **name**, ascending.

### Operations

- View role metadata and assigned permissions (read-only).
- Edit or delete a custom role (FR-ROL-003).
- Remove a user from the role after confirmation.
- Open user details for an assigned user (FR-USR-004).
- **System** roles cannot be edited or deleted.

### Validation

- **Remove from role**: must not leave the user with zero roles; rejection message: **`Each user must have at least one role. Assign another role before removing this one.`**
- **Remove from role**: confirmation message **`Remove {full name} from role {role name}? The user will lose permissions granted only through this role.`**

### Business rules

- Permissions are read-only on role details; editing happens through edit role (FR-ROL-003).
- Adding a user to a role is done by editing the user's role assignments (FR-ROL-005), not from role details.
- Successful removal shows message **`User removed from role.`**

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
