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
- Search field placeholder: **`Search roles...`**
- Search matches **name** or **description** fragment (case-insensitive).
- **Search** button and form submit apply the current search text.
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

### List behavior

- Inherits `FR-UI-001` (**Administrative list screens**) for pagination, loading, and overflow menu visibility unless stated below.

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

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ROL-002-01 | Signed-in user without **Roles.View** | User views the sidebar | **Roles** entry is **not shown** |
| AC-ROL-002-02 | Signed-in user with **Roles.View** | User opens **Roles list** from the sidebar | Screen loads; table default sort is **Name**, ascending |
| AC-ROL-002-03 | Signed-in user with **Roles.View** on **Roles list** | User enters a fragment matching a role **name** (any case) and clicks **Search** or submits the search form | Table shows only roles whose **name** or **description** contains that fragment (case-insensitive) |
| AC-ROL-002-04 | Signed-in user with **Roles.View** on **Roles list** | User views the search field | Placeholder text is **`Search roles...`** |
| AC-ROL-002-05 | Signed-in user with **Roles.Manage** on **Roles list** | User views the actions bar | **Add role** button is shown; clicking it opens **Create role** (FR-ROL-003) |
| AC-ROL-002-06 | Signed-in user with **Roles.View** but without **Roles.Manage** on **Roles list** | User views the actions bar | **Add role** button is **not shown** |
| AC-ROL-002-07 | Signed-in user with **Roles.View** on **Roles list** | User views a table row | **Name** links to **Role details**; **Description** shows text or **`—`** when empty; **Permissions** shows **`{n} permissions`**; **Users** shows **`{n} users`**; seeded roles show **`System`** badge |
| AC-ROL-002-08 | Signed-in user with **Roles.View** on **Roles list** | User clicks a role **Name** link or chooses **Open details** from the row overflow menu | **Role details** opens for that role |
| AC-ROL-002-09 | Signed-in user with **Roles.Manage** on a **custom** role row overflow menu | User opens the menu | **Open details**, **Edit role**, and **Delete role** are shown |
| AC-ROL-002-10 | Signed-in user with **Roles.Manage** on **Administrator** or **User** system role row overflow menu | User opens the menu | **Open details** is shown; **Edit role** and **Delete role** are **not shown** |
| AC-ROL-002-11 | Signed-in user with **Roles.View** on **Roles list** | User clicks a sortable column header (**Name**, **Users**, or **Permissions**) | Table re-sorts by the selected column |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
