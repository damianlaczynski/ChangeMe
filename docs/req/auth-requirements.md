# Requirements - Auth

This document covers fifteen REQs for the **Auth** area:
login and registration with tracked sessions, staying signed in, logout, own sessions, password change, self-service email change, self-service reset, auth emails, password policy, password expiration, account invite acceptance, optional email verification, optional public registration, optional two-factor authentication, and optional external identity providers.

**Passkeys (WebAuthn)** are specified in `docs/req/passkeys-requirements.md` (REQ-PKY-001 through REQ-PKY-007). When implemented, **Login** and compliance gates integrate with passkey policy per REQ-PKY-002 and REQ-PKY-006.

Screens **Login**, **Forgot password**, **Reset password**, **Accept invitation**, **Confirm email change**, **Two-factor verification**, and **External sign-in callback** are available to guests when applicable. **Register** and **Verify email** are available to guests only when the corresponding deployment settings allow them (REQ-AUTH-012, REQ-AUTH-011). All other application screens require an authenticated user whose account is **not deactivated** and whose email is **verified** when email verification is enabled (REQ-AUTH-011).

Account lifecycle terms (**awaiting invitation acceptance**, **account invitation**, **external-only account**, **local password**) are defined in `docs/req/invitations-requirements.md` and `docs/req/users-requirements.md` (**Business terms**).

---

# REQ-AUTH-001: Login and Registration with Sessions

## Goal

The user must be able to sign in with email and password and begin an authenticated session tracked by the system. When public registration is enabled (REQ-AUTH-012), the user must also be able to register a new account.

## Features

### Login screen

| Field        | Behavior                                                                          |
| ------------ | --------------------------------------------------------------------------------- |
| **Email**    | Text field, **required**; valid email format; max **320** characters.             |
| **Password** | Password field, **required**; **8–128** characters (same bounds as registration). |

- Successful sign-in opens the **Issues list** screen with the user authenticated, unless a post-authentication gate applies: **Required password change** (REQ-AUTH-009), **Two-factor verification** (REQ-AUTH-013), or **strict two-factor setup** (REQ-AUTH-013).
- The system creates a **session** (full or enrollment bootstrap per REQ-AUTH-013) only after all gates for the sign-in path succeed; **pending sign-in challenge** (two-factor verification before any JWT) is not a session.
- Each created session records **signed in at**, **device / browser label**, and **IP address**.
- The system provides the user’s effective permissions defined in REQ-ROL-001.
- Failed sign-in (unknown email or wrong password) shows form-level error: **`Invalid email or password.`** The message does not reveal whether the email exists.
- Sign-in attempt when the account is **deactivated** shows form-level error: **`This account has been deactivated. Contact an administrator.`**
- Sign-in attempt when the user is **awaiting invitation acceptance** shows form-level error: **`Complete your account setup using the invitation link sent to your email.`** (External sign-in with a matching verified email completes the invitation instead; see REQ-AUTH-010.)
- Sign-in attempt when email verification is **enabled** and the mailbox is **not yet verified** shows form-level error: **`Verify your email before signing in.`** with link **Resend verification email** → **Verify email** (REQ-AUTH-011).
- Successful sign-in when the password has **expired** (REQ-AUTH-009) opens **Required password change** instead of **Issues list**.
- When two-factor authentication is enabled for the account (REQ-AUTH-013), successful password validation opens **Two-factor verification** (pending challenge) instead of issuing a session immediately, unless **strict two-factor setup** or **Required password change** applies per **Combined account compliance gates**.
- When email verification is **enabled**, successful sign-in is allowed only if the mailbox is **verified** (evaluated before password expiration).
- Footer link on **Login**: **Forgot password?** → **Forgot password** (REQ-AUTH-006).
- Footer link on **Login**: **Create an account** → **Register** — shown only when **public registration** is **enabled** (REQ-AUTH-012).
- When **external identity providers** are **enabled** (REQ-AUTH-014), **Login** shows a **Continue with {Provider}** button for each configured provider below the email and password form, separated by an **or** divider.
- When **passkeys authentication enabled** is **true** (REQ-PKY-002), **Login** shows **Sign in with a passkey** per passkeys requirements; passkey sign-in is subject to the same account gates as password sign-in and to **Combined account compliance gates** in REQ-PKY-006 (which extends REQ-AUTH-013 ordering).

### Register screen

- Available only when **Public registration enabled** is **true** (REQ-AUTH-012). When disabled, guests cannot open **Register** (see REQ-AUTH-012).

| Field                | Behavior                                                                       |
| -------------------- | ------------------------------------------------------------------------------ |
| **First name**       | Text field, **required**; max **100** characters.                              |
| **Last name**        | Text field, **required**; max **100** characters.                              |
| **Email**            | Text field, **required**; valid email; max **320** characters; must be unique. |
| **Password**         | Password field, **required**; **8–128** characters.                            |
| **Confirm password** | **Required**; must match **Password**.                                         |

- When **Email verification enabled** is **false** (REQ-AUTH-011), successful registration creates a user with **Deactivated** false, **Email verified** true, assigns the default **User** role (REQ-ROL-006), sets **password last changed at**, creates a session, and opens **Issues list**.
- When **Email verification enabled** is **true**, successful registration creates a user with **Deactivated** false, **Email verified** false, assigns the **User** role, sets **password last changed at**, sends **Verify your email**, does **not** create a session, and opens **Verify email** with message **`Account created. Check your email to verify your address before signing in.`**
- Duplicate email shows form-level error: **`An account with this email already exists.`**, **except** when the email belongs to an existing account that may be completed via public registration (**Invitation canceled** path — REQ-INV-005): no local password, not **awaiting invitation acceptance**, not **deactivated**. In that case registration **sets the password** and profile on the existing row (does not create a second user).

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
- A guest who opens a protected screen is redirected to **Login**.

---

# REQ-AUTH-002: Staying Signed In

## Goal

The user must remain signed in during normal application use without repeated manual sign-in while the session remains valid.

## Features

### Session lifetime

| Setting                         | Default        |
| ------------------------------- | -------------- |
| **Short-lived credential**      | **30 minutes** |
| **Long-lived session lifetime** | **14 days**    |

- Every successful sign-in (password, external provider, or registration when email verification is disabled) creates a session with the **long-lived session lifetime** of **14 days**, counted from **signed in at**.
- The client stores session credentials in **browser local storage** so the user remains signed in after closing and reopening the browser, within the configured lifetime.
- The **short-lived credential** expires **30 minutes** after issuance.

### Automatic renewal

- **60 seconds** before the short-lived credential expires, the system renews it automatically without user action while the session is active.
- On each successful renewal, the system updates **last activity** on the current session.
- The user’s effective permissions are refreshed on renewal (REQ-ROL-001).
- Sessions can be renewed after the browser is closed and reopened, within the **14-day** lifetime.
- If renewal fails because the session expired, was revoked, or **Deactivated** is **true**, the user is signed out and redirected to **Login**.

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

| Column               | Description                                                                                  |
| -------------------- | -------------------------------------------------------------------------------------------- |
| **Device / browser** | Label in format **`{Browser} on {Platform}`** (for example **`Chrome on Windows`**).         |
| **IP address**       | Secondary line under device/browser; shows session IP or **`Unknown`**.                      |
| **Signed in at**     | Session start date and time.                                                                 |
| **Last activity**    | Date and time of last credential renewal or authenticated activity.                          |
| **Current**          | Badge **`Current session`** on the row for the browser the user is signed in with.           |
| **Actions**          | **Revoke** button on every row except the current session (requires **Sessions.ManageOwn**). |

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

### Interaction with two-factor authentication (REQ-AUTH-013)

- **Reset password** does **not** disable **Two-factor enabled** or clear the TOTP secret.
- On successful reset, all **recovery codes** are invalidated; the user must **Regenerate recovery codes** on **My account** after the next successful sign-in when **Two-factor enabled** is **true**.
- The next sign-in after reset follows the normal flow: primary authentication, then **Two-factor verification** when **Two-factor enabled** is **true**, or **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false** (unless **Trust identity provider MFA** applies on external sign-in).

