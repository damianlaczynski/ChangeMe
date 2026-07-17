---
id: FR-ISS-002
title: Issue Create and Edit Flow
domain: issues
type: functional
status: active
depends_on: [FR-USR-005, FR-ISS-004]
inherits_conventions: [STD-ACC-001, STD-FRM-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data.

## Functional requirements

### Authorization

- Available only to authenticated users.

### Data

| Field                    | Constraints                                                                                          |
| ------------------------ | ---------------------------------------------------------------------------------------------------- |
| **Title**                | **Required**; **3–255** characters.                                                                  |
| **Description**          | **Required**; max **2000** characters.                                                               |
| **Status**               | **Required**; one of **New**, **In Progress**, **Resolved**, **Closed**; default on create: **New**. |
| **Priority**             | **Required**; one of **Low**, **Medium**, **High**, **Critical**; default on create: **Medium**.     |
| **Assigned to**          | **Not required**; when set, must be an assignable user with **Deactivated** false (FR-USR-005).      |
| **Watch after creation** | On create only; default **true**; when selected, adds the author as a watcher.                       |
| **Acceptance criteria**  | Zero or more items; each item **required** when present; max **2000** characters per item.           |

**System fields (on edit, read-only):**

- **Author** is the currently signed-in user on create.
- **Issue identifier** is assigned by the system on first save; not editable.
- **Created at** and **Last activity** are maintained by the system.

### Operations

- Create a new issue with core fields and acceptance criteria.
- Edit **title**, **description**, **status**, **priority**, **assigned to**, and acceptance criteria on an existing issue.
- After create, the author is added to watchers when **Watch after creation** is selected (FR-ISS-004).
- Every create and edit writes entries to change history (FR-ISS-003), including acceptance-criterion add, update, and remove events.

### Business rules

- Edit uses the same core fields and constraints as create, plus read-only system metadata.
- Field violations use inline errors per STD-VAL-001.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
