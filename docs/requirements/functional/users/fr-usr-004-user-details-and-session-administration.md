---
id: FR-USR-004
title: User Details and Session Administration
domain: users
type: functional
status: active
depends_on:
  [
    FR-AUTH-004,
    FR-AUTH-007,
    FR-AUTH-009,
    FR-AUTH-011,
    FR-AUTH-013,
    FR-AUTH-014,
    FR-AUTH-015,
    FR-INV-002,
    FR-INV-003,
    FR-INV-004,
    FR-INV-005,
    FR-PKY-005,
    FR-ROL-001,
    FR-ROL-004,
    FR-USR-003,
    FR-USR-005,
  ]
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

| Field                         | Behavior                                                                                                                                                                                                                                                                                       |
| ----------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **First name**                | Read-only; **`—`** when empty.                                                                                                                                                                                                                                                                 |
| **Last name**                 | Read-only; **`—`** when empty.                                                                                                                                                                                                                                                                 |
| **Email**                     | **Profile email** — account email address; notification destination (FR-AUTH-014).                                                                                                                                                                                                             |
| **Status**                    | **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** (FR-INV-005).                                                                                                                                                                                                     |
| **Email verified**            | Badge **`Verified`** or **`Unverified`** when email verification is enabled (FR-AUTH-011); omitted when verification is disabled.                                                                                                                                                              |
| **Email verified at**         | Date and time when **Email verified** is true; omitted when verification is disabled or **Email verified** is false.                                                                                                                                                                           |
| **Member since**              | Account creation date and time.                                                                                                                                                                                                                                                                |
| **Last sign-in**              | Most recent session **signed in at**; **`Never`** when the user has no sessions.                                                                                                                                                                                                               |
| **Password last changed at**  | Date and time; **`—`** when the user has no password yet (invite pending).                                                                                                                                                                                                                     |
| **Password expires at**       | Read-only, **UI only** (not stored). Shown when password expiration is **enabled** (FR-AUTH-009): **Password last changed at** + **Maximum password age (days)**; **`—`** for users **without a local password** or without **Password last changed at**; omitted when expiration is disabled. |
| **Two-factor authentication** | Badge **`Enabled`** or **`Disabled`** when **Two-factor authentication enabled** is **true** (FR-AUTH-013); omitted when disabled in deployment settings.                                                                                                                                      |
| **Two-factor enabled at**     | Date and time when **Two-factor enabled** is **true**; omitted when two-factor is disabled or deployment setting is off.                                                                                                                                                                       |
| **Deactivated at**            | Date and time when **Deactivated** is **true**; omitted when **Deactivated** is **false**.                                                                                                                                                                                                     |

### Invitation panel

- Pending invitation presentation, **Resend invitation**, and **Cancel invitation**: FR-INV-002, FR-INV-003, FR-INV-004.
- When `pendingInvitation` is present, the **Invitation** panel is the **first** block on the page (above profile summary).
- Invitation actions are **not** duplicated in the page header.

### Pending email change panel

- When a **pending email change** exists (FR-AUTH-015), show **Pending email change** as the **first** block on the page when no **Invitation** panel is shown; when both exist, **Invitation** remains first, then **Pending email change**, then profile summary.
- Panel shows read-only **New email**, **Requested at**, and message **`The user must confirm from the new mailbox before sign-in uses the new address.`**
- Header action **Cancel pending email change** (requires **Users.Manage**): confirmation **`Cancel the pending email change to "{new email}"? The current email will stay unchanged.`** On confirm: clears the pending change, sends **Email change cancelled** to the user's **current email** (FR-AUTH-007), shows message **`Pending email change cancelled.`**, and refreshes **User details** in place.
- When **no pending email change** exists, the panel and header action are **not shown**.

### External sign-in methods section

- Collapsible section **External sign-in methods**; shown only when **External providers enabled** is **true** (FR-AUTH-014).
- Lists linked **Provider** (display name) and **Linked at** per row; **Unlink** per row when the administrator has **Users.Manage** (FR-AUTH-014).
- Empty state: **`No external sign-in methods linked.`**

### Passkeys section

