# Requirements - Users

This document covers six REQs for the **Users** area:
my account profile, user list, admin edit user, user details with session administration, account deactivation, and admin email confirmation.

**Account invitations** (invite, resend, cancel, pending-invitation UI, user **Status**): `docs/req/invitations-requirements.md`.

**Passkeys (WebAuthn):** `docs/req/passkeys-requirements.md` — **My account** and **User details** sections when implemented (REQ-PKY-003, REQ-PKY-005).

Role assignment is performed on **Invite user** (REQ-INV-001) and **Edit user** (REQ-USR-003). Removing a user from a role is available on **Role details** (REQ-ROL-005).

## Business terms (account and sign-in)

The following terms are used across Users and Auth requirements. They describe observable account state, not implementation details.

| Term                               | Meaning                                                                                                                                                                                                                                                                                    |
| ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Account enabled**                | The user is not deactivated by an administrator and may sign in when other rules allow.                                                                                                                                                                                                    |
| **Account deactivated**            | An administrator disabled the account; the user cannot sign in and has no effective permissions until reactivated.                                                                                                                                                                         |
| **Account invitation**             | See `docs/req/invitations-requirements.md` (**Business terms**).                                                                                                                                                                                                                           |
| **Awaiting invitation acceptance** | See `docs/req/invitations-requirements.md` (**Business terms**). Sign-in and acceptance: REQ-AUTH-010, REQ-AUTH-014.                                                                                                                                                                       |
| **Local password**                 | A password stored in ChangeMe for email/password sign-in. A user **with a local password** has completed invitation acceptance with a password, self-registration, or **Set password** on **My account**.                                                                                  |
| **External-only account**          | The user can sign in through one or more linked external providers but **has no local password yet** and is **not** awaiting invitation acceptance (for example after invitation acceptance via OIDC, self-service registration via an IdP, or when no administrator invitation was sent). |
| **Email verified**                 | When email verification is enabled in deployment settings, the user proved control of the mailbox (verification link, invitation acceptance, or administrator confirmation). When verification is disabled, every account is treated as verified for sign-in purposes.                     |
| **Profile email**                  | The **current email** on the ChangeMe account; used for sign-in, display, and all notifications (REQ-AUTH-014). Shown as **Email** on **My account** and admin screens.                                                                                                                    |
| **Two-factor enrolled**            | The user completed authenticator setup; password sign-in requires a verification code unless external **Trust identity provider MFA** applies on that sign-in.                                                                                                                             |
| **Passkey enrolled**               | The user has at least one **Passkey credential** (REQ-PKY-003). Passkey sign-in is available when **Passkeys authentication enabled** is **true** (REQ-PKY-001).                                                                                                                           |
| **Passkey-only account**           | The user has at least one passkey, **no local password**, and **no external login**; allowed only when **Allow passkey-only accounts** is **true** (REQ-PKY-001).                                                                                                                          |

Cross-reference: invitations — `docs/req/invitations-requirements.md`; invitation acceptance — REQ-AUTH-010; external sign-in — REQ-AUTH-014; email verification — REQ-AUTH-011; self-service email change — REQ-AUTH-015; passkeys — `docs/req/passkeys-requirements.md`.

## Account model (all Users REQs)

Administrative enablement is separate from onboarding and how the user signs in.

