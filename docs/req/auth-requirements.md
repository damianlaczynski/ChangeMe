# Requirements - Auth

This document covers twelve REQs for the **Auth** area:
login and registration with tracked sessions, staying signed in, logout, own sessions, password change, self-service reset, auth emails, password policy, password expiration, account invite acceptance, optional email verification, and optional public registration.

Screens **Login**, **Forgot password**, **Reset password**, and **Accept invitation** are available to guests. **Register** and **Verify email** are available to guests only when the corresponding deployment settings allow them (REQ-AUTH-012, REQ-AUTH-011). All other application screens require an authenticated user with **Deactivated** false whose email is **verified** when email verification is enabled (REQ-AUTH-011).

---

# REQ-AUTH-001: Login and Registration with Sessions

## Goal

The user must be able to sign in with email and password and begin an authenticated session tracked by the system. When public registration is enabled (REQ-AUTH-012), the user must also be able to register a new account.

## Features

### Login screen

| Field           | Behavior                                                                          |
| --------------- | --------------------------------------------------------------------------------- |
| **Email**       | Text field, **required**; valid email format; max **320** characters.             |
| **Password**    | Password field, **required**; **8–128** characters (same bounds as registration). |
| **Remember me** | Checkbox, **not required**; default **unchecked**. Label: **`Remember me`**.      |

- Successful sign-in opens the **Issues list** screen with the user authenticated, unless the password has expired per REQ-AUTH-009.
- The system creates a **session** recording **signed in at**, **device / browser label**, **IP address**, and whether **Remember me** was selected.
- When **Remember me** is **checked**, the session is **persistent** (survives closing the browser) per REQ-AUTH-002.
- When **Remember me** is **unchecked**, the session is a **browser session** (ends when the browser is closed) per REQ-AUTH-002.
- The system provides the user’s effective permissions defined in REQ-ROL-001.
- Failed sign-in (unknown email or wrong password) shows form-level error: **`Invalid email or password.`** The message does not reveal whether the email exists.
- Sign-in attempt when **Deactivated** is **true** shows form-level error: **`This account has been deactivated. Contact an administrator.`**
- Sign-in attempt for an invite-pending account (**Has password set** false) shows form-level error: **`Complete your account setup using the invitation link sent to your email.`**
- Sign-in attempt when email verification is enabled and **Email verified** is false shows form-level error: **`Verify your email before signing in.`** with link **Resend verification email** → **Verify email** (REQ-AUTH-011).
- Successful sign-in when the password has **expired** (REQ-AUTH-009) opens **Required password change** instead of **Issues list**.
- When email verification is enabled, successful sign-in is allowed only if **Email verified** is true (evaluated before password expiration).
- Footer link on **Login**: **Forgot password?** → **Forgot password** (REQ-AUTH-006).
- Footer link on **Login**: **Create an account** → **Register** — shown only when **Public registration enabled** is **true** (REQ-AUTH-012).

### Register screen

- Available only when **Public registration enabled** is **true** (REQ-AUTH-012). When disabled, guests cannot open **Register** (see REQ-AUTH-012).

| Field                | Behavior                                                                       |
| -------------------- | ------------------------------------------------------------------------------ |
| **First name**       | Text field, **required**; max **100** characters.                              |
| **Last name**        | Text field, **required**; max **100** characters.                              |
| **Email**            | Text field, **required**; valid email; max **320** characters; must be unique. |
| **Password**         | Password field, **required**; **8–128** characters.                            |
| **Confirm password** | **Required**; must match **Password**.                                         |

- When **Email verification enabled** is **false** (REQ-AUTH-011), successful registration creates a user with **Deactivated** false, **Email verified** true, assigns the default **User** role (REQ-ROL-006), sets **password last changed at**, creates a **browser session**, and opens **Issues list** — same outcome as sign-in with **Remember me** unchecked.
- When **Email verification enabled** is **true**, successful registration creates a user with **Deactivated** false, **Email verified** false, assigns the **User** role, sets **password last changed at**, sends **Verify your email**, does **not** create a session, and opens **Verify email** with message **`Account created. Check your email to verify your address before signing in.`**
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
- **Create account** button: on success navigate to **Issues list** when email verification is disabled; navigate to **Verify email** when email verification is enabled; on failure remain on **Register**.
- Footer link on **Register**: **Sign in** → **Login**.
- While submit is in progress, the submit button shows a loading state; the form remains visible.

### States and business rules

- Registration is available only when **Public registration enabled** is **true** (REQ-AUTH-012).
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
- If renewal fails because the session expired, was revoked, the browser was closed (**browser session**), or **Deactivated** is **true**, the user is signed out and redirected to **Login**.

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

