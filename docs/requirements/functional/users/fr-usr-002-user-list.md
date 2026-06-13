---
id: FR-USR-002
title: User List
domain: users
type: functional
status: active
depends_on: [FR-USR-003, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to browse users, search and filter them, and open administration flows.

## Functional requirements

### Search and actions bar

- Screen: **Users list**
- Sidebar entry **Users** is visible only with permission **Users.View**.
- **Create user** button opens **Create user** (FR-USR-003); visible only with permission **Users.Manage**.

### Users table

| Column           | Description                                                                                   |
| ---------------- | --------------------------------------------------------------------------------------------- |
| **Name**         | **First name** and **Last name** when set; **`—`** when both empty; link to **User details**. |
| **Email**        | User email address.                                                                           |
| **Status**       | **`Active`** or **`Deactivated`**.                                                            |
| **Roles**        | One status badge per assigned role showing the role name.                                     |
| **Last sign-in** | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions. |
| **Created at**   | Account creation date and time.                                                               |
| **Actions**      | Overflow menu (see below).                                                                    |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Inherits `FR-UI-001` (**Administrative list screens**) for filters panel, applied filter chips, pagination, loading, and overflow menu visibility unless stated below.
- **Status** multi-select: **`Active`**, **`Deactivated`**. Empty selection means no restriction.
- Default filter state: **no status restriction**.

### Row overflow menu

| Action           | Permission required  | Behavior                                                   |
| ---------------- | -------------------- | ---------------------------------------------------------- |
| **Open details** | **Users.View**       | Opens **User details**.                                    |
| **Edit**         | **Users.Manage**     | Opens **Edit user**.                                       |
| **Deactivate**   | **Users.Deactivate** | Shown only when **Deactivated** is **false** (FR-USR-005). |
| **Activate**     | **Users.Deactivate** | Shown only when **Deactivated** is **true** (FR-USR-005).  |

### Permissions and visibility

- **Users.View**: required for **Users list** and **Open details**.
- **Users.Manage**: required for **Create user** and **Edit**.
- **Users.Deactivate**: required for **Deactivate** and **Activate**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