| Concept                      | Shown in UI / admin                                                                             | Meaning                                                                                                                                                            |
| ---------------------------- | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Deactivated**              | **Status** **`Deactivated`**                                                                    | Whether an administrator disabled the account.                                                                                                                     |
| **Deactivated at**           | **User details**                                                                                | When the account was last deactivated, if applicable.                                                                                                              |
| **Local password**           | Implied by **Change password**, invitation state                                                | Whether the user has completed setting a ChangeMe password. Users **without** a local password are either **awaiting invitation acceptance** or **external-only**. |
| **Email verified**           | **Email verified** badge, filters                                                               | Whether the mailbox is considered confirmed when email verification is enabled (REQ-AUTH-011).                                                                     |
| **Email verified at**        | **User details**                                                                                | When verification last succeeded.                                                                                                                                  |
| **Password last changed at** | **User details**, password expiration                                                           | When the local password was last set or changed (REQ-AUTH-009).                                                                                                    |
| **Pending invitation**       | **Invitation** panel (REQ-INV-002); API: `pendingInvitation`                                    | Summary while **awaiting invitation acceptance**; hidden after acceptance or cancel. Closed rows: REQ-INV-006.                                                     |
| **Two-factor enabled**       | **My account**, **User details**                                                                | Whether the user enrolled in app TOTP when two-factor is enabled in deployment settings (REQ-AUTH-013).                                                            |
| **Two-factor enabled at**    | **User details**                                                                                | When two-factor enrollment last completed.                                                                                                                         |
| **External login**           | **External sign-in methods**                                                                    | A linked external provider identity (provider name, linked date).                                                                                                  |
| **Passkey credential**       | **Passkeys** (REQ-PKY-003, REQ-PKY-005)                                                         | A registered WebAuthn credential (name, created at, last used at, authenticator type).                                                                             |
| **Pending email change**     | **Pending email change** panel on **My account** (REQ-AUTH-015); **User details** (REQ-USR-004) | Self-service request to replace **current email** with a **new email** until confirmed or cancelled.                                                               |

**Email verified** when email verification is enabled (REQ-AUTH-011):

- **Self-registration:** not verified until the user completes **Verify email**; verification time recorded on success.
- **Administrator invite user:** verified when **Invite user** succeeds (REQ-INV-001) — the invitation is sent to that email address, which is treated as confirmed.
- **Initial administrator:** verified at creation (REQ-ROL-006).
- **Accept invitation:** remains verified if already set at invite; otherwise verified on success (mailbox proof via the invitation link).

When email verification is **disabled** in deployment settings, every account is treated as verified for sign-in.

**Status** (UI only, read-only): **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** on **Users list** and **User details**. Rules: REQ-INV-005. On the list, **Account** column becomes **Status**; **Account state** column is dropped.

On admin invite, **First name** and **Last name** are **optional** on **Invite user** (REQ-INV-001) and **Edit user** (REQ-USR-003). On **Accept invitation** (REQ-AUTH-010), fields are pre-filled from values already stored (including admin-set names) and the user may edit them before submit.

**Password expires at (admin UI only):** Not stored. When password expiration is **enabled** (REQ-AUTH-009), **User details** shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)**. Omitted when expiration is disabled; shown as **`—`** for users without a local password. Not shown on **My account** (REQ-USR-001).

---

# REQ-USR-001: My Account Profile

## Goal

The signed-in user must be able to view their own profile, edit it on a separate screen, and reach account security actions.

## Features

### My account screen

- Screen: **My account**
- Sidebar entry: **My account** (visible to all authenticated users with **Deactivated** false).

### Profile section (read-only)

| Field            | Behavior                                  |
| ---------------- | ----------------------------------------- |
| **First name**   | Read-only.                                |
| **Last name**    | Read-only.                                |
| **Email**        | Read-only.                                |
| **Member since** | Read-only account creation date and time. |

- **Roles** section: read-only list of assigned role names (badges); collapsible panel; links to **Role details** when the user has **Roles.View**, otherwise badges only.
- Empty state: **`No roles assigned.`**
- **Permissions** section: read-only list (REQ-ROL-001); collapsible panel; default **collapsed**.
- **Password last changed at**, **Password expires at**, and other admin-only account metadata are **not shown** on **My account** (see REQ-USR-004).

### Header actions

- **Edit** button (header action) opens **Edit profile** (same placement as **Edit** on other detail screens).
- **Change email** button (header action) opens **Change email** (REQ-AUTH-015) when **Self-service email change enabled** is **true**, the user is **not** **awaiting invitation acceptance**, and has **no pending email change**.
- **Change password** button (header action) opens **Change password** (REQ-AUTH-005).
- **Sign out everywhere** button (header action) when the user has **Sessions.ManageOwn**; same behavior as REQ-AUTH-003.

