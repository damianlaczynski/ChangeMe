---
id: REQ-USR-004
title: User Details and Session Administration
domain: users
status: active
depends_on:
  [
    REQ-AUTH-004,
    REQ-AUTH-007,
    REQ-AUTH-009,
    REQ-AUTH-011,
    REQ-AUTH-013,
    REQ-AUTH-014,
    REQ-AUTH-015,
    REQ-BIL-003,
    REQ-INV-002,
    REQ-INV-003,
    REQ-INV-004,
    REQ-INV-005,
    REQ-PKY-005,
    REQ-ROL-001,
    REQ-ROL-004,
    REQ-USR-003,
    REQ-USR-005,
  ]
---

## Goal

An authorized administrator must be able to inspect a user's account, roles, effective permissions, and active sessions and revoke sessions when needed.

## Features

### User details screen

- Screen: **User details**
- Requires permission **Users.View**.

### Profile summary

Displays read-only:

| Field                         | Behavior                                                                                                                                                                                                                                                                                        |
| ----------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **First name**                | Read-only; **`—`** when empty.                                                                                                                                                                                                                                                                  |
| **Last name**                 | Read-only; **`—`** when empty.                                                                                                                                                                                                                                                                  |
| **Email**                     | **Profile email** — account email address; notification destination (REQ-AUTH-014).                                                                                                                                                                                                             |
| **Status**                    | **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** (REQ-INV-005).                                                                                                                                                                                                     |
| **Email verified**            | Badge **`Verified`** or **`Unverified`** when email verification is enabled (REQ-AUTH-011); omitted when verification is disabled.                                                                                                                                                              |
| **Email verified at**         | Date and time when **Email verified** is true; omitted when verification is disabled or **Email verified** is false.                                                                                                                                                                            |
| **Member since**              | Account creation date and time.                                                                                                                                                                                                                                                                 |
| **Last sign-in**              | Most recent session **signed in at**; **`Never`** when the user has no sessions.                                                                                                                                                                                                                |
| **Password last changed at**  | Date and time; **`—`** when the user has no password yet (invite pending).                                                                                                                                                                                                                      |
| **Password expires at**       | Read-only, **UI only** (not stored). Shown when password expiration is **enabled** (REQ-AUTH-009): **Password last changed at** + **Maximum password age (days)**; **`—`** for users **without a local password** or without **Password last changed at**; omitted when expiration is disabled. |
| **Two-factor authentication** | Badge **`Enabled`** or **`Disabled`** when **Two-factor authentication enabled** is **true** (REQ-AUTH-013); omitted when disabled in deployment settings.                                                                                                                                      |
| **Two-factor enabled at**     | Date and time when **Two-factor enabled** is **true**; omitted when two-factor is disabled or deployment setting is off.                                                                                                                                                                        |
| **Deactivated at**            | Date and time when **Deactivated** is **true**; omitted when **Deactivated** is **false**.                                                                                                                                                                                                      |

### Invitation panel

- Pending invitation presentation, **Resend invitation**, and **Cancel invitation**: REQ-INV-002, REQ-INV-003, REQ-INV-004.
- When `pendingInvitation` is present, the **Invitation** panel is the **first** block on the page (above profile summary).
- Invitation actions are **not** duplicated in the page header.

### Pending email change panel

- When a **pending email change** exists (REQ-AUTH-015), show **Pending email change** as the **first** block on the page when no **Invitation** panel is shown; when both exist, **Invitation** remains first, then **Pending email change**, then profile summary.
- Panel shows read-only **New email**, **Requested at**, and message **`The user must confirm from the new mailbox before sign-in uses the new address.`**
- Header action **Cancel pending email change** (requires **Users.Manage**): confirmation **`Cancel the pending email change to "{new email}"? The current email will stay unchanged.`** On confirm: clears the pending change, sends **Email change cancelled** to the user's **current email** (REQ-AUTH-007), shows message **`Pending email change cancelled.`**, and refreshes **User details** in place.
- When **no pending email change** exists, the panel and header action are **not shown**.

### External sign-in methods section

- Collapsible section **External sign-in methods**; shown only when **External providers enabled** is **true** (REQ-AUTH-014).
- Lists linked **Provider** (display name) and **Linked at** per row; **Unlink** per row when the administrator has **Users.Manage** (REQ-AUTH-014).
- Empty state: **`No external sign-in methods linked.`**

### Passkeys section

- Collapsible section **Passkeys**; shown only when **Passkeys authentication enabled** is **true** (REQ-PKY-005).
- Read-only table: **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**; per-row **Remove** when the administrator has **Users.Manage** (REQ-PKY-005).
- Empty state: **`No passkeys registered.`**