- Collapsible section **Passkeys**; shown only when **Passkeys authentication enabled** is **true** (FR-PKY-005).
- Read-only table: **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**; per-row **Remove** when the administrator has **Users.Manage** (FR-PKY-005).
- Empty state: **`No passkeys registered.`**

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

| Action                  | Permission required    | Behavior                                                                                                                                                                                                                                                                                  |
| ----------------------- | ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Edit**                | **Users.Manage**       | Opens **Edit user** (profile, deactivation, and role assignments when permitted).                                                                                                                                                                                                         |
| **Send invitation**     | **Users.Manage**       | Shown when the user has **no** pending invitation (for example after **Cancel invitation**); same behavior as **Resend invitation** (FR-INV-003).                                                                                                                                         |
| **Deactivate**          | **Users.Deactivate**   | Shown when **Deactivated** is **false**; confirmation and behavior per FR-USR-005.                                                                                                                                                                                                        |
| **Activate**            | **Users.Deactivate**   | Shown when **Deactivated** is **true**; confirmation and behavior per FR-USR-005.                                                                                                                                                                                                         |
| **Revoke all sessions** | **Sessions.ManageAny** | Opens confirmation: **`Revoke all active sessions for this user? They will be signed out on every device.`**                                                                                                                                                                              |
| **Send password reset** | **Users.Manage**       | Sends password reset email; confirmation: **`Send a password reset link to "{email}"?`**; success message: **`Password reset email sent.`**                                                                                                                                               |
| **Confirm email**       | **Users.Manage**       | Shown when email verification is enabled and **Email verified** is false (typical for self-registered users); not shown for admin-invited users who are already verified; confirmation: **`Mark email as verified for "{full name}"?`**; success message: **`Email marked as verified.`** |
| **Reset two-factor**    | **Users.Manage**       | Shown when **Two-factor authentication enabled** is **true** and **Two-factor enabled** is **true**; behavior per FR-AUTH-013.                                                                                                                                                            |
| **Reset passkeys**      | **Users.Manage**       | Shown when **Passkeys authentication enabled** is **true** and the user has at least one passkey; behavior per FR-PKY-005.                                                                                                                                                                |
| **Unlink external**     | **Users.Manage**       | **Unlink** on rows in **External sign-in methods** when providers are enabled; confirmation **`Remove {Display name} sign-in from this account?`**; success **`External sign-in method removed.`**; **External account unlinked** email (FR-AUTH-007).                                    |

### Actions and navigation

- Clicking a role badge in **Roles** opens **Role details** for that role.
- **Back** returns to **Users list**.

### Active sessions section

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) for pagination and section loading unless stated below.
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

---

## Acceptance scenarios

| ID            | Given                                                                                                   | When                                                     | Then                                                                                                                                  |
| ------------- | ------------------------------------------------------------------------------------------------------- | -------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| AC-USR-004-01 | Signed-in user without **Users.View**                                                                   | User navigates to **User details**                       | Access denied per FR-ROL-001                                                                                                          |
| AC-USR-004-02 | Administrator with **Users.View**; target user has **pending invitation**                               | User opens **User details**                              | **Invitation** panel is the **first** block above profile summary                                                                     |
| AC-USR-004-03 | Administrator with **Users.Manage**; target user has **pending email change** and no pending invitation | User clicks **Cancel pending email change** and confirms | Pending change cleared; **`Pending email change cancelled.`** toast; **Email change cancelled** email sent; screen refreshes in place |
| AC-USR-004-04 | Administrator with **Users.View** on **User details**                                                   | User clicks a role badge in **Roles**                    | **Role details** opens for that role (FR-ROL-004)                                                                                     |
| AC-USR-004-05 | Administrator with **Sessions.ViewAny** and **Sessions.ManageAny**; target has active sessions          | User clicks **Revoke** on a session row and confirms     | Confirmation **`Revoke this session? That device will be signed out.`**; session removed from list on current page                    |
| AC-USR-004-06 | Administrator with **Users.Deactivate**; target **Deactivated** is false                                | User views header actions                                | **Deactivate** is shown; **Activate** is **not shown**                                                                                |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
