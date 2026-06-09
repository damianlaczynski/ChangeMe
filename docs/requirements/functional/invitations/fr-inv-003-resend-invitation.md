---
id: FR-INV-003
title: Resend Invitation
domain: invitations
type: functional
status: active
depends_on: [FR-AUTH-007, FR-INV-002]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

An administrator must be able to send a **new** invitation email when the previous link is missing, expired, or should be rotated.

## Functional requirements

- Available only from the **Invitation** panel on **User details** (FR-INV-002).
- Shown when **Status** is **`Invited`** and user has **Users.Manage**.
- Confirmation: **`Resend invitation to "{email}"? A new invitation link will be sent. Previous unused links will stop working.`**
- On confirm:
  - invalidate unused invitation tokens (FR-AUTH-007);
  - issue new token and send **Account invitation** email;
  - revoke previous **pending** account invitation rows and create a new pending row (**sent at** = now);
  - message **`Invitation resent.`**;
  - refresh **User details** in place.
- Does not change roles or **Email verified**.
- Does not apply when user already has a local password (no pending invitation).

### Permissions and visibility

- **Users.Manage**: required.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
