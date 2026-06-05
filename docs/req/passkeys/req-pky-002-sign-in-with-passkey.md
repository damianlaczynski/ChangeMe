---
id: REQ-PKY-002
title: Sign-In with Passkey
domain: passkeys
status: active
depends_on:
  [
    REQ-AUTH-001,
    REQ-AUTH-004,
    REQ-AUTH-006,
    REQ-AUTH-009,
    REQ-AUTH-010,
    REQ-AUTH-012,
    REQ-AUTH-013,
    REQ-AUTH-014,
    REQ-PKY-003,
    REQ-PKY-006,
    REQ-USR-004,
  ]
---

## Goal

When passkeys are enabled, guests must be able to sign in using a registered passkey instead of (or before) email and password, subject to the same account gates as other primary authentication methods.

## Features

### Login screen — passkey entry

- When **Passkeys authentication enabled** is **true**, **Login** shows action **Sign in with a passkey** below the email and password form (above external provider buttons when present).
- When **Discoverable passkey sign-in on Login** is **true**, **Sign in with a passkey** starts discoverable authentication (no email required).
- When **Discoverable passkey sign-in on Login** is **false**, the user must enter **Email** first; **Sign in with a passkey** is enabled only when **Email** is a valid format; the ceremony uses allow-list credentials for that account only.
- **Sign in with a passkey** is disabled with inline hint **`Enter your email to use a passkey.`** when discoverable sign-in is **false** and **Email** is empty or invalid.

### Passkey sign-in ceremony (guest)

1. User activates **Sign in with a passkey**.
2. The system issues a short-lived **authentication challenge** bound to the browser session (or email when non-discoverable).
3. The browser displays the platform or security-key UI; the user completes verification on the authenticator.
4. The system validates the assertion, resolves the credential to exactly one user, and continues the sign-in decision tree (below).

- On unsupported browser: form-level error **`Passkeys are not supported in this browser. Use email and password or try another browser.`**
- On user cancel: remain on **Login** with no error (ceremony aborted).
- On invalid or expired challenge: form-level error **`Passkey sign-in timed out. Try again.`**
- On unknown credential: form-level error **`No passkey matched. Sign in with email and password or use a different passkey.`** (does not reveal whether an email exists when discoverable flow was used).
- On credential linked to **deactivated** account: **`This account has been deactivated. Contact an administrator.`**
- On **awaiting invitation acceptance**: **`Complete your account setup using the invitation link sent to your email.`**
- On email verification **enabled** and mailbox **not verified**: **`Verify your email before signing in.`** (same as REQ-AUTH-001).
- On **Passkeys authentication enabled** but user has no passkeys: **`No passkey is registered for this account.`** when email was supplied; discoverable flow uses the generic no-match message above.
- On **Allow passkey-only accounts** **false** and user would be passkey-only: **`Set a password or use external sign-in before using passkeys on this account.`**

### Post-passkey primary authentication

After successful passkey primary authentication, evaluate gates per `docs/req/_shared/compliance-gates.md` (REQ-PKY-006):

1. **Required password change** when the user **has a local password** and password expiration applies (REQ-AUTH-009).
2. **Two-factor verification** when **Two-factor enabled** is **true** and **Passkey satisfies two-factor** did not apply on this assertion.
3. **Strict two-factor setup** when required and not satisfied (REQ-AUTH-013).
4. **Strict passkey setup** when **Passkeys authentication required** is **true** and the user has zero passkeys (should not occur after successful passkey sign-in; included for completeness).
5. **Issues list** (full session) when no gate applies.

- Passkey sign-in creates a **new session** per REQ-AUTH-001 when a full session is issued; records **signed in at**, **device / browser label**, **IP address**, and **Sign-in method** badge **`Passkey`** on session list (REQ-AUTH-004, REQ-USR-004).
- Failed passkey verification attempts per challenge: maximum **5**; after **5** failures, invalidate the challenge and show **`Too many passkey attempts. Try again.`**

### Interaction with other Login methods

| Other method                        | Coexistence on **Login**                                                                                        |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Email and password (REQ-AUTH-001)   | Always available when the user is eligible for password sign-in.                                                |
| External providers (REQ-AUTH-014)   | Shown when enabled; independent of passkeys.                                                                    |
| **Forgot password?** (REQ-AUTH-006) | Available; irrelevant for passkey-only users until they set a password.                                         |
| **Register** (REQ-AUTH-012)         | Unchanged; new self-registered users do not receive passkeys until they enroll on **My account** after sign-in. |

### Accept invitation and Register

- **Accept invitation** (REQ-AUTH-010) does **not** register a passkey automatically; after successful acceptance the user may add passkeys on **My account** when signed in.
- After successful **Register** when email verification is disabled, the success path may show optional prompt **`Add a passkey for faster sign-in`** with action **Add passkey now** → **Add passkey** flow (REQ-PKY-003) before navigating to **Issues list**; declining navigates to **Issues list** without enrollment.
- When email verification is enabled, passkey enrollment prompt appears on first successful sign-in after **Verify email**, not on **Verify email** itself.

### States and business rules

- Passkey sign-in never bypasses **Deactivated**, **awaiting invitation acceptance**, or unverified-email rules.
- Passkey sign-in does not create an account for unknown credentials (no self-registration via passkey).
- **Out of scope for this REQ:** passwordless username enumeration beyond existing email-first flows; NFC-specific UX; conditional UI autofill (may be added later without changing security rules).

---
