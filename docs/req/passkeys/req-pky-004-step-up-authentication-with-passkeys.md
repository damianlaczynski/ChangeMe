---
id: REQ-PKY-004
title: Step-Up Authentication with Passkeys
domain: passkeys
status: active
depends_on: [REQ-AUTH-005, REQ-AUTH-010, REQ-AUTH-013, REQ-AUTH-014, REQ-PKY-003]
---
## Goal

Sensitive account actions must accept a recent passkey assertion as step-up proof, consistent with password and two-factor step-up rules.

## Features

### Passkey step-up

- A **passkey step-up** is a successful authentication ceremony (allow-listed credentials for the signed-in user only) completed within **15 minutes** before the sensitive action.
- The server stores **passkey step-up completed at** per user (single timestamp; any registered passkey satisfies step-up).

### Sensitive account actions (updated)

The following actions require **step-up authentication** per REQ-AUTH-013, extended with passkey step-up:

| Action                                | Where               | Step-up options (all that apply must succeed)                                                                                                                                                            |
| ------------------------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Disable two-factor authentication** | **My account**      | Password (if set) + TOTP/recovery **or** recent passkey step-up + TOTP/recovery when **Two-factor enabled**                                                                                              |
| **Regenerate recovery codes**         | **My account**      | Same as above                                                                                                                                                                                            |
| **Link {Display name}**               | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Unlink** external provider          | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Set password**                      | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Add passkey**                       | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Rename passkey**                    | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Remove passkey**                    | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Change password**                   | **Change password** | REQ-AUTH-005 — **Current password** **or** passkey step-up when user has passkeys and no password is set is **not** allowed; password change always requires **Current password** when a password exists |

Passkey step-up rules:

| User state                                            | Passkey step-up alone sufficient?                                                                                                                                                                      |
| ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Has passkeys; **Two-factor enabled** **false**        | **Yes** — replaces password requirement for external-only-style step-up.                                                                                                                               |
| Has passkeys; **Two-factor enabled** **true**         | **No** — passkey step-up **plus** valid **TOTP** or **recovery code** required (same as password + TOTP).                                                                                              |
| Has passkeys; action is **Add/Rename/Remove passkey** | For **Add** first passkey during voluntary enrollment: password if set, else passkey step-up not applicable (use password or external step-up per REQ-AUTH-013). Subsequent **Add** uses full step-up. |

- Step-up UI on **My account** and security dialogs offers **Verify with passkey** when the user has at least one passkey and **Passkeys authentication enabled** is **true**.
- **Verify with passkey** runs a WebAuthn authentication ceremony; on success sets **passkey step-up completed at** and returns the user to the pending action.
- Failed step-up: **`Passkey verification failed. Try again.`**; counts toward the same **5**-attempt limit per step-up session as REQ-AUTH-013.

### States and business rules

- Passkey step-up does not issue a new application session; it only unlocks the pending action.
- **Out of scope for this REQ:** step-up for **Accept invitation** external onboarding (invitation email match per REQ-AUTH-010 / REQ-AUTH-014).

---
