---
id: REQ-ISS-002
title: Issue Create and Edit Flow
domain: issues
status: active
depends_on: [REQ-ISS-003, REQ-USR-005]
---
## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data, and after saving be taken to **Issue details**.

## Features

### Access

- Screens: **Create issue**, **Edit issue**
- Available only to authenticated users.

### "Issue details" section (create and edit)

| Field                    | Behavior                                                                                                                  |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| **Title**                | Text field, **required**; **3–255** characters.                                                                           |
| **Description**          | Multiline text area, **required**; up to **2000** characters.                                                             |
| **Status**               | Issue status dropdown; **required**; default on create: **New**.                                                          |
| **Priority**             | Priority dropdown; **required**; default on create: **Medium**.                                                           |
| **Assigned to**          | User selector from assignable users (REQ-USR-005); options show **Display label**; **not required** (clear = unassigned). |
| **Watch after creation** | Checkbox on create only; selected by default; adds the author as a watcher when checked.                                  |

### "Acceptance criteria" section

- List of acceptance criteria; the user can add multiple items and remove rows.
- Each item is a multiline **criterion** field, **required** when the row is present; up to **2000** characters per item.
- Zero acceptance-criteria rows are allowed on create.

**System fields (read-only on edit):**

- **Author**, **Created at**, and **Last activity** in a read-only summary block.
- **Issue identifier** is assigned by the system on first save; not editable in the form.
- **Author** is the currently signed-in user on create.
- **Created at** and **Last activity** are maintained by the system.

### Validation

- **Title**: required; **3–255** characters.
- **Description**: required; max **2000** characters.
- **Status**: required; one of **New**, **In Progress**, **Resolved**, **Closed**.
- **Priority**: required; one of **Low**, **Medium**, **High**, **Critical**.
- **Assigned to**: when selected, must be an assignable user with **Deactivated** false (REQ-USR-005).
- **Acceptance criterion**: when a row exists, its text is required; max **2000** characters.
- Validation errors are inline next to the relevant field; the form stays open on failure.

### Form actions

- **Back to issues list** button navigates to **Issues list** without saving.
- **Cancel** (create): same as **Back to issues list** — leaves without saving.
- **Create issue** / **Save changes**: on success save and open **Issue details**; on failure keep the form open with validation messages.

### Back navigation (create and edit)

| Screen           | Back button label         | Destination                            |
| ---------------- | ------------------------- | -------------------------------------- |
| **Create issue** | **Back to issues list**   | **Issues list**                        |
| **Edit issue**   | **Back to issue details** | **Issue details** for the edited issue |

### Consistency between create and edit

- Edit uses the same core fields and validation as create, plus read-only system metadata.
- On edit, the user can change **Title**, **Description**, **Status**, **Priority**, **Assigned to**, and all **acceptance criteria** rows.
- After create, the author is added to watchers when **Watch after creation** is checked.
- Every create and edit writes entries to change history (REQ-ISS-003), including acceptance-criterion add, update, and remove events.

### Loading

- Before the first load on create/edit, a loading state covers the form area until initial data arrives.

---
