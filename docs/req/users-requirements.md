# Requirements - Users

This document covers five REQs for the **Users** area:
my account profile, user list, admin create/edit flow, user details with session administration, and account deactivation.

Role assignment is performed on **Create user** and **Edit user** (REQ-USR-003). Removing a user from a role is available on **Role details** (REQ-ROL-005).

---

# REQ-USR-001: My Account Profile

## Goal

The signed-in user must be able to view and update their own profile and reach account security actions.

## Features

### My account screen

- Screen: **My account**
- Sidebar entry: **My account** (visible to all authenticated **Active** users).

### Profile section

| Field            | Behavior                                         |
| ---------------- | ------------------------------------------------ |
| **First name**   | Editable; **required**; max **100** characters.  |
| **Last name**    | Editable; **required**; max **100** characters.  |
| **Email**        | **Read-only**.                                   |
| **Status**       | Read-only badge: **`Active`** or **`Inactive`**. |
| **Member since** | Read-only account creation date and time.        |

### Security actions

- Button **Change password** opens **Change password** (REQ-AUTH-005).
- Button **Active sessions** opens **My sessions** (REQ-AUTH-004).

### Validation

- **First name** and **Last name** follow the same rules as registration (REQ-AUTH-001); errors are inline on the relevant field.

### Form actions

- **Save changes** button: validate, save profile, show message **`Profile updated.`**, and refresh displayed values in place.
- The screen does **not** expose role assignment or account status changes.

### Permissions and visibility

- Any authenticated **Active** user can view and edit their own **First name** and **Last name**.
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

| Column           | Description                                                                                          |
| ---------------- | ---------------------------------------------------------------------------------------------------- |
| **Name**         | Full name; link to **User details**.                                                                 |
| **Email**        | User email address.                                                                                  |
| **Status**       | Badge **`Active`** or **`Inactive`**.                                                                |
| **Roles**        | One status badge per assigned role showing the role name.                                            |
| **Last sign-in** | Most recent session **signed in at** across all sessions; **`Never`** when the user has no sessions. |
| **Created at**   | Account creation date and time.                                                                      |
| **Actions**      | Overflow menu (see below).                                                                           |

### Sorting

- Sortable columns: **Name**, **Created at**, **Last sign-in**.
- Default sort: **Name**, ascending.

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Status** multi-select: **`Active`**, **`Inactive`**. Empty selection means no status restriction.
- Default filter state: **no status restriction** (all statuses shown).
- Filters combine with search text using **AND** logic.
- **Apply filters** submits filters with the current search text.
- **Clear filters** resets the filter form and removes all filter constraints from the active query.
- Applied filters list

### Row overflow menu

| Action           | Permission required  | Behavior                                         |
| ---------------- | -------------------- | ------------------------------------------------ |
| **Open details** | **Users.View**       | Opens **User details**.                          |
| **Edit**         | **Users.Manage**     | Opens **Edit user**.                             |
| **Deactivate**   | **Users.Deactivate** | Shown only for **Active** users (REQ-USR-005).   |
| **Activate**     | **Users.Deactivate** | Shown only for **Inactive** users (REQ-USR-005). |

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

An authorized administrator must be able to create users and update their profile data and role assignments.

## Features

### Create user screen

- Screen: **Create user**
- Requires permission **Users.Manage**.

| Field                | Behavior                                                      |
| -------------------- | ------------------------------------------------------------- |
| **First name**       | **Required**; max **100** characters.                         |
| **Last name**        | **Required**; max **100** characters.                         |
| **Email**            | **Required**; valid email; unique; max **320** characters.    |
| **Password**         | **Required**; **8–128** characters.                           |
| **Confirm password** | **Required**; must match **Password**.                        |
| **Roles**            | Multi-select; assignment rules per REQ-ROL-005.               |
| **Status**           | Dropdown **`Active`** / **`Inactive`**; default **`Active`**. |

- **Roles** field is visible and editable only with permission **Roles.Manage**. Creating a user requires **Roles.Manage** so every new user receives role assignment.

### Effective permissions preview (create and edit)

- Below the **Roles** field, a read-only section **Effective permissions** shows the union of permissions from the currently selected roles (REQ-ROL-001).
- Each permission row shows:
  - **Label** and **description** from the catalog;
  - **From roles** — comma-separated list of selected role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one selected role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**.
- The preview updates immediately when the **Roles** selection changes, before save.
- When no role is selected, the section shows: **`Select at least one role to preview effective permissions.`**
- When the selected roles grant no permissions, the section shows: **`No permissions.`**
- The section is read-only; it does not replace the **Roles** field for assignment.
- The preview is shown only when the **Roles** field is visible (requires **Roles.Manage**).

### Edit user screen

- Screen: **Edit user**
- Requires permission **Users.Manage**.

| Field          | Behavior                                                                             |
| -------------- | ------------------------------------------------------------------------------------ |
| **First name** | **Required**; max **100** characters.                                                |
| **Last name**  | **Required**; max **100** characters.                                                |
| **Email**      | **Required**; valid email; unique; max **320** characters.                           |
| **Roles**      | Same rules as create (REQ-ROL-005); visible and editable only with **Roles.Manage**. |
| **Status**     | Dropdown **`Active`** / **`Inactive`**; editable only with **Users.Deactivate**.     |

- **Password** fields are **not shown** on edit.
- **Edit user** is the screen for managing a user's role assignments; there is no separate role-assignment screen in **Users** administration.
- **Out of scope for this REQ:** administrator-initiated password reset.

### Validation

- Duplicate email shows form-level error: **`A user with this email already exists.`**
- **Roles**: validation and save behavior per REQ-ROL-005 (entry point — Users administration).
- Other field errors are inline on the relevant field.

### Form actions

