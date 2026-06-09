---
id: FR-AUTH-006
title: Forgot and Reset Password (Self-Service)
domain: identity
type: functional
status: active
depends_on: [FR-AUTH-007, FR-AUTH-008, FR-AUTH-009, FR-AUTH-013]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

A user who forgot their password must be able to request a reset link by email and set a new password without signing in.

## Functional requirements

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
| **New password**         | **Required**; must satisfy **Password policy** (FR-AUTH-008). |
| **Confirm new password** | **Required**; must match **New password**.                     |

- **Reset password** button: on success revoke all sessions, redirect to **Login** with message **`Password reset. Sign in with your new password.`**
- On success, **password last changed at** is set to the time of reset (FR-AUTH-009).
- Sends **Password reset completed** email (FR-AUTH-007).

### Interaction with two-factor authentication (FR-AUTH-013)

- **Reset password** does **not** disable **Two-factor enabled** or clear the TOTP secret.
- On successful reset, all **recovery codes** are invalidated; the user must **Regenerate recovery codes** on **My account** after the next successful sign-in when **Two-factor enabled** is **true**.
- The next sign-in after reset follows the normal flow: primary authentication, then **Two-factor verification** when **Two-factor enabled** is **true**, or **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false** (unless **Trust identity provider MFA** applies on external sign-in).

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