### Pending email change panel

- When a **pending email change** exists (REQ-AUTH-015), show the **Pending email change** panel as the **first content block** on **My account** (above the profile summary).
- Panel content and actions follow REQ-AUTH-015.
- When **no pending email change** exists, the panel is **not shown**.

### Edit profile screen

- Screen: **Edit profile**
- Route: linked from **My account** via header action **Edit**; **Back to my account** at the top.
- **First name** and **Last name** are editable; **required**; max **100** characters.
- **Email** is not shown on this screen (read-only on **My account** only).
- **Save changes**: validate, save, show message **`Profile updated.`**, return to **My account**.
- **Cancel**: return to **My account** without saving.

### Active sessions section

- Collapsible section **Active sessions** on the same screen when the user has **Sessions.ViewOwn** (REQ-AUTH-004).
- Not a separate screen or sidebar entry.

### Two-factor authentication section

- Collapsible section **Two-factor authentication** on the same screen when **Two-factor authentication enabled** is **true** (REQ-AUTH-013).
- Not a separate screen or sidebar entry.

### External sign-in methods section

- Collapsible section **External sign-in methods** on the same screen when **External providers enabled** is **true** (REQ-AUTH-014).
- Not a separate screen or sidebar entry.

### Passkeys section

- Collapsible section **Passkeys** on the same screen when **Passkeys authentication enabled** is **true** (REQ-PKY-003).
- Not a separate screen or sidebar entry.

### Set password

- Header action **Set password** on **My account** when the signed-in user is **external-only** (no local password); opens **Set password** (REQ-AUTH-014).
- **Change password** (REQ-AUTH-005) is shown only when the user **has a local password**.

### Validation (edit profile)

- **First name** and **Last name** follow the same rules as registration (REQ-AUTH-001); errors are inline on the relevant field.

### Permissions and visibility

- Any authenticated user with **Deactivated** false can view **My account** and open **Edit profile** to change **First name** and **Last name**.
- **My account** does **not** expose role assignment or account status changes.
- Self-service **Change email** is specified in REQ-AUTH-015; this REQ links to it from header actions and the **Pending email change** panel.

---

# REQ-USR-002: User List

## Goal

An authorized administrator must be able to browse users, search and filter them, and open administration flows.

## Features

### Search and actions bar

- Screen: **Users list**
- Sidebar entry **Users** is visible only with permission **Users.View**.
- **Invite user** button opens **Invite user** (REQ-INV-001); visible only with permission **Users.Manage**.

### Users table