- **Back** button and **Cancel** button navigate to **Users list** when creating, or to **User details** when editing, without saving.
- **Create user** button: on success show message **`User created.`** and open **User details** for the new user.
- **Save changes** button: on success show message **`User saved.`** and open **User details** for the edited user.

### Business rules

- Public registration (REQ-AUTH-001) remains available; registered users receive the **User** role automatically (REQ-ROL-006).
- Admin-created users receive exactly the roles selected in the form; no implicit **Administrator** assignment.
- An administrator **cannot** remove their own **Administrator** role assignment; save is rejected with message **`You cannot remove your own administrator access.`**
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field is **not shown**; **Effective permissions** preview is **not shown**.
- An administrator **cannot** set their own **Status** to **Inactive**; save is rejected with message **`You cannot deactivate your own account.`**

### Permissions and visibility

- **Users.Manage**: required to open create and edit screens and save profile fields.
- **Roles.Manage**: required to view and edit the **Roles** field.
- **Users.Deactivate**: required to view and edit the **Status** field.

---

# REQ-USR-004: User Details and Session Administration

## Goal

An authorized administrator must be able to inspect a user's account, roles, effective permissions, and active sessions and revoke sessions when needed.

## Features

### User details screen

- Screen: **User details**
- Requires permission **Users.View**.

### Profile summary

Displays read-only: **Name**, **Email**, **Status** badge, **Member since**, and **Last sign-in**.

### Roles section

- Section title: **`Roles`**
- Shows one badge per assigned role name.
- Each role badge is a link to **Role details** for that role (REQ-ROL-004).
- Empty state: **`No roles assigned.`**

### Effective permissions section

- Section title: **`Effective permissions`**
- Read-only list of the user's effective permissions (union of all assigned roles, REQ-ROL-001).
- Each row shows:
  - permission **label** and **description**;
  - **From roles** — comma-separated list of assigned role **names** that grant this permission (for example **`From roles: Administrator, Support`**).
- When a permission is granted by only one assigned role, the text uses singular: **`From role: User`**.
- Rows are grouped by **Users**, **Roles**, **Sessions**.
- Empty state when the user has roles but no permissions in the union: **`No permissions.`**
- This section is informational; role changes are made on **Edit user** (REQ-USR-003).

### Header actions

| Action                  | Permission required    | Behavior                                                                                                     |
| ----------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------ |
| **Edit**                | **Users.Manage**       | Opens **Edit user** (profile, status, and role assignments when permitted).                                  |
| **Deactivate**          | **Users.Deactivate**   | Shown for **Active** users; confirmation and behavior per REQ-USR-005.                                       |
| **Activate**            | **Users.Deactivate**   | Shown for **Inactive** users; confirmation and behavior per REQ-USR-005.                                     |
| **Revoke all sessions** | **Sessions.ManageAny** | Opens confirmation: **`Revoke all active sessions for this user? They will be signed out on every device.`** |

- Actions the current user lacks permission for are **not shown**.

### Actions and navigation

- Clicking a role badge in **Roles** opens **Role details** for that role.
- **Back** returns to **Users list**.

### Active sessions section

- Visible only with permission **Sessions.ViewAny**.
- Section title: **`Active sessions`**
- Table columns match **My sessions** (REQ-AUTH-004): **Device / browser**, **IP address**, **Session type**, **Signed in at**, **Last activity**, **Actions**.
- The **Current session** badge is **not shown** in the administrator view.
- **Revoke** button on each row requires **Sessions.ManageAny** and opens confirmation: **`Revoke this session? That device will be signed out.`**
- Empty state: **`No active sessions.`**
- While loading, a loading indicator is shown in the section.
- The sessions table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.

### States and business rules

- **Inactive** users display **Status** badge **`Inactive`**; the active sessions table shows empty state **`No active sessions.`**
- Revoking a session signs out that device on next activity; the list refreshes on the current page.

### Permissions and visibility

- **Users.View**: required for **User details**, **Roles** section, and **Effective permissions** section.
- **Sessions.ViewAny**: required to render the active sessions section.
- **Sessions.ManageAny**: required for **Revoke** on session rows and **Revoke all sessions**.

---

# REQ-USR-005: Deactivate and Activate Accounts

## Goal

An authorized administrator must be able to deactivate and reactivate user accounts, immediately removing access for deactivated users.

## Features

### Deactivate

- Available from **Users list** overflow **Deactivate**, **User details** **Deactivate**, and **Edit user** when **Status** is set to **Inactive** (requires **Users.Deactivate**).
- Confirmation dialog: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- On confirm:
  - user **Status** becomes **Inactive**;
  - all active sessions for that user are revoked;
  - show message **`User deactivated.`**;
  - refresh the current screen in place.

### Activate

- Available from **Users list** overflow **Activate**, **User details** **Activate**, and **Edit user** when **Status** is set to **Active** (requires **Users.Deactivate**).
- Confirmation dialog: **`Activate "{full name}"? The user will be able to sign in again.`**
- On confirm:
  - user **Status** becomes **Active**;
  - show message **`User activated.`**;
  - refresh the current screen in place.
- Activation does **not** restore previously revoked sessions.

### Business rules

- An administrator **cannot** deactivate their own account; the action is rejected with message **`You cannot deactivate your own account.`**
- Deactivating the first seeded administrator requires another **Active** user with **Users.Deactivate** and the **Administrator** role (REQ-ROL-006).
- Deactivation does **not** delete the user record, issue authorship, or comments.
- **Inactive** users are excluded from assignable-user selectors (REQ-ISS-002).

### Assignable users

- Assignable-user lists (for example **Assigned to** on issues, REQ-ISS-002) include **Active** users only.

### Permissions and visibility

- **Users.Deactivate** is required for **Deactivate** and **Activate** actions.
