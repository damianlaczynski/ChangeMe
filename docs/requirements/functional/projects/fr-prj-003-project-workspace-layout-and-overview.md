---
id: FR-PRJ-003
title: Project Workspace Layout and Overview
domain: projects
type: functional
status: active
depends_on: [FR-PRJ-001, FR-PRJ-002, FR-ISS-001]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When the user opens a project, the application must present a dedicated **project workspace** with its own navigation and a single application header, so global options (Users, Roles, account administration) do not consume space while the user works inside the project.

## Functional requirements

### Access

- Screens: **Project overview**, and nested routes **Project issues list**, **Create issue**, **Issue details**, **Edit issue**, **Project settings**
- Requires permission **Projects.View** and access to the project per visibility rules (FR-PRJ-001).
- Users without access receive **`You do not have permission to perform this action.`** or equivalent not-found behavior for private projects they are not members of.

### Dedicated workspace chrome

- When any **project workspace** screen is open (`/projects/{project}` and nested routes), the application uses **project workspace mode**:
  - The global sidebar (**Projects**, **Users**, **Roles**, **My account**) is **not shown**.
  - Exactly **one** top application header is shown (no nested duplicate header inside page content).
  - The project sidebar replaces the global sidebar.

### Project sidebar

- Shows project **Key** and **Name** with accent color dot when expanded; color dot only when collapsed.
- Navigation items:

| Label        | Destination             |
| ------------ | ----------------------- |
| **Overview** | **Project overview**    |
| **Issues**   | **Project issues list** |
| **Settings** | **Project settings**    |

- Sidebar collapse and mobile drawer behavior inherit `FR-UI-001` shell patterns.

### Project workspace header

- **All projects** link (label **`All projects`**, back arrow icon) navigates to **Projects list**.
- On desktop, shows project **Name**, accent dot, and **Status** badge.
- Header includes **Notifications**, theme toggle, signed-in user reference, and **Logout** (same as global authenticated header).
- Loading state while project context loads: content area message **`Loading project workspace...`**
- Load failure: **`Unable to open this project workspace.`**

### Project overview screen

- Card **Overview** with project **Description** (or empty-description handling from FR-PRJ-002).
- Status and visibility badges: **Active** / **On hold** / **Archived**; **Internal** / **Private**.
- Summary counters: **Total issues**, **New**, **In progress**, **Members**.
- Actions:
  - **Browse issues** → **Project issues list**
  - **Create issue** → **Create issue** (FR-ISS-002)

### Navigation after sign-in

- Successful sign-in opens **Projects list** (replacing the former default **Issues list**) unless a post-authentication gate applies (see `docs/requirements/_shared/reference/compliance-gates.md`).

### States and business rules

- Issue screens (**Project issues list**, **Create issue**, **Issue details**, **Edit issue**) are reachable only inside a project workspace, not from the global sidebar.
- Legacy direct navigation to a global **Issues list** redirects to **Projects list**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) unless stated above.

## Out of scope

- Additional workspace tabs (boards, reports, automations) until defined in a separate functional specification.
- Second nested layout or header inside **Project overview** content.
