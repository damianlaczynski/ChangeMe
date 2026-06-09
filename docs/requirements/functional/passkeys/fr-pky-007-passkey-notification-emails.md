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

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