- **Sign out everywhere** button is a header action on **My account** (REQ-USR-001).
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

### Active sessions on My account

- Section on **My account** (REQ-USR-001), not a separate screen.
- Section title: **Active sessions**; collapsible panel; default **collapsed**.
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
- The sessions table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- While the list is loading, a loading indicator is shown in the list area; the rest of the screen remains visible.

### Actions

- **Revoke** on a non-current row opens confirmation dialog: **`Revoke this session? That device will be signed out.`**
- On confirm, that session is revoked and the row is removed from the list without reloading the entire screen.
- The **Revoke** button is **not shown** on the **Current session** row; the user signs out the current browser via **Logout** (REQ-AUTH-003).
- **Sign out everywhere** is a header action (REQ-AUTH-003); requires **Sessions.ManageOwn**.

### Permissions and visibility

- **Sessions.ViewOwn**: required to show the **Active sessions** section and view the list.
- **Sessions.ManageOwn**: required for **Revoke** on non-current rows and **Sign out everywhere**.

---

# REQ-AUTH-005: Change Password

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

# REQ-AUTH-006: Forgot and Reset Password (Self-Service)

## Goal

A user who forgot their password must be able to request a reset link by email and set a new password without signing in.

## Features

### Forgot password screen

- Screen: **Forgot password**; available to guests.
- Route linked from **Login** footer **Forgot password?**
- **Back to sign in** at the top → **Login**.

| Field     | Behavior                                      |
| --------- | --------------------------------------------- |
| **Email** | **Required**; valid email; max **320** chars. |

- **Send reset link** button: always shows success message **`If an account exists for this email, a reset link has been sent.`** and remains on **Forgot password** (does not reveal whether the email exists).
- Reset link is valid for **24 hours** (configurable by administrators in deployment settings; default **24**).

### Reset password screen

- Screen: **Reset password**; available to guests with valid token in the link query string.
- Invalid or expired token shows form-level error: **`This reset link is invalid or has expired. Request a new link from the sign-in page.`** with link to **Forgot password**.

| Field                    | Behavior                                                       |
| ------------------------ | -------------------------------------------------------------- |
| **New password**         | **Required**; must satisfy **Password policy** (REQ-AUTH-008). |
| **Confirm new password** | **Required**; must match **New password**.                     |

- **Reset password** button: on success revoke all sessions, redirect to **Login** with message **`Password reset. Sign in with your new password.`**
- On success, **password last changed at** is set to the time of reset (REQ-AUTH-009).
- Sends **Password reset completed** email (REQ-AUTH-007).

---

# REQ-AUTH-007: Auth Email Notifications

## Goal

The system must notify users by email about important account security events.

## Features

### Notification types

| Event                        | When sent                                                             | Subject (example)              |
| ---------------------------- | --------------------------------------------------------------------- | ------------------------------ |
| **Account invitation**       | Admin **Create user** or **Resend invitation** (REQ-USR-008) succeeds | `You're invited to ChangeMe`   |
| **Password reset requested** | Self-service forgot password or admin send reset                      | `Reset your ChangeMe password` |
| **Password reset completed** | Reset password or accept invite succeeds                              | `Your password was changed`    |
| **Password changed**         | Signed-in **Change password** succeeds                                | `Your password was changed`    |
| **Verify your email**        | Registration when verification enabled; **Resend verification email** | `Verify your ChangeMe email`   |

- Each email contains a short summary and, where applicable, a button link to the frontend (invitation, reset, verify email, or sign-in).
- Email delivery uses the configured mail server (same as issue notifications).
- Failed email delivery does not roll back the triggering action; the UI still shows the success message for the action.

---

# REQ-AUTH-008: Password Policy

## Goal

Password strength rules must be consistent across registration, user creation flows, password change, and reset, and configurable per deployment.

## Features

### Default rules (when not overridden in deployment settings)

- Minimum length **8**, maximum length **128**.
- At least one uppercase letter, one lowercase letter, and one digit.
- Special characters are **not required** by default.

### User-visible validation

- Violations show inline on the password field with a specific message (for example **`Password must contain at least one uppercase letter.`**).
- All password forms load policy hints from the server on screen open.

### Configuration

- Deployment settings define minimum length, maximum length, and each character-class requirement.
- Changing settings affects new password entry only; existing passwords are not re-validated until change.

---

# REQ-AUTH-009: Password Expiration

## Goal

When password expiration is enabled in deployment settings, users whose password is older than the configured maximum age must set a new password immediately after sign-in, before using the rest of the application.

## Features

### Password expiration policy

