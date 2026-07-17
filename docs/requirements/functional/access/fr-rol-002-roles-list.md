---
id: FR-ROL-002
title: Roles List
domain: access
type: functional
status: active
depends_on: [FR-ROL-003]
inherits_conventions: [STD-ACC-001, STD-LST-001, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to browse roles, search and sort them, and open role administration flows.

## Functional requirements

### Authorization

- **Roles.View**: required to browse roles and open role details.
- **Roles.Manage**: required to create, edit, and delete custom roles.

### Data

- Each role exposes **name**, **description**, assigned **permission** count, assigned **user** count, and whether it is a **system** role.
- Inherits STD-LST-001 unless stated below.
- **Filterable** attributes: name, description, permission count, user count.
- Global search matches **name** and **description**.
- Default sort: **name**, ascending.

### Operations

- Browse all roles.
- Open role details (FR-ROL-004).
- Create a custom role (FR-ROL-003).
- Edit a custom role (FR-ROL-003).
- Delete a custom role (FR-ROL-003).
- **System** roles (**Administrator**, **User**) cannot be edited or deleted from the list.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
