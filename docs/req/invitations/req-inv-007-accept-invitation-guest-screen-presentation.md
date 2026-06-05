---
id: REQ-INV-007
title: Accept Invitation — Guest Screen Presentation
domain: invitations
status: active
depends_on: [REQ-AUTH-010, REQ-AUTH-014]
---
## Goal

On **Accept invitation**, the invitee must see which account they are activating.

## Features

- When invitation preview is **valid** (`isValid` = true), show read-only line above the form, for example: **`Activating account for {email}`** using `preview.email` from `GET` invitation preview (REQ-AUTH-010). See also REQ-AUTH-010 cross-reference.
- When preview is invalid, keep existing error: **`This invitation link is invalid or has expired. Contact your administrator.`**
- External provider buttons and password form behavior unchanged (REQ-AUTH-010, REQ-AUTH-014).

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

---

## Out of scope (this document)

- Administrator UI listing all past invitation sends (revoked/accepted) — accepted rows exist in storage for history only.
- Automatic deletion of user account on cancel invitation.
- Invitee self-service “decline invitation” without administrator cancel.
