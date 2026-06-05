---
id: REQ-AUTH-006
title: Forgot and Reset Password (Self-Service)
domain: identity
status: active
depends_on: [REQ-AUTH-007, REQ-AUTH-008, REQ-AUTH-009, REQ-AUTH-013]
---
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
