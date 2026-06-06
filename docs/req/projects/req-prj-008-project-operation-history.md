---
id: REQ-PRJ-008
title: Project Operation History
domain: projects
status: active
depends_on: [REQ-PRJ-004, REQ-PRJ-005]
---

## Goal

The system must retain a durable audit trail of project metadata operations and expose **Operations** history on **Project details**.

## Features

### Access

- **Operations** tab on **Project details** (REQ-PRJ-004).
- Visible when the user has **Project.View** on the project.
- History is read-only.

### Operations tab

- Timeline of project metadata operations.
- The tab loads independently when selected.
- Event types:
  - **project created**,
  - **name changed**,
  - **description changed**.
- Each entry shows: **summary** (event type), **acting user**, **date and time**, and **Before** / **After** when values apply.
- **project created** shows summary only (no before/after inline).
- **description changed** shows summary only (no before/after inline).
- History is **server-paginated**, sorted by **date and time** descending (**newest first**).
- The first page shows up to **10** most recent entries.
- When older entries exist, a **Show more** control loads the next page of **older** entries and **appends** them.
- **Show more** is hidden when all entries are loaded.
- Empty state: **`No operations history yet.`**
- While the first page loads, a loading indicator is shown in the tab area.
- While **Show more** is loading, the button shows a loading state; already loaded entries remain visible.

### Events that write operation history

| User action                            | Event type              |
| -------------------------------------- | ----------------------- |
| Create project (REQ-PRJ-003)           | **project created**     |
| Save project name (REQ-PRJ-003)        | **name changed**        |
| Save project description (REQ-PRJ-003) | **description changed** |

- History entries are retained for the lifetime of the project.
- Deleting a project (REQ-PRJ-003) removes the project together with its operation history.

### Presentation

- Event types use distinct timeline markers (icons and colors).

### Permissions and visibility

- **Project.View**: required to open the **Operations** tab and load its content.
