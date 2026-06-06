---
id: FR-AUTH-007
title: Auth Email Notifications
domain: identity
type: functional
status: active
depends_on:
  [
    FR-AUTH-014,
    FR-AUTH-015,
    FR-INV-001,
    FR-INV-003,
    FR-PKY-003,
    FR-PKY-005,
    FR-USR-003,
    FR-USR-004,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The system must notify users by email about important account security events.

## Functional requirements

### Notification destination

- Every email in FR-AUTH-007 is sent to the account **Profile email** — the **current email** stored on the user account in ChangeMe.
- **Provider email** from an external identity provider is **never** used as a notification destination.
- Linking or signing in with Google or Microsoft does **not** change **Profile email** (FR-AUTH-014).
- **Change email** (FR-AUTH-015) sends **Confirm email change** to the pending **new email** only until confirmation succeeds; all other auth emails during a pending change still use **Profile email** (the current email).

### Notification types

| Event                         | When sent                                                                                              | Subject (example)                          |
| ----------------------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------ |
| **Account invitation**        | Admin **Invite user**, **Resend invitation**, or **Send invitation** (FR-INV-001, FR-INV-003) succeeds | `You're invited to ChangeMe`               |
| **Password reset requested**  | Self-service forgot password or admin send reset                                                       | `Reset your ChangeMe password`             |
| **Password reset completed**  | Reset password or accept invite succeeds                                                               | `Your password was changed`                |
| **Password changed**          | Signed-in **Change password** succeeds                                                                 | `Your password was changed`                |
| **Verify your email**         | Registration when verification enabled; **Resend verification email**                                  | `Verify your ChangeMe email`               |
| **Two-factor enabled**        | User enables two-factor on **My account**                                                              | `Two-factor authentication enabled`        |
| **Two-factor disabled**       | User disables two-factor on **My account**                                                             | `Two-factor authentication disabled`       |
| **Two-factor reset by admin** | Administrator **Reset two-factor** on **User details** (FR-USR-004)                                    | `Two-factor authentication was reset`      |
| **External account linked**   | User links an external provider on **My account** (FR-AUTH-014)                                        | `External sign-in method linked`           |
| **External account unlinked** | User or administrator removes an external provider link                                                | `External sign-in method removed`          |
| **Recovery code used**        | A recovery code succeeds at sign-in or step-up authentication                                          | `A recovery code was used on your account` |
| **Passkey added**             | User completes **Add passkey** on **My account** (FR-PKY-003)                                          | `Passkey added to your account`            |
| **Passkey removed**           | User or administrator removes a passkey credential (FR-PKY-003, FR-PKY-005)                            | `Passkey removed from your account`        |
| **Passkeys reset by admin**   | Administrator **Reset passkeys** on **User details** (FR-PKY-005)                                      | `Passkeys reset on your account`           |
| **Email change requested**    | Signed-in user submits **Change email** (FR-AUTH-015)                                                  | `Email change requested on your account`   |
| **Confirm email change**      | **Change email** succeeds; link sent to the **new** email address                                      | `Confirm your new ChangeMe email address`  |
| **Email change completed**    | User completes **Confirm email change** (FR-AUTH-015)                                                  | `Your ChangeMe email address was changed`  |
| **Email change cancelled**    | User **Cancel email change** on **My account** (FR-AUTH-015)                                           | `Email change cancelled on your account`   |
| **Email changed by admin**    | Administrator saves a different **Email** on **Edit user** (FR-USR-003)                                | `Your ChangeMe email address was changed`  |

- Each email contains a short summary and, where applicable, a button link to the frontend (invitation, reset, verify email, confirm email change, or sign-in).
- Email delivery uses the configured mail server (same as issue notifications).
- Failed email delivery does not roll back the triggering action; the UI still shows the success message for the action.

---

## Acceptance scenarios

| ID             | Given                                                                                                                      | When                                                            | Then                                                                                                      |
| -------------- | -------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| AC-AUTH-007-01 | User account with **Profile email** set; user has linked external provider with different **Provider email** (FR-AUTH-014) | Any auth notification specified in FR-AUTH-007 is triggered     | Email is sent to **Profile email** only; **Provider email** is never used as destination                  |
| AC-AUTH-007-02 | Signed-in user with **pending email change** (FR-AUTH-015); **Profile email** is current email                             | A notification other than **Confirm email change** is triggered | Email sent to **Profile email** (current email), not the pending **new email**                            |
| AC-AUTH-007-03 | Signed-in user submits **Change email** successfully (FR-AUTH-015)                                                         | Pending change is created                                       | **Email change requested** sent to **Profile email**; **Confirm email change** sent to **new email** only |
| AC-AUTH-007-04 | Signed-in **Change password** succeeds (FR-AUTH-005)                                                                       | Password change completes                                       | **Password changed** email sent to **Profile email** with subject `Your password was changed`             |
| AC-AUTH-007-05 | User enables two-factor on **My account** (FR-AUTH-013)                                                                    | **Confirm setup** succeeds                                      | **Two-factor enabled** email sent to **Profile email**                                                    |
| AC-AUTH-007-06 | User completes **Add passkey** on **My account** (FR-PKY-003)                                                              | Passkey registration succeeds                                   | **Passkey added** email sent to **Profile email** with subject `Passkey added to your account`            |
| AC-AUTH-007-07 | Mail server delivery fails for a triggered notification                                                                    | Triggering action completes in UI                               | Triggering action is **not** rolled back; UI still shows success message for the action                   |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
