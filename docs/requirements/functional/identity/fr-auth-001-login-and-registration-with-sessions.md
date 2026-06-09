---
id: FR-AUTH-001
title: Login and Registration with Sessions
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

> Passkeys: `docs/requirements/functional/passkeys/`. Account terms: `docs/requirements/_shared/reference/glossary.md`.

## Goal

The user must be able to sign in with email and password and begin an authenticated session tracked by the system. When public registration is enabled (FR-AUTH-012), the user must also be able to register a new account.

## Functional requirements

### Login screen

| Field        | Behavior                                                                          |
| ------------ | --------------------------------------------------------------------------------- |
| **Email**    | Text field, **required**; valid email format; max **320** characters.             |
| **Password** | Password field, **required**; **8–128** characters (same bounds as registration). |

- Successful sign-in opens the **Issues list** screen with the user authenticated, unless a post-authentication gate applies: **Required password change** (FR-AUTH-009), **Two-factor verification** (FR-AUTH-013), or **strict two-factor setup** (FR-AUTH-013).
- The system creates a **session** (full or enrollment bootstrap per FR-AUTH-013) only after all gates for the sign-in path succeed; **pending sign-in challenge** (two-factor verification before any JWT) is not a session.
- Each created session records **signed in at**, **device / browser label**, and **IP address**.
- The system provides the user’s effective permissions defined in FR-ROL-001.
- Failed sign-in (unknown email or wrong password) shows form-level error: **`Invalid email or password.`** The message does not reveal whether the email exists.
- Sign-in attempt when the account is **deactivated** shows form-level error: **`This account has been deactivated. Contact an administrator.`**
- Sign-in attempt when the user is **awaiting invitation acceptance** shows form-level error: **`Complete your account setup using the invitation link sent to your email.`** (External sign-in with a matching verified email completes the invitation instead; see FR-AUTH-010.)
- Sign-in attempt when email verification is **enabled** and the mailbox is **not yet verified** shows form-level error: **`Verify your email before signing in.`** with link **Resend verification email** → **Verify email** (FR-AUTH-011).
- Successful sign-in when the password has **expired** (FR-AUTH-009) opens **Required password change** instead of **Issues list**.
- When two-factor authentication is enabled for the account (FR-AUTH-013), successful password validation opens **Two-factor verification** (pending challenge) instead of issuing a session immediately, unless **strict two-factor setup** or **Required password change** applies per **Combined account compliance gates** (`docs/requirements/_shared/reference/compliance-gates.md`).
- When email verification is **enabled**, successful sign-in is allowed only if the mailbox is **verified** (evaluated before password expiration).
- Footer link on **Login**: **Forgot password?** → **Forgot password** (FR-AUTH-006).
- Footer link on **Login**: **Create an account** → **Register** — shown only when **public registration** is **enabled** (FR-AUTH-012).
- When **external identity providers** are **enabled** (FR-AUTH-014), **Login** shows a **Continue with {Provider}** button for each configured provider below the email and password form, separated by an **or** divider.
- When **passkeys authentication enabled** is **true** (FR-PKY-002), **Login** shows **Sign in with a passkey** per passkeys requirements; passkey sign-in is subject to the same account gates as password sign-in and to **Combined account compliance gates** (`docs/requirements/_shared/reference/compliance-gates.md`) in FR-PKY-006 (which extends FR-AUTH-013 ordering).

### Register screen

- Available only when **Public registration enabled** is **true** (FR-AUTH-012). When disabled, guests cannot open **Register** (see FR-AUTH-012).

| Field                | Behavior                                                                       |
| -------------------- | ------------------------------------------------------------------------------ |
| **First name**       | Text field, **required**; max **100** characters.                              |
| **Last name**        | Text field, **required**; max **100** characters.                              |
| **Email**            | Text field, **required**; valid email; max **320** characters; must be unique. |
| **Password**         | Password field, **required**; **8–128** characters.                            |
| **Confirm password** | **Required**; must match **Password**.                                         |

- When **Email verification enabled** is **false** (FR-AUTH-011), successful registration creates a user with **Deactivated** false, **Email verified** true, assigns the default **User** role (FR-ROL-006), sets **password last changed at**, creates a session, and opens **Issues list**.
- When **Email verification enabled** is **true**, successful registration creates a user with **Deactivated** false, **Email verified** false, assigns the **User** role, sets **password last changed at**, sends **Verify your email**, does **not** create a session, and opens **Verify email** with message **`Account created. Check your email to verify your address before signing in.`**
- Duplicate email shows form-level error: **`An account with this email already exists.`**, **except** when the email belongs to an existing account that may be completed via public registration (**Invitation canceled** path — FR-INV-005): no local password, not **awaiting invitation acceptance**, not **deactivated**. In that case registration **sets the password** and profile on the existing row (does not create a second user).

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for inline validation presentation unless stated below.

**Login**

- **Email**: required; valid email format; max **320** characters.
- **Password**: required; **8–128** characters. Rejects out-of-range input before authentication (limits oversized requests; minimum matches passwords set at registration and user creation).

**Register**

- **First name** and **Last name**: required; max **100** characters.
- **Email**: required; valid email format; max **320** characters.
- **Password**: required; **8–128** characters.
- **Confirm password**: required; must match **Password**.

### Form actions

- **Sign in** button: on success navigate to **Issues list**; on failure remain on **Login**.
- **Create account** button: on success navigate to **Issues list** when email verification is disabled; navigate to **Verify email** when email verification is enabled; on failure remain on **Register**.
- Footer link on **Register**: **Sign in** → **Login**.
- While submit is in progress, the submit button shows a loading state; the form remains visible.

### States and business rules

- Registration is available only when **Public registration enabled** is **true** (FR-AUTH-012).
- Registration assigns only the **User** role; it does not grant administrative permissions (FR-ROL-006).
- Each sign-in creates a **new session**; signing in from multiple devices creates multiple independent sessions.
- A guest who opens a protected screen is redirected to **Login**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
