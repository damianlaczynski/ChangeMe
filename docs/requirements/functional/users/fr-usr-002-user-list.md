---
id: FR-USR-002
title: User List
domain: users
type: functional
status: active
depends_on: [FR-AUTH-011, FR-INV-001, FR-INV-005, FR-USR-005]
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
- **Invite user** button opens **Invite user** (FR-INV-001); visible only with permission **Users.Manage**.

### Users table

| Column             | Description                                                                                                                                        |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**           | **First name** and **Last name** when set; **`—`** when both empty; link to **User details**.                                                      |
| **Email**          | User email address.                                                                                                                                |
| **Status**         | **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** (FR-INV-005). Replaces **Account**; **Account state** column removed. |
| **Email verified** | Badge **`Verified`** or **`Unverified`** when email verification is enabled (FR-AUTH-011); omitted when verification is disabled.                  |
| **Roles**          | One status badge per assigned role showing the role name.                                                                                          |
| **Last sign-in**   | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions.                                               |
| **Created at**     | Account creation date and time.                                                                                                                    |
| **Actions**        | Overflow menu (see below).                                                                                                                         |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Inherits `FR-UI-001` (**Administrative list screens**) for filters panel, applied filter chips, pagination, loading, and overflow menu visibility unless stated below.
- **Status** multi-select: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`** (FR-INV-005). Empty selection means no restriction. Replaces the former **Account** filter.
- **Email verified** multi-select: **`Verified`**, **`Unverified`**. Shown only when email verification is enabled (FR-AUTH-011). Empty selection means no restriction.
- Default filter state: **no status restriction**; **no email verified restriction** when that filter is shown.

### Row overflow menu

| Action           | Permission required  | Behavior                                                   |
| ---------------- | -------------------- | ---------------------------------------------------------- |
| **Open details** | **Users.View**       | Opens **User details**.                                    |
| **Edit**         | **Users.Manage**     | Opens **Edit user**.                                       |
| **Deactivate**   | **Users.Deactivate** | Shown only when **Deactivated** is **false** (FR-USR-005). |
| **Activate**     | **Users.Deactivate** | Shown only when **Deactivated** is **true** (FR-USR-005).  |

### Permissions and visibility

- **Users.View**: required for **Users list** and **Open details**.
- **Users.Manage**: required for **Invite user** and **Edit**.
- **Users.Deactivate**: required for **Deactivate** and **Activate**.

---

## Acceptance scenarios

| ID            | Given                                                                            | When                         | Then                                                                                  |
| ------------- | -------------------------------------------------------------------------------- | ---------------------------- | ------------------------------------------------------------------------------------- |
| AC-USR-002-01 | Signed-in user with **Users.View**                                               | User opens **Users list**    | Table loads with default sort **Name** ascending; sidebar entry **Users** is visible  |
| AC-USR-002-02 | Signed-in user with **Users.Manage**                                             | User views the actions bar   | **Invite user** button is visible and opens **Invite user** (FR-INV-001)              |
| AC-USR-002-03 | Signed-in user without **Users.Manage**                                          | User views the actions bar   | **Invite user** button is **not shown**                                               |
| AC-USR-002-04 | Signed-in user with **Users.View**; target user has **Deactivated** false        | User opens row overflow menu | **Deactivate** is shown only with **Users.Deactivate**; **Activate** is **not shown** |
| AC-USR-002-05 | Signed-in user with **Users.View**; target user has **Deactivated** true         | User opens row overflow menu | **Activate** is shown only with **Users.Deactivate**; **Deactivate** is **not shown** |
| AC-USR-002-06 | Signed-in user applies **Status** filter **Active** and clicks **Apply filters** | Filters submit               | Table shows only users matching **Active**; list reloads from page **1**              |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
