---
id: FR-AUTH-001
title: Login with Sessions
domain: identity
type: functional
status: active
depends_on: []
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

> Account terms: `docs/requirements/_shared/domain/glossary.md`.

## Goal

The user must be able to sign in with email and password and begin an authenticated session tracked by the system.

## Functional requirements

### Data

| Field        | Constraints                                                        |
| ------------ | ------------------------------------------------------------------ |
| **Email**    | **Required**; valid email format; max **320** characters.          |
| **Password** | **Required**; **8–128** characters (same bounds as user creation). |

### Operations

- Sign in with email and password.
- On success, the system creates a **session** and provides the user's effective permissions (FR-ROL-001).
- Each created session records **signed in at**, **device / browser label**, and **IP address**.
- A guest who attempts a protected action is redirected to sign-in.
- There is no public registration or self-service account creation; new accounts are created by administrators (FR-USR-003).

### Validation

- Unknown email or wrong password: rejection message **`Invalid email or password.`** The message does not reveal whether the email exists.
- Account **deactivated** (FR-USR-005): rejection message **`This account has been deactivated. Contact an administrator.`**

### Business rules

- Each sign-in creates a **new session**; signing in from multiple devices creates multiple independent sessions.
- After successful sign-in, the user receives a full application session immediately. There are no intermediate gates (for example required password change, two-factor verification, or email verification).
- Sign-in is evaluated in this order: (1) unknown credentials → **`Invalid email or password.`**; (2) account **deactivated** → **`This account has been deactivated. Contact an administrator.`**; (3) success → create session.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
