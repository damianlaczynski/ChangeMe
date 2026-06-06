---
id: FR-PKY-007
title: Passkey Notification Emails
domain: passkeys
type: functional
status: active
depends_on: [FR-AUTH-007]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Users must receive email notification when passkeys are added, removed, or administratively reset, consistent with other auth notification emails.

## Functional requirements

### Email types

| Event                       | Trigger                              | Subject line (approximate)          |
| --------------------------- | ------------------------------------ | ----------------------------------- |
| **Passkey added**           | User completes **Add passkey**       | `Passkey added to your account`     |
| **Passkey removed**         | User **Remove passkey** self-service | `Passkey removed from your account` |
| **Passkeys reset by admin** | Administrator **Reset passkeys**     | `Passkeys reset on your account`    |

- Each email includes **account email**, **event time** (UTC), **passkey name** when applicable, and guidance **`If you did not perform this action, contact your administrator immediately.`**
- Emails require working **Email** configuration (same as FR-AUTH-007).
- Admin per-credential **Remove** sends **Passkey removed** with the credential name.

### Auth notification list (FR-AUTH-007)

- Extend FR-AUTH-007 notification catalog with the three rows above when passkeys are implemented.

### States and business rules

- **Out of scope:** email on every passkey sign-in (high volume); push notifications.

## Acceptance scenarios

| ID            | Given                                                                                    | When                          | Then                                                                                                                                                                                                                                                             |
| ------------- | ---------------------------------------------------------------------------------------- | ----------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-PKY-007-01 | Signed-in user completes **Add passkey** on **My account** (FR-PKY-003)                  | Passkey registration succeeds | **Passkey added** email sent to **Profile email** (FR-AUTH-007) with subject `Passkey added to your account`; includes account email, event time (UTC), passkey name, and guidance `If you did not perform this action, contact your administrator immediately.` |
| AC-PKY-007-02 | Signed-in user completes self-service **Remove passkey** (FR-PKY-003)                    | Removal succeeds              | **Passkey removed** email sent with subject `Passkey removed from your account` and credential name                                                                                                                                                              |
| AC-PKY-007-03 | Administrator completes **Reset passkeys** on **User details** (FR-PKY-005)              | Reset succeeds                | **Passkeys reset by admin** email sent with subject `Passkeys reset on your account`                                                                                                                                                                             |
| AC-PKY-007-04 | Administrator removes single passkey via row **Remove** on **User details** (FR-PKY-005) | Removal succeeds              | **Passkey removed** email sent with the credential name                                                                                                                                                                                                          |
| AC-PKY-007-05 | Any passkey notification email triggered                                                 | Email is composed             | Delivery uses same mail configuration as FR-AUTH-007 auth notifications                                                                                                                                                                                          |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
