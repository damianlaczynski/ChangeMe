---
id: FR-INV-004
title: Cancel Invitation
domain: invitations
type: functional
status: active
depends_on: [FR-INV-002]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

An administrator must be able to **withdraw** a pending invitation when the invite should no longer be valid, without deleting the user account from the directory.

## Functional requirements

- Available only from the **Invitation** panel on **User details** (FR-INV-002).
- Shown when **Status** is **`Invited`** and user has **Users.Manage**.
- Confirmation: **`Cancel invitation for "{email}"? They will not be able to use the current invitation link. You can send a new invitation later.`**
- On confirm:
  - revoke all **pending** account invitation rows (`RevokedAtUtc` set);
  - invalidate all unused invitation tokens for that user;
  - clear `pendingInvitation` on subsequent **User details** load;
  - set `invitationPending` to **`false`** on **Users list**;
  - message **`Invitation cancelled.`**;
  - refresh **User details** in place.
- The user account **remains** in the system (roles, email, audit history). They still have **no local password** unless set later by another flow.
- **Cancel invitation** does not deactivate the account. Administrator may **Deactivate**, **Invite** again later (new invite flow if account was never completed), or **Resend** is unavailable until a new invitation is sent (after cancel, admin uses **Invite** path only if no pending invite — typically **Resend** hidden until a new invite exists; if account exists without pending, show action to send invitation from **Edit** or dedicated **Send invitation** — see business rule below).

### Business rules

- After cancel, the user is **not** **awaiting invitation acceptance** until a new invitation is sent.
- When the account has **no local password** and **no** pending invitation (for example after cancel), **User details** shows **`Send invitation`** in the profile header (same backend behavior as **Resend invitation**: new token, email, and pending row).
- Cancel does not delete the user row.
- Cancel does not sign the user in or out.

### Permissions and visibility

- **Users.Manage**: required.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-INV-004-01 | Administrator with **Users.Manage**; user **Status** **`Invited`** | User clicks **Cancel invitation** in the **Invitation** panel | Confirmation **`Cancel invitation for "{email}"? They will not be able to use the current invitation link. You can send a new invitation later.`** is shown |
| AC-INV-004-02 | Administrator confirms **Cancel invitation** | Confirm completes | Toast **`Invitation cancelled.`**; **User details** refreshes in place; all **pending** invitation rows are revoked; unused invitation tokens are invalidated; `pendingInvitation` is cleared; **Users list** shows `invitationPending` **false** |
| AC-INV-004-03 | After **Cancel invitation**; account has **no local password** and **no** pending invitation | Administrator opens **User details** | **`Send invitation`** appears in the profile header |
| AC-INV-004-04 | After **Cancel invitation** | User account state is inspected | User row **remains** in the directory with roles and email unchanged; user is **not** **awaiting invitation acceptance**; account is **not** deactivated |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
