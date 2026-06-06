---
id: REQ-PRJ-004
title: Project Details
domain: projects
status: active
depends_on: [REQ-PRJ-003, REQ-PRJ-005, REQ-PRJ-006, REQ-ISS-001]
---

## Goal

An authorized project member must be able to review a project's metadata, members, histories, and issue count, and navigate to related flows.

## Features

### Project details screen

- Screen: **Project details**
- Requires **Project.View** on the project.
- Opened from **Projects list** (**Name** link or **Open details**).
- Users without **Project.View** on the project cannot open **Project details**; the system rejects access with message **`You do not have permission to perform this action on this project.`**

### Project summary

| Field           | Behavior                                                                                                                                                                                                                                                   |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Name**        | Read-only project name.                                                                                                                                                                                                                                    |
| **Description** | Read-only description, or em dash (**`—`**) when empty.                                                                                                                                                                                                    |
| **System**      | Read-only badge **`System`** for the **Default** project; not shown for custom projects.                                                                                                                                                                   |
| **My role**     | Read-only badge for the current user's project role: **Owner**, **Member**, or **Viewer**.                                                                                                                                                                 |
| **Issues**      | Read-only count in format **`{n} issues`**.                                                                                                                                                                                                                |
| **Logged time** | Read-only total logged time for the project in REQ-TIM-007 format; visible with **Project.Time.View**; **`0m`** when none. **View time report** link opens **Time reports** filtered to this project when the user has **Time.ViewReports** (REQ-TIM-007). |

### Issues section

- Section title: **`Issues`**
- Visible when the user has **Project.Issues.View** on the project.
- Shows the same issue count as **Issues** in the summary.
- When the count is greater than **0**, a **View issues** link opens **Issues list** filtered to this project only.
- When the count is **0**, empty state: **`No issues in this project.`**
- The issues section does **not** embed the full issues table.

### Members section

- **Members** section per REQ-PRJ-006.

### History tabs

- **Membership history** tab per REQ-PRJ-007.
- **Operations** tab per REQ-PRJ-008.

### Header actions

| Action             | Permission required on project | Behavior                                        |
| ------------------ | ------------------------------ | ----------------------------------------------- |
| **Edit project**   | **Project.Manage**             | Opens **Edit project** (custom projects only).  |
| **Delete project** | **Project.Manage**             | Custom projects only; behavior per REQ-PRJ-003. |

- Actions the current user lacks permission for are **not shown**.
- **Edit project** and **Delete project** are **not shown** for the **Default** system project.

### Actions and navigation

- **Back** returns to **Projects list**.
- **View issues** opens **Issues list** with the **Project** filter set to this project.

### Permissions and visibility

- **Project.View**: required for **Project details**, **Operations** tab (REQ-PRJ-008), and history tabs area.
- **Project.Manage**: required for **Edit project** and **Delete project**.
- **Project.Issues.View**: required for the **Issues** section and **View issues** link.
- **Project.Members.View**: required for the **Members** section and **Membership history** tab (REQ-PRJ-007).
- **Project.Time.View**: required for **Logged time** in the summary; **View time report** requires **Time.ViewReports**.
