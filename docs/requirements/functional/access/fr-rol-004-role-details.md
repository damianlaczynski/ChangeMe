---
id: FR-ROL-004
title: Role Details
domain: access
type: functional
status: active
depends_on: [FR-ROL-001, FR-ROL-003, FR-USR-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to review a role's metadata, permissions, and assigned users, and navigate to related administration flows.

## Functional requirements

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
- Each row shows permission **label** and **description** from FR-ROL-001, grouped by **Users**, **Roles**, **Sessions**.
- Permissions are read-only on this screen; editing happens on **Edit role** (FR-ROL-003).

### Assigned users section

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) for pagination and section loading unless stated below.
- Section title: **`Assigned users`**
- Visible with permission **Roles.View**.
- Table columns:

| Column      | Description                                                                                                |
| ----------- | ---------------------------------------------------------------------------------------------------------- |
| **Name**    | **First name** and **Last name** when set; **`—`** when both empty; link to **User details** (FR-USR-004). |
| **Email**   | User email address.                                                                                        |
| **Account** | Badge **`Active`** or **`Deactivated`** (from **Deactivated**).                                            |
| **Actions** | **Remove from role** when the user has **Roles.Manage**.                                                   |

- Default sort within section: **Name**, ascending.
- Search field placeholder within section: **`Search assigned users...`**; filters **name** and **email** (case-insensitive).
- Empty state: **`No users are assigned to this role.`**

### Header actions

| Action          | Permission required | Behavior                                    |
| --------------- | ------------------- | ------------------------------------------- |
| **Edit role**   | **Roles.Manage**    | Opens **Edit role** (custom roles only).    |
| **Delete role** | **Roles.Manage**    | Custom roles only; behavior per FR-ROL-003. |

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

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ROL-004-01 | Signed-in user without **Roles.View** | User navigates to **Role details** | Access is denied per FR-ROL-001 |
| AC-ROL-004-02 | Administrator with **Roles.View** on **Roles list** | User clicks a role **Name** link or **Open details** in the overflow menu | **Role details** opens for that role |
| AC-ROL-004-03 | Administrator with **Roles.View** on **Role details** | User views the role summary | Read-only **Name**, **Description** (or **`—`** when empty), **`{n} permissions`**, and **`{n} users`** are shown; seeded roles show **`System`** badge |
| AC-ROL-004-04 | Administrator with **Roles.View** on **Role details** | User views the **Permissions** section | Section title is **`Permissions`**; every assigned permission is listed with **label** and **description** from FR-ROL-001, grouped by **Users**, **Roles**, and **Sessions** |
| AC-ROL-004-05 | Administrator with **Roles.View** on **Role details** with assigned users | User views the **Assigned users** section | Table shows **Name**, **Email**, **Account** badge (**`Active`** or **`Deactivated`**), default sort **Name** ascending; search placeholder is **`Search assigned users...`** |
| AC-ROL-004-06 | Administrator with **Roles.View** on **Role details** for a role with no assigned users | User views **Assigned users** | Empty state **`No users are assigned to this role.`** is shown |
| AC-ROL-004-07 | Administrator with **Roles.Manage** on **Role details** for a **custom** role | User views header actions | **Edit role** and **Delete role** are shown |
| AC-ROL-004-08 | Administrator with **Roles.View** without **Roles.Manage** on **Role details** | User views header and row actions | **Edit role**, **Delete role**, and **Remove from role** are **not shown** |
| AC-ROL-004-09 | Administrator with **Roles.Manage** on **Role details** for **Administrator** or **User** | User views header actions | **Edit role** and **Delete role** are **not shown** |
| AC-ROL-004-10 | Administrator with **Roles.Manage** on **Role details**; target user retains at least one other role | User clicks **Remove from role** and confirms | Dialog **`Remove "{full name}" from role "{role name}"? The user will lose permissions granted only through this role.`**; toast **`User removed from role.`**; **Assigned users** list refreshes in place |
| AC-ROL-004-11 | Administrator with **Roles.Manage** on **Role details**; target user would have zero roles after removal | User clicks **Remove from role** and confirms | Removal is rejected with **`Each user must have at least one role. Assign another role before removing this one.`** |
| AC-ROL-004-12 | Administrator with **Roles.View** on **Role details** | User clicks a user **Name** in **Assigned users** | **User details** opens for that user (FR-USR-004) |
| AC-ROL-004-13 | Administrator with **Roles.View** on **Role details** | User clicks **Back** | Navigates to **Roles list** |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
