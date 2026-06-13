---
id: FR-PRJ-001
title: Projects List
domain: projects
type: functional
status: active
depends_on: [FR-AUTH-001, FR-PRJ-002, FR-ROL-001]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized user must be able to browse projects they can access, search and sort them, open a project workspace, and start creating a new project when permitted.

## Functional requirements

### Access

- Screen: **Projects list**
- Sidebar entry **Projects** is visible to every authenticated user with **Deactivated** false.
- Opening **Projects list** requires permission **Projects.View**; users without it are denied with message **`You do not have permission to perform this action.`**
- Guests are redirected to **Login** (FR-AUTH-001).

### Search and actions bar

- Search field placeholder: **`Search projects...`**
- Search matches **name**, **key**, or **description** fragment (case-insensitive).
- **Search** button and form submit apply the current search text.
- **Create project** button opens **Create project** (FR-PRJ-002); visible only with permission **Projects.Manage**.

### Projects table

| Column          | Description                                                          |
| --------------- | -------------------------------------------------------------------- |
| **Name**        | Project name with color accent dot; link opens **Project overview**. |
| **Key**         | Short uppercase project code.                                        |
| **Description** | Project description text, or **`No description`** when empty.        |
| **Status**      | Status badge (**Active**, **On hold**, **Archived**).                |
| **Issues**      | Count of issues in the project.                                      |
| **Members**     | Count of project members.                                            |
| **Actions**     | Overflow menu (see below).                                           |

### Sorting

- Sortable columns: **Name**, **Key**, **Status**, **Issues**, **Members**.
- Default sort: **Name**, ascending.

### List behavior

- Inherits `FR-UI-001` (**Administrative list screens**) for pagination, loading, empty state, and overflow menu visibility unless stated below.
- The list shows only projects the current user may access:
  - **Internal** visibility: every user with **Projects.View**.
  - **Private** visibility: project **members** only (FR-PRJ-004).

### Row overflow menu

| Action               | Requirement               | Behavior                                    |
| -------------------- | ------------------------- | ------------------------------------------- |
| **Open workspace**   | **Projects.View**         | Opens **Project overview** (FR-PRJ-003).    |
| **Browse issues**    | **Projects.View**         | Opens **Project issues list** (FR-ISS-001). |
| **Project settings** | **Owner** on that project | Opens **Project settings** (FR-PRJ-004).    |
| **Delete project**   | **Owner** on that project | Confirmation and behavior in FR-PRJ-002.    |

### Permissions and visibility

- **Projects.View**: required for **Projects list**, **Open workspace**, and **Browse issues**.
- **Projects.Manage**: required for **Create project** only.
- **Owner** (project member role): required for **Project settings** and **Delete project** on that project.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) unless stated above.

## Out of scope

- Cross-project issue search (issues are listed inside each project workspace only).
