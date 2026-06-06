---
id: FR-INV-007
title: Accept Invitation — Guest Screen Presentation
domain: invitations
type: functional
status: active
depends_on: [FR-AUTH-010, FR-AUTH-014]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

On **Accept invitation**, the invitee must see which account they are activating.

## Functional requirements

- When invitation preview is **valid** (`isValid` = true), show read-only line above the form, for example: **`Activating account for {email}`** using `preview.email` from `GET` invitation preview (FR-AUTH-010). See also FR-AUTH-010 cross-reference.
- When preview is invalid, keep existing error: **`This invitation link is invalid or has expired. Contact your administrator.`**
- External provider buttons and password form behavior unchanged (FR-AUTH-010, FR-AUTH-014).

### Permissions and visibility

- Guest (no sign-in required).

---

## Permissions summary (invitations)

| Action                        | Permission                                                |
| ----------------------------- | --------------------------------------------------------- |
| **Invite user**               | **Users.Manage** (+ **Roles.Manage** for role assignment) |
| **Resend invitation**         | **Users.Manage**                                          |
| **Cancel invitation**         | **Users.Manage**                                          |
| View pending invitation panel | **Users.View**                                            |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.

## Out of scope (this document)

- Administrator UI listing all past invitation sends (revoked/accepted) — accepted rows exist in storage for history only.
- Automatic deletion of user account on cancel invitation.
- Invitee self-service “decline invitation” without administrator cancel.
