# Requirements - Auth

This document covers five REQs for the **Auth** area:
login and registration with tracked sessions, staying signed in across credential expiry, logout, viewing and revoking own sessions, and password change.

Screens **Login** and **Register** are available to guests. All other application screens require an authenticated **Active** user.

---

# REQ-AUTH-001: Login and Registration with Sessions

## Goal

The user must be able to register a new account or sign in with email and password and begin an authenticated session tracked by the system.

## Features

### Login screen

| Field           | Behavior                                                                     |
| --------------- | ---------------------------------------------------------------------------- |
| **Email**       | Text field, **required**; valid email format; max **320** characters.        |
| **Password**    | Password field, **required**; **8–128** characters (same bounds as registration). |
| **Remember me** | Checkbox, **not required**; default **unchecked**. Label: **`Remember me`**. |

- Successful sign-in opens the **Issues list** screen with the user authenticated.
- The system creates a **session** recording **signed in at**, **device / browser label**, **IP address**, and whether **Remember me** was selected.
- When **Remember me** is **checked**, the session is **persistent** (survives closing the browser) per REQ-AUTH-002.
- When **Remember me** is **unchecked**, the session is a **browser session** (ends when the browser is closed) per REQ-AUTH-002.
- The system provides the user’s effective permissions defined in REQ-ROL-001.
- Failed sign-in (unknown email or wrong password) shows form-level error: **`Invalid email or password.`** The message does not reveal whether the email exists.
- Sign-in attempt for an **Inactive** account shows form-level error: **`This account has been deactivated. Contact an administrator.`**

### Register screen

| Field                | Behavior                                                                       |
| -------------------- | ------------------------------------------------------------------------------ |
| **First name**       | Text field, **required**; max **100** characters.                              |
| **Last name**        | Text field, **required**; max **100** characters.                              |
| **Email**            | Text field, **required**; valid email; max **320** characters; must be unique. |
| **Password**         | Password field, **required**; **8–128** characters.                            |
| **Confirm password** | **Required**; must match **Password**.                                         |

- Successful registration creates an **Active** user, assigns the default **User** role (REQ-ROL-006), creates a **browser session** (**Remember me** equivalent: **unchecked**), and opens the **Issues list** screen with the user authenticated — same outcome as sign-in with **Remember me** unchecked.
- Duplicate email shows form-level error: **`An account with this email already exists.`**

### Validation

**Login**

- **Email**: required; valid email format; max **320** characters.
- **Password**: required; **8–128** characters. Rejects out-of-range input before authentication (limits oversized requests; minimum matches passwords set at registration and user creation).

**Register**

- **First name** and **Last name**: required; max **100** characters.
- **Email**: required; valid email format; max **320** characters.
- **Password**: required; **8–128** characters.
- **Confirm password**: required; must match **Password**.

**Both screens**

- Validation errors are shown inline next to the relevant field without closing the form.
- The form does not navigate away on validation failure.

### Form actions

- **Sign in** button: on success navigate to **Issues list**; on failure remain on **Login**.
- **Create account** button: on success navigate to **Issues list**; on failure remain on **Register**.
- Footer link on **Login**: **Create an account** → **Register**.
- Footer link on **Register**: **Sign in** → **Login**.
- While submit is in progress, the submit button shows a loading state; the form remains visible.

### States and business rules

- Registration assigns only the **User** role; it does not grant administrative permissions (REQ-ROL-006).
- Each sign-in creates a **new session**; signing in from multiple devices creates multiple independent sessions.
- **Remember me** applies only on **Login**; **Register** does not show the checkbox and always creates a **browser session**.
- A guest who opens a protected screen is redirected to **Login**.

---

# REQ-AUTH-002: Staying Signed In

## Goal

The user must remain signed in during normal application use without repeated manual sign-in when credentials expire, according to the **Remember me** choice made at sign-in.

## Features

### Session types

| Session type           | Created when                                                         | Persists after browser close |
| ---------------------- | -------------------------------------------------------------------- | ---------------------------- |
| **Browser session**    | **Remember me** is **unchecked** on **Login**, or after **Register** | **No**                       |
| **Persistent session** | **Remember me** is **checked** on **Login**                          | **Yes**                      |

- A **browser session** ends when the user closes the browser or signs out, whichever comes first.
- After closing the browser, a **browser session** cannot be renewed; the user must sign in again on **Login**.
- A **persistent session** remains available after the browser is closed until it expires, is revoked, or the user signs out.

### Credential lifetime