---

# REQ-AUTH-007: Auth Email Notifications

## Goal

The system must notify users by email about important account security events.

## Features

### Notification destination

- Every email in this REQ is sent to the account **Profile email** — the **current email** stored on the user account in ChangeMe.
- **Provider email** from an external identity provider is **never** used as a notification destination.
- Linking or signing in with Google or Microsoft does **not** change **Profile email** (REQ-AUTH-014).
- **Change email** (REQ-AUTH-015) sends **Confirm email change** to the pending **new email** only until confirmation succeeds; all other auth emails during a pending change still use **Profile email** (the current email).

### Notification types

| Event                         | When sent                                                                                                | Subject (example)                          |
| ----------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| **Account invitation**        | Admin **Invite user**, **Resend invitation**, or **Send invitation** (REQ-INV-001, REQ-INV-003) succeeds | `You're invited to ChangeMe`               |
| **Password reset requested**  | Self-service forgot password or admin send reset                                                         | `Reset your ChangeMe password`             |
| **Password reset completed**  | Reset password or accept invite succeeds                                                                 | `Your password was changed`                |
| **Password changed**          | Signed-in **Change password** succeeds                                                                   | `Your password was changed`                |
| **Verify your email**         | Registration when verification enabled; **Resend verification email**                                    | `Verify your ChangeMe email`               |
| **Two-factor enabled**        | User enables two-factor on **My account**                                                                | `Two-factor authentication enabled`        |
| **Two-factor disabled**       | User disables two-factor on **My account**                                                               | `Two-factor authentication disabled`       |
| **Two-factor reset by admin** | Administrator **Reset two-factor** on **User details** (REQ-USR-004)                                     | `Two-factor authentication was reset`      |
| **External account linked**   | User links an external provider on **My account** (REQ-AUTH-014)                                         | `External sign-in method linked`           |
| **External account unlinked** | User or administrator removes an external provider link                                                  | `External sign-in method removed`          |
| **Recovery code used**        | A recovery code succeeds at sign-in or step-up authentication                                            | `A recovery code was used on your account` |
| **Passkey added**             | User completes **Add passkey** on **My account** (REQ-PKY-003)                                           | `Passkey added to your account`            |
| **Passkey removed**           | User or administrator removes a passkey credential (REQ-PKY-003, REQ-PKY-005)                            | `Passkey removed from your account`        |
| **Passkeys reset by admin**   | Administrator **Reset passkeys** on **User details** (REQ-PKY-005)                                       | `Passkeys reset on your account`           |
| **Email change requested**    | Signed-in user submits **Change email** (REQ-AUTH-015)                                                   | `Email change requested on your account`   |
| **Confirm email change**      | **Change email** succeeds; link sent to the **new** email address                                        | `Confirm your new ChangeMe email address`  |
| **Email change completed**    | User completes **Confirm email change** (REQ-AUTH-015)                                                   | `Your ChangeMe email address was changed`  |
| **Email change cancelled**    | User **Cancel email change** on **My account** (REQ-AUTH-015)                                            | `Email change cancelled on your account`   |
| **Email changed by admin**    | Administrator saves a different **Email** on **Edit user** (REQ-USR-003)                                 | `Your ChangeMe email address was changed`  |

- Each email contains a short summary and, where applicable, a button link to the frontend (invitation, reset, verify email, confirm email change, or sign-in).
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

- The system records **password last changed at** for each user who **has a local password**.
- **Password last changed at** is set when the user first receives a password and whenever the password is changed successfully (registration, accept invitation, reset password, change password, required password change).
- Users **awaiting invitation acceptance** are not evaluated for expiration.
- **Password expires at** is not stored. For administrators viewing **User details** (REQ-USR-004), when **Password expiration enabled** is **true**, the UI shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)** (calendar date and time in the deployment time zone). The field is omitted when expiration is disabled.

### Sign-in and expiration

- After successful authentication (REQ-AUTH-001), if **Password expiration enabled** is **true** and the password age exceeds **Maximum password age (days)**, the user opens **Required password change** instead of **Issues list**.
- When **Combined account compliance gates** (REQ-AUTH-013) also apply, **Required password change** takes precedence over two-factor verification and **strict two-factor setup** until the password is updated.
- If the password is within age, sign-in opens **Issues list** as usual.
- The initial administrator account created at first startup is subject to the same expiration rules as other users; first sign-in does **not** require a password change solely because the account is new.

### Password expiry warnings (signed-in)

- When password expiration is **enabled** and the signed-in user **has a local password**, the client receives **Password expires at** (UTC) on each successful sign-in and session refresh (same derivation as **Password expires at** on **User details** in REQ-USR-004).
- While the password is still within age, the application shows **expiry warning toasts** at **14**, **7**, and **1** calendar day(s) before **Password expires at** (inclusive of the warning day; compare using the deployment time zone for display only).
- Each warning threshold is shown **at most once per browser profile** until the password is changed or the threshold no longer applies (for example after a voluntary password change resets **password last changed at**).
- Warning toast copy includes how many day(s) remain and an action to open **Required password change** (dialog; see below). Suggested summary: **`Password expiring soon`**; detail states the remaining day count and that changing the password now avoids interruption.

### Expiration during an active session

- When **Password expiration enabled** is **true** and the password age exceeds **Maximum password age (days)** while the user is already signed in, the user **stays on the current screen and route**; the application does **not** force navigation away from in-progress work on the client.
- The application shows a **sticky expiry toast** with summary **`Password expired`** and detail **`Your password has expired. Set a new password to save your work to the server.`** The toast includes an action **`Change password`** that opens **Required password change** (dialog).
- Until the password is changed, server requests for application data (except sign-out, session refresh, and required password change) are rejected; purely local UI state on the current screen remains available so the user can copy or note work before changing the password.
- When session refresh or the next blocked request establishes that **password change required** is true, the sticky expiry toast is shown if it is not already visible.
- **Logout** remains available at all times.

### Required password change

Two surfaces share the same validation and API behavior:

| Surface                               | When used                                                                                                |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| **Required password change** (screen) | After sign-in when the password is already expired (see **Sign-in and expiration**).                     |
| **Required password change** (dialog) | From expiry warning toasts, the sticky expiry toast, or when expiration occurs during an active session. |

| Field                    | Behavior                                                                                         |
| ------------------------ | ------------------------------------------------------------------------------------------------ |
| **New password**         | **Required**; **Password policy** (REQ-AUTH-008); must differ from the password used to sign in. |
| **Confirm new password** | **Required**; must match **New password**.                                                       |

- **Change password** button: on success updates the password, sets **password last changed at** to the current time, revokes all other active sessions for the user, keeps the current session, clears expiry warnings and the sticky expiry toast, and shows message **`Password updated.`**
- After success following **Sign-in and expiration**, the application opens **Issues list**.
- After success from the dialog during an active session, the application **closes the dialog** and **keeps the current route**; the user may retry server actions without signing in again.
- Sends **Password changed** email (REQ-AUTH-007).

#### Required password change screen

- Full-page route: **Required password change**
- After sign-in with an expired password, the client enters **strict password change** mode: the user cannot navigate to other application screens until the password is changed (except **Logout**); the application shows only the minimal chrome (no sidebar or main navigation).
- **Strict password change** applies only after sign-in (or register) with an expired password, not when expiration is detected during an active session.

### States and business rules

- Voluntary **Change password** (REQ-AUTH-005) and completed **Reset password** (REQ-AUTH-006) update **password last changed at**; the user signs in again before expiration is evaluated.
- **Send password reset** (REQ-USR-006) does not change age until the user completes **Reset password**.
- **Out of scope for this REQ:** email warnings before expiry, grace logins after expiry, per-user exemption from expiration, configurable warning day thresholds (fixed at 14, 7, and 1).

---

# REQ-AUTH-010: Accept Account Invitation

## Goal

A user created by an administrator must complete onboarding before using the application. Onboarding may be completed **either** by setting a local password through the invitation email link **or** by signing in with an external identity provider when external sign-in is enabled (REQ-AUTH-014). Profile name may be supplied on the invitation screen or taken from the identity provider when the administrator did not set both names.

