---
id: FR-PKY-005
title: Administrator Passkey Management
domain: passkeys
type: functional
status: active
depends_on: [FR-PKY-003, FR-PKY-006, FR-PKY-007, FR-USR-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Administrators must be able to inspect passkeys registered to a user and revoke them when necessary, with the same session-revocation safeguards as other security resets.

## Functional requirements

### User details — Passkeys section

- When **Passkeys authentication enabled** is **true**, **User details** (FR-USR-004) shows collapsible section **Passkeys**; default **collapsed**.
- Read-only table: **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**.
- Empty state: **`No passkeys registered.`**
- Administrators cannot add passkeys for another user (device-bound ceremony).

### Reset all passkeys

- Header action **Reset passkeys** on **User details**; requires **Users.Manage**.
- Shown when **Passkeys authentication enabled** is **true** and the user has at least one **Passkey credential**.
- Confirmation: **`Remove all passkeys for "{full name}"? They will need to register a passkey again if required by policy.`**
- On success: deletes all **Passkey credential** rows for the user; revokes **all active sessions**; success message **`Passkeys reset.`**
- Sends **Passkeys reset by admin** email (FR-PKY-007).
- When **Passkeys authentication required** is **true**, the user's next sign-in enters **strict passkey setup** (FR-PKY-006) after primary authentication succeeds.

### Per-credential remove (admin)

- Row action **Remove** on **User details**; requires **Users.Manage**; confirmation **`Remove passkey "{name}" from this account?`**
- Same **no last sign-in method** rules as self-service **Remove passkey** (FR-PKY-003).
- On success: message **`Passkey removed.`**; does **not** revoke all sessions unless that was the user's only passkey and policy triggers re-auth (session remains valid).

### Interaction with other admin actions

| Admin action (FR-USR-004) | Interaction with passkeys                                         |
| ------------------------- | ----------------------------------------------------------------- |
| **Reset two-factor**      | Does not remove passkeys.                                         |
| **Deactivate**            | Passkeys remain; user cannot sign in until activated.             |
| **Unlink external**       | Independent; user may still sign in with passkey and/or password. |
| **Send password reset**   | Independent.                                                      |

### States and business rules

- **Out of scope:** bulk export of passkey metadata for compliance archives beyond on-screen list; remote wipe of platform passkeys on user devices.

---

## Acceptance scenarios

| ID            | Given                                                                                                                                           | When                                                                                                                                                   | Then                                                                                                                                                                         |
| ------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-PKY-005-01 | Administrator with **Users.Manage** on **User details** (FR-USR-004); **Passkeys authentication enabled** is **true**; target user has passkeys | User views **Passkeys** section                                                                                                                        | Read-only table shows **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**; no **Add passkey** action for administrator |
| AC-PKY-005-02 | Administrator with **Users.Manage**; target user has at least one passkey                                                                       | User clicks **Reset passkeys** and confirms `Remove all passkeys for "{full name}"? They will need to register a passkey again if required by policy.` | All **Passkey credential** rows deleted; all active sessions revoked; message `Passkeys reset.`; **Passkeys reset by admin** email sent (FR-PKY-007)                         |
| AC-PKY-005-03 | **Passkeys authentication required** is **true**; administrator reset all passkeys for a user                                                   | Target user signs in next with primary authentication                                                                                                  | User enters **strict passkey setup** (FR-PKY-006) after primary auth succeeds                                                                                                |
| AC-PKY-005-04 | Administrator with **Users.Manage** on **User details**                                                                                         | User clicks row **Remove** for one passkey and confirms `Remove passkey "{name}" from this account?`                                                   | That credential removed; message `Passkey removed.`; **Passkey removed** email sent; sessions remain valid unless last-sign-in-method rules apply (FR-PKY-003)               |
| AC-PKY-005-05 | Administrator **without** **Users.Manage** on **User details**                                                                                  | User views passkey actions                                                                                                                             | **Reset passkeys** and row **Remove** are **not available**                                                                                                                  |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
