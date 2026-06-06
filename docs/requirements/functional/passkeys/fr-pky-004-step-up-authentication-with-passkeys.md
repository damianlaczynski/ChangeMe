---
id: FR-PKY-004
title: Step-Up Authentication with Passkeys
domain: passkeys
type: functional
status: active
depends_on: [FR-AUTH-005, FR-AUTH-010, FR-AUTH-013, FR-AUTH-014, FR-PKY-003]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Sensitive account actions must accept a recent passkey assertion as step-up proof, consistent with password and two-factor step-up rules.

## Functional requirements

### Passkey step-up

- A **passkey step-up** is a successful authentication ceremony (allow-listed credentials for the signed-in user only) completed within **15 minutes** before the sensitive action.
- The server stores **passkey step-up completed at** per user (single timestamp; any registered passkey satisfies step-up).

### Sensitive account actions (updated)

The following actions require **step-up authentication** per FR-AUTH-013, extended with passkey step-up:

| Action                                | Where               | Step-up options (all that apply must succeed)                                                                                                                                                           |
| ------------------------------------- | ------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Disable two-factor authentication** | **My account**      | Password (if set) + TOTP/recovery **or** recent passkey step-up + TOTP/recovery when **Two-factor enabled**                                                                                             |
| **Regenerate recovery codes**         | **My account**      | Same as above                                                                                                                                                                                           |
| **Link {Display name}**               | **My account**      | FR-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Unlink** external provider          | **My account**      | FR-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Set password**                      | **My account**      | FR-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Add passkey**                       | **My account**      | FR-PKY-003                                                                                                                                                                                              |
| **Rename passkey**                    | **My account**      | FR-PKY-003                                                                                                                                                                                              |
| **Remove passkey**                    | **My account**      | FR-PKY-003                                                                                                                                                                                              |
| **Change password**                   | **Change password** | FR-AUTH-005 — **Current password** **or** passkey step-up when user has passkeys and no password is set is **not** allowed; password change always requires **Current password** when a password exists |

Passkey step-up rules:

| User state                                            | Passkey step-up alone sufficient?                                                                                                                                                                     |
| ----------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Has passkeys; **Two-factor enabled** **false**        | **Yes** — replaces password requirement for external-only-style step-up.                                                                                                                              |
| Has passkeys; **Two-factor enabled** **true**         | **No** — passkey step-up **plus** valid **TOTP** or **recovery code** required (same as password + TOTP).                                                                                             |
| Has passkeys; action is **Add/Rename/Remove passkey** | For **Add** first passkey during voluntary enrollment: password if set, else passkey step-up not applicable (use password or external step-up per FR-AUTH-013). Subsequent **Add** uses full step-up. |

- Step-up UI on **My account** and security dialogs offers **Verify with passkey** when the user has at least one passkey and **Passkeys authentication enabled** is **true**.
- **Verify with passkey** runs a WebAuthn authentication ceremony; on success sets **passkey step-up completed at** and returns the user to the pending action.
- Failed step-up: **`Passkey verification failed. Try again.`**; counts toward the same **5**-attempt limit per step-up session as FR-AUTH-013.

### States and business rules

- Passkey step-up does not issue a new application session; it only unlocks the pending action.
- **Out of scope:** step-up for **Accept invitation** external onboarding (invitation email match per FR-AUTH-010 / FR-AUTH-014).

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
