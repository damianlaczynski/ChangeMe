---
id: FR-PRJ-004
title: Project Members and Settings
domain: projects
type: functional
status: active
depends_on: [FR-PRJ-002, FR-PRJ-003, FR-USR-002, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to review and manage project membership and manage the project profile from **Project settings** inside the project workspace. Stewardship is scoped per project through the **Owner** member role.

## Functional requirements

### Access

- Screen: **Project settings**
- Requires **Projects.View** and project access per FR-PRJ-001.
- Profile editing and member management require **Owner** on that project.

### Layout

- **Project settings** contains:
  1. **Project profile** form (FR-PRJ-002).
  2. **Members** section with add-member form (when permitted) and members table.

### Members table

| Column      | Description                                      |
| ----------- | ------------------------------------------------ |
| **User**    | Member display label (**Display label** format). |
| **Role**    | **Owner**, **Member**, or **Viewer**.            |
| **Joined**  | Date and time the member was added.              |
| **Actions** | **Remove** when the user is a project **Owner**. |

- Empty state: **`No members`**
- Users who are not **Owner** see the table without **Actions** and without the add-member form; roles are read-only badges.

### Add member form

- Visible only to project **Owners**.
- Fields:

| Field    | Behavior                                                                                                                                          |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| **User** | Single-select from assignable active users (FR-USR-005); **required**. Users already in the project are excluded. Placeholder: **`Select user`**. |
| **Role** | Dropdown: **Owner**, **Member**, **Viewer**; **required**; default **Member**.                                                                    |

- **Add member** button adds the selected user with the selected role.
- On success: toast **`Member added`**; members table refreshes in place; **User** field clears.
- Duplicate membership error: **`User is already a member of this project.`**
- Deactivated user cannot be added; validation error on **User**: **`user is deactivated`**.

### Change member role

- Project **Owners** can change a member's **Role** inline in the members table.
- On success: toast **`Member role updated`**; table refreshes in place.
- Demoting or removing the last **Owner** is rejected with error **`Cannot remove the last owner from the project.`**

### Remove member

- **Remove** opens confirmation: **`Remove "{display label}" from project "{project name}"?`** (HTML emphasis on display label and project name).
- Header: **`Remove member`**
- On success: toast **`Member removed`**; members table refreshes in place.
- Removing the last **Owner** is rejected with error **`Cannot remove the last owner from the project.`**

### Member roles (business rules)

| Role       | Meaning (requirements level)                                                 |
| ---------- | ---------------------------------------------------------------------------- |
| **Owner**  | Project stewardship: profile, members, and delete for **this project only**. |
| **Member** | Standard project participant; cannot manage project settings or membership.  |
| **Viewer** | Read-oriented participant; cannot manage project settings or membership.     |

- A project must have at least one **Owner**; removing or demoting the last **Owner** is rejected.
- **Private** projects are visible only to listed **Members** (FR-PRJ-001).
- On project create, the creator is added as **Owner**.

### Read-only vs manage

- Users who are not project **Owners** see **Project settings** in read-only mode:
  - Info message: **`You can view project settings, but only project owners can edit them.`**
  - **Save changes**, **Add member**, **Remove**, and inline role editing are **not shown**.
- Stewardship actions without **Owner** role are rejected with **`You do not have permission to manage this project.`**

### Permissions and visibility

- **Projects.Manage** is required only to **Create project** (FR-PRJ-002), not to manage existing projects globally.
- **Owner** (project member role) is required to edit project profile fields and manage members on that project.
- **Projects.View** allows read-only access to profile and members when the user can access the project.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) unless stated above.

## Out of scope

- Bulk import of project members.
- Inviting users who do not yet exist in the system from **Project settings** (use **Invite user**, FR-USR-002).
