---
id: FR-PRJ-002
title: Create and Edit Project
domain: projects
type: functional
status: active
depends_on: [FR-PRJ-001, FR-PRJ-003, FR-ROL-001]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized user must be able to create a project workspace when permitted and update its profile when they are a project **Owner**. Every issue in the system belongs to exactly one project.

## Functional requirements

### Access

- Screens: **Create project**, **Project settings** (profile section, FR-PRJ-004)
- **Create project** requires permission **Projects.Manage**.
- **Project settings** profile editing requires **Owner** on that project; read-only view for **Member**, **Viewer**, and non-members with access.

### Project entity (business attributes)

| Attribute       | Rules                                                                                                                              |
| --------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| **Name**        | **Required**; **2–100** characters.                                                                                                |
| **Key**         | **Required**; **2–10** characters; uppercase letters and digits only; unique across all projects; normalized to uppercase on save. |
| **Description** | **Not required**; max **1000** characters.                                                                                         |
| **Visibility**  | **Required**; **Internal** or **Private** (see FR-PRJ-001). Default on create: **Internal**.                                       |
| **Status**      | **Required** on edit; **Active**, **On hold**, or **Archived**. Default on create: **Active**.                                     |
| **Color**       | Accent color for UI badges; hex format **`#RRGGBB`**; default **`#3B82F6`**.                                                       |

### "Project profile" section (create)

| Field            | Behavior                                                                 |
| ---------------- | ------------------------------------------------------------------------ |
| **Name**         | Text field, **required**.                                                |
| **Key**          | Text field, **required**; input is normalized to uppercase alphanumeric. |
| **Description**  | Multiline text area; **not required**.                                   |
| **Visibility**   | Dropdown: **Internal**, **Private**.                                     |
| **Accent color** | Color picker; default **`#3B82F6`**.                                     |

### "Project profile" section (edit on **Project settings**)

- Same fields as create, plus **Status** dropdown (**Active**, **On hold**, **Archived**).
- **Key** remains editable; must stay unique.

### Validation

- **Name**: required; **2–100** characters.
- **Key**: required; **2–10** characters; uppercase letters and digits only; must be unique.
- **Description**: max **1000** characters when provided.
- **Visibility**: required; **Internal** or **Private**.
- **Status** (edit only): required; **Active**, **On hold**, or **Archived**.
- **Color**: when provided, must be valid hex **`#RRGGBB`**.
- Duplicate **Key** error: **`A project with this key already exists.`**
- Validation errors are shown next to the relevant fields without closing the form.

### Form actions (create)

- **Create project**: on success save and open **Project overview** (FR-PRJ-003).
- **Back to projects** button on **Create project** navigates to **Projects list**.

### Form actions (edit on **Project settings**)

- **Save changes**: on success update the project profile and remain on **Project settings**; success toast **`Project updated`**.
- When the user is not a project **Owner**, all profile fields are read-only and **Save changes** is **not shown**.
- Attempting to update a project without **Owner** stewardship is rejected with **`You do not have permission to manage this project.`**

### States and business rules

- On create, the signed-in user becomes a project **Owner** member (FR-PRJ-004).
- A project with **Archived** status must **not** accept new issues; validation error on issue create: field **Project**, message **`issues cannot be created in an archived project`**.
- **Delete project** (from **Projects list** overflow menu; **Owner** on that project only):
  - Confirmation: **`Delete project {name}? This action cannot be undone. Projects with issues cannot be deleted.`** (HTML emphasis on `{name}`).
  - Succeeds only when the project has **zero** issues.
  - When issues exist, error: **`Cannot delete a project that still has issues.`**
- Every issue stores exactly one **Project** reference; an issue cannot exist without a project.

### Back navigation

| Screen             | Back button label    | Destination       |
| ------------------ | -------------------- | ----------------- |
| **Create project** | **Back to projects** | **Projects list** |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) unless stated above.

## Out of scope

- Bulk import of projects or issues.
- Moving an issue from one project to another after creation.
