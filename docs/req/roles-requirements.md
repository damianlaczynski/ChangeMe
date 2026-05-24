# Requirements - Roles and Permissions

This document covers six REQs for the **Roles and Permissions** area:
permission catalog, roles list, role create and edit flow, role details, role and user assignment management, and initial administrator setup.

Permissions are **fixed** in REQ-ROL-001 and are **not editable** from the application UI. Roles are **managed in the application** by selecting permissions from that catalog.

Role assignments are managed on **Create user** and **Edit user** (REQ-USR-003). From **Roles** administration, administrators can view assigned users and **Remove from role** on **Role details** (REQ-ROL-005).

---

# REQ-ROL-001: Permission Catalog and Effective Permissions

## Goal

The system must define a fixed set of permissions and determine each user's effective permissions from their assigned roles.

## Features

### Permission catalog

The catalog contains exactly these permissions:

| Permission             | Label (exact)        | Description                                                                   | Group    |
| ---------------------- | -------------------- | ----------------------------------------------------------------------------- | -------- |
| **Users.View**         | View users           | View the users list, user details, and read-only role badges on user screens. |
| **Users.Manage**       | Manage users         | Create and edit user profile data (name, email).                              | Users    |
| **Users.Deactivate**   | Deactivate users     | Deactivate and reactivate user accounts.                                      | Users    |
| **Roles.View**         | View roles           | View the roles list and role details.                                         | Roles    |
| **Roles.Manage**       | Manage roles         | Create, edit, and delete custom roles; manage role and user assignments.      | Roles    |
| **Sessions.ViewOwn**   | View own sessions    | View the current user's active sessions on **My account**.                    | Sessions |
| **Sessions.ManageOwn** | Manage own sessions  | Revoke non-current own sessions and use **Sign out everywhere**.              | Sessions |
| **Sessions.ViewAny**   | View user sessions   | View active sessions of any user in **User details**.                         | Sessions |
| **Sessions.ManageAny** | Manage user sessions | Revoke sessions of any user, including **Revoke all sessions**.               | Sessions |

- New permissions are added only by updating requirements and a subsequent release; administrators cannot create new permission codes in the UI.
- **Out of scope for this REQ:** issue-level permissions. Issues remain available to all authenticated users with **Deactivated** false until a separate requirements document introduces issue permissions.

### Effective permissions

- A user has **one or more roles**. Effective permissions are the **union** of all permissions from assigned roles, without duplicates.
- After sign-in, registration, or credential renewal (REQ-AUTH-001, REQ-AUTH-002), the user receives their current effective permission set.
- **My account** (REQ-USR-001) reflects the current effective permission set when opened.
- After an administrator changes role assignments (REQ-ROL-005), the new permissions apply when the affected user next renews credentials or signs in again.

### Access denial

- A signed-in user who lacks a required permission cannot perform the protected action; the system rejects the action with message **`You do not have permission to perform this action.`**
- A guest cannot perform actions that require sign-in.

### States and business rules

- A role with zero permissions cannot be saved; validation error: **`At least one permission is required.`**
- Users with **Deactivated** true cannot sign in and have no effective permissions (REQ-USR-005).

### Permissions and visibility

- Permission names in this REQ are used across **Users**, **Roles**, **Auth**, and **Sessions** requirements to control screen and action visibility.

---

# REQ-ROL-002: Roles List

## Goal

An authorized administrator must be able to browse roles, search and sort them, and open role administration flows.

## Features

### Search and actions bar

- Screen: **Roles list**
- Sidebar entry **Roles** is visible only with permission **Roles.View**.
- Search field placeholder: **`Search roles...`**
- Search matches **name** or **description** fragment (case-insensitive).
- **Search** button and form submit apply the current search text.
- **Add role** button opens **Create role** (REQ-ROL-003); visible only with permission **Roles.Manage**.

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

### Row overflow menu

