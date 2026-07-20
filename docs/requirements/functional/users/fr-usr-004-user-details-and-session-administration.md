---
id: FR-USR-004
title: User Details and Session Administration
domain: users
type: functional
status: active
depends_on: [FR-AUTH-004, FR-ROL-001, FR-ROL-004, FR-USR-003, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to inspect a user's account, roles, effective permissions, and active sessions and revoke sessions when needed.

## Functional requirements

### User details screen

- Screen: **User details**
- Requires permission **Users.View**.

### Profile summary

Displays read-only:

| Field                        | Behavior                                                                                   |
| ---------------------------- | ------------------------------------------------------------------------------------------ |
| **First name**               | Read-only; **`—`** when empty.                                                             |
| **Last name**                | Read-only; **`—`** when empty.                                                             |
| **Email**                    | Account email address.                                                                     |
| **Status**                   | **`Active`** or **`Deactivated`**.                                                         |
| **Member since**             | Account creation date and time.                                                            |
| **Last sign-in**             | Most recent session **signed in at**; **`Never`** when the user has no sessions.           |
| **Password last changed at** | Date and time; **`—`** when not yet recorded.                                              |
| **Deactivated at**           | Date and time when **Deactivated** is **true**; omitted when **Deactivated** is **false**. |

### Roles section

- Section title: **`Roles`**
- Shows one badge per assigned role name.
- Each role badge is a link to **Role details** for that role (FR-ROL-004).
- Empty state: **`No roles assigned.`**

### Permissions section

- Section title: **`Permissions`**
- Read-only list of the user's effective permissions (union of all assigned roles, FR-ROL-001).
- Each row shows:
  - permission **label** and **description**;
  - **From roles** — comma-separated list of assigned role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one assigned role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**.
- Empty state when the user has roles but no permissions in the union: **`No permissions.`**
- This section is informational; role changes are made on **Edit user** (FR-USR-003).

### Header actions

| Action                  | Permission required    | Behavior                                                                                                     |
| ----------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Edit**                | **Users.Manage**       | Opens **Edit user** (profile, deactivation, and role assignments when permitted).                            |
| **Deactivate**          | **Users.Deactivate**   | Shown when **Deactivated** is **false**; confirmation and behavior per FR-USR-005.                           |
| **Activate**            | **Users.Deactivate**   | Shown when **Deactivated** is **true**; confirmation and behavior per FR-USR-005.                            |
| **Revoke all sessions** | **Sessions.ManageAny** | Opens confirmation: **`Revoke all active sessions for this user? They will be signed out on every device.`** |

### Actions and navigation

- Clicking a role badge in **Roles** opens **Role details** for that role.
- **Back** returns to **Users list**.

### Active sessions section

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) unless stated below.
- Visible only with permission **Sessions.ViewAny**.
- Section title: **`Active sessions`**
- Table columns match **Active sessions** on **My account** (FR-AUTH-004): **Device / browser**, **IP address**, **Session type**, **Signed in at**, **Last activity**, **Actions**.
- The **Current session** badge is **not shown** in the administrator view.
- **Revoke** button on each row requires **Sessions.ManageAny** and opens confirmation: **`Revoke this session? That device will be signed out.`**
- Empty state: **`No active sessions.`**

### States and business rules

- Users with **Deactivated** true display **Status** **`Deactivated`**; the active sessions table shows empty state **`No active sessions.`**
- Revoking a session signs out that device on next activity; the list refreshes on the current page.

### Permissions and visibility

- **Users.View**: required for **User details**, **Roles** section, and **Permissions** section.
- **Sessions.ViewAny**: required to render the active sessions section.
- **Sessions.ManageAny**: required for **Revoke** on session rows and **Revoke all sessions**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
