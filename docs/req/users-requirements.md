# Requirements - Users

This document covers seven REQs for the **Users** area:
my account profile, user list, admin invite flow, user details with session administration, account deactivation, admin email confirmation, and resend invitation.

Role assignment is performed on **Create user** and **Edit user** (REQ-USR-003). Removing a user from a role is available on **Role details** (REQ-ROL-005).

## Account model (all Users REQs)

Administrative enablement is separate from onboarding and email proof.

| Concept                      | Storage                         | Meaning                                                                                                                         |
| ---------------------------- | ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| **Deactivated**              | Boolean; default **false**      | **true** — administrator disabled the account; cannot sign in; no effective permissions. **false** — account is enabled.        |
| **Deactivated at**           | Date and time; empty when false | Set when **Deactivated** becomes **true**; cleared when **Deactivated** becomes **false**.                                      |
| **Has password set**         | Boolean                         | **false** — invitation pending (REQ-AUTH-010).                                                                                  |
| **Email verified**           | Boolean                         | Meaning depends on how the account was created (see below).                                                                     |
| **Email verified at**        | Date and time; empty when false | Set when **Email verified** becomes **true**.                                                                                   |
| **Password last changed at** | Date and time                   | Set when the user first receives a password and on each successful password change (REQ-AUTH-009).                              |
| **Invitation sent at**       | Date and time                   | Set when **Create user** or **Resend invitation** (REQ-USR-008) sends an invitation email; shown in the **Invitation** section. |

**Email verified** rules when **Email verification enabled** is **true** (REQ-AUTH-011):

- **Self-registration:** **false** until the user completes **Verify email**; **Email verified at** set on success.
- **Admin create user (invitation):** **true** when **Create user** succeeds — the invitation is sent to that email address, which is treated as confirmed.
- **Initial administrator:** **true** at creation (REQ-ROL-006).
- **Accept invitation:** remains **true** if already set at invite; otherwise set on success (same mailbox proof).

When verification is disabled, every account is **Email verified** true.

**Account** badge (UI only, derived from **Deactivated**): **`Active`** when **Deactivated** is **false**; **`Deactivated`** when **true**.

**Account state** (UI only, read-only): **`Complete`**, **`Awaiting invitation`**, or **`Awaiting email verification`** — derived from the flags above when **Deactivated** is **false**; hidden or **`—`** when the account is deactivated.

**Profile name:** **Full name** is shown when **First name** and **Last name** are both set; otherwise UI shows **`Pending profile`**. On admin invite, **First name** and **Last name** are **optional** on **Create user** and **Edit user** (REQ-USR-003). On **Accept invitation** (REQ-AUTH-010), fields are pre-filled from values already stored (including admin-set names) and the user may edit them before submit.

**Password expires at (admin UI only):** Not stored. When **Password expiration enabled** is **true** (REQ-AUTH-009), **User details** shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)**. Omitted when expiration is disabled; **`—`** when **Has password set** is **false**. Not shown on **My account** (REQ-USR-001).

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
- **Change password** button (header action) opens **Change password** (REQ-AUTH-005).
- **Sign out everywhere** button (header action) when the user has **Sessions.ManageOwn**; same behavior as REQ-AUTH-003.

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

### Validation (edit profile)

- **First name** and **Last name** follow the same rules as registration (REQ-AUTH-001); errors are inline on the relevant field.

### Permissions and visibility

- Any authenticated user with **Deactivated** false can view **My account** and open **Edit profile** to change **First name** and **Last name**.
- **My account** does **not** expose role assignment or account status changes.
- **Out of scope for this REQ:** email change.

---

# REQ-USR-002: User List

## Goal

An authorized administrator must be able to browse users, search and filter them, and open administration flows.

## Features

### Search and actions bar

- Screen: **Users list**
- Sidebar entry **Users** is visible only with permission **Users.View**.
- **Add user** button opens **Create user**; visible only with permission **Users.Manage**.

### Users table

| Column             | Description                                                                                                                                  |
| ------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**           | **Full name** when **First name** and **Last name** are both set; **`Pending profile`** otherwise; link to **User details**.                 |
| **Email**          | User email address.                                                                                                                          |
| **Account**        | Badge **`Active`** or **`Deactivated`**.                                                                                                     |
| **Account state**  | **`Complete`**, **`Awaiting invitation`**, or **`Awaiting email verification`** when **Deactivated** is **false**; omitted when deactivated. |
| **Email verified** | Badge **`Verified`** or **`Unverified`** when email verification is enabled (REQ-AUTH-011); omitted when verification is disabled.           |
| **Roles**          | One status badge per assigned role showing the role name.                                                                                    |
| **Last sign-in**   | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions.                                         |
| **Created at**     | Account creation date and time.                                                                                                              |
| **Actions**        | Overflow menu (see below).                                                                                                                   |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Account** multi-select: **`Active`** (**Deactivated** false), **`Deactivated`** (**Deactivated** true). Empty selection means no restriction.
- **Email verified** multi-select: **`Verified`**, **`Unverified`**. Shown only when email verification is enabled (REQ-AUTH-011). Empty selection means no restriction.
- Default filter state: **no account restriction**; **no email verified restriction** when that filter is shown.
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
- **Users.Manage**: required for **Add user** and **Edit**.
- **Users.Deactivate**: required for **Deactivate** and **Activate**.

