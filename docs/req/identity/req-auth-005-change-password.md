---
id: REQ-AUTH-005
title: Change Password
domain: identity
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-007, REQ-AUTH-008, REQ-AUTH-009]
---
## Goal

The signed-in user must be able to change their password securely.

## Features

### Change password screen

- Screen: **Change password**
- Linked from **My account** via header action **Change password**; not editable on **My account**.
- **Back to my account** control at the top of the screen (same placement as **Back** on other detail screens).

| Field                    | Behavior                                       |
| ------------------------ | ---------------------------------------------- |
| **Current password**     | **Required**; must match the current password. |
| **New password**         | **Required**; **8–128** characters.            |
| **Confirm new password** | **Required**; must match **New password**.     |

### Sign-out notice

- Above the form actions, the screen displays a persistent notice: **`Changing your password will sign you out on all devices, including this one. You will need to sign in again with your new password.`**
- The notice is visible whenever the **Change password** screen is open; it is not dismissible.

### Validation

- Wrong **Current password** shows inline field error: **`Current password is incorrect.`** Other fields retain their values.
- **New password** identical to the current password shows inline field error: **`New password must differ from the current password.`**
- Required and length errors are inline on the relevant field.

### Form actions

- **Cancel** button navigates to **My account** without saving.
- **Change password** button opens confirmation dialog: **`Change password and sign out everywhere? You will be signed out on every device and must sign in again with your new password.`**
- **Cancel** in the dialog closes the dialog without saving; entered field values remain unchanged.
- **Confirm** in the dialog validates the form; when validation passes, the system saves the new password, revokes **all active sessions** (including the current session), and redirects to **Login**.
- After redirect to **Login**, show message: **`Password changed. Sign in with your new password.`**
- On validation failure before confirm, the dialog does not open; inline field errors are shown on the form.
- On save failure after confirm, the dialog closes, the form stays open with inline or form-level errors, and the user remains signed in.

### States and business rules

- Only authenticated users with **Deactivated** false can access **Change password**.
- After a successful password change, the user is signed out on **every device** and must sign in again with the new password (REQ-AUTH-001).
- On success, **password last changed at** is updated (REQ-AUTH-009).
- Password rules follow **Password policy** (REQ-AUTH-008).
- On success, the system sends a **Password changed** email (REQ-AUTH-007).

---