| Column             | Description                                                                                                                                         |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**           | **First name** and **Last name** when set; **`—`** when both empty; link to **User details**.                                                       |
| **Email**          | User email address.                                                                                                                                 |
| **Status**         | **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** (REQ-INV-005). Replaces **Account**; **Account state** column removed. |
| **Email verified** | Badge **`Verified`** or **`Unverified`** when email verification is enabled (REQ-AUTH-011); omitted when verification is disabled.                  |
| **Roles**          | One status badge per assigned role showing the role name.                                                                                           |
| **Last sign-in**   | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions.                                                |
| **Created at**     | Account creation date and time.                                                                                                                     |
| **Actions**        | Overflow menu (see below).                                                                                                                          |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Status** multi-select: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`** (REQ-INV-005). Empty selection means no restriction. Replaces the former **Account** filter.
- **Email verified** multi-select: **`Verified`**, **`Unverified`**. Shown only when email verification is enabled (REQ-AUTH-011). Empty selection means no restriction.
- Default filter state: **no status restriction**; **no email verified restriction** when that filter is shown.
- Filters combine with search text using **AND** logic.
- **Apply filters** submits filters with the current search text.
- **Clear filters** resets the filter form and removes all filter constraints from the active query.
- Applied filters list

### Row overflow menu

| Action           | Permission required  | Behavior                                                    |
| ---------------- | -------------------- | ----------------------------------------------------------- |
| **Open details** | **Users.View**       | Opens **User details**.                                     |
| **Edit**         | **Users.Manage**     | Opens **Edit user**.                                        |
| **Deactivate**   | **Users.Deactivate** | Shown only when **Deactivated** is **false** (REQ-USR-005). |
| **Activate**     | **Users.Deactivate** | Shown only when **Deactivated** is **true** (REQ-USR-005).  |

- Menu actions the current user lacks permission for are **not shown**.

### Pagination

- The users table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search, filters, and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area.

### Permissions and visibility

- **Users.View**: required for **Users list** and **Open details**.
- **Users.Manage**: required for **Invite user** and **Edit**.
- **Users.Deactivate**: required for **Deactivate** and **Activate**.

---

# REQ-USR-003: Edit User (Admin)

## Goal

An authorized administrator must be able to update an existing user's profile, role assignments, and deactivation state.

**Invite user** (new account + first invitation): REQ-INV-001.

## Features

### Permissions preview (edit)

- Below the **Roles** field, a read-only section **Permissions** shows the union of permissions from the currently selected roles (REQ-ROL-001).
- Each permission row shows:
  - **Label** and **description** from the catalog;
  - **From roles** — comma-separated list of selected role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one selected role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**.
- The preview updates immediately when the **Roles** selection changes, before save.
- When no role is selected, the section shows: **`Select at least one role to preview permissions.`**
- When the selected roles grant no permissions, the section shows: **`No permissions.`**
- The section is read-only; it does not replace the **Roles** field for assignment.
- The preview is shown only when the **Roles** field is visible (requires **Roles.Manage**).

### Edit user screen

- Screen: **Edit user**
- Requires permission **Users.Manage**.

| Field           | Behavior                                                                                                                            |
| --------------- | ----------------------------------------------------------------------------------------------------------------------------------- |
| **First name**  | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Last name**   | **Required** once the user **has a local password**; **optional** while **awaiting invitation acceptance**; max **100** characters. |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                                                          |
| **Roles**       | Same rules as create (REQ-ROL-005); visible and editable only with **Roles.Manage**.                                                |
| **Deactivated** | Checkbox; editable only with **Users.Deactivate**; label **`Deactivated`**. When checked, **Status** becomes **`Deactivated`**.     |

- **Password** fields are **not shown** on **Edit user**.
- When **External providers enabled** is **true** and the edited user has at least one **External login**, show persistent notice: **`External sign-in stays linked. Profile email is used for notifications; provider addresses may differ.`** (REQ-AUTH-014).
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.

### Validation

- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per REQ-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Back** button and **Cancel** button navigate to **User details** without saving.
- **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Permissions** preview is **not shown**.
- An administrator **cannot** set their own **Deactivated** to **true**; save is rejected with message **`You cannot deactivate your own account.`**
- When **Email** is changed on save:
  - cancel any **pending email change** on that user (REQ-AUTH-015);
  - apply the new **Email** immediately as **current email**;
  - set **Email verified** true and **Email verified at** to the current date and time when email verification is enabled (REQ-AUTH-011);
  - revoke **all active sessions** for that user;
  - send **Email changed by admin** to the previous **current email** and to the new **Email** (REQ-AUTH-007).
- When **Email** is unchanged on save, admin email rules above do **not** run.

### Permissions and visibility

- **Users.Manage**: required to open **Edit user** and save profile fields.
- **Roles.Manage**: required to view and edit the **Roles** field.
- **Users.Deactivate**: required to view and edit the **Deactivated** field on **Edit user**.

---

# REQ-USR-004: User Details and Session Administration

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
- Rows are grouped by **Users**, **Roles**, **Sessions**.
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

# REQ-USR-005: Deactivate and Activate Accounts

## Goal

An authorized administrator must be able to set **Deactivated** to **true** or **false**, immediately removing or restoring sign-in access.

## Features

### Deactivate

- Available from **Users list** overflow **Deactivate**, **User details** **Deactivate**, and **Edit user** when **Deactivated** is set to **true** (requires **Users.Deactivate**).
- Confirmation dialog: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- On confirm:
  - **Deactivated** becomes **true**;
  - **Deactivated at** is set to the current date and time;
  - all active sessions for that user are revoked;
  - show message **`User deactivated.`**;
  - refresh the current screen in place.

### Activate

- Available from **Users list** overflow **Activate**, **User details** **Activate**, and **Edit user** when **Deactivated** is set to **false** (requires **Users.Deactivate**).
- Confirmation dialog: **`Activate "{full name}"? The user will be able to sign in again.`**
- On confirm:
  - **Deactivated** becomes **false**;
  - **Deactivated at** is cleared;
  - show message **`User activated.`**;
  - refresh the current screen in place.
- Activation does **not** restore previously revoked sessions and does **not** by itself complete invitation or email verification.

### Business rules

- An administrator **cannot** set their own **Deactivated** to **true**; the action is rejected with message **`You cannot deactivate your own account.`**
- Deactivating the first seeded administrator requires another user with **Deactivated** false, **Users.Deactivate**, and the **Administrator** role (REQ-ROL-006).
- Deactivation does **not** delete the user record, issue authorship, or comments.
- Users with **Deactivated** true are excluded from assignable-user selectors (REQ-ISS-002).

### Assignable users

- Assignable-user lists include only users with **Deactivated** false.
- When email verification is enabled (REQ-AUTH-011), assignable users must also have a **verified email** and a **local password**.
- Each option shows **Display label** (`displayLabel`): **`{first name} {last name} ({email})`** or **Email** only when both names are empty.

### Permissions and visibility

- **Users.Deactivate** is required for **Deactivate** and **Activate** actions.

---

# REQ-USR-006: Admin Send Password Reset

## Goal

An authorized administrator must be able to send a password reset link to a user who forgot their password.

## Features

### User details action

- **Send password reset** header action on **User details** (REQ-USR-004).
- Requires permission **Users.Manage**.
- Shown only when the account is enabled and the user **has a local password** (completed invite or registration).
- Confirmation dialog: **`Send a password reset link to "{email}"?`**
- On confirm, the system sends a **Password reset** email (REQ-AUTH-007) and shows message **`Password reset email sent.`**
- The action can be repeated; each send invalidates previous unused reset tokens for that user.

### Business rules

- Users with **Deactivated** true cannot receive a reset link; the action is not shown.
- Users **awaiting invitation acceptance** cannot receive a password reset link; use **Resend invitation** (REQ-INV-003) instead.

### Permissions and visibility

- **Users.Manage**: required for **Send password reset**.

---

# REQ-USR-007: Admin Confirm Email

## Goal

When email verification is enabled, an authorized administrator must be able to mark a user's email as verified without the user clicking the verification link — for example after self-registration.

## Features

### User details action

- **Confirm email** header action on **User details** (REQ-USR-004).
- Requires permission **Users.Manage**.
- Shown only when email verification is enabled (REQ-AUTH-011) and the user's **Email verified** is false (typically self-registered accounts).
- **Not shown** when the user was invited via **Invite user** and is already verified from the invitation email (REQ-INV-001).
- Shown for users with an email address on record regardless of **Deactivated**.
- Confirmation dialog: **`Mark email as verified for "{full name}"?`**
- On confirm:
  - **Email verified** becomes true;
  - **Email verified at** is set to the current time;
  - show message **`Email marked as verified.`**;
  - refresh **User details** in place.
- The action is **not shown** when **Email verified** is already true.

### Business rules

- **Confirm email** does not sign the user in and does not revoke or create sessions.
- Admin-invited users are already **email verified** when the invitation is sent; they must still complete invitation acceptance (via the email link **or** external sign-in) before they can use the application.
- Manual confirmation does not send email (REQ-AUTH-007).

### Permissions and visibility

- **Users.Manage**: required for **Confirm email**.