## Features

### Accept invitation screen (email link)

- Screen: **Accept invitation**; available to guests with valid token in the link.
- When invitation preview is **valid**, show read-only line above the form: **`Activating account for {email}`** (REQ-INV-007).
- Invalid or expired token shows: **`This invitation link is invalid or has expired. Contact your administrator.`**

| Field                    | Behavior                                                                                                                                   |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **First name**           | **Required**; max **100** characters; pre-filled from the account when the administrator already set a value (REQ-USR-003); user may edit. |
| **Last name**            | **Required**; max **100** characters; pre-filled when already set; user may edit.                                                          |
| **New password**         | **Required**; **Password policy** (REQ-AUTH-008).                                                                                          |
| **Confirm new password** | **Required**; must match **New password**.                                                                                                 |

- **Activate account** button: on success stores the submitted **First name** and **Last name** (replacing any admin-entered values), establishes a **local password**, sets **password last changed at**, marks the pending **account invitation** as accepted (invitation utilized; clears **Pending invitation** on **User details**; **accepted** row kept for history; **revoked** rows removed by retention — REQ-INV-006), redirects to **Login** with message **`Account activated. Sign in with your new password.`**
- When **external providers enabled** is **true**, the screen also shows **Continue with {Display name}** actions (same layout as **Login** and **Register**): an **or** divider and one button per enabled provider. These start the same OIDC flow as guest external sign-in (REQ-AUTH-014); they do not require the invitation token. External sign-in actions are shown only when the invitation link preview is **valid**; invalid or expired links show the error message only.
- If **Email verified** is not already true, sets **Email verified** true and **Email verified at** (mailbox proof via invitation link). Admin-invited users are already verified when the invitation was sent (REQ-INV-001).
- Sends **Password reset completed** email (same template as password set confirmation).

### Accept invitation via external sign-in

When **external providers enabled** is **true** and the user is **awaiting invitation acceptance**:

- On **Login** and **Accept invitation**, the user may choose **Continue with {Display name}** instead of setting a local password (email link) or signing in with email and password.
- The administrator invited a **specific email address**; external sign-in onboarding is allowed **only** when the provider returns a **verified email** that **exactly matches** the invited account **Profile email**. This is the **only** guest external sign-in path that links a provider to an existing account without a prior ChangeMe session (REQ-AUTH-014).
- When the verified provider email matches the invited account, the system **completes the invitation** in one step:
  - links the external provider to the account;
  - does **not** require a local password — the account becomes **external-only**;
  - marks the pending **account invitation** as accepted and clears **Pending invitation** (**accepted** row kept for history; **revoked** rows removed by retention — REQ-INV-006);
  - invalidates unused invitation tokens for that user;
  - applies **First name** and **Last name** from the provider when the administrator left both empty and the provider supplies both; otherwise keeps administrator-entered values;
  - signs the user in (subject to deactivation, two-factor, and other gates per REQ-AUTH-013 and REQ-AUTH-014).
- Sends **External account linked** email (REQ-AUTH-007) to the invited **Profile email**.
- If the provider email does **not** match the invited **Profile email**, is not verified, or external sign-in is disabled: do **not** complete the invitation; redirect to **Login** or **Accept invitation** with form-level error **`The external account email does not match the invited email address.`**

### Business rules (all invitation paths)

- Invitation link is valid for **72 hours** by default (configurable in deployment settings).
- Until invitation acceptance completes, the user cannot sign in with email and password (REQ-AUTH-001).
- When the administrator left **First name** and **Last name** empty, the user must supply both on **Accept invitation** (email link). When accepting via external sign-in, both names are taken from the provider when available; otherwise the user may complete the profile later on **Edit profile**.
- When the administrator set one or both names, those values are kept unless the user edits them on **Accept invitation** (email link).
- When email verification is enabled, invitation acceptance (either path) satisfies **Email verified**; the email-link path does not automatically sign the user in; the external path signs the user in immediately on success.

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

| Flow                                         | When verification enabled                                                                                      |
| -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| **Register** (REQ-AUTH-001)                  | No session; **Verify email** screen; verification email sent                                                   |
| **Accept invitation** (REQ-AUTH-010)         | **Email verified** on success (email link or external sign-in)                                                 |
| **Admin invite user** (REQ-INV-001)          | **Email verified** true when invitation is sent to the user's email                                            |
| **Initial administrator** (REQ-ROL-006)      | **Email verified** true at creation                                                                            |
| **Password expiration** (REQ-AUTH-009)       | Evaluated only after a successful sign-in; sign-in requires **Email verified** true first                      |
| **Deactivated** (REQ-USR-005)                | **Deactivated** true blocks sign-in regardless of verification                                                 |
| **Self-service email change** (REQ-AUTH-015) | **Current email** remains for sign-in until confirmation; successful confirmation sets **Email verified** true |

### States and business rules

- Email verification status and whether the user **has a local password** are independent of deactivation; an **enabled** account may still be unverified or **awaiting invitation acceptance**.
- Assignable-user lists (REQ-USR-005) include only users with **Deactivated** false; when verification is enabled they must also be verified and have a password set.
- Self-service email change follows REQ-AUTH-015; completing a pending change sets **Email verified** true and **Email verified at** to the confirmation time regardless of the global **Email verification enabled** flag.

---

# REQ-AUTH-012: Public Registration Policy

## Goal

Deployments must be able to turn off self-service account registration so new users are onboarded only by administrators.

## Features

### Public registration policy

- Deployment settings include **Public registration enabled**; default **true**.
- When **Public registration enabled** is **true**, behavior matches REQ-AUTH-001 (**Register** screen, **Create an account** link, registration API).
- When **Public registration enabled** is **false**, guests cannot create accounts; administrators onboard users via **Invite user** (REQ-INV-001).

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

---

# REQ-AUTH-013: Two-Factor Authentication

## Goal

When two-factor authentication is enabled in deployment settings, users must be able to protect their account with a time-based one-time password (TOTP) from an authenticator app. Deployments may require two-factor for every account or allow voluntary opt-in.

## Features

### Two-factor authentication policy

- Deployment settings include **Two-factor authentication enabled**; default **false**.
- When **Two-factor authentication enabled** is **false**, two-factor authentication is not offered or enforced; existing enrollment data may remain in storage but is inactive until the setting is turned on again (see **Deployment policy changes**).
- When **Two-factor authentication enabled** is **true**, deployment settings include **Two-factor authentication required**; default **false**.
- When **two-factor authentication required** is **enabled**, every active account must enroll in two-factor before using the application, except users **awaiting invitation acceptance** (they complete **Accept invitation** first).
- When **Two-factor authentication required** is **false**, two-factor authentication is **optional**; users enable or disable it on **My account** (REQ-USR-001).
- Deployment settings include **Trust identity provider MFA**; default **false**. Applies only when **Two-factor authentication enabled** and **External providers enabled** (REQ-AUTH-014) are both **true**. When **true**, external sign-in may satisfy two-factor requirements using the IdP’s MFA assertion instead of app TOTP (see **Trust identity provider MFA** and REQ-AUTH-014).

### Trust identity provider MFA

- When **Trust identity provider MFA** is **false**, external sign-in follows the same two-factor rules as password sign-in (verification or **strict two-factor setup**); IdP MFA is ignored.
- When **Trust identity provider MFA** is **true**, after external sign-in the system inspects the IdP assertion for provider MFA. For **Google** and **Microsoft** templates, MFA is recognized when the OIDC **`amr`** claim includes **`mfa`** (provider-specific rules are documented in deployment configuration; generic OIDC providers use the same **`amr`** convention when present).
- When provider MFA is **asserted** on external sign-in:
  - Skip **Two-factor verification** for that sign-in, even when **Two-factor enabled** is **true**.
  - When **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**, allow full application access without **strict two-factor setup** on that sign-in path.
