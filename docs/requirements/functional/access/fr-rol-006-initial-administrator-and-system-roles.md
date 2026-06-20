---
id: FR-ROL-006
title: Initial Administrator and System Roles
domain: access
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-ROL-001,
    FR-ROL-003,
    FR-ROL-004,
    FR-ROL-005,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When the application is first deployed, the system must provide seeded system roles and a first administrator account so user and role administration can begin without manual data preparation.

## Functional requirements

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
- The first administrator signs in through **Login** (FR-AUTH-001) and can access **Users list**, **Roles list**, and session administration per their permissions.

### Seeded system roles

On first startup, the system ensures these roles exist:

| Role              | **System** badge | Permissions                                        |
| ----------------- | ---------------- | -------------------------------------------------- |
| **Administrator** | Yes              | All permissions from FR-ROL-001.                   |
| **User**          | Yes              | **Sessions.ViewOwn**, **Sessions.ManageOwn** only. |

- If the **Administrator** role already exists, the system adds any newly defined catalog permissions that role does not yet have.
- System roles follow edit, delete, and assignment rules from FR-ROL-003, FR-ROL-004, and FR-ROL-005.

### States and business rules

- Initial administrator **Password** values must not appear in user-visible logs or messages.
- Production deployments must use a strong, unique password for the initial administrator.
- New users created by administrators receive role assignments per FR-ROL-005; they do **not** receive **Administrator** unless explicitly selected.

### Out of scope

- **Out of scope:** forced password change on first sign-in (including the seeded administrator).

### Permissions and visibility

- The seeded **Administrator** role grants all permissions from FR-ROL-001, including **Roles.Manage** and **Users.Manage**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
