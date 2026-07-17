---
id: FR-ROL-005
title: Role and User Assignments
domain: access
type: functional
status: active
depends_on: [FR-ROL-004, FR-USR-003, FR-USR-004, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to assign roles to users and remove users from roles using consistent rules from either user or role administration.

## Functional requirements

### Authorization

- **Roles.Manage**: required to assign roles on user create/edit and to remove users from a role.
- **Roles.View**: allows read-only role and permission display when the user lacks **Roles.Manage**.

### Data

- Every user has at least one role at all times.
- Role assignment replaces the user's entire role set with the selected set on save.
- All roles are available for selection, sorted by **name** ascending.

### Operations

- Assign roles when creating or editing a user (FR-USR-003).
- View assigned roles and effective permissions read-only on user details (FR-USR-004).
- Remove a user from a role on role details (FR-ROL-004).
- Role changes are made only through user edit, not from user details.

### Validation

- **Roles** on user create/edit: at least one role selected; rejection message: **`At least one role is required.`**
- **Remove from role**: must not leave the user with zero roles; rejection message: **`Each user must have at least one role. Assign another role before removing this one.`**
- Self role change from any entry point is rejected; rejection message: **`You cannot change your own roles.`**

### Business rules

- Without **Roles.Manage**, user create is **not available** (every new user must receive role assignment at creation).
- Standard users cannot change their own roles; the **Roles** field is hidden when an administrator edits their own account.
- Assignment changes use the same replace semantics and validation rules from either entry point.
- Changes take effect for the affected user after credential renewal or next sign-in.
- Changing role assignments does **not** revoke active sessions.
- A user with **Deactivated** true can remain assigned to a role; deactivation is controlled in FR-USR-005.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