- When provider MFA is **not asserted** on external sign-in, normal two-factor rules apply (**Two-factor verification** or **strict two-factor setup**). When the user has **Two-factor enabled** **true** and signs in via IdP without MFA assertion, **Two-factor verification** is **required** — IdP sign-in alone is not sufficient.
- **Trust identity provider MFA** never affects password sign-in: password sign-in always requires app TOTP when **Two-factor enabled** is **true**, and **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**.
- Voluntarily enrolled app TOTP remains stored; password sign-in continues to use it regardless of **Trust identity provider MFA**.

### Sign-in challenge types

Two mechanisms cover post-primary-auth flows; they are not interchangeable:

| Mechanism                        | When used                                                                                                                                                                 | Session issued?                                                                                                    |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Pending sign-in challenge**    | Primary auth succeeded; **Two-factor enabled** true; verification needed                                                                                                  | **No** — short-lived server-side challenge only (guest **Two-factor verification**).                               |
| **Enrollment bootstrap session** | Primary auth succeeded; two-factor required but **Two-factor enabled** false (and IdP MFA does not satisfy policy); or policy change forces setup while already signed in | **Yes** — limited JWT; middleware allows only two-factor setup, logout, and session refresh until setup completes. |

- **Pending sign-in challenge** expires after **10 minutes**; no application JWT is created until verification succeeds.
- **Enrollment bootstrap session** uses the same restriction pattern as password expiration (REQ-AUTH-009): **strict two-factor setup** on the client; server rejects other application APIs with **403 Forbidden** until **Two-factor enabled** becomes **true** or policy no longer requires enrollment.

### Account model

| Concept                   | Storage                               | Meaning                                                                                                                                                                   |
| ------------------------- | ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Two-factor enabled**    | Boolean; default **false**            | **true** — password sign-in requires TOTP or recovery code after primary authentication unless an external path satisfies **Trust identity provider MFA** (REQ-AUTH-013). |
| **Two-factor enabled at** | Date and time; empty when disabled    | Set when **Two-factor enabled** becomes **true**; cleared when two-factor is disabled or reset.                                                                           |
| **Two-factor secret**     | Encrypted secret; empty when disabled | TOTP shared secret for authenticator apps; never exposed after initial setup except during enrollment QR display.                                                         |
| **Recovery codes**        | Hashed one-time codes                 | Single-use backup codes generated at enrollment and on regeneration; plain codes are shown only once.                                                                     |

- Recovery codes are stored hashed; each code is invalidated after one successful use.
- The system stores **10** recovery codes when enrollment completes and when the user regenerates codes.
- TOTP validation accepts the current time step and **±1** adjacent step (**30**-second window each) to tolerate clock skew between server and authenticator app.

### Sensitive account actions (step-up authentication)

The following actions require **step-up authentication** in addition to an active session:

| Action                                | Where                           |
| ------------------------------------- | ------------------------------- |
| **Disable two-factor authentication** | **My account**                  |
| **Regenerate recovery codes**         | **My account**                  |
| **Link {Display name}** (signed-in)   | **My account** (REQ-AUTH-014)   |
| **Unlink** external provider          | **My account** (REQ-AUTH-014)   |
| **Set password**                      | **My account** (REQ-AUTH-014)   |
| **Change email** (submit)             | **Change email** (REQ-AUTH-015) |
| **Cancel email change**               | **My account** (REQ-AUTH-015)   |

Step-up rules (all that apply must succeed):

| Condition                                                      | Requirement                                                                                                                                                                                   |
| -------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| User **has a local password**                                  | **Current password** must match.                                                                                                                                                              |
| User is **two-factor enrolled**                                | Valid **TOTP** or unused **recovery code**.                                                                                                                                                   |
| User is **external-only** (no local password, linked provider) | **Step-up external sign-in**: complete a fresh OIDC flow with a **linked** provider within the last **15 minutes** before the action (server-stored step-up timestamp per user and provider). |

- **Enable two-factor authentication** and **Set up two-factor authentication** already require password (when applicable) and verification code as part of enrollment; they follow enrollment rules, not this table.
- Failed step-up attempts use the same rate limits as **Two-factor verification** where codes are involved.
- When a **recovery code** is consumed during step-up, sends **Recovery code used** email (REQ-AUTH-007).
- Guest **Accept invitation** external onboarding uses invited **Profile email** match only (REQ-AUTH-010); step-up session rules do not apply on that path.

### My account — two-factor section

- Collapsible section **Two-factor authentication** on **My account** (REQ-USR-001); shown only when **Two-factor authentication enabled** is **true**.
- Section title: **Two-factor authentication**; default **collapsed**.

| State                    | Section content                                                                                                                              |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Disabled**             | Description of two-factor authentication; **Enable two-factor authentication** button.                                                       |
| **Enabled**              | Badge **`Enabled`**; **Two-factor enabled at** (read-only); **Regenerate recovery codes** and **Disable two-factor authentication** actions. |
| **Required, not set up** | Warning **`Two-factor authentication is required for your account.`** and **Set up two-factor authentication** button (same flow as enable). |

- **Enable two-factor authentication** and **Set up two-factor authentication** open **Set up two-factor authentication** (dialog or dedicated screen on **My account** route subtree).
- **Disable two-factor authentication** is available only when **Two-factor authentication required** is **false**; opens confirmation and requires **Sensitive account actions** step-up authentication (REQ-AUTH-013).

### Set up two-factor authentication

| Step / element        | Behavior                                                                                                                   |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| **Current password**  | **Required** when the user **has a local password**; must match the current password. Omitted for **external-only** users. |
| **Authenticator QR**  | Displays a QR code and manual entry key for the authenticator app; issuer name **`ChangeMe`**.                             |
| **Verification code** | **Required**; **6** digits; must match the current TOTP from the configured secret.                                        |

- **Confirm setup** button: on success sets **Two-factor enabled** true, **Two-factor enabled at**, stores the encrypted secret, generates **10** recovery codes, shows the recovery codes once in a read-only list with copy guidance, and shows message **`Two-factor authentication enabled.`**
- Sends **Two-factor enabled** email (REQ-AUTH-007).
- The user must acknowledge **`I have saved my recovery codes`** before closing the recovery-code step.
- **Cancel** closes the flow without saving.

### Regenerate recovery codes

- Requires **Sensitive account actions** step-up authentication (REQ-AUTH-013).
- On success invalidates all previous recovery codes, generates **10** new codes, shows them once, and shows message **`Recovery codes regenerated.`**
- Does not change **Two-factor enabled** or the TOTP secret.

### Disable two-factor authentication

- Available only when **Two-factor authentication required** is **false**.
- Requires **Sensitive account actions** step-up authentication (REQ-AUTH-013).
- On success clears **Two-factor enabled**, secret, and recovery codes; shows message **`Two-factor authentication disabled.`**
- Sends **Two-factor disabled** email (REQ-AUTH-007).

### Two-factor verification screen (guest)

- Screen: **Two-factor verification**; available to guests during sign-in when primary authentication succeeded and **Two-factor enabled** is **true**.
- Reached from **Login** (REQ-AUTH-001) and from external provider sign-in (REQ-AUTH-014) before a session is created.
- **Back to sign in** at the top → **Login** (clears the pending sign-in challenge).

| Field / element       | Behavior                                                                                                                       |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| **Verification code** | **Required**; **6** digits for TOTP, or a recovery code in the same field (recovery codes are alphanumeric, case-insensitive). |

- **Verify** button: on success creates the session per REQ-AUTH-001 / REQ-AUTH-014 and continues the normal post-sign-in flow (password expiration, **Issues list**, etc.). When a **recovery code** was used, sends **Recovery code used** email (REQ-AUTH-007).
- Invalid code shows form-level error: **`Invalid verification code.`**
- **Use a recovery code** helper text explains that a recovery code may be entered instead of a TOTP code.
- Pending sign-in challenge expires after **10 minutes**; expired challenge redirects to **Login** with message **`Sign-in timed out. Try again.`**
- Applies only to the **pending sign-in challenge** path; it does not apply to users in **enrollment bootstrap session** (they already hold a limited JWT).

### Two-factor verification rate limiting

