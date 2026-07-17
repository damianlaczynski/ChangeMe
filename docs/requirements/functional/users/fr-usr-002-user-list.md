---
id: FR-USR-002
title: User List
domain: users
type: functional
status: active
depends_on: [FR-USR-003, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-LST-001, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to browse users, search and filter them, and open administration flows.

## Functional requirements

### Authorization

- **Users.View**: required to browse users and open user details.
- **Users.Manage**: required to create and edit users.
- **Users.Deactivate**: required to deactivate and activate users.

### Data

- Each user exposes **name**, **email**, **status** (**Active** / **Deactivated**), assigned **roles**, **last sign-in**, and **created at**.
- Inherits STD-LST-001 unless stated below.
- **Filterable** attributes: name, email, status, last sign-in, created at.
- Global search matches **first name**, **last name**, and **email**.
- Default sort: **name** (last name), ascending.

### Operations

- Browse all users.
- Create a user (FR-USR-003).
- Open user details (FR-USR-004).
- Edit a user (FR-USR-003).
- Deactivate or activate a user (FR-USR-005).

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