| Action           | Permission required | Behavior                                                               |
| ---------------- | ------------------- | ---------------------------------------------------------------------- |
| **Open details** | **Roles.View**      | Opens **Role details**.                                                |
| **Edit role**    | **Roles.Manage**    | Opens **Edit role** for custom roles only.                             |
| **Delete role**  | **Roles.Manage**    | Shown for custom roles only; confirmation and behavior in REQ-ROL-003. |

- Menu actions the current user lacks permission for are **not shown**.
- **Edit role** and **Delete role** are **not shown** for system roles (**Administrator**, **User**); system roles are opened via **Open details** only.

### Pagination

- The roles table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area.

### Permissions and visibility

- **Roles.View**: required for **Roles list** and **Open details**.
- **Roles.Manage**: required for **Add role**, **Edit role**, and **Delete role**.

---

# REQ-ROL-003: Create and Edit Role

## Goal

An authorized administrator must be able to create custom roles and edit their name, description, and permissions.

## Features

### Create role screen

- Screen: **Create role**
- Requires permission **Roles.Manage**.

| Field           | Behavior                                                                                                           |
| --------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Name**        | Text field, **required**; **2–100** characters; unique case-insensitive.                                           |
| **Description** | Multiline text area, **not required**; max **500** characters; empty when omitted.                                 |
| **Permissions** | Checkbox list grouped by **Users**, **Roles**, **Sessions** (REQ-ROL-001); at least **one** checkbox **required**. |

- Each checkbox shows permission **label** and **description** from REQ-ROL-001.

### Edit role screen

- Screen: **Edit role**
- Requires permission **Roles.Manage**.
- Available only for **custom** roles.
- Same fields and rules as **Create role**, pre-filled with current role data.

### System role edit restriction

- **Administrator** and **User** system roles **cannot** be edited.
- Navigating to **Edit role** for a system role opens **Role details** in read-only mode with message **`System roles cannot be modified.`** and **Back** to **Roles list**.

### Validation

- **Name**: required; **2–100** characters; unique case-insensitive; inline error on duplicate: **`A role with this name already exists.`**
- **Description**: max **500** characters when not empty; inline error: **`Description cannot exceed 500 characters.`**
- **Permissions**: at least one selected; form-level error: **`At least one permission is required.`**
- Validation errors are inline on the relevant field or form-level for **Permissions**; the form stays open on failure.

### Form actions

- **Back** button and **Cancel** button navigate to **Roles list** when creating, or to **Role details** when editing, without saving.
- **Create role** button: on success show message **`Role created.`** and open **Role details** for the new role.
- **Save changes** button: on success show message **`Role saved.`** and open **Role details** for the edited role.

### Delete role

- **Delete role** is available from **Role details** and **Roles list** overflow menu (custom roles only).
- Confirmation dialog: **`Delete role "{role name}"? Users will lose permissions granted only through this role.`**
- On confirm: show message **`Role deleted.`** and navigate to **Roles list**.
- **System roles cannot be deleted**; **Delete role** is not shown for system roles.
- A role assigned to **one or more users** cannot be deleted; show message **`Role is assigned to one or more users. Remove all user assignments before deleting this role.`**

### System role rules

| Role              | **System** badge | Editable | Deletable            | Permissions                                               |
| ----------------- | ---------------- | -------- | -------------------- | --------------------------------------------------------- |
| **Administrator** | Yes              | No       | No                   | All catalog permissions; fixed.                           |
| **User**          | Yes              | No       | No                   | **Sessions.ViewOwn**, **Sessions.ManageOwn** only; fixed. |
| Custom roles      | No               | Yes      | Yes, when unassigned | Selected from catalog at create/edit time.                |

### States and business rules

- Creating or editing a role does **not** change user assignments; assignments are managed on **Create user** / **Edit user** and **Remove from role** on **Role details** (REQ-ROL-005).
- Permission changes on a role take effect for assigned users after their next credential renewal or sign-in.

### Permissions and visibility

- **Roles.Manage**: required to open **Create role**, **Edit role**, and **Delete role**.

