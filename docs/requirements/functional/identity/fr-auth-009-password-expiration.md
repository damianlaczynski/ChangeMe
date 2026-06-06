---
id: FR-AUTH-009
title: Password Expiration
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When password expiration is enabled in deployment settings, users whose password is older than the configured maximum age must set a new password immediately after sign-in, before using the rest of the application.

## Functional requirements

### Password expiration policy

- Deployment settings include **Password expiration enabled**; default **false**.
- When **Password expiration enabled** is **true**, **Maximum password age (days)** applies; default **90**.
- When **Password expiration enabled** is **false**, expiration is not evaluated and sign-in never redirects to **Required password change** for age reasons.

### Password age

- The system records **password last changed at** for each user who **has a local password**.
- **Password last changed at** is set when the user first receives a password and whenever the password is changed successfully (registration, accept invitation, reset password, change password, required password change).
- Users **awaiting invitation acceptance** are not evaluated for expiration.
- **Password expires at** is not stored. For administrators viewing **User details** (FR-USR-004), when **Password expiration enabled** is **true**, the UI shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)** (calendar date and time in the deployment time zone). The field is omitted when expiration is disabled.

### Sign-in and expiration

- After successful authentication (FR-AUTH-001), if **Password expiration enabled** is **true** and the password age exceeds **Maximum password age (days)**, the user opens **Required password change** instead of **Issues list**.
- When **Combined account compliance gates** (`docs/requirements/_shared/reference/compliance-gates.md`) (FR-AUTH-013) also apply, **Required password change** takes precedence over two-factor verification and **strict two-factor setup** until the password is updated.
- If the password is within age, sign-in opens **Issues list** as usual.
- The initial administrator account created at first startup is subject to the same expiration rules as other users; first sign-in does **not** require a password change solely because the account is new.

### Password expiry warnings (signed-in)

- When password expiration is **enabled** and the signed-in user **has a local password**, the client receives **Password expires at** (UTC) on each successful sign-in and session refresh (same derivation as **Password expires at** on **User details** in FR-USR-004).
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

| Field                    | Behavior                                                                                        |
| ------------------------ | ----------------------------------------------------------------------------------------------- |
| **New password**         | **Required**; **Password policy** (FR-AUTH-008); must differ from the password used to sign in. |
| **Confirm new password** | **Required**; must match **New password**.                                                      |

- **Change password** button: on success updates the password, sets **password last changed at** to the current time, revokes all other active sessions for the user, keeps the current session, clears expiry warnings and the sticky expiry toast, and shows message **`Password updated.`**
- After success following **Sign-in and expiration**, the application opens **Issues list**.
- After success from the dialog during an active session, the application **closes the dialog** and **keeps the current route**; the user may retry server actions without signing in again.
- Sends **Password changed** email (FR-AUTH-007).

#### Required password change screen

- Full-page route: **Required password change**
- After sign-in with an expired password, the client enters **strict password change** mode: the user cannot navigate to other application screens until the password is changed (except **Logout**); the application shows only the minimal chrome (no sidebar or main navigation).
- **Strict password change** applies only after sign-in (or register) with an expired password, not when expiration is detected during an active session.

### States and business rules

- Voluntary **Change password** (FR-AUTH-005) and completed **Reset password** (FR-AUTH-006) update **password last changed at**; the user signs in again before expiration is evaluated.
- **Send password reset** (FR-USR-006) does not change age until the user completes **Reset password**.
- **Out of scope:** email warnings before expiry, grace logins after expiry, per-user exemption from expiration, configurable warning day thresholds (fixed at 14, 7, and 1).

---

## Acceptance scenarios

| ID             | Given                                                                                                                                                         | When                                                                                                             | Then                                                                                                                                                                                                                                                            |
| -------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-AUTH-009-01 | **Password expiration enabled** is **true**; user signs in successfully (FR-AUTH-001); password age exceeds **Maximum password age (days)**                   | Sign-in completes                                                                                                | **Required password change** (screen) opens instead of **Issues list**                                                                                                                                                                                          |
| AC-AUTH-009-02 | **Password expiration enabled** is **true**; user signs in successfully; password is within age                                                               | Sign-in completes                                                                                                | **Issues list** opens as usual                                                                                                                                                                                                                                  |
| AC-AUTH-009-03 | **Password expiration enabled** is **false**                                                                                                                  | User signs in with any password age                                                                              | Sign-in never redirects to **Required password change** for age reasons                                                                                                                                                                                         |
| AC-AUTH-009-04 | Signed-in user; **Password expiration enabled** is **true**; password age exceeds maximum while user is on an application screen                              | Session refresh or next blocked server request establishes **password change required**                          | User stays on current screen and route; sticky expiry toast with summary `Password expired` and detail `Your password has expired. Set a new password to save your work to the server.`; action **Change password** opens **Required password change** (dialog) |
| AC-AUTH-009-05 | Signed-in user in expired-password state; sticky expiry toast visible                                                                                         | User triggers a server request for application data (not sign-out, session refresh, or required password change) | Request rejected; local UI state on current screen remains available                                                                                                                                                                                            |
| AC-AUTH-009-06 | User on **Required password change** (screen) after sign-in with expired password                                                                             | User attempts to navigate to another application screen                                                          | Navigation blocked (**strict password change** mode); only **Logout** remains available; minimal chrome shown (no sidebar or main navigation)                                                                                                                   |
| AC-AUTH-009-07 | User on **Required password change** (screen) after sign-in with expired password; valid **New password** and **Confirm new password**                        | User clicks **Change password**                                                                                  | Password updated; **password last changed at** set; other sessions revoked; current session kept; message `Password updated.`; application opens **Issues list**                                                                                                |
| AC-AUTH-009-08 | Signed-in user; password expired during active session; **Required password change** (dialog) open with valid fields                                          | User clicks **Change password** in dialog                                                                        | Dialog closes; current route kept; user may retry server actions without signing in again                                                                                                                                                                       |
| AC-AUTH-009-09 | Signed-in user with **local password**; **Password expiration enabled** is **true**; password within age; **14** calendar days before **Password expires at** | Warning threshold not yet shown for this browser profile                                                         | Expiry warning toast with summary `Password expiring soon` and remaining day count; action opens **Required password change** (dialog)                                                                                                                          |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