| Setting                         | Default        | Applies to                  |
| ------------------------------- | -------------- | --------------------------- |
| **Short-lived credential**      | **30 minutes** | Both session types          |
| **Long-lived session lifetime** | **14 days**    | **Persistent session** only |

- The **short-lived credential** expires **30 minutes** after issuance.
- The **long-lived session lifetime** of **14 days** applies only to **persistent sessions**, counted from **signed in at**.

### Automatic renewal

- **60 seconds** before the short-lived credential expires, the system renews it automatically without user action while the session is active.
- On each successful renewal, the system updates **last activity** on the current session.
- The user’s effective permissions are refreshed on renewal (REQ-ROL-001).
- **Persistent sessions** can be renewed after the browser is closed and reopened, within the **14-day** lifetime.
- **Browser sessions** can be renewed only while the browser remains open; renewal fails after the browser was closed.
- If renewal fails because the session expired, was revoked, the browser was closed (**browser session**), or the account is **Inactive**, the user is signed out and redirected to **Login**.

### Renewal after failed action

- When a signed-in user triggers an action that fails because the short-lived credential expired, the system attempts **one** automatic renewal and repeats the action once.
- If renewal fails, the user is signed out and redirected to **Login**.

### Real-time updates

- After successful credential renewal, open real-time views (issue list refresh and notifications per REQ-ISS-004) continue to receive updates without manual page reload.

### States and business rules

- A **revoked** session cannot be renewed.
- Deactivating a user revokes all active sessions immediately (REQ-USR-005); renewal attempts for that user fail and redirect to **Login**.

---

# REQ-AUTH-003: Logout

## Goal

The user must be able to sign out from the current browser or from all devices.

## Features

### Logout (current browser)

- **Logout** button in the application header signs the user out of the **current session**.
- The user is redirected to **Login**.
- Protected screens are no longer accessible until the user signs in again.

### Sign out everywhere

- **Sign out everywhere** button is available on **My sessions** (REQ-AUTH-004).
- Clicking **Sign out everywhere** opens confirmation dialog: **`Sign out from all devices? You will be signed out on every browser and device.`**
- On confirm, the system revokes **all active sessions** for the user, signs out the current browser, and redirects to **Login**.

### States and business rules

- Repeating logout when already signed out redirects to **Login** without error.
- A revoked session cannot access protected screens or renew credentials.

---

# REQ-AUTH-004: My Sessions

## Goal

The user must be able to review active sign-in sessions and revoke sessions they no longer trust.

## Features

### My sessions screen

- Screen: **My sessions**
- Linked from **My account** via button **Active sessions**.
- Requires permission **Sessions.ViewOwn**.

| Column               | Description                                                                                   |
| -------------------- | --------------------------------------------------------------------------------------------- |
| **Device / browser** | Label in format **`{Browser} on {Platform}`** (for example **`Chrome on Windows`**).          |
| **IP address**       | Secondary line under device/browser; shows session IP or **`Unknown`**.                       |
| **Session type**     | **`Persistent`** when **Remember me** was selected at sign-in; **`Browser`** when it was not. |
| **Signed in at**     | Session start date and time.                                                                  |
| **Last activity**    | Date and time of last credential renewal or authenticated activity.                           |
| **Current**          | Badge **`Current session`** on the row for the browser the user is signed in with.            |
| **Actions**          | **Revoke** button on every row except the current session (requires **Sessions.ManageOwn**).  |

- The list shows **active sessions only**; revoked sessions do not appear.
- Empty state: **`No active sessions.`**
- While the list is loading, a loading indicator is shown in the list area; the rest of the screen remains visible.

### Actions

- **Revoke** on a non-current row opens confirmation dialog: **`Revoke this session? That device will be signed out.`**
- On confirm, that session is revoked and the row is removed from the list without reloading the entire screen.
- The **Revoke** button is **not shown** on the **Current session** row; the user signs out the current browser via **Logout** (REQ-AUTH-003).
- **Sign out everywhere** is a header action (REQ-AUTH-003); requires **Sessions.ManageOwn**.

### Permissions and visibility

- **Sessions.ViewOwn**: required to open **My sessions** and view the list.
- **Sessions.ManageOwn**: required for **Revoke** on non-current rows and **Sign out everywhere**.

---

# REQ-AUTH-005: Change Password

## Goal

The signed-in user must be able to change their password securely.

## Features

### Change password screen

- Screen: **Change password**
- Linked from **My account** via button **Change password**.

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

- Only authenticated **Active** users can access **Change password**.
- After a successful password change, the user is signed out on **every device** and must sign in again with the new password (REQ-AUTH-001).
- **Out of scope for this REQ:** email notification on password change.
