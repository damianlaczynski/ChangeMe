---
id: REQ-AUTH-007
title: Auth Email Notifications
domain: identity
status: active
depends_on: [REQ-AUTH-014, REQ-AUTH-015, REQ-INV-001, REQ-INV-003, REQ-PKY-003, REQ-PKY-005, REQ-USR-003, REQ-USR-004]
---
## Goal

The system must notify users by email about important account security events.

## Features

### Notification destination

- Every email in this REQ is sent to the account **Profile email** — the **current email** stored on the user account in ChangeMe.
- **Provider email** from an external identity provider is **never** used as a notification destination.
- Linking or signing in with Google or Microsoft does **not** change **Profile email** (REQ-AUTH-014).
- **Change email** (REQ-AUTH-015) sends **Confirm email change** to the pending **new email** only until confirmation succeeds; all other auth emails during a pending change still use **Profile email** (the current email).

### Notification types

| Event                         | When sent                                                                                                | Subject (example)                          |
| ----------------------------- | -------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| **Account invitation**        | Admin **Invite user**, **Resend invitation**, or **Send invitation** (REQ-INV-001, REQ-INV-003) succeeds | `You're invited to ChangeMe`               |
| **Password reset requested**  | Self-service forgot password or admin send reset                                                         | `Reset your ChangeMe password`             |
| **Password reset completed**  | Reset password or accept invite succeeds                                                                 | `Your password was changed`                |
| **Password changed**          | Signed-in **Change password** succeeds                                                                   | `Your password was changed`                |
| **Verify your email**         | Registration when verification enabled; **Resend verification email**                                    | `Verify your ChangeMe email`               |
| **Two-factor enabled**        | User enables two-factor on **My account**                                                                | `Two-factor authentication enabled`        |
| **Two-factor disabled**       | User disables two-factor on **My account**                                                               | `Two-factor authentication disabled`       |
| **Two-factor reset by admin** | Administrator **Reset two-factor** on **User details** (REQ-USR-004)                                     | `Two-factor authentication was reset`      |
| **External account linked**   | User links an external provider on **My account** (REQ-AUTH-014)                                         | `External sign-in method linked`           |
| **External account unlinked** | User or administrator removes an external provider link                                                  | `External sign-in method removed`          |
| **Recovery code used**        | A recovery code succeeds at sign-in or step-up authentication                                            | `A recovery code was used on your account` |
| **Passkey added**             | User completes **Add passkey** on **My account** (REQ-PKY-003)                                           | `Passkey added to your account`            |
| **Passkey removed**           | User or administrator removes a passkey credential (REQ-PKY-003, REQ-PKY-005)                            | `Passkey removed from your account`        |
| **Passkeys reset by admin**   | Administrator **Reset passkeys** on **User details** (REQ-PKY-005)                                       | `Passkeys reset on your account`           |
| **Email change requested**    | Signed-in user submits **Change email** (REQ-AUTH-015)                                                   | `Email change requested on your account`   |
| **Confirm email change**      | **Change email** succeeds; link sent to the **new** email address                                        | `Confirm your new ChangeMe email address`  |
| **Email change completed**    | User completes **Confirm email change** (REQ-AUTH-015)                                                   | `Your ChangeMe email address was changed`  |
| **Email change cancelled**    | User **Cancel email change** on **My account** (REQ-AUTH-015)                                            | `Email change cancelled on your account`   |
| **Email changed by admin**    | Administrator saves a different **Email** on **Edit user** (REQ-USR-003)                                 | `Your ChangeMe email address was changed`  |

- Each email contains a short summary and, where applicable, a button link to the frontend (invitation, reset, verify email, confirm email change, or sign-in).
- Email delivery uses the configured mail server (same as issue notifications).
- Failed email delivery does not roll back the triggering action; the UI still shows the success message for the action.

---