---

# REQ-ROL-004: Role Details

## Goal

An authorized administrator must be able to review a role's metadata, permissions, and assigned users, and navigate to related administration flows.

## Features

### Role details screen

- Screen: **Role details**
- Requires permission **Roles.View**.
- Opened from **Roles list** (**Name** link or **Open details**).

### Role summary

| Field              | Behavior                                                                   |
| ------------------ | -------------------------------------------------------------------------- |
| **Name**           | Read-only role name.                                                       |
| **Description**    | Read-only description, or em dash (**`—`**) when empty.                    |
| **System**         | Read-only badge **`System`** for seeded roles; not shown for custom roles. |
| **Permissions**    | Read-only count in format **`{n} permissions`**.                           |
| **Assigned users** | Read-only count in format **`{n} users`**.                                 |

### Permissions section

- Section title: **`Permissions`**
- Lists every permission assigned to the role.
- Each row shows permission **label** and **description** from REQ-ROL-001, grouped by **Users**, **Roles**, **Sessions**.
- Permissions are read-only on this screen; editing happens on **Edit role** (REQ-ROL-003).

### Assigned users section

- Section title: **`Assigned users`**
- Visible with permission **Roles.View**.
- Table columns:

| Column      | Description                                                                                                 |
| ----------- | ----------------------------------------------------------------------------------------------------------- |
| **Name**    | **First name** and **Last name** when set; **`—`** when both empty; link to **User details** (REQ-USR-004). |
| **Email**   | User email address.                                                                                         |
| **Account** | Badge **`Active`** or **`Deactivated`** (from **Deactivated**).                                             |
| **Actions** | **Remove from role** when the user has **Roles.Manage**.                                                    |

- Default sort within section: **Name**, ascending.
- Search field placeholder within section: **`Search assigned users...`**; filters **name** and **email** (case-insensitive).
- Empty state: **`No users are assigned to this role.`**
- While loading, a loading indicator is shown in the section.
- The assigned users table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search resets to **page 1**.

### Header actions

| Action          | Permission required | Behavior                                     |
| --------------- | ------------------- | -------------------------------------------- |
| **Edit role**   | **Roles.Manage**    | Opens **Edit role** (custom roles only).     |
| **Delete role** | **Roles.Manage**    | Custom roles only; behavior per REQ-ROL-003. |

- Actions the current user lacks permission for are **not shown**.
- **Edit role** and **Delete role** are **not shown** for system roles.

### Row action — Remove from role

- **Remove from role** opens confirmation: **`Remove "{full name}" from role "{role name}"? The user will lose permissions granted only through this role.`**
- On confirm: remove the role from that user; show message **`User removed from role.`**; refresh the assigned users list in place.
- Removal is rejected when the user would have zero roles; show message **`Each user must have at least one role. Assign another role before removing this one.`**

### Actions and navigation

- **Back** returns to **Roles list**.
- Clicking a user **Name** in **Assigned users** opens **User details** for that user.

### Permissions and visibility

- **Roles.View**: required for **Role details**, **Permissions** section, and read-only **Assigned users** list.
- **Roles.Manage**: required for **Edit role**, **Delete role**, and **Remove from role**.

---

# REQ-ROL-005: Role and User Assignments

## Goal

An authorized administrator must be able to assign roles to users from **Create user** and **Edit user**, and remove a user from a role from **Role details**, using consistent rules in both places.

## Features

### Entry point — Users administration

- Role assignment for a user is performed on **Create user** and **Edit user** (REQ-USR-003); there is no separate role-assignment screen under **Users**.
- **Roles** multi-select lists all roles sorted by **name** ascending; each option shows role **name** and **System** badge when applicable.
- At least **one** role must be selected before save.
- Saving **Create user** or **Edit user** replaces the user's entire role set with the selected set.
- The **Roles** field is visible and editable only with permission **Roles.Manage**.
- Without **Roles.Manage**, **Create user** is **not available** (every new user must receive role assignment at creation).
- **Permissions** preview on create/edit follows REQ-USR-003.

