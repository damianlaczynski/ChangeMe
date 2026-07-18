---
id: FR-ROL-006
title: Initial Administrator and System Roles
domain: access
type: functional
status: active
depends_on: [FR-AUTH-001, FR-ROL-001, FR-ROL-003, FR-ROL-004, FR-ROL-005]
inherits_conventions: [STD-ACC-001, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

When the application is first deployed, the system must provide seeded system roles and a first administrator account so user and role administration can begin without manual data preparation.

## Functional requirements

### Data

The deployment supplies these values for the first administrator:

| Value          | Required |
| -------------- | -------- |
| **Email**      | Yes      |
| **Password**   | Yes      |
| **First name** | Yes      |
| **Last name**  | Yes      |

On first startup, the system ensures these roles exist:

| Role              | Permissions                                        |
| ----------------- | -------------------------------------------------- |
| **Administrator** | All permissions from FR-ROL-001.                   |
| **User**          | **Sessions.ViewOwn**, **Sessions.ManageOwn**, **Issues.View**, **Issues.Create**, **Issues.Comment**. |

### Operations

- On first startup, if no administrator account exists for the configured **Email**, the system creates an administrator user with **Deactivated** false, the supplied profile, and password, assigned the **Administrator** role.
- If an administrator with that **Email** already exists, the system does **not** recreate the account or reset the password.
- If the **Administrator** role already exists, the system adds any newly defined catalog permissions that role does not yet have.
- System roles follow edit, delete, and assignment rules from FR-ROL-003, FR-ROL-004, and FR-ROL-005.

### Business rules

- Initial administrator **Password** values must not appear in user-visible logs or messages.
- Production deployments must use a strong, unique password for the initial administrator.
- New users created by administrators receive role assignments per FR-ROL-005; they do **not** receive **Administrator** unless explicitly selected.
- The seeded **Administrator** role grants all permissions from FR-ROL-001.

## Out of scope

- Forced password change on first sign-in (including the seeded administrator).

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
