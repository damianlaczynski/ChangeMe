---
id: FR-USR-001
title: My Account Profile
domain: users
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-003, FR-AUTH-004, FR-ROL-001]
inherits_conventions: [STD-ACC-001, STD-FRM-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The signed-in user must be able to view their own profile, edit name fields, and manage their sessions.

## Functional requirements

### Authorization

- Any authenticated user with **Deactivated** false can view and edit their own profile name fields.
- **Sessions.ViewOwn** and **Sessions.ManageOwn**: required for session management (FR-AUTH-004, FR-AUTH-003).
- **Roles.View**: required to link assigned roles to role details.

### Data

Read-only profile:

| Field            | Notes                           |
| ---------------- | ------------------------------- |
| **First name**   | Editable on edit profile.       |
| **Last name**    | Editable on edit profile.       |
| **Email**        | Read-only on profile view.      |
| **Member since** | Account creation date and time. |

Editable on **Edit profile**:

| Field          | Constraints                           |
| -------------- | ------------------------------------- |
| **First name** | **Required**; max **100** characters. |
| **Last name**  | **Required**; max **100** characters. |

- **Roles**: read-only list of assigned role names.
- **Permissions**: read-only effective permission set (FR-ROL-001).
- Admin-only metadata such as **Password last changed at** is **not shown** (see FR-USR-004).

### Operations

- View own profile, assigned roles, and effective permissions.
- Edit **First name** and **Last name**.
- Manage active sessions (FR-AUTH-004).
- Sign out everywhere (FR-AUTH-003).

### Business rules

- **My account** does **not** expose role assignment or account status changes.
- Profile save success message: **`Profile updated.`**

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
