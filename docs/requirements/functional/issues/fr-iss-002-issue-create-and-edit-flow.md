---
id: FR-ISS-002
title: Issue Create and Edit Flow
domain: issues
type: functional
status: active
depends_on: [FR-ISS-003, FR-USR-005]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data, and after saving be taken to **Issue details**.

## Functional requirements

### Access

- Screens: **Create issue**, **Edit issue**
- Available only to authenticated users.

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

| Screen           | Back button label         | Destination                            |
| ---------------- | ------------------------- | -------------------------------------- |
| **Create issue** | **Back to issues list**   | **Issues list**                        |
| **Edit issue**   | **Back to issue details** | **Issue details** for the edited issue |

### Consistency between create and edit

- Edit uses the same core fields and validation as create, plus read-only system metadata.
- On edit, the user can change **Title**, **Description**, **Status**, **Priority**, **Assigned to**, and all **acceptance criteria** rows.
- After create, the author is added to watchers when **Watch after creation** is checked.
- Every create and edit writes entries to change history (FR-ISS-003), including acceptance-criterion add, update, and remove events.

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ISS-002-01 | Guest or unauthenticated user                                 | User navigates to **Create issue** or **Edit issue** | User is redirected to **Login** (FR-AUTH-001)                                                          |
| AC-ISS-002-02 | Authenticated user on **Create issue**                        | User views the **Issue details** section          | **Status** defaults to **New**; **Priority** defaults to **Medium**; **Watch after creation** is checked |
| AC-ISS-002-03 | Authenticated user on **Create issue** with valid required fields and **Watch after creation** checked | User clicks **Create issue** | Issue is saved; **Issue details** opens; the author is added as a watcher                                 |
| AC-ISS-002-04 | Authenticated user on **Create issue** with valid required fields and zero acceptance-criteria rows | User clicks **Create issue** | Issue is saved successfully and **Issue details** opens                                                 |
| AC-ISS-002-05 | Authenticated user on **Create issue**                        | User clicks **Back to issues list**               | User navigates to **Issues list** without saving                                                          |
| AC-ISS-002-06 | Authenticated user on **Edit issue** for an existing issue    | User clicks **Back to issue details**             | User navigates to **Issue details** for the edited issue without saving                                   |
| AC-ISS-002-07 | Authenticated user on **Edit issue**                          | User views the form                               | **Author**, **Created at**, and **Last activity** are read-only; **Watch after creation** is not shown    |
| AC-ISS-002-08 | Authenticated user on **Create issue** or **Edit issue** with **Title** shorter than 3 characters | User submits the form              | Inline validation error is shown on **Title**; form is not saved                                          |
| AC-ISS-002-09 | Authenticated user on **Edit issue** with valid changes       | User clicks **Save changes**                      | Issue is saved and **Issue details** opens with refreshed data                                            |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
