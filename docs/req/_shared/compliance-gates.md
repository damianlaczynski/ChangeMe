# Combined account compliance gates

> Single ordered list enforced after **primary authentication** (password, external provider, or passkey). REQ files reference this document; do not duplicate the full order elsewhere.

When multiple policies apply, the system enforces **one gate at a time** in this order:

1. **Required password change** — **`passwordChangeRequired`** (REQ-AUTH-009); applies when the user **has a local password** and password is expired.
2. **Two-factor verification** — pending challenge (REQ-AUTH-013); skipped when **Passkey satisfies two-factor** applied on a passkey primary sign-in, or **Trust identity provider MFA** on external sign-in (REQ-AUTH-014).
3. **Strict two-factor setup** — **`twoFactorSetupRequired`** (REQ-AUTH-013).
4. **Strict passkey setup** — **`passkeySetupRequired`** when **Passkeys authentication required** is **true** and the user has zero passkeys (REQ-PKY-001).
5. **Full session** → **Issues list** (and other app screens).

## Precedence rules

- When both **`passwordChangeRequired`** and **`twoFactorSetupRequired`** are **true**, only **`passwordChangeRequired`** is active until the password is updated; **`twoFactorSetupRequired`** is evaluated immediately after a successful required password change on the same sign-in path.
- When both **`passwordChangeRequired`** and **`passkeySetupRequired`** are **true**, only **`passwordChangeRequired`** is active until the password is updated; **`passkeySetupRequired`** is evaluated immediately after successful required password change on the same sign-in path.
- When both **`twoFactorSetupRequired`** and **`passkeySetupRequired`** are **true**, **`twoFactorSetupRequired`** runs before **`passkeySetupRequired`**.
- **Enrollment bootstrap session** and **strict password change** middleware allowlists include only endpoints needed for the **active** gate plus sign-out and session refresh (REQ-AUTH-009, REQ-AUTH-013).
- Auth responses include at most one active compliance flag as primary; the client shows **one** sticky toast for the active gate (password before two-factor before passkey).

## Auth response flags

| Flag                         | Meaning                                                                         |
| ---------------------------- | ------------------------------------------------------------------------------- |
| **`passwordChangeRequired`** | User must complete **Required password change** before full application access. |
| **`twoFactorSetupRequired`** | User must complete **Set up two-factor authentication** (strict setup mode).    |
| **`passkeySetupRequired`**   | User must register at least one passkey (strict passkey setup mode).            |

## Strict setup modes

- **Strict two-factor setup** (REQ-AUTH-013): minimal chrome; only two-factor setup, **Logout**, and session refresh until enrollment completes.
- **Strict passkey setup** (REQ-PKY-006): minimal chrome; only **Add passkey**, **Logout**, and session refresh until at least one passkey exists.
- Users **awaiting invitation acceptance** are exempt from passkey requirement until they complete **Accept invitation** (REQ-AUTH-010).

## Client toasts (active gate)

| Gate             | Summary                                  | Detail                                                                             | Action           |
| ---------------- | ---------------------------------------- | ---------------------------------------------------------------------------------- | ---------------- |
| Password         | **`Password change required`**           | Per REQ-AUTH-009                                                                   | **`Change now`** |
| Two-factor setup | **`Two-factor authentication required`** | **`Set up two-factor authentication to continue saving your work to the server.`** | **`Set up now`** |
| Passkey setup    | **`Passkey required`**                   | **`Add a passkey to continue saving your work to the server.`**                    | **`Add now`**    |

## Sign-in evaluation before gates (primary auth)

Primary authentication (password on **Login**, external provider, or passkey) is evaluated first:

1. Unknown credentials / provider failure — fail without revealing account existence where applicable.
2. Account is **deactivated** — **`This account has been deactivated. Contact an administrator.`**
3. User is **awaiting invitation acceptance** (password sign-in) — **`Complete your account setup using the invitation link sent to your email.`**
4. Email verification is **enabled** and the mailbox is **not verified** (REQ-AUTH-011) — **`Verify your email before signing in.`**
5. Compliance gates (this document) — one at a time after primary auth succeeds.

## Exemptions

- Initial administrator (REQ-ROL-006) follows the same rules when applicable deployment settings are enabled.
- **Trust identity provider MFA** and **Passkey satisfies two-factor** are independent deployment settings; both may be **true**.
