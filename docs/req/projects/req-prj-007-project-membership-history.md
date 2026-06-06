---
id: REQ-PRJ-007
title: Project Membership History
domain: projects
status: active
depends_on: [REQ-PRJ-004, REQ-PRJ-005, REQ-PRJ-006]
---

## Goal

The system must retain a durable audit trail of project membership changes and expose **Membership history** on **Project details**.

## Features

### Access

- **Membership history** tab on **Project details** (REQ-PRJ-004).
- Tab label is visible when the user has **Project.View** on the project.
- Tab content requires **Project.Members.View** on the project.
- History is read-only.

### Membership history tab

- Timeline of membership events for the project.
- The tab loads independently when selected.
- Event types:
  - **member added** — user joined the project with a project role,
  - **member removed** — user left the project,
  - **member role changed** — user's project role changed.
- Each entry shows: **summary** (event type), **acting user**, **date and time**, **affected user**, and **Before** / **After** for role values.
- **member added** shows **After** role only.
- **member removed** shows **Before** role only.
- History is **server-paginated**, sorted by **date and time** descending (**newest first**).
- The first page shows up to **10** most recent entries.
- When older entries exist, a **Show more** control loads the next page of **older** entries and **appends** them.
- **Show more** is hidden when all entries are loaded.
- Empty state: **`No membership history yet.`**
- While the first page loads, a loading indicator is shown in the tab area.
- While **Show more** is loading, the button shows a loading state; already loaded entries remain visible.

### Events that write membership history

| User action                                            | Event type              |
| ------------------------------------------------------ | ----------------------- |
| Add member (REQ-PRJ-006)                               | **member added**        |
| Remove member (REQ-PRJ-006)                            | **member removed**      |
| Change member role (REQ-PRJ-006)                       | **member role changed** |
| Create project — creator as Owner (REQ-PRJ-003)        | **member added**        |
| **Default** project seeding (REQ-PRJ-001, REQ-PRJ-005) | **member added**        |

- Automatic **member added** entries from project creation and **Default** seeding use acting user **`System`**.
- History entries are retained for the lifetime of the project.
- Deleting a project (REQ-PRJ-003) removes the project together with its membership history.

### Presentation

- Event types use distinct timeline markers (icons and colors).
- **Acting user** is **`System`** for automated seed and default-membership events.

### Permissions and visibility

- **Project.View**: required to see the **Membership history** tab on **Project details**.
- **Project.Members.View**: required to load **Membership history** tab content.
