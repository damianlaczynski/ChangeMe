---
id: FR-PKY-006
title: Combined Compliance Gates and Cross-Auth Interaction
domain: passkeys
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Passkey requirements must compose predictably with password expiration, two-factor authentication, external sign-in, invitations, and session compliance flags already defined in Auth requirements.

## Functional requirements

### Combined account compliance gates (full order)

See `docs/requirements/_shared/reference/compliance-gates.md`.

### Strict passkey setup mode

- Client mode mirrors **strict two-factor setup** (FR-AUTH-013): minimal chrome; only **Add passkey**, **Logout**, and session refresh until at least one passkey exists.
- Server rejects other application APIs with access denied until the user registers a passkey or policy no longer requires it.
- Users **awaiting invitation acceptance** are exempt until they complete **Accept invitation** (FR-AUTH-010).

### Primary authentication equivalence

| Method            | Role               | Creates session when gates pass?     |
| ----------------- | ------------------ | ------------------------------------ |
| Email + password  | Primary            | **Yes** (full or bootstrap)          |
| External OIDC     | Primary            | **Yes**                              |
| Passkey assertion | Primary            | **Yes**                              |
| TOTP / recovery   | Second factor only | **No** (completes pending challenge) |
| Passkey (step-up) | Step-up only       | **No**                               |

### External sign-in and passkeys

- External sign-in (FR-AUTH-014) does not register passkeys automatically.
- A user may have **external login**, **local password**, **passkeys**, and **two-factor** concurrently.
- **Trust identity provider MFA** (FR-AUTH-013) and **Passkey satisfies two-factor** are independent; both may be **true** in deployment settings.

### Password reset and invitation

- **Reset password** (FR-AUTH-006) does not remove passkeys.
- **Accept invitation** (FR-AUTH-010) does not remove passkeys on accounts that already had them (edge case: re-invited email).

### Deactivation and sessions

- Deactivating a user (FR-USR-005) does not delete passkeys; reactivation preserves credentials.
- **Sign out everywhere** does not remove passkeys.

### FR-AUTH-013 out of scope update

- WebAuthn / passkeys are **in scope** in this document (`docs/requirements/functional/passkeys/`), not FR-AUTH-013.
- FR-AUTH-013 **Out of scope** list must exclude passkeys (cross-reference FR-PKY-001 through FR-PKY-007).

### States and business rules

- Initial administrator (FR-ROL-006) follows the same passkey rules when **Passkeys authentication required** is **true**.
- **Out of scope:** per-tenant passkey policy; conditional passkey bypass for break-glass accounts.

---

## Acceptance scenarios

| ID            | Given                                                                                                                                                                       | When                                                                                          | Then                                                                                                                                                     |
| ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-PKY-006-01 | User in **strict passkey setup**; **Passkeys authentication required** is **true**; zero passkeys registered                                                                | User attempts to navigate to application screens other than passkey setup                     | Client shows minimal chrome; only **Add passkey**, **Logout**, and session refresh allowed until at least one passkey exists                             |
| AC-PKY-006-02 | User in **strict passkey setup**                                                                                                                                            | User calls application APIs other than sign-out, session refresh, and passkey setup endpoints | Server rejects requests with access denied                                                                                                               |
| AC-PKY-006-03 | User **awaiting invitation acceptance**; **Passkeys authentication required** is **true**                                                                                   | User has not completed **Accept invitation** (FR-AUTH-010)                                    | **Strict passkey setup** does **not** apply until invitation acceptance completes                                                                        |
| AC-PKY-006-04 | User completes passkey primary sign-in (FR-PKY-002); **Password expiration enabled**; password expired; **Two-factor enabled** true; **Passkey satisfies two-factor** false | Post-passkey gates evaluated                                                                  | Gates follow ordered list in `docs/requirements/_shared/reference/compliance-gates.md` (**Required password change** before **Two-factor verification**) |
| AC-PKY-006-05 | User with **external login**, **local password**, passkeys, and **two-factor** enrolled                                                                                     | User views account sign-in methods                                                            | All methods may coexist; external sign-in (FR-AUTH-014) does not register passkeys automatically                                                         |
| AC-PKY-006-06 | User completes **Reset password** (FR-AUTH-006)                                                                                                                             | Reset succeeds                                                                                | Passkeys on the account are **not** removed                                                                                                              |
| AC-PKY-006-07 | User **deactivated** (FR-USR-005) or uses **Sign out everywhere** (FR-AUTH-003)                                                                                             | Action completes                                                                              | Passkey credentials remain stored; reactivation preserves them                                                                                           |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
