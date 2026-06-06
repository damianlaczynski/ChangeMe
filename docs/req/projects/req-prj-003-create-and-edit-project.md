---
id: REQ-PRJ-003
title: Create and Edit Project
domain: projects
status: active
depends_on: [REQ-PRJ-001, REQ-PRJ-005, REQ-PRJ-007, REQ-PRJ-008]
---

## Goal

An authenticated user must be able to create custom projects, and project **Owners** must be able to edit and delete their projects.

## Features

### Create project screen

- Screen: **Create project**
- Available to every authenticated user.

| Field           | Behavior                                                                           |
| --------------- | ---------------------------------------------------------------------------------- |
| **Name**        | Text field, **required**; **2–100** characters; unique case-insensitive.           |
| **Description** | Multiline text area, **not required**; max **500** characters; empty when omitted. |

- On successful create, the creator becomes **Owner** on the new project (REQ-PRJ-005).
- A **project created** entry is written to **Operations** history (REQ-PRJ-008).
- A **member added** entry for the creator as **Owner** is written to **Membership history** (REQ-PRJ-007).

### Edit project screen

- Screen: **Edit project**
- Requires **Project.Manage** on the project.
- Available only for **custom** projects.
- Same fields and rules as **Create project**, pre-filled with current project data.
- On successful save, **name changed** and/or **description changed** entries are written to **Operations** history (REQ-PRJ-008) when the corresponding field changed.

### System project edit restriction

- The **Default** system project **cannot** be edited.
- Navigating to **Edit project** for the **Default** project opens **Project details** in read-only mode with message **`System projects cannot be modified.`** and **Back** to **Projects list**.

### Validation

- **Name**: required; **2–100** characters; unique case-insensitive; inline error on duplicate: **`A project with this name already exists.`**
- **Description**: max **500** characters when not empty; inline error: **`Description cannot exceed 500 characters.`**
- Validation errors are inline on the relevant field; the form stays open on failure.

### Form actions

- **Back** button and **Cancel** button navigate to **Projects list** when creating, or to **Project details** when editing, without saving.
- **Create project** button: on success show message **`Project created.`** and open **Project details** for the new project.
- **Save changes** button: on success show message **`Project saved.`** and open **Project details** for the edited project.

### Delete project

- **Delete project** is available from **Project details** and **Projects list** overflow menu (custom projects only).
- Requires **Project.Manage** on the project.
- Confirmation dialog: **`Delete project "{project name}"? This cannot be undone.`**
- Dialog presentation matches the standard destructive confirmation pattern used elsewhere (for example issue delete): warning icon; primary action **Delete** with danger styling; **Cancel** as secondary outlined button.
- On confirm: show message **`Project deleted.`** and navigate to **Projects list**.
- The **Default** system project **cannot** be deleted; **Delete project** is not shown for the **Default** project.
- A project with **one or more issues** cannot be deleted; show message **`Project has one or more issues. Move or delete all issues before deleting this project.`**

### System project rules

| Project     | **System** badge | Editable | Deletable            |
| ----------- | ---------------- | -------- | -------------------- |
| **Default** | Yes              | No       | No                   |
| Custom      | No               | Yes      | Yes, when issue-free |

### Permissions and visibility

- Create: every authenticated user.
- **Project.Manage** on the project: required to open **Edit project** and **Delete project**.