- Deployment settings include **Password expiration enabled**; default **false**.
- When **Password expiration enabled** is **true**, **Maximum password age (days)** applies; default **90**.
- When **Password expiration enabled** is **false**, expiration is not evaluated and sign-in never redirects to **Required password change** for age reasons.

### Password age

- The system records **password last changed at** for each user who **Has password set** true.
- **Password last changed at** is set when the user first receives a password and whenever the password is changed successfully (registration, accept invitation, reset password, change password, required password change).
- Invite-pending users (**Has password set** false) are not evaluated for expiration.
- **Password expires at** is not stored. For administrators viewing **User details** (REQ-USR-004), when **Password expiration enabled** is **true**, the UI shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)** (calendar date and time in the deployment time zone). The field is omitted when expiration is disabled.

### Sign-in and expiration

- After successful authentication (REQ-AUTH-001), if **Password expiration enabled** is **true** and the password age exceeds **Maximum password age (days)**, the user opens **Required password change** instead of **Issues list**.
- If the password is within age, sign-in opens **Issues list** as usual.
- The initial administrator account created at first startup is subject to the same expiration rules as other users; first sign-in does **not** require a password change solely because the account is new.

### Required password change screen

- Screen: **Required password change**
- User cannot navigate to other application screens until the password is changed (except **Logout**).

| Field                    | Behavior                                                                                         |
| ------------------------ | ------------------------------------------------------------------------------------------------ |
| **New password**         | **Required**; **Password policy** (REQ-AUTH-008); must differ from the password used to sign in. |
| **Confirm new password** | **Required**; must match **New password**.                                                       |

- **Change password** button: on success updates the password, sets **password last changed at** to the current time, revokes all other active sessions for the user, keeps the current session, opens **Issues list** with message **`Password updated.`**
- Sends **Password changed** email (REQ-AUTH-007).

### States and business rules

- Voluntary **Change password** (REQ-AUTH-005) and completed **Reset password** (REQ-AUTH-006) update **password last changed at**; the user signs in again before expiration is evaluated.
- **Send password reset** (REQ-USR-006) does not change age until the user completes **Reset password**.
- **Out of scope for this REQ:** email warnings before expiry, grace logins after expiry, per-user exemption from expiration.

---

# REQ-AUTH-010: Accept Account Invitation

## Goal

A user created by an administrator must set their password via a secure link before signing in and must provide or confirm their profile name on the same screen.

## Features

### Accept invitation screen

- Screen: **Accept invitation**; available to guests with valid token in the link.
- Invalid or expired token shows: **`This invitation link is invalid or has expired. Contact your administrator.`**

| Field                    | Behavior                                                                                                                                   |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **First name**           | **Required**; max **100** characters; pre-filled from the account when the administrator already set a value (REQ-USR-003); user may edit. |
| **Last name**            | **Required**; max **100** characters; pre-filled when already set; user may edit.                                                          |
| **New password**         | **Required**; **Password policy** (REQ-AUTH-008).                                                                                          |
| **Confirm new password** | **Required**; must match **New password**.                                                                                                 |

- **Activate account** button: on success stores the submitted **First name** and **Last name** (replacing any admin-entered values), sets **Has password set** true, sets **password last changed at**, clears invite-pending state, redirects to **Login** with message **`Account activated. Sign in with your new password.`**
- If **Email verified** is not already true, sets **Email verified** true and **Email verified at** (mailbox proof via invitation link). Admin-invited users are already verified when the invitation was sent (REQ-USR-003).
- Sends **Password reset completed** email (same template as password set confirmation).

### Business rules

- Invitation link is valid for **72 hours** by default (configurable in deployment settings).
- Until invitation is accepted, the user cannot sign in with a password (REQ-AUTH-001).
- When the administrator left **First name** and **Last name** empty, the user must enter both on **Accept invitation**. When the administrator set one or both, those values appear pre-filled and the user may change them before submit.
- When email verification is enabled, accepting an invitation satisfies **Email verified**; the user must still sign in and is not automatically signed in on the **Accept invitation** screen.

---

# REQ-AUTH-011: Email Verification

## Goal

When email verification is enabled in deployment settings, users must prove control of their email address before using the application. Administrators can manually confirm a user's email when needed.

## Features

### Email verification policy

- Deployment settings include **Email verification enabled**; default **false**.
- When **Email verification enabled** is **false**, every account is treated as **Email verified** true (including new registrations and the initial administrator).
- When **Email verification enabled** is **true**, each account has **Email verified** (true or false) and optional **Email verified at** (date and time when verification last succeeded).
- **Email verification link lifetime (hours)** applies when verification is enabled; default **72**.

### Sign-in gate