### Employment section

- Collapsible section **Employment** with employment profile, contracts table, and related actions: REQ-BIL-003.
- Section header quick links **View leave** and **View availability** per REQ-BIL-009.
- Placed after **Passkeys** (when shown) or after profile-related panels, and **before** **Roles**.
- Visible when the viewer has **Billing.ViewAny** or **Billing.ManageEmployment**.

### Roles section

- Section title: **`Roles`**
- Shows one badge per assigned role name.
- Each role badge is a link to **Role details** for that role (REQ-ROL-004).
- Empty state: **`No roles assigned.`**

### Permissions section

- Section title: **`Permissions`**
- Read-only list of the user's effective permissions (union of all assigned roles, REQ-ROL-001).
- Each row shows:
  - permission **label** and **description**;
  - **From roles** — comma-separated list of assigned role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one assigned role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**, **Time**, **Billing**.
- Empty state when the user has roles but no permissions in the union: **`No permissions.`**
- This section is informational; role changes are made on **Edit user** (REQ-USR-003).

### Header actions

| Action                  | Permission required    | Behavior                                                                                                                                                                                                                                                                                  |
| ----------------------- | ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Edit**                | **Users.Manage**       | Opens **Edit user** (profile, deactivation, and role assignments when permitted).                                                                                                                                                                                                         |
| **Send invitation**     | **Users.Manage**       | Shown when the user has **no** pending invitation (for example after **Cancel invitation**); same behavior as **Resend invitation** (REQ-INV-003).                                                                                                                                        |
| **Deactivate**          | **Users.Deactivate**   | Shown when **Deactivated** is **false**; confirmation and behavior per REQ-USR-005.                                                                                                                                                                                                       |
| **Activate**            | **Users.Deactivate**   | Shown when **Deactivated** is **true**; confirmation and behavior per REQ-USR-005.                                                                                                                                                                                                        |
| **Revoke all sessions** | **Sessions.ManageAny** | Opens confirmation: **`Revoke all active sessions for this user? They will be signed out on every device.`**                                                                                                                                                                              |
| **Send password reset** | **Users.Manage**       | Sends password reset email; confirmation: **`Send a password reset link to "{email}"?`**; success message: **`Password reset email sent.`**                                                                                                                                               |
| **Confirm email**       | **Users.Manage**       | Shown when email verification is enabled and **Email verified** is false (typical for self-registered users); not shown for admin-invited users who are already verified; confirmation: **`Mark email as verified for "{full name}"?`**; success message: **`Email marked as verified.`** |
| **Reset two-factor**    | **Users.Manage**       | Shown when **Two-factor authentication enabled** is **true** and **Two-factor enabled** is **true**; behavior per REQ-AUTH-013.                                                                                                                                                           |
| **Reset passkeys**      | **Users.Manage**       | Shown when **Passkeys authentication enabled** is **true** and the user has at least one passkey; behavior per REQ-PKY-005.                                                                                                                                                               |
| **Unlink external**     | **Users.Manage**       | **Unlink** on rows in **External sign-in methods** when providers are enabled; confirmation **`Remove {Display name} sign-in from this account?`**; success **`External sign-in method removed.`**; **External account unlinked** email (REQ-AUTH-007).                                   |

- Actions the current user lacks permission for are **not shown**.

### Actions and navigation

- Clicking a role badge in **Roles** opens **Role details** for that role.
- **Back** returns to **Users list**.

### Active sessions section

- Visible only with permission **Sessions.ViewAny**.
- Section title: **`Active sessions`**
- Table columns match **Active sessions** on **My account** (REQ-AUTH-004): **Device / browser**, **IP address**, **Session type**, **Signed in at**, **Last activity**, **Actions**.
- The **Current session** badge is **not shown** in the administrator view.
- **Revoke** button on each row requires **Sessions.ManageAny** and opens confirmation: **`Revoke this session? That device will be signed out.`**
- Empty state: **`No active sessions.`**
- While loading, a loading indicator is shown in the section.
- The sessions table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.

### States and business rules

- Users with **Deactivated** true display **Status** **`Deactivated`**; the active sessions table shows empty state **`No active sessions.`**
- Revoking a session signs out that device on next activity; the list refreshes on the current page.

### Permissions and visibility

- **Users.View**: required for **User details**, **Roles** section, and **Permissions** section.
- **Sessions.ViewAny**: required to render the active sessions section.
- **Sessions.ManageAny**: required for **Revoke** on session rows and **Revoke all sessions**.

---
