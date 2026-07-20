---
id: FR-ROL-002
title: Roles List
domain: access
type: functional
status: active
depends_on: [FR-ROL-003]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to browse roles, search and sort them, and open role administration flows.

## Functional requirements

### Search and actions bar

- Screen: **Roles list**
- Sidebar entry **Roles** is visible only with permission **Roles.View**.
- **Add role** button opens **Create role** (FR-ROL-003); visible only with permission **Roles.Manage**.

### Roles table

| Column          | Description                                                            |
| --------------- | ---------------------------------------------------------------------- |
| **Name**        | Role name; link to **Role details**.                                   |
| **Description** | Role description text, or em dash (**`—`**) when empty.                |
| **Permissions** | Exact format: **`{n} permissions`** where `{n}` is the assigned count. |
| **Users**       | Exact format: **`{n} users`** where `{n}` is the assignment count.     |
| **System**      | Badge **`System`** for seeded roles; blank for custom roles.           |
| **Actions**     | Overflow menu (see below).                                             |

### Sorting

- Sortable columns: **Name**, **Users**, **Permissions**.
- Default sort: **Name**, ascending.

### Search and filters

- Inherits `FR-UI-001` (**Administrative list screens**) unless stated below.
- **Filterable columns**: **Name**, **Description**, **Permissions**, **Users**.
- Global search matches **name** and **description**.

### Row overflow menu

| Action           | Permission required | Behavior                                                              |
| ---------------- | ------------------- | --------------------------------------------------------------------- |
| **Open details** | **Roles.View**      | Opens **Role details**.                                               |
| **Edit role**    | **Roles.Manage**    | Opens **Edit role** for custom roles only.                            |
| **Delete role**  | **Roles.Manage**    | Shown for custom roles only; confirmation and behavior in FR-ROL-003. |

- **Edit role** and **Delete role** are **not shown** for system roles (**Administrator**, **User**); system roles are opened via **Open details** only.

### Permissions and visibility

- **Roles.View**: required for **Roles list** and **Open details**.
- **Roles.Manage**: required for **Add role**, **Edit role**, and **Delete role**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