- **User details** (REQ-USR-004) shows assigned roles and effective permissions read-only; role badges link to **Role details**.
- Role changes are made only through **Edit user**, not from **User details**.

### Entry point — Roles administration

- **Role details** (REQ-ROL-004) shows the **Assigned users** section and **Remove from role** per user.
- Adding a user to a role is done by editing the user (**Edit user**) and selecting the role in the **Roles** multi-select.

### Validation

- **Roles** (on **Create user** and **Edit user**): at least one role selected; error: **`At least one role is required.`**
- **Remove from role** (on **Role details**): must not leave the user with zero roles; error: **`Each user must have at least one role. Assign another role before removing this one.`**

### Self-service restrictions

- Standard users cannot change their own roles.
- On **Edit user**, when the administrator edits **their own** account, the **Roles** field and **Permissions** preview are **not shown**.
- Self role change from any entry point is rejected with message **`You cannot change your own roles.`**

### States and business rules

- Assignment changes from either entry point use the same replace semantics and validation rules.
- Changes take effect for the affected user after credential renewal or next sign-in.
- Changing role assignments does **not** revoke active sessions.
- A user with **Deactivated** true can remain assigned to a role; deactivation is controlled in REQ-USR-005.

### Permissions and visibility

- **Roles.Manage**: required for the **Roles** field on **Create user** / **Edit user** and for **Remove from role** on **Role details**.
- **Roles.View**: allows read-only **Roles** and **Permissions** on **User details** and read-only **Assigned users** on **Role details** when the user lacks **Roles.Manage**.

---

# REQ-ROL-006: Initial Administrator and System Roles

## Goal

When the application is first deployed, the system must provide seeded system roles and a first administrator account so user and role administration can begin without manual data preparation.

## Features

### Initial administrator

The deployment supplies these values for the first administrator:

| Value          | Required |
| -------------- | -------- |
| **Email**      | Yes      |
| **Password**   | Yes      |
| **First name** | Yes      |
| **Last name**  | Yes      |

- On first startup, if no administrator account exists for the configured **Email**, the system creates an administrator user with **Deactivated** false, the supplied profile, and password.
- If an administrator with that **Email** already exists, the system does **not** recreate the account or reset the password.
- The first administrator is assigned the **Administrator** role.
- The first administrator signs in through **Login** (REQ-AUTH-001) and can access **Users list**, **Roles list**, and session administration per their permissions.

### Seeded system roles

On first startup, the system ensures these roles exist:

| Role              | **System** badge | Permissions                                        |
| ----------------- | ---------------- | -------------------------------------------------- |
| **Administrator** | Yes              | All permissions from REQ-ROL-001.                  |
| **User**          | Yes              | **Sessions.ViewOwn**, **Sessions.ManageOwn** only. |

- If the **Administrator** role already exists, the system adds any newly defined catalog permissions that role does not yet have.
- System roles follow edit, delete, and assignment rules from REQ-ROL-003, REQ-ROL-004, and REQ-ROL-005.

### Registration default role

- Public registration (REQ-AUTH-001), when **Public registration enabled** is **true** (REQ-AUTH-012), assigns the **User** role automatically.
- Registration does **not** assign **Administrator**.

### States and business rules

- Initial administrator **Password** values must not appear in user-visible logs or messages.
- Production deployments must use a strong, unique password for the initial administrator; password expiration (REQ-AUTH-009) and two-factor authentication (REQ-AUTH-013) apply under the same rules as for other accounts when enabled.
- The initial administrator account is created with **Email verified** true so sign-in is possible when email verification is enabled (REQ-AUTH-011).

### Out of scope

- **Out of scope for this REQ:** forced password change on first sign-in (including the seeded administrator).

### Permissions and visibility

- The seeded **Administrator** role grants all permissions from REQ-ROL-001, including **Roles.Manage** and **Users.Manage**.