- Each **pending sign-in challenge** allows at most **5** failed verification attempts (invalid TOTP or recovery code).
- After **5** failures, the challenge is invalidated immediately; the user is redirected to **Login** with message **`Too many attempts. Sign in again.`**
- A new challenge requires successful primary authentication again.
- The same **5**-attempt limit applies per step-up action flow when **Two-factor enabled** true and a code is required (**Sensitive account actions**).
- Rate limits are enforced server-side; error messages do not indicate whether a recovery or TOTP format was expected.

### Mandatory enrollment

- When **Two-factor authentication required** is **true**, the user has **Two-factor enabled** false, and IdP MFA does not satisfy policy (**Trust identity provider MFA** is **false**, or external sign-in was not used, or the IdP did not assert MFA), the system issues an **enrollment bootstrap session** after successful primary authentication (password or external provider).
- The client enters **strict two-factor setup** mode: the user cannot navigate to other application screens until setup completes (except **Logout**); the application shows only minimal chrome.
- **Strict two-factor setup** uses the same **Set up two-factor authentication** flow as voluntary enrollment on **My account**.
- After successful setup, the application opens **Issues list**, subject to **Combined account compliance gates** (password expiration is resolved before two-factor setup when both apply on the same sign-in).

### Deployment policy changes

Deployment settings are read on each sign-in, session refresh, and authenticated API request (same evaluation model as **Password expiration enabled** in REQ-AUTH-009). When settings change in `appsettings` and the application restarts or reloads configuration, the system applies the **current** policy immediately; users are not grandfathered out of new requirements.

| Setting change                                                                                               | Effect on existing signed-in users                                                                                                                                                                                                                       |
| ------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Two-factor authentication enabled** **false** → **true**, **Two-factor authentication required** **false** | No forced action; two-factor becomes available on **My account** only.                                                                                                                                                                                   |
| **Two-factor authentication required** turned **on** (while two-factor is enabled)                           | Users who are not yet **two-factor enrolled** (except those **awaiting invitation acceptance**) receive **`twoFactorSetupRequired`** on the next session refresh or blocked API response; client enters **strict two-factor setup** without signing out. |
| **Two-factor authentication enabled** **true** → **false**                                                   | Enforcement stops on next refresh; stored secrets remain but are inactive; **strict two-factor setup** ends immediately.                                                                                                                                 |
| **Two-factor authentication required** **true** → **false**                                                  | **Strict two-factor setup** ends on next refresh for users who had not enrolled; enrolled users keep two-factor until they disable it voluntarily.                                                                                                       |
| **Trust identity provider MFA** toggled                                                                      | Affects the next external sign-in only; does not disable existing app TOTP enrollment.                                                                                                                                                                   |

- While **`twoFactorSetupRequired`** is **true**, the user **stays on the current screen and route** when the flag is raised during an active session (same UX principle as password expiration during an active session in REQ-AUTH-009); the application shows a **sticky toast** with summary **`Two-factor authentication required`** and detail **`Set up two-factor authentication to continue saving your work to the server.`** with action **`Set up now`** opening **Set up two-factor authentication**.
- Until setup completes, server requests for application data (except sign-out, session refresh, and two-factor setup endpoints) are rejected; purely local UI state on the current screen remains available.
- On the next sign-in after a policy change, users without required two-factor follow **Mandatory enrollment** (bootstrap session) or **Two-factor verification** when already enrolled.
- Auth responses (sign-in, refresh) include **`twoFactorSetupRequired`** when enrollment is required under current deployment settings and **Two-factor enabled** is **false**, analogous to **`passwordChangeRequired`**.

### Combined account compliance gates

When both password expiration and two-factor requirements apply, the system enforces **one gate at a time** in this order:

1. **Required password change** (**`passwordChangeRequired`** / strict password change after sign-in).
2. **Two-factor verification** (pending challenge when **Two-factor enabled** true).
3. **Strict two-factor setup** (**`twoFactorSetupRequired`** / enrollment bootstrap session).

- When both **`passwordChangeRequired`** and **`twoFactorSetupRequired`** are **true**, only **`passwordChangeRequired`** is active until the password is updated; **`twoFactorSetupRequired`** is evaluated immediately after a successful required password change on the same sign-in path.
- **Enrollment bootstrap session** and **strict password change** middleware allowlists include only endpoints needed for the **active** gate plus sign-out and session refresh (same pattern as REQ-AUTH-009).
- Auth responses include at most one active compliance flag as primary; the client shows **one** sticky toast for the active gate (password before two-factor).
- The initial administrator account (REQ-ROL-006) follows the same two-factor rules as other users when **Two-factor authentication enabled** and **Two-factor authentication required** apply.

### Sign-in order with other auth rules

Primary authentication (password on **Login** or external provider per REQ-AUTH-014) is evaluated first in this order before two-factor verification:

1. Unknown credentials / provider failure — fail without revealing account existence where applicable.
2. Account is **deactivated** — **`This account has been deactivated. Contact an administrator.`**
3. User is **awaiting invitation acceptance** (password sign-in) — **`Complete your account setup using the invitation link sent to your email.`**
4. Email verification is **enabled** and the mailbox is **not verified** (REQ-AUTH-011) — **`Verify your email before signing in.`**
5. Password expiration after session would otherwise be issued (REQ-AUTH-009) — **Required password change**; two-factor is **not** required until after the password is updated on that sign-in path.

When **Two-factor enabled** is **true**, step 5 applies to password sign-in only after a valid TOTP or recovery code. External provider sign-in applies two-factor after provider success and before session creation unless **Trust identity provider MFA** satisfies the step, or password expiration redirects first.

When **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**, password sign-in issues an **enrollment bootstrap session** (**strict two-factor setup**). External sign-in issues a bootstrap session only when **Trust identity provider MFA** does not apply or the IdP did not assert MFA; otherwise a normal full session is issued.

### Administrator reset

- **Reset two-factor** header action on **User details** (REQ-USR-004); requires **Users.Manage**.
- Shown only when **Two-factor authentication enabled** is **true** and the user's **Two-factor enabled** is **true**.
- Confirmation dialog: **`Reset two-factor authentication for "{full name}"? They will need to set it up again on next sign-in.`**
- On success clears **Two-factor enabled**, secret, and recovery codes; revokes **all active sessions** for the user; success message **`Two-factor authentication reset.`**
- Sends **Two-factor reset by admin** email (REQ-AUTH-007).
- When **Two-factor authentication required** is **true**, the user's next sign-in enters **strict two-factor setup** after primary authentication.

### User details (admin read-only)

- When **Two-factor authentication enabled** is **true**, **User details** shows **Two-factor authentication** badge **`Enabled`** or **`Disabled`** and **Two-factor enabled at** when enabled (REQ-USR-004).

### States and business rules

- Changing password (REQ-AUTH-005) does not disable two-factor authentication.
- **Sign out everywhere** and session revoke (REQ-AUTH-003, REQ-AUTH-004) do not disable two-factor authentication.
- Deactivating a user (REQ-USR-005) does not clear two-factor settings; reactivation preserves them.
- A used recovery code cannot be reused; the system sends **Recovery code used** email (REQ-AUTH-007).
- **Out of scope for this REQ:** SMS or email one-time codes as the second factor; per-role two-factor requirement; trusted devices that skip two-factor; admin UI to change deployment two-factor flags at runtime.
- Passkeys (WebAuthn): `docs/req/passkeys-requirements.md` (REQ-PKY-001 through REQ-PKY-007). **Passkey satisfies two-factor** deployment setting interacts with this REQ; see REQ-PKY-001 and REQ-PKY-006.

---

# REQ-AUTH-014: External Identity Providers

## Goal

Deployments must be able to allow sign-in through configured external identity providers (OpenID Connect) in addition to email and password. Users must be able to link and unlink providers from their account when permitted.

## Features

### External providers policy

