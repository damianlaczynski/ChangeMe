---
id: REQ-PKY-006
title: Combined Compliance Gates and Cross-Auth Interaction
domain: passkeys
status: active
depends_on:
  [
    REQ-AUTH-006,
    REQ-AUTH-010,
    REQ-AUTH-013,
    REQ-AUTH-014,
    REQ-PKY-001,
    REQ-PKY-007,
    REQ-ROL-006,
    REQ-USR-005,
  ]
---

## Goal

Passkey requirements must compose predictably with password expiration, two-factor authentication, external sign-in, invitations, and session compliance flags already defined in Auth requirements.

## Features

### Combined account compliance gates (full order)

See `docs/req/_shared/compliance-gates.md`.

### Strict passkey setup mode

- Client mode mirrors **strict two-factor setup** (REQ-AUTH-013): minimal chrome; only **Add passkey**, **Logout**, and session refresh until at least one passkey exists.
- Server rejects other application APIs with access denied until the user registers a passkey or policy no longer requires it.
- Users **awaiting invitation acceptance** are exempt until they complete **Accept invitation** (REQ-AUTH-010).

### Primary authentication equivalence

| Method            | Role               | Creates session when gates pass?     |
| ----------------- | ------------------ | ------------------------------------ |
| Email + password  | Primary            | **Yes** (full or bootstrap)          |
| External OIDC     | Primary            | **Yes**                              |
| Passkey assertion | Primary            | **Yes**                              |
| TOTP / recovery   | Second factor only | **No** (completes pending challenge) |
| Passkey (step-up) | Step-up only       | **No**                               |

### External sign-in and passkeys

- External sign-in (REQ-AUTH-014) does not register passkeys automatically.
- A user may have **external login**, **local password**, **passkeys**, and **two-factor** concurrently.
- **Trust identity provider MFA** (REQ-AUTH-013) and **Passkey satisfies two-factor** are independent; both may be **true** in deployment settings.

### Password reset and invitation

- **Reset password** (REQ-AUTH-006) does not remove passkeys.
- **Accept invitation** (REQ-AUTH-010) does not remove passkeys on accounts that already had them (edge case: re-invited email).

### Deactivation and sessions

- Deactivating a user (REQ-USR-005) does not delete passkeys; reactivation preserves credentials.
- **Sign out everywhere** does not remove passkeys.

### REQ-AUTH-013 out of scope update

- WebAuthn / passkeys are **in scope** in this document (`docs/req/passkeys/`), not REQ-AUTH-013.
- REQ-AUTH-013 **Out of scope** list must exclude passkeys (cross-reference REQ-PKY-001 through REQ-PKY-007).

### States and business rules

- Initial administrator (REQ-ROL-006) follows the same passkey rules when **Passkeys authentication required** is **true**.
- **Out of scope for this REQ:** per-tenant passkey policy; conditional passkey bypass for break-glass accounts.

---