- A user with **Deactivated** false and **Email verified** false cannot sign in when verification is enabled (REQ-AUTH-001).
- Password reset (REQ-AUTH-006) and forgot password remain available for unverified users so they can recover access to the mailbox and complete verification.

### Verify email screen (guest)

- Screen: **Verify email**; available to guests.
- Shown after registration when verification is enabled, and reachable from **Resend verification email** on **Login**.
- **Back to sign in** at the top → **Login**.

| Field / element               | Behavior                                                                                                                                                              |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Email**                     | **Required** when resending; valid email; max **320** characters. Pre-filled when the user arrives from registration (read-only) or empty when opened from **Login**. |
| **Resend verification email** | Sends a new link if an unverified account exists; always shows **`If an unverified account exists for this email, a verification link has been sent.`**               |

- Opening a valid verification link from email (guest, token in URL) sets **Email verified** true and **Email verified at**, invalidates the token, and redirects to **Login** with message **`Email verified. You can sign in now.`**
- Invalid or expired link shows: **`This verification link is invalid or has expired.`** with **Resend verification email** on the same screen.

### Interaction with other auth flows

| Flow                                    | When verification enabled                                                                 |
| --------------------------------------- | ----------------------------------------------------------------------------------------- |
| **Register** (REQ-AUTH-001)             | No session; **Verify email** screen; verification email sent                              |
| **Accept invitation** (REQ-AUTH-010)    | **Email verified** true on success (mailbox proven by invitation link)                    |
| **Admin create user** (REQ-USR-003)     | **Email verified** true when invitation is sent to the user's email                       |
| **Initial administrator** (REQ-ROL-006) | **Email verified** true at creation                                                       |
| **Password expiration** (REQ-AUTH-009)  | Evaluated only after a successful sign-in; sign-in requires **Email verified** true first |
| **Deactivated** (REQ-USR-005)           | **Deactivated** true blocks sign-in regardless of verification                            |

### States and business rules

- **Email verified** and **Has password set** are independent of **Deactivated**; an enabled account (**Deactivated** false) may still be unverified or invite-pending.
- Assignable-user lists (REQ-USR-005) include only users with **Deactivated** false; when verification is enabled they must also be verified and have a password set.
- **Out of scope for this REQ:** changing the email address on an account; email change would reset verification (see REQ-USR-001 out of scope).

---

# REQ-AUTH-012: Public Registration Policy

## Goal

Deployments must be able to turn off self-service account registration so new users are onboarded only by administrators.

## Features

### Public registration policy

- Deployment settings include **Public registration enabled**; default **true**.
- When **Public registration enabled** is **true**, behavior matches REQ-AUTH-001 (**Register** screen, **Create an account** link, registration API).
- When **Public registration enabled** is **false**, guests cannot create accounts; administrators onboard users via **Create user** (REQ-USR-003).

### Login screen

- Footer link **Create an account** → **Register** is **not shown** when **Public registration enabled** is **false**.

### Register screen and registration API

- When **Public registration enabled** is **false**:
  - the **Register** route is not available to guests;
  - a guest who opens the register URL is redirected to **Login** with message **`Registration is disabled. Contact an administrator.`**;
  - registration API requests are rejected (for example **403 Forbidden**) with a clear error; the UI does not expose registration when disabled.

### Unaffected flows when registration is disabled

| Flow                                                    | Still available                                        |
| ------------------------------------------------------- | ------------------------------------------------------ |
| **Login** (REQ-AUTH-001)                                | Yes                                                    |
| **Forgot password** / **Reset password** (REQ-AUTH-006) | Yes                                                    |
| **Accept invitation** (REQ-AUTH-010)                    | Yes                                                    |
| **Verify email** (REQ-AUTH-011)                         | Yes — for existing unverified self-registered accounts |
| **Admin create user** (REQ-USR-003)                     | Yes                                                    |

### Interaction with other auth flows

| Flow                                          | When **Public registration enabled** is **false**                                     |
| --------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Register** (REQ-AUTH-001)                   | Unavailable                                                                           |
| **Verify email** (REQ-AUTH-011)               | Still available for accounts created before disable or while registration was enabled |
| **Email verification enabled** (REQ-AUTH-011) | Independent; admin invite still sets **Email verified** at send (REQ-USR-003)         |
| **Default User role** (REQ-ROL-006)           | Assigned on admin invite and on registration when enabled                             |

### States and business rules

- Disabling public registration does not affect existing accounts, sessions, or pending invitation or verification links.
- **Out of scope for this REQ:** admin UI to change **Public registration enabled** at runtime (setting is deployment configuration only, same as other auth policy flags).
