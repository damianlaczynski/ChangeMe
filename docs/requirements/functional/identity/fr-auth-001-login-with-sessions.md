---
id: FR-AUTH-001
title: Login with Sessions
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

> Account terms: `docs/requirements/_shared/reference/glossary.md`.

## Goal

The user must be able to sign in with email and password and begin an authenticated session tracked by the system.

## Functional requirements

### Login screen

| Field        | Behavior                                                                          |
| ------------ | --------------------------------------------------------------------------------- |
| **Email**    | Text field, **required**; valid email format; max **320** characters.             |
| **Password** | Password field, **required**; **8–128** characters (same bounds as user creation). |

- Successful sign-in opens the **Issues list** screen with the user authenticated.
- The system creates a **session** after password validation succeeds.
- Each created session records **signed in at**, **device / browser label**, and **IP address**.
- The system provides the user's effective permissions defined in FR-ROL-001.
- Failed sign-in (unknown email or wrong password) shows form-level error: **`Invalid email or password.`** The message does not reveal whether the email exists.
- Sign-in attempt when the account is **deactivated** shows form-level error: **`This account has been deactivated. Contact an administrator.`**
- There is no public registration or self-service account creation on **Login**; new accounts are created by administrators (FR-USR-003).

### Validation and form behavior

- Inherits `FR-UI-001` (**Create and edit form screens**) for inline validation presentation unless stated below.

**Login**

- **Email**: required; valid email format; max **320** characters.
- **Password**: required; **8–128** characters. Rejects out-of-range input before authentication (limits oversized requests; minimum matches passwords set at user creation).

### Form actions

- **Sign in** button: on success navigate to **Issues list**; on failure remain on **Login**.
- While submit is in progress, the submit button shows a loading state; the form remains visible.

### States and business rules

- Each sign-in creates a **new session**; signing in from multiple devices creates multiple independent sessions.
- A guest who opens a protected screen is redirected to **Login**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
