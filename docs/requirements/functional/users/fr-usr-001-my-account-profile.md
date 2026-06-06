---
id: FR-USR-001
title: My Account Profile
domain: users
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-003,
    FR-AUTH-004,
    FR-AUTH-005,
    FR-AUTH-013,
    FR-AUTH-014,
    FR-AUTH-015,
    FR-PKY-003,
    FR-ROL-001,
    FR-USR-004,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The signed-in user must be able to view their own profile, edit it on a separate screen, and reach account security actions.

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
- **Password last changed at**, **Password expires at**, and other admin-only account metadata are **not shown** on **My account** (see FR-USR-004).

### Header actions

- **Edit** button (header action) opens **Edit profile** (same placement as **Edit** on other detail screens).
- **Change email** button (header action) opens **Change email** (FR-AUTH-015) when **Self-service email change enabled** is **true**, the user is **not** **awaiting invitation acceptance**, and has **no pending email change**.
- **Change password** button (header action) opens **Change password** (FR-AUTH-005).
- **Sign out everywhere** button (header action) when the user has **Sessions.ManageOwn**; same behavior as FR-AUTH-003.

### Pending email change panel

- When a **pending email change** exists (FR-AUTH-015), show the **Pending email change** panel as the **first content block** on **My account** (above the profile summary).
- Panel content and actions follow FR-AUTH-015.
- When **no pending email change** exists, the panel is **not shown**.

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

### Two-factor authentication section

- Collapsible section **Two-factor authentication** on the same screen when **Two-factor authentication enabled** is **true** (FR-AUTH-013).
- Not a separate screen or sidebar entry.

### External sign-in methods section

- Collapsible section **External sign-in methods** on the same screen when **External providers enabled** is **true** (FR-AUTH-014).
- Not a separate screen or sidebar entry.

### Passkeys section

- Collapsible section **Passkeys** on the same screen when **Passkeys authentication enabled** is **true** (FR-PKY-003).
- Not a separate screen or sidebar entry.

### Set password

- Header action **Set password** on **My account** when the signed-in user is **external-only** (no local password); opens **Set password** (FR-AUTH-014).
- **Change password** (FR-AUTH-005) is shown only when the user **has a local password**.

### Validation (edit profile)

- **First name** and **Last name** follow the same rules as registration (FR-AUTH-001); errors are inline on the relevant field.

### Permissions and visibility

- Any authenticated user with **Deactivated** false can view **My account** and open **Edit profile** to change **First name** and **Last name**.
- **My account** does **not** expose role assignment or account status changes.
- Self-service **Change email** is specified in FR-AUTH-015; FR-USR-001 links to it from header actions and the **Pending email change** panel.

---

## Acceptance scenarios

| ID            | Given                                                                                                                                          | When                                              | Then                                                                                                  |
| ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| AC-USR-001-01 | Signed-in user with **Deactivated** false on **My account**                                                                                    | User clicks header **Edit**                       | **Edit profile** opens; **Back to my account** is shown at the top                                    |
| AC-USR-001-02 | Signed-in user; **Self-service email change enabled** is **true**; user is not **awaiting invitation acceptance**; no **pending email change** | User views **My account** header actions          | **Change email** is visible and opens **Change email** (FR-AUTH-015)                                  |
| AC-USR-001-03 | Signed-in user **has a local password**                                                                                                        | User clicks header **Change password**            | **Change password** screen opens (FR-AUTH-005)                                                        |
| AC-USR-001-04 | Signed-in **external-only** user (no local password)                                                                                           | User views **My account** header actions          | **Set password** is shown (not **Change password**); clicking it opens **Set password** (FR-AUTH-014) |
| AC-USR-001-05 | Signed-in user on **Edit profile**                                                                                                             | User saves valid **First name** and **Last name** | Toast **`Profile updated.`**; user returns to **My account** with updated read-only profile           |
| AC-USR-001-06 | Signed-in user on **Edit profile**                                                                                                             | User views the form                               | **Email** field is **not shown** (email remains read-only on **My account** only)                     |
| AC-USR-001-07 | Signed-in user with a **pending email change** (FR-AUTH-015)                                                                                   | User opens **My account**                         | **Pending email change** panel is the **first** content block above the profile summary               |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
