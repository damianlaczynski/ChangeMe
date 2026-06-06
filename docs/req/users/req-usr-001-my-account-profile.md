---
id: REQ-USR-001
title: My Account Profile
domain: users
status: active
depends_on:
  [
    REQ-AUTH-001,
    REQ-AUTH-003,
    REQ-AUTH-004,
    REQ-AUTH-005,
    REQ-AUTH-013,
    REQ-AUTH-014,
    REQ-AUTH-015,
    REQ-BIL-003,
    REQ-PKY-003,
    REQ-ROL-001,
    REQ-USR-004,
  ]
---

## Goal

The signed-in user must be able to view their own profile, edit it on a separate screen, and reach account security actions.

## Features

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
- **Permissions** section: read-only list (REQ-ROL-001); collapsible panel; default **collapsed**.
- **Password last changed at**, **Password expires at**, and other admin-only account metadata are **not shown** on **My account** (see REQ-USR-004).

### Header actions

- **Edit** button (header action) opens **Edit profile** (same placement as **Edit** on other detail screens).
- **Change email** button (header action) opens **Change email** (REQ-AUTH-015) when **Self-service email change enabled** is **true**, the user is **not** **awaiting invitation acceptance**, and has **no pending email change**.
- **Change password** button (header action) opens **Change password** (REQ-AUTH-005).
- **Sign out everywhere** button (header action) when the user has **Sessions.ManageOwn**; same behavior as REQ-AUTH-003.

### Pending email change panel

- When a **pending email change** exists (REQ-AUTH-015), show the **Pending email change** panel as the **first content block** on **My account** (above the profile summary).
- Panel content and actions follow REQ-AUTH-015.
- When **no pending email change** exists, the panel is **not shown**.

### Edit profile screen

- Screen: **Edit profile**
- Route: linked from **My account** via header action **Edit**; **Back to my account** at the top.
- **First name** and **Last name** are editable; **required**; max **100** characters.
- **Email** is not shown on this screen (read-only on **My account** only).
- **Save changes**: validate, save, show message **`Profile updated.`**, return to **My account**.
- **Cancel**: return to **My account** without saving.

### Active sessions section

- Collapsible section **Active sessions** on the same screen when the user has **Sessions.ViewOwn** (REQ-AUTH-004).
- Not a separate screen or sidebar entry.

### Two-factor authentication section

- Collapsible section **Two-factor authentication** on the same screen when **Two-factor authentication enabled** is **true** (REQ-AUTH-013).
- Not a separate screen or sidebar entry.

### External sign-in methods section

- Collapsible section **External sign-in methods** on the same screen when **External providers enabled** is **true** (REQ-AUTH-014).
- Not a separate screen or sidebar entry.

### Passkeys section

- Collapsible section **Passkeys** on the same screen when **Passkeys authentication enabled** is **true** (REQ-PKY-003).
- Not a separate screen or sidebar entry.

### Employment summary section

- Collapsible section **Employment summary** on the same screen when the user has **Billing.ViewOwn** and an active employment contract exists (REQ-BIL-003).
- Not a separate screen or sidebar entry.
- When no active contract exists, the section is **not shown**.

### Set password

- Header action **Set password** on **My account** when the signed-in user is **external-only** (no local password); opens **Set password** (REQ-AUTH-014).
- **Change password** (REQ-AUTH-005) is shown only when the user **has a local password**.

### Validation (edit profile)

- **First name** and **Last name** follow the same rules as registration (REQ-AUTH-001); errors are inline on the relevant field.

### Permissions and visibility

- Any authenticated user with **Deactivated** false can view **My account** and open **Edit profile** to change **First name** and **Last name**.
- **My account** does **not** expose role assignment or account status changes.
- Self-service **Change email** is specified in REQ-AUTH-015; this REQ links to it from header actions and the **Pending email change** panel.

---
