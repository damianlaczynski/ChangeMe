---
id: FR-ISS-002
title: Issue Create and Edit Flow
domain: issues
type: functional
status: active
depends_on: [FR-ISS-003, FR-PRJ-003, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data, and after saving be taken to **Issue details**.

## Functional requirements

### Access

- Screens: **Create issue**, **Edit issue**
- Available only inside a **project workspace** (FR-PRJ-003).
- **Create issue** assigns the issue to the **current project** automatically; the form does **not** include a project selector.
- Creating an issue in an **Archived** project is rejected (FR-PRJ-002).

### "Issue details" section (create and edit)

| Field                    | Behavior                                                                                                                 |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| **Title**                | Text field, **required**; **3–255** characters.                                                                          |
| **Description**          | Multiline text area, **required**; up to **2000** characters.                                                            |
| **Status**               | Issue status dropdown; **required**; default on create: **New**.                                                         |
| **Priority**             | Priority dropdown; **required**; default on create: **Medium**.                                                          |
| **Assigned to**          | User selector from assignable users (FR-USR-005); options show **Display label**; **not required** (clear = unassigned). |
| **Watch after creation** | Checkbox on create only; selected by default; adds the author as a watcher when checked.                                 |

### "Acceptance criteria" section

- List of acceptance criteria; the user can add multiple items and remove rows.
- Each item is a multiline **criterion** field, **required** when the row is present; up to **2000** characters per item.
- Zero acceptance-criteria rows are allowed on create.

**System fields (read-only on edit):**

- **Author**, **Created at**, and **Last activity** in a read-only summary block.
- **Issue identifier** is assigned by the system on first save; not editable in the form.
- **Author** is the currently signed-in user on create.
- **Created at** and **Last activity** are maintained by the system.

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation, **Back** / **Cancel** defaults, and form-area loading unless stated below.
- **Title**: required; **3–255** characters.
- **Description**: required; max **2000** characters.
- **Status**: required; one of **New**, **In Progress**, **Resolved**, **Closed**.
- **Priority**: required; one of **Low**, **Medium**, **High**, **Critical**.
- **Assigned to**: when selected, must be an assignable user with **Deactivated** false (FR-USR-005).
- **Acceptance criterion**: when a row exists, its text is required; max **2000** characters.

### Form actions

- **Create issue** / **Save changes**: on success save and open **Issue details**.

### Back navigation (create and edit)

| Screen           | Back button label         | Destination                                     |
| ---------------- | ------------------------- | ----------------------------------------------- |
| **Create issue** | **Back to issues list**   | **Project issues list** for the current project |
| **Edit issue**   | **Back to issue details** | **Issue details** for the edited issue          |

### Consistency between create and edit

- Edit uses the same core fields and validation as create, plus read-only system metadata.
- On edit, the user can change **Title**, **Description**, **Status**, **Priority**, **Assigned to**, and all **acceptance criteria** rows.
- After create, the author is added to watchers when **Watch after creation** is checked.
- Every create and edit writes entries to change history (FR-ISS-003), including acceptance-criterion add, update, and remove events.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
