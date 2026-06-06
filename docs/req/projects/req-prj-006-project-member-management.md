---
id: REQ-PRJ-006
title: Project Member Management
domain: projects
status: active
depends_on: [REQ-PRJ-004, REQ-PRJ-005, REQ-USR-005]
---

## Goal

An authorized project member must be able to review current project members and, when permitted, add members, change member roles, and remove members.

## Features

### Members section on project details

- Section title: **`Members`**
- Visible on **Project details** (REQ-PRJ-004) when the user has **Project.Members.View** on the project.
- Member management actions require **Project.Members.Manage** on the project.

### Members table

| Column      | Description                                                                                                |
| ----------- | ---------------------------------------------------------------------------------------------------------- |
| **Name**    | **First name** and **Last name** when set; **`—`** when both empty; link to **User details** when allowed. |
| **Email**   | User email address.                                                                                        |
| **Role**    | Current project role badge: **Owner**, **Member**, or **Viewer**.                                          |
| **Account** | Badge **`Active`** or **`Deactivated`** (from **Deactivated**).                                            |
| **Actions** | Overflow menu (see below).                                                                                 |

- Default sort within section: **Name**, ascending.
- Search field placeholder within section: **`Search members...`**; filters **name** and **email** (case-insensitive).
- Empty state: **`No members in this project.`**
- While loading, a loading indicator is shown in the section.
- The members table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search resets to **page 1**.

### Add member

- **Add member** button is visible only with **Project.Members.Manage**.
- Opens **Add project member** dialog.

| Field    | Behavior                                                                                                                                                            |
| -------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **User** | Single-select from active users (**Deactivated** false) who are **not** already members of the project (REQ-USR-005); options show **Display label**. **Required**. |
| **Role** | Project role dropdown: **Owner**, **Member**, **Viewer**; **required**; default: **Member**.                                                                        |

- **Add member** confirmation action: on success show message **`Member added.`**; refresh the members list; append a **member added** entry to **Membership history** (REQ-PRJ-007).
- Duplicate membership is rejected with message **`User is already a member of this project.`**

### Row overflow menu

| Action            | Permission required        | Behavior                               |
| ----------------- | -------------------------- | -------------------------------------- |
| **Change role**   | **Project.Members.Manage** | Opens **Change member role** dialog.   |
| **Remove member** | **Project.Members.Manage** | Opens remove confirmation (see below). |

- Menu actions the current user lacks permission for are **not shown**.

### Change member role

- Dialog title: **`Change member role`**
- Shows read-only member **name** and **email**.
- **Role** dropdown: **Owner**, **Member**, **Viewer**; pre-filled with the current role; **required**.
- **Save** on success: show message **`Member role updated.`**; refresh the members list; append a **member role changed** entry to **Membership history** (REQ-PRJ-007).
- Changing a member to the same role they already hold is rejected with message **`Member already has this role.`**

### Remove member

- Confirmation dialog: **`Remove "{full name}" from project "{project name}"?`**
- Dialog presentation matches the standard destructive confirmation pattern used elsewhere (for example remove from role): warning icon; primary action **Remove** with danger styling; **Cancel** as secondary outlined button.
- On confirm: remove the member from the project; show message **`Member removed.`**; refresh the members list; append a **member removed** entry to **Membership history** (REQ-PRJ-007).

### Validation and business rules

- **User** on add: required; must be an active user not already a member.
- **Role** on add and change: required; one of **Owner**, **Member**, **Viewer**.
- A project cannot be left without an **Owner**; remove and role-change actions that would leave zero **Owners** are rejected with message **`Project must have at least one owner.`**
- The acting user cannot remove themselves when they are the sole **Owner**; rejected with message **`Assign another owner before removing yourself.`**
- The acting user cannot change their own role when they are the sole **Owner**; rejected with message **`Assign another owner before changing your own role.`**
- Deactivated users who remain listed as members cannot be added again; they stay visible with **Deactivated** badge until removed.

### Permissions and visibility

- **Project.Members.View**: required to open the **Members** section.
- **Project.Members.Manage**: required for **Add member**, **Change role**, and **Remove member**.
