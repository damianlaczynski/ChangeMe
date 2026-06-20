---
id: FR-USR-001
title: My Account Profile
domain: users
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-003, FR-AUTH-004, FR-ROL-001, FR-USR-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The signed-in user must be able to view their own profile, edit it on a separate screen, and manage their sessions.

## Functional requirements

### My account screen

- Screen: **My account**
- Sidebar entry: **My account** (visible to all authenticated users with **Deactivated** false).

### Profile section (read-only)

| Field            | Behavior                                  |
| ---------------- | ----------------------------------------- |
| **First name**   | Read-only.                                |
| **Last name**    | Read-only.                                |
| **Email**        | Read-only.                                |
| **Member since** | Read-only account creation date and time. |

- **Roles** section: read-only list of assigned role names (badges); collapsible panel; links to **Role details** when the user has **Roles.View**, otherwise badges only.
- Empty state: **`No roles assigned.`**
- **Permissions** section: read-only list (FR-ROL-001); collapsible panel; default **collapsed**.
- Admin-only account metadata such as **Password last changed at** is **not shown** on **My account** (see FR-USR-004).

### Header actions

- **Edit** button (header action) opens **Edit profile** (same placement as **Edit** on other detail screens).
- **Sign out everywhere** button (header action) when the user has **Sessions.ManageOwn**; same behavior as FR-AUTH-003.

### Edit profile screen

- Screen: **Edit profile**
- Route: linked from **My account** via header action **Edit**; **Back to my account** at the top.
- **First name** and **Last name** are editable; **required**; max **100** characters.
- **Email** is not shown on this screen (read-only on **My account** only).
- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation and form-area loading; **Back** / **Cancel** return to **My account**.
- **Save changes**: on success show message **`Profile updated.`** and return to **My account**.

### Active sessions section

- Collapsible section **Active sessions** on the same screen when the user has **Sessions.ViewOwn** (FR-AUTH-004).
- Not a separate screen or sidebar entry.

### Validation (edit profile)

- **First name** and **Last name** follow the same rules as user creation (FR-USR-003); errors are inline on the relevant field.

### Permissions and visibility

- Any authenticated user with **Deactivated** false can view **My account** and open **Edit profile** to change **First name** and **Last name**.
- **My account** does **not** expose role assignment or account status changes.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
