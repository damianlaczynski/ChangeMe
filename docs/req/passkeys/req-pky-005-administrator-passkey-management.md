---
id: REQ-PKY-005
title: Administrator Passkey Management
domain: passkeys
status: active
depends_on: [REQ-PKY-003, REQ-PKY-006, REQ-PKY-007, REQ-USR-004]
---
## Goal

Administrators must be able to inspect passkeys registered to a user and revoke them when necessary, with the same session-revocation safeguards as other security resets.

## Features

### User details — Passkeys section

- When **Passkeys authentication enabled** is **true**, **User details** (REQ-USR-004) shows collapsible section **Passkeys**; default **collapsed**.
- Read-only table: **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**.
- Empty state: **`No passkeys registered.`**
- Administrators cannot add passkeys for another user (device-bound ceremony).

### Reset all passkeys

- Header action **Reset passkeys** on **User details**; requires **Users.Manage**.
- Shown when **Passkeys authentication enabled** is **true** and the user has at least one **Passkey credential**.
- Confirmation: **`Remove all passkeys for "{full name}"? They will need to register a passkey again if required by policy.`**
- On success: deletes all **Passkey credential** rows for the user; revokes **all active sessions**; success message **`Passkeys reset.`**
- Sends **Passkeys reset by admin** email (REQ-PKY-007).
- When **Passkeys authentication required** is **true**, the user's next sign-in enters **strict passkey setup** (REQ-PKY-006) after primary authentication succeeds.

### Per-credential remove (admin)

- Row action **Remove** on **User details**; requires **Users.Manage**; confirmation **`Remove passkey "{name}" from this account?`**
- Same **no last sign-in method** rules as self-service **Remove passkey** (REQ-PKY-003).
- On success: message **`Passkey removed.`**; does **not** revoke all sessions unless that was the user's only passkey and policy triggers re-auth (session remains valid).

### Interaction with other admin actions

| Admin action (REQ-USR-004) | Interaction with passkeys                                         |
| -------------------------- | ----------------------------------------------------------------- |
| **Reset two-factor**       | Does not remove passkeys.                                         |
| **Deactivate**             | Passkeys remain; user cannot sign in until activated.             |
| **Unlink external**        | Independent; user may still sign in with passkey and/or password. |
| **Send password reset**    | Independent.                                                      |

### States and business rules

- **Out of scope for this REQ:** bulk export of passkey metadata for compliance archives beyond on-screen list; remote wipe of platform passkeys on user devices.

---
