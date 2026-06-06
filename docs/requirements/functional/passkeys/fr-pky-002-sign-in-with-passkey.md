---
id: FR-PKY-002
title: Sign-In with Passkey
domain: passkeys
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When passkeys are enabled, guests must be able to sign in using a registered passkey instead of (or before) email and password, subject to the same account gates as other primary authentication methods.

## Functional requirements

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
- On email verification **enabled** and mailbox **not verified**: **`Verify your email before signing in.`** (same as FR-AUTH-001).
- On **Passkeys authentication enabled** but user has no passkeys: **`No passkey is registered for this account.`** when email was supplied; discoverable flow uses the generic no-match message above.
- On **Allow passkey-only accounts** **false** and user would be passkey-only: **`Set a password or use external sign-in before using passkeys on this account.`**

### Post-passkey primary authentication

After successful passkey primary authentication, evaluate gates per `docs/requirements/_shared/reference/compliance-gates.md` (FR-PKY-006):

1. **Required password change** when the user **has a local password** and password expiration applies (FR-AUTH-009).
2. **Two-factor verification** when **Two-factor enabled** is **true** and **Passkey satisfies two-factor** did not apply on this assertion.
3. **Strict two-factor setup** when required and not satisfied (FR-AUTH-013).
4. **Strict passkey setup** when **Passkeys authentication required** is **true** and the user has zero passkeys (should not occur after successful passkey sign-in; included for completeness).
5. **Issues list** (full session) when no gate applies.

- Passkey sign-in creates a **new session** per FR-AUTH-001 when a full session is issued; records **signed in at**, **device / browser label**, **IP address**, and **Sign-in method** badge **`Passkey`** on session list (FR-AUTH-004, FR-USR-004).
- Failed passkey verification attempts per challenge: maximum **5**; after **5** failures, invalidate the challenge and show **`Too many passkey attempts. Try again.`**

### Interaction with other Login methods

| Other method                       | Coexistence on **Login**                                                                                        |
| ---------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Email and password (FR-AUTH-001)   | Always available when the user is eligible for password sign-in.                                                |
| External providers (FR-AUTH-014)   | Shown when enabled; independent of passkeys.                                                                    |
| **Forgot password?** (FR-AUTH-006) | Available; irrelevant for passkey-only users until they set a password.                                         |
| **Register** (FR-AUTH-012)         | Unchanged; new self-registered users do not receive passkeys until they enroll on **My account** after sign-in. |

### Accept invitation and Register

- **Accept invitation** (FR-AUTH-010) does **not** register a passkey automatically; after successful acceptance the user may add passkeys on **My account** when signed in.
- After successful **Register** when email verification is disabled, the success path may show optional prompt **`Add a passkey for faster sign-in`** with action **Add passkey now** → **Add passkey** flow (FR-PKY-003) before navigating to **Issues list**; declining navigates to **Issues list** without enrollment.
- When email verification is enabled, passkey enrollment prompt appears on first successful sign-in after **Verify email**, not on **Verify email** itself.

### States and business rules

- Passkey sign-in never bypasses **Deactivated**, **awaiting invitation acceptance**, or unverified-email rules.
- Passkey sign-in does not create an account for unknown credentials (no self-registration via passkey).
- **Out of scope:** passwordless username enumeration beyond existing email-first flows; NFC-specific UX; conditional UI autofill (may be added later without changing security rules).

---

## Acceptance scenarios

| ID            | Given                                                                                                                                                       | When                                                          | Then                                                                                                                                                        |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-PKY-002-01 | **Passkeys authentication enabled** is **true**; **Discoverable passkey sign-in on Login** is **true**; guest on **Login**                                  | User clicks **Sign in with a passkey** and completes ceremony | Discoverable authentication starts without entering **Email** first; on success new session created and post-passkey gates evaluated (FR-PKY-006)           |
| AC-PKY-002-02 | **Passkeys authentication enabled** is **true**; **Discoverable passkey sign-in on Login** is **false**; guest on **Login** with empty or invalid **Email** | User views **Sign in with a passkey**                         | Action disabled with hint `Enter your email to use a passkey.`                                                                                              |
| AC-PKY-002-03 | **Discoverable passkey sign-in on Login** is **false**; guest on **Login** with valid **Email** matching account with registered passkey                    | User completes passkey ceremony                               | Assertion validated against allow-listed credentials for that account only                                                                                  |
| AC-PKY-002-04 | Guest on **Login**; passkey ceremony returns unknown credential                                                                                             | Assertion fails                                               | Form-level error `No passkey matched. Sign in with email and password or use a different passkey.` (does not reveal email existence in discoverable flow)   |
| AC-PKY-002-05 | Guest on **Login**; matched account **Deactivated** is **true**                                                                                             | Passkey assertion succeeds                                    | Form-level error `This account has been deactivated. Contact an administrator.`                                                                             |
| AC-PKY-002-06 | Guest on **Login**; matched user **awaiting invitation acceptance**                                                                                         | Passkey assertion succeeds                                    | Form-level error `Complete your account setup using the invitation link sent to your email.`                                                                |
| AC-PKY-002-07 | Guest on **Login**; **Email verification enabled**; matched mailbox **not verified**                                                                        | Passkey assertion succeeds                                    | Form-level error `Verify your email before signing in.` (same as FR-AUTH-001)                                                                               |
| AC-PKY-002-08 | Guest on **Login**; passkey primary authentication succeeds; no compliance gate applies                                                                     | Session issued                                                | New session created with **signed in at**, **device / browser label**, **IP address**, and **Sign-in method** badge **`Passkey`** (FR-AUTH-004, FR-USR-004) |
| AC-PKY-002-09 | Guest on **Login**; user cancels passkey browser UI                                                                                                         | Ceremony aborted                                              | User remains on **Login** with no error                                                                                                                     |
| AC-PKY-002-10 | Guest on **Login**; five failed passkey verification attempts on same challenge                                                                             | Further attempts                                              | Challenge invalidated; message `Too many passkey attempts. Try again.`                                                                                      |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