- Deployment settings include **External providers enabled**; default **false**.
- Deployment settings include **External provider linking enabled**; default **true**. Effective only when **External providers enabled** is **true**.
- When **External providers enabled** is **false**, external sign-in buttons, **Link**/**Unlink** UI on **My account**, and external sign-in APIs are unavailable.
- When **External providers enabled** is **true** and **External provider linking enabled** is **false**, **Continue with {Display name}** on **Login**, **Register**, and **Accept invitation** remains available; existing **External login** rows remain usable for sign-in and step-up; **Link {Display name}** on **My account** and OIDC **link mode** APIs are unavailable; **Unlink** on **My account** remains available subject to last-sign-in-method rules.
- When **External providers enabled** is **true**, the deployment configures one or more providers. Each provider entry includes at minimum:
  - **Provider key** — stable identifier (for example **`google`**, **`microsoft`**).
  - **Display name** — button label (for example **`Google`**, **`Microsoft`**).
  - **Authority** — OIDC issuer URL.
  - **Client id** and **Client secret** — deployment secrets, not editable from the application UI.
  - **Allowed email domains** — optional list (for example **`example.com`**). When non-empty, external sign-in and linking succeed only when the normalized provider email ends with `@` + one listed domain; otherwise redirect to **Login** with **`Sign-in with this account is not allowed.`**
  - **Issuer validation mode** — optional deployment setting. **Discovery** (default): accept only the issuer published in the provider’s OIDC metadata (typical for Google, single-tenant Microsoft, generic OIDC). **Microsoft multi-tenant**: accept sign-in from any Microsoft Entra tenant; **required** when the configured authority uses `/common` or `/organizations`. Operator detail: `docs/auth-operations-guide.md`.
  - **Trust IdP email without email verified** — optional per provider. When **enabled**, treat the identity provider’s email as verified even when the token does not include an explicit **email verified** flag (common for Microsoft Entra). When **disabled**, new account creation via OIDC registration and **Accept invitation** external onboarding require a verified email assertion from the provider.
- Supported built-in provider templates in documentation and default configuration: **Google** and **Microsoft** (Entra ID / Microsoft identity platform). Additional providers may use the same generic OIDC configuration shape.
- The frontend loads the list of enabled providers (key and display name only) from a public auth settings endpoint; secrets remain server-side. The same endpoint exposes **External providers enabled**, **External provider linking enabled**, and **Self-service email change enabled** (REQ-AUTH-015).
- When **Trust identity provider MFA** is **true** (REQ-AUTH-013), the backend evaluates the IdP **`amr`** claim (and provider-specific equivalents) on each external sign-in callback.

### OIDC protocol security

- Authorization requests use **authorization code** flow with **PKCE** (S256).
- Each authorization request generates a cryptographically random **`state`**; the callback rejects mismatched or missing **state** (CSRF protection).
- Each request includes **`nonce`**; the backend validates **`nonce`** in the ID token on callback (replay protection).
- On callback, the backend discovers authorize and token endpoints from `{Authority}/.well-known/openid-configuration`. ID token **issuer** validation follows **Issuer validation mode** (default: metadata **issuer**); **audience** (client id), **expiry**, and **signature** are always validated against provider metadata before trusting claims.
- For **new account creation** via OIDC on **Register** or **Login**, and for **Accept invitation** external onboarding (REQ-AUTH-010), the provider email is used only when the identity provider asserts it is verified, when **Trust IdP email without email verified** is **enabled** for that provider, or when the token includes provider-specific verified email claims (for example **verified primary email**). If the email is missing or not treated as verified, the system does not create an account by email; an existing **External login** for this provider subject may still sign the user in.
- Authorization codes are single-use; exchanged server-side only (secrets never sent to the frontend).

### External providers disabled at runtime

- When **external providers** are **disabled**, **External login** rows are **retained** but **inactive** (not usable for sign-in or step-up until re-enabled).
- When **external provider linking** is **disabled** (and external providers remain enabled), **Login**, **Register**, and **Accept invitation** still show **Continue with {Display name}**; **My account** hides **Link {Display name}** only.
- **Login**, **Register**, and **My account** hide all external provider UI when **External providers enabled** is **false** (on next settings load).
- Users who **have a local password** continue signing in with email and password (and two-factor when applicable).
- **External-only** users cannot sign in until an administrator re-enables external providers or the user sets a local password; show **`External sign-in is unavailable. Contact an administrator or set a password when sign-in is available.`** on **Login** when detected (edge case for accounts that relied solely on external sign-in while providers were turned off).

### Trust identity provider MFA (external sign-in)

See REQ-AUTH-013 for deployment flag and password-sign-in rules. External sign-in behavior:

| IdP MFA asserted | **Two-factor enabled** | **Two-factor required** | Outcome after provider success                                  |
| ---------------- | ---------------------- | ----------------------- | --------------------------------------------------------------- |
| Yes, trust on    | false                  | true                    | Full session; no **strict two-factor setup**.                   |
| Yes, trust on    | true                   | any                     | Full session; skip **Two-factor verification**.                 |
| No or trust off  | true                   | any                     | **Pending sign-in challenge** → **Two-factor verification**.    |
| No or trust off  | false                  | true                    | **Enrollment bootstrap session** → **strict two-factor setup**. |
| No or trust off  | false                  | false                   | Full session.                                                   |

### Account model

| Concept                      | Storage                   | Meaning                                                                                         |
| ---------------------------- | ------------------------- | ----------------------------------------------------------------------------------------------- |
| **External login**           | Row per linked provider   | Associates **Provider key** and **Provider subject** (stable id from the issuer) with one user. |
| **External login linked at** | Date and time on each row | When the link was created.                                                                      |

- A user may have zero or more external logins.
- The pair (**Provider key**, **Provider subject**) is unique across all users.
- A user may link at most one account per **Provider key**.

### Profile email and provider email

| Term               | Meaning                                                                                                                                                                                                                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Profile email**  | The **current email** on the ChangeMe user account. Used for email/password sign-in, uniqueness (REQ-USR-003), display on **My account**, and **every** application and auth notification (REQ-AUTH-007, issue notifications).                                                       |
| **Provider email** | The email address returned by the identity provider on an OIDC callback. Used for **allowed email domains**, **Accept invitation** external onboarding (must match invited **Profile email**), and optional display on **My account**. **Never** used as a notification destination. |

**Profile email** changes only through: registration, **Invite user**, first-time OIDC account creation (initial **Profile email** = verified provider email), **Change email** (REQ-AUTH-015), or administrator **Edit user** (REQ-USR-003).

**Linking** a provider from **My account** does **not** change **Profile email**, even when **Provider email** differs.

Subsequent OIDC sign-ins do **not** update **Profile email** from provider claims.

### Guest external sign-in (Login and Register)

The system maintains **at most one user account per normalized Profile email** (REQ-USR-003).

Guest **Continue with {Display name}** on **Login** or **Register** does **not** link a provider to an existing account except **Accept invitation** external onboarding (REQ-AUTH-010). To add Google or Microsoft to an existing account, the user signs in to ChangeMe first, then uses **Link {Display name}** on **My account**.

Evaluate rows **in table order** (first match wins). **Allowed email domains** are evaluated on **Provider email** before other rows.

| Condition                                                                                                         | Outcome                                                                                                                                                                                                                                                     |
| ----------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Provider email not allowed by domain allowlist (when configured)                                                  | **`Sign-in with this account is not allowed.`** on **Login** or **Register**.                                                                                                                                                                               |
| **External login** exists for this (**Provider key**, **Provider subject**)                                       | Sign in the linked user (subject to deactivation, email verification, two-factor, and password expiration below). **Provider email** may differ from **Profile email**.                                                                                     |
| User is **awaiting invitation acceptance**; verified **Provider email** matches invited **Profile email**         | Complete invitation and link provider per REQ-AUTH-010 (only email-match link on guest OIDC).                                                                                                                                                               |
| User is **awaiting invitation acceptance**; **Provider email** does not match invited **Profile email**           | **`The external account email does not match the invited email address.`** on **Login** or **Accept invitation**.                                                                                                                                           |
| **Provider subject** not linked; a user account already exists with the same normalized **Provider email**        | **`An account already exists for this email. Sign in with your password, then link {Display name} from My account.`** on **Login** or **Register**. Does **not** open a linking screen.                                                                     |
| **Provider subject** not linked; no account with that **Provider email**; **public registration** is **enabled**  | Create **one** new user; set initial **Profile email** from verified **Provider email**; link provider; create as **external-only**; issue full session, enrollment bootstrap session, or pending two-factor challenge per **Trust identity provider MFA**. |
| **Provider subject** not linked; no account with that **Provider email**; **public registration** is **disabled** | **`No account exists for this email. Contact an administrator.`** on **Login**.                                                                                                                                                                             |
| Matched user’s account is **deactivated**                                                                         | **`This account has been deactivated. Contact an administrator.`** on **Login**.                                                                                                                                                                            |
| Email verification **enabled**; matched user’s **Profile email** **not verified**                                 | **`Verify your email before signing in.`** on **Login** (verified **Provider email** alone does not override an unverified local registration).                                                                                                             |

- New users created via external sign-in receive **First name** and **Last name** from provider claims when present; otherwise empty (user may complete profile on **Edit profile**).
- External sign-in never bypasses app TOTP when **Trust identity provider MFA** is **disabled**, or when the IdP does not assert MFA. When **Trust identity provider MFA** is **enabled** and MFA is asserted, external sign-in may bypass **Two-factor verification** and **strict two-factor setup** per REQ-AUTH-013.

### Linking external providers (signed-in only)

Linking adds an **External login** row to the **signed-in** user’s account. The user must already have an active ChangeMe session (email/password, an already-linked provider, or passkey per other REQs).

| Step | Behavior                                                                                                                                                                                                                                                     |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1    | User opens **My account** → **Link {Display name}** for an enabled provider not yet linked. Requires **External provider linking enabled** is **true**.                                                                                                      |
| 2    | **Sensitive account actions** step-up (REQ-AUTH-013) completes before OIDC starts.                                                                                                                                                                           |
| 3    | When **Provider email** differs from **Profile email**, show confirmation before OIDC: **`Link {Display name} to your account? Your profile email is {profile email}. The provider may use a different address. Notifications stay on your profile email.`** |
| 4    | OIDC callback in **link mode** attaches (**Provider key**, **Provider subject**) to the signed-in user. **Provider email match is not required.**                                                                                                            |
| 5    | **Allowed email domains** (when configured) are evaluated against **Provider email** on callback.                                                                                                                                                            |
| 6    | (**Provider key**, **Provider subject**) must not already belong to another user — **`This external account is already linked to another user.`**                                                                                                            |
| 7    | On success: message **`External sign-in method linked.`**; **External account linked** email to **Profile email** (REQ-AUTH-007); refresh **My account**.                                                                                                    |

- A user may link multiple providers (for example Google and Microsoft) from **My account**; each requires its own **Link** action and step-up. **Provider email** values may differ from each other and from **Profile email**.
- **Out of scope for this REQ:** guest **Link external account** screen; auto-linking a provider during **Login** or **Register** when **Provider email** matches an existing account (except **Accept invitation** per REQ-AUTH-010).

### Login screen — external sign-in

- When **External providers enabled** is **true** and at least one provider is configured, **Login** shows **Continue with {Display name}** for each provider (REQ-AUTH-001).
- Clicking a provider starts the OIDC authorization code flow with PKCE; the user is redirected to the provider and returns to **External sign-in callback** (guest route).
- On provider success the system reads the provider email and subject id.

### External sign-in callback (guest)

- Screen/route: **External sign-in callback**; processes the OIDC redirect, shows a loading state, then continues sign-in logic without manual input when possible.
- On unrecoverable error (provider error, invalid state, denied consent): redirect to **Login** with form-level error **`External sign-in failed. Try again or use email and password.`**

### My account — external sign-in methods

- Section **External sign-in methods** on **My account** when **External providers enabled** is **true**; collapsible; default **collapsed**.
- Persistent notice at the top of the section: **`Notifications are sent to your profile email ({profile email}), not to provider addresses.`**
- Lists each linked provider as a row: **Provider** (display name), **Provider email** (last known from the most recent OIDC callback for that link, or **`—`** when unknown), **Linked at**, **Unlink** action.
- When **Provider email** differs from **Profile email**, show inline hint on the row: **`May differ from your profile email.`**
- Lists enabled providers not yet linked with **Link {Display name}** when **External provider linking enabled** is **true**; action starts OIDC in **link mode** (returns to signed-in **My account** on success).
- **Link {Display name}** (when linking is enabled) and **Unlink** require **Sensitive account actions** step-up authentication (REQ-AUTH-013) before the action completes.
- **Unlink** requires confirmation: **`Remove {Display name} sign-in from your account?`**
- **Unlink** is blocked when it would leave the user with no sign-in method (**external-only** with only one linked provider) — show message **`Set a password before removing your only sign-in method.`** with link to **Set password** (see below).
- On successful link: message **`External sign-in method linked.`** and **External account linked** email (REQ-AUTH-007).
- On successful unlink: message **`External sign-in method removed.`** and **External account unlinked** email (REQ-AUTH-007).

### Set password (signed-in)

- Screen: **Set password**; linked from **My account** when the signed-in user is **external-only**.
- **Set password** requires **Sensitive account actions** step-up authentication (REQ-AUTH-013) before submit.
- Same fields and validation as **Change password** except **Current password** is omitted (REQ-AUTH-008).
- On success establishes a **local password** and sets **password last changed at**; does not revoke other sessions.
- Enables **Change password** (REQ-AUTH-005) and **Forgot password** self-service thereafter.

### Register screen

- When **External providers enabled** is **true** and **Public registration enabled** is **true**, **Register** shows the same **Continue with {Display name}** buttons as **Login** (shared behavior: creates account when email is new).

### Password and expiration interaction

- **External-only** users do not use **Forgot password** until they set a local password; external sign-in remains available when linked.
- Password expiration (REQ-AUTH-009) applies only when the user **has a local password**; **external-only** users are not redirected to **Required password change** for age reasons.
- **Change password** (REQ-AUTH-005) requires a **local password**; **external-only** users use **Set password** first.

### Administrator unlink (external sign-in)

- Administrators may **Unlink** a provider from **User details** (REQ-USR-004); requires **Users.Manage**.
- Confirmation: **`Remove {Display name} sign-in from this account?`**
- On success removes the **External login** row. **Unlink** is blocked when it would leave the user with no sign-in method (**external-only** with only one provider), same as self-service unlink.
- Sends **External account unlinked** email (REQ-AUTH-007).

### User details (admin read-only)

- When **External providers enabled** is **true**, **User details** shows section **External sign-in methods**: linked **Provider** display names, **Linked at**, and per-row **Unlink** for administrators (REQ-USR-004).

### Admin invite user and invitation

- **Invite user** (REQ-INV-001): administrators onboard by **Profile email**; the invitee must use that address for **Accept invitation** external onboarding (REQ-AUTH-010).

### Interaction with other auth flows

| Flow                                   | Behavior when external providers enabled                                                                                |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| **Login** (REQ-AUTH-001)               | Email/password unchanged; provider buttons when configured.                                                             |
| **Register** (REQ-AUTH-001)            | Provider buttons when public registration enabled.                                                                      |
| **Two-factor** (REQ-AUTH-013)          | Applies after provider success unless **Trust identity provider MFA** and IdP MFA assertion apply.                      |
| **Email verification** (REQ-AUTH-011)  | Provider-asserted verified email sets **Email verified** true on new account; existing unverified users remain blocked. |
| **Public registration** (REQ-AUTH-012) | When disabled, provider sign-in for unknown emails fails with contact administrator message.                            |
| **Password expiration** (REQ-AUTH-009) | Not evaluated for users **without a local password**.                                                                   |

### States and business rules

- **Link {Display name}** and **Unlink** require a signed-in session and **Sensitive account actions** step-up (REQ-AUTH-013), except administrator **Unlink** on **User details**.
- After linking, all **External login** rows for the user share one **Profile email**, roles, sessions, and two-factor settings.
- Admin **Edit user** **Profile email** change (REQ-USR-003): when **External providers enabled** is **true** and the user has at least one **External login**, **Edit user** shows notice **`External sign-in stays linked. Profile email is used for notifications; provider addresses may differ.`** Saving does **not** remove **External login** rows; administrators may **Unlink** providers from **User details** when appropriate.
- Self-service **Change email** (REQ-AUTH-015): **External login** rows are retained; notice on **Change email** when the user has linked providers: **`External sign-in methods stay linked. Notifications stay on your profile email.`**
- **Out of scope for this REQ:** SAML 2.0; social providers without email claim; automatic admin provisioning rules; SCIM; admin UI to configure providers at runtime; merging two user records with **different Profile emails** into one account; guest **Link external account** screen.

---

# REQ-AUTH-015: Self-Service Email Change

## Goal

The signed-in user must be able to change the email address on their account by confirming control of the new mailbox. The current email address remains active for sign-in until the change is confirmed.

## Features

### Deployment policy

- Deployment settings include **Self-service email change enabled** (`AuthOptions:EmailChange:Enabled`); default **true**.
- When **Self-service email change enabled** is **false**, **Change email** is hidden on **My account**, APIs to start a new email change are unavailable, and direct navigation to **Change email** redirects to **My account** with no new request allowed.
- When **Self-service email change enabled** is **false** and a **pending email change** already exists, **My account** still shows the **Pending email change** panel (resend, cancel) until the pending change is cleared or confirmed.
- Administrator **Edit user** email change (REQ-USR-003) is unaffected by this flag.

### Business terms

| Term                     | Meaning                                                                                                                                                                                                      |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Pending email change** | A self-service request to replace the current email with a new address. The new address is stored as pending until the user opens the confirmation link sent to that address or an administrator cancels it. |
| **Current email**        | The account **Profile email** used for sign-in, display, and notifications until a pending change is confirmed or cancelled (REQ-AUTH-014).                                                                  |
| **New email**            | The target address in a **pending email change**; not used for sign-in until confirmation succeeds.                                                                                                          |

### My account — entry and pending state

- Header action **Change email** on **My account** (REQ-USR-001) opens **Change email**.
- **Change email** is **not shown** when:
  - **Self-service email change enabled** is **false**;
  - the user is **awaiting invitation acceptance**;
  - a **pending email change** already exists.
- When a **pending email change** exists, **My account** shows a **Pending email change** panel above the profile summary with:
  - read-only line **`New email: {new email}`**;
  - read-only line **`Requested at: {date and time}`**;
  - message **`Sign in with your current email until you confirm the change from the new mailbox.`**
  - action **Resend confirmation email**;
  - action **Cancel email change** (requires **Sensitive account actions** step-up authentication — REQ-AUTH-013).
- **Resend confirmation email** sends a new **Confirm email change** message to the **new email** (REQ-AUTH-007), invalidates the previous unused confirmation link for this pending change, and shows message **`If the pending change is still valid, a new confirmation link has been sent to the new email address.`**
- **Cancel email change** opens confirmation dialog **`Cancel the pending email change to "{new email}"? Your current email will stay unchanged.`** On confirm: clears the pending change, sends **Email change cancelled** to the **current email** (REQ-AUTH-007), shows message **`Email change cancelled.`**, and refreshes **My account** in place.

### Change email screen

- Screen: **Change email**
- Linked from **My account** via header action **Change email**; **Back to my account** at the top.
- Only authenticated users with **Deactivated** false who are **not** **awaiting invitation acceptance** and have **no pending email change** can open this screen.

| Field             | Behavior                                                                                                  |
| ----------------- | --------------------------------------------------------------------------------------------------------- |
| **New email**     | Text field, **required**; valid email format; max **320** characters; must differ from **current email**. |
| **Current email** | Read-only; shows the signed-in user's **current email**.                                                  |

- When **External providers enabled** is **true** and the user has at least one **External login** row, show persistent notice: **`External sign-in methods stay linked. Notifications stay on your profile email ({current email}). Provider addresses may differ.`**

### Step-up before submit

- **Change email** button opens the **Sensitive account actions** step-up dialog (REQ-AUTH-013) before the request is submitted.
- Step-up collects **Current password** when the user **has a local password**, **TOTP** or **recovery code** when **two-factor enrolled**, and **step-up external sign-in** when the user is **external-only**.
- When step-up succeeds, the system validates **New email** and creates the **pending email change**.

### Validation (change email)

- **New email** identical to **current email** shows inline field error: **`New email must differ from your current email.`**
- Duplicate **new email** (already used by another account) shows form-level error: **`An account with this email already exists.`**
- Required and format errors are inline on **New email**.

### Submit success

- On successful submit after step-up:
  - create **pending email change** with **new email** and **requested at** (current date and time);
  - send **Confirm email change** to the **new email** (REQ-AUTH-007);
  - send **Email change requested** to the **current email** (REQ-AUTH-007);
  - show message **`Check the new email address for a confirmation link.`**;
  - navigate to **My account**.
- Submit does **not** change **current email**, **Email verified**, or active sessions.

### Confirmation link lifetime

- **Email change confirmation link lifetime (hours)** applies to self-service email change; default **72** (same default as **Email verification link lifetime (hours)** in REQ-AUTH-011).
- Each **Resend confirmation email** issues a new link and invalidates previous unused links for the same pending change.

### Confirm email change screen (guest)

- Screen: **Confirm email change**; available to guests (confirmation token in URL).
- **Back to sign in** at the top → **Login**.

| Outcome            | Behavior                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Valid link         | Applies the **new email** as **current email**; clears **pending email change**; sets **Email verified** true and **Email verified at** to the current date and time; revokes **all active sessions** for that user; sends **Email change completed** to the **previous current email** and to the **new email** (REQ-AUTH-007); redirects to **Login** with message **`Email changed. Sign in with your new email address.`** |
| Invalid or expired | Shows form-level error **`This confirmation link is invalid or has expired.`** with action **Back to sign in** → **Login**. When the user is signed in and a pending change still exists, also show **Resend confirmation email** (same behavior as on **My account**).                                                                                                                                                        |

- Opening a valid confirmation link while signed in as a **different** user shows error **`This confirmation link belongs to another account. Sign out and open the link again, or sign in as the account that requested the change.`** with actions **Sign out** → **Login**, and **Back to my account** when the signed-in user has access to **My account**.

### Form actions (change email)

- **Cancel** button navigates to **My account** without saving.
- **Change email** button: collect step-up (REQ-AUTH-013), then validate and submit as above.

### Interaction with other auth flows

| Flow                                  | Behavior                                                                                                                                                          |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Login** (REQ-AUTH-001)              | Sign-in uses **current email** until confirmation; **new email** cannot sign in while the change is pending.                                                      |
| **Forgot password** (REQ-AUTH-006)    | Available for **current email** while a change is pending.                                                                                                        |
| **Email verification** (REQ-AUTH-011) | Pending change does not alter **Email verified** on **current email** until confirmation; after confirmation, **Email verified** is true on the **new email**.    |
| **Passkeys** (REQ-PKY-001)            | Passkey credentials remain on the account; discoverable passkey sign-in is unchanged. Non-discoverable passkey sign-in uses **current email** until confirmation. |
| **External providers** (REQ-AUTH-014) | **External login** rows retained; **Profile email** is notification destination; provider emails may differ.                                                      |
| **Admin edit user** (REQ-USR-003)     | Administrator email change cancels any **pending email change**, applies immediately, and follows REQ-USR-003 admin email rules.                                  |
| **Admin user details** (REQ-USR-004)  | Shows **Pending email change** when present; administrator may **Cancel pending email change**.                                                                   |

### States and business rules

- Only one **pending email change** per user at a time.
- Users **awaiting invitation acceptance** cannot start self-service email change; they complete onboarding first (REQ-AUTH-010).
- Confirmation success revokes **every** active session; the user must sign in again with the **new Profile email** (or a linked external provider / passkey per existing rules).
- **Out of scope for this REQ:** changing email from **Edit profile**; administrator-initiated email change except cancellation of a pending change (REQ-USR-003, REQ-USR-004); changing email without confirmation of the new mailbox on self-service paths.