---

# REQ-USR-003: Create and Edit User (Admin)

## Goal

An authorized administrator must be able to invite users by email and role assignment, optionally set profile name at invite or edit time, and manage role assignments.

## Features

### Create user screen

- Screen: **Create user**
- Requires permission **Users.Manage**.

| Field          | Behavior                                                   |
| -------------- | ---------------------------------------------------------- |
| **First name** | **Optional**; max **100** characters.                      |
| **Last name**  | **Optional**; max **100** characters.                      |
| **Email**      | **Required**; valid email; unique; max **320** characters. |
| **Roles**      | Multi-select; assignment rules per REQ-ROL-005.            |

- When **First name** and **Last name** are omitted, the invited user supplies them on **Accept invitation** (REQ-AUTH-010). When provided, they are stored and pre-filled on **Accept invitation** for the user to confirm or edit.
- New users are created with **Deactivated** **false**; **Deactivated** is **not shown** on **Create user** (use **Deactivate** / **Activate** or **Edit user** to change later).

- **Roles** field is visible and editable only with permission **Roles.Manage**. Creating a user requires **Roles.Manage** so every new user receives role assignment.

### Permissions preview (create and edit)

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

| Field           | Behavior                                                                                                                          |
| --------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| **First name**  | **Required** when **Has password set** is **true**; **optional** when invitation is pending; max **100** characters.              |
| **Last name**   | **Required** when **Has password set** is **true**; **optional** when invitation is pending; max **100** characters.              |
| **Email**       | **Required**; valid email; unique; max **320** characters.                                                                        |
| **Roles**       | Same rules as create (REQ-ROL-005); visible and editable only with **Roles.Manage**.                                              |
| **Deactivated** | Checkbox; editable only with **Users.Deactivate**; label **`Deactivated`**. When checked, the account badge is **`Deactivated`**. |

- **Password** fields are **not shown** on create or edit.
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.

### Validation

- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per REQ-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Back** button and **Cancel** button navigate to **Users list** when creating, or to **User details** when editing, without saving.
- **Create user** button: on success show message **`User created. An invitation email has been sent.`** and open **User details** for the new user.
- **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- When **Public registration enabled** is **true** (REQ-AUTH-012), self-registration (REQ-AUTH-001) remains available; registered users receive the **User** role automatically (REQ-ROL-006). When disabled, new accounts are created only through admin **Create user**.
- Admin-created users receive exactly the roles selected in the form; no implicit **Administrator** assignment.
- Admin-created users are **invite-pending** until they complete **Accept invitation** (password required; name required on accept if not already complete) (REQ-AUTH-010).
- On **Create user** success, **Email verified** is **true** and **Email verified at** is set — the invitation is sent to that email address (REQ-AUTH-011).
- On **Create user** success, **Invitation sent at** is set to the current date and time.
- The system sends an **Account invitation** email when **Create user** succeeds (REQ-AUTH-007).
- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Permissions** preview is **not shown**.
- An administrator **cannot** set their own **Deactivated** to **true**; save is rejected with message **`You cannot deactivate your own account.`**

### Permissions and visibility

- **Users.Manage**: required to open create and edit screens and save profile fields.
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

| Field                        | Behavior                                                                                                                                                                                                                                                                                    |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**                     | **Full name** when **First name** and **Last name** are both set; **`Pending profile`** otherwise.                                                                                                                                                                                          |
| **Email**                    | Email address.                                                                                                                                                                                                                                                                              |
| **Account**                  | Badge **`Active`** or **`Deactivated`**.                                                                                                                                                                                                                                                    |
| **Account state**            | **`Complete`**, **`Awaiting invitation`**, or **`Awaiting email verification`** when **Deactivated** is **false**; **`—`** when deactivated.                                                                                                                                                |
| **Email verified**           | Badge **`Verified`** or **`Unverified`** when email verification is enabled (REQ-AUTH-011); omitted when verification is disabled.                                                                                                                                                          |
| **Email verified at**        | Date and time when **Email verified** is true; omitted when verification is disabled or **Email verified** is false.                                                                                                                                                                        |
| **Member since**             | Account creation date and time.                                                                                                                                                                                                                                                             |
| **Last sign-in**             | Most recent session **signed in at**; **`Never`** when the user has no sessions.                                                                                                                                                                                                            |
| **Password last changed at** | Date and time; **`—`** when the user has no password yet (invite pending).                                                                                                                                                                                                                  |
| **Password expires at**      | Read-only, **UI only** (not stored). Shown only when **Password expiration enabled** is **true** (REQ-AUTH-009): **Password last changed at** + **Maximum password age (days)**; **`—`** when invite pending or **Password last changed at** is empty; omitted when expiration is disabled. |
| **Deactivated at**           | Date and time when **Deactivated** is **true**; omitted when **Deactivated** is **false**.                                                                                                                                                                                                  |

