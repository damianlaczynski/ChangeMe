---
id: FR-ROL-006
title: Initial Administrator and System Roles
domain: access
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-009,
    FR-AUTH-011,
    FR-AUTH-012,
    FR-AUTH-013,
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

### Registration default role

- Public registration (FR-AUTH-001), when **Public registration enabled** is **true** (FR-AUTH-012), assigns the **User** role automatically.
- Registration does **not** assign **Administrator**.

### States and business rules

- Initial administrator **Password** values must not appear in user-visible logs or messages.
- Production deployments must use a strong, unique password for the initial administrator; password expiration (FR-AUTH-009) and two-factor authentication (FR-AUTH-013) apply under the same rules as for other accounts when enabled.
- The initial administrator account is created with **Email verified** true so sign-in is possible when email verification is enabled (FR-AUTH-011).

### Out of scope

- **Out of scope:** forced password change on first sign-in (including the seeded administrator).

### Permissions and visibility

- The seeded **Administrator** role grants all permissions from FR-ROL-001, including **Roles.Manage** and **Users.Manage**.

## Acceptance scenarios

| ID            | Given                                                                                                                                           | When                                                                | Then                                                                                                                                                    |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-ROL-006-01 | Fresh deployment with configured **Email**, **Password**, **First name**, and **Last name**; no administrator account exists for that **Email** | Application starts for the first time                               | Administrator user is created with **Deactivated** false, supplied profile, and **Administrator** role assigned                                         |
| AC-ROL-006-02 | Deployment where an administrator with the configured **Email** already exists                                                                  | Application starts                                                  | Account is **not** recreated and **Password** is **not** reset                                                                                          |
| AC-ROL-006-03 | Seeded initial administrator with **Administrator** role                                                                                        | User signs in via **Login** (FR-AUTH-001)                           | User can open **Users list**, **Roles list**, and session administration per granted permissions                                                        |
| AC-ROL-006-04 | Fresh deployment on first startup                                                                                                               | System seeds roles                                                  | **Administrator** role (all FR-ROL-001 permissions) and **User** role (**Sessions.ViewOwn**, **Sessions.ManageOwn** only) exist with **`System`** badge |
| AC-ROL-006-05 | Deployment where **Administrator** role already exists and FR-ROL-001 catalog includes permissions that role lacks                              | Application starts                                                  | **Administrator** role receives any newly defined catalog permissions it does not yet have                                                              |
| AC-ROL-006-06 | **Public registration enabled** is **true** (FR-AUTH-012); guest completes public registration                                                  | Registration completes                                              | New account is assigned **User** role only; **Administrator** is **not** assigned                                                                       |
| AC-ROL-006-07 | Fresh deployment creates the initial administrator account                                                                                      | User-visible logs or messages are inspected during or after startup | Configured **Password** value does **not** appear                                                                                                       |
| AC-ROL-006-08 | Fresh deployment with email verification enabled (FR-AUTH-011)                                                                                  | Initial administrator account is created                            | **Email verified** is **true** so the administrator can sign in without completing verification                                                         |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