### Invitation section

- Collapsible section **Invitation**; shown only when **Has password set** is **false**.
- Section title: **`Invitation`**
- Displays read-only:
  - **Invitation status:** **`Pending`** — user has not yet accepted the invitation.
  - **Invitation sent at:** **Invitation sent at** date and time (last invitation email from **Create user** or **Resend invitation**).
  - **Email verified:** **`Yes`** when email verification is enabled — the invitation was sent to this email address.
  - **Profile name:** **Full name** when both names are set (admin and/or user); **`Not set`** when both are empty; user confirms or updates on **Accept invitation** (REQ-AUTH-010).
- **Resend invitation** action (REQ-USR-008) is available in this section and in the screen header.
- Empty state when **Has password set** is **true**: section is **not shown** (not an empty panel).

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
| **Resend invitation**   | **Users.Manage**       | Shown when **Has password set** is **false**; behavior per REQ-USR-008.                                                                                                                                                                                                                   |
| **Deactivate**          | **Users.Deactivate**   | Shown when **Deactivated** is **false**; confirmation and behavior per REQ-USR-005.                                                                                                                                                                                                       |
| **Activate**            | **Users.Deactivate**   | Shown when **Deactivated** is **true**; confirmation and behavior per REQ-USR-005.                                                                                                                                                                                                        |
| **Revoke all sessions** | **Sessions.ManageAny** | Opens confirmation: **`Revoke all active sessions for this user? They will be signed out on every device.`**                                                                                                                                                                              |
| **Send password reset** | **Users.Manage**       | Sends password reset email; confirmation: **`Send a password reset link to "{email}"?`**; success message: **`Password reset email sent.`**                                                                                                                                               |
| **Confirm email**       | **Users.Manage**       | Shown when email verification is enabled and **Email verified** is false (typical for self-registered users); not shown for admin-invited users who are already verified; confirmation: **`Mark email as verified for "{full name}"?`**; success message: **`Email marked as verified.`** |

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

- Users with **Deactivated** true display **Account** badge **`Deactivated`**; the active sessions table shows empty state **`No active sessions.`**
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
- When email verification is enabled (REQ-AUTH-011), assignable users must also have **Email verified** true and **Has password set** true.

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
- Shown only when **Deactivated** is **false** and **Has password set** is true (user completed invite or registration).
- Confirmation dialog: **`Send a password reset link to "{email}"?`**
- On confirm, the system sends a **Password reset** email (REQ-AUTH-007) and shows message **`Password reset email sent.`**
- The action can be repeated; each send invalidates previous unused reset tokens for that user.

### Business rules

- Users with **Deactivated** true cannot receive a reset link; the action is not shown.
- Invite-pending users (**Has password set** false) cannot receive a password reset link; use **Resend invitation** (REQ-USR-008) instead.

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
- **Not shown** when the user was created via **Create user** and is already verified from the invitation email (REQ-USR-003).
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
- Admin-invited users are already **Email verified** when the invitation is sent; they still must **Accept invitation** before sign-in if **Has password set** is false.
- Manual confirmation does not send email (REQ-AUTH-007).

### Permissions and visibility

- **Users.Manage**: required for **Confirm email**.

---

# REQ-USR-008: Resend Invitation

## Goal

An authorized administrator must be able to send a new invitation link to a user who has not yet accepted a previous invitation.

## Features

### Resend invitation action

- **Resend invitation** header action on **User details** (REQ-USR-004) and in the **Invitation** section.
- Requires permission **Users.Manage**.
- Shown only when **Has password set** is **false** (invitation still pending).
- Shown only when **Deactivated** is **false**.
- Confirmation dialog: **`Resend invitation to "{email}"? A new invitation link will be sent. Previous unused links will stop working.`**
- On confirm:
  - the system issues a new invitation token and sends **Account invitation** email (REQ-AUTH-007);
  - previous unused invitation tokens for that user are invalidated;
  - **Invitation sent at** is updated to the current date and time (displayed in the **Invitation** section);
  - show message **`Invitation resent.`**;
  - refresh the current screen in place.

### Business rules

- **Resend invitation** does not change assigned roles or **Email verified** (remains **true** when verification is enabled).
- The action can be repeated; each send invalidates earlier unused invitation links.
- Users with **Has password set** true do not show this action.
- Users with **Deactivated** true do not show this action.

### Permissions and visibility

- **Users.Manage**: required for **Resend invitation**.
