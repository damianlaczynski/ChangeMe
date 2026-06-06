---
id: FR-USR-006
title: Admin Send Password Reset
domain: users
type: functional
status: active
depends_on: [FR-AUTH-007, FR-INV-003, FR-USR-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to send a password reset link to a user who forgot their password.

## Functional requirements

### User details action

- **Send password reset** header action on **User details** (FR-USR-004).
- Requires permission **Users.Manage**.
- Shown only when the account is enabled and the user **has a local password** (completed invite or registration).
- Confirmation dialog: **`Send a password reset link to "{email}"?`**
- On confirm, the system sends a **Password reset** email (FR-AUTH-007) and shows message **`Password reset email sent.`**
- The action can be repeated; each send invalidates previous unused reset tokens for that user.

### Business rules

- Users with **Deactivated** true cannot receive a reset link; the action is not shown.
- Users **awaiting invitation acceptance** cannot receive a password reset link; use **Resend invitation** (FR-INV-003) instead.

### Permissions and visibility

- **Users.Manage**: required for **Send password reset**.

---

## Acceptance scenarios

| ID            | Given                                                                                                  | When                                             | Then                                                                                                                                       |
| ------------- | ------------------------------------------------------------------------------------------------------ | ------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| AC-USR-006-01 | Administrator with **Users.Manage**; target user **has a local password** and **Deactivated** is false | User clicks **Send password reset** and confirms | Dialog **`Send a password reset link to "{email}"?`**; **Password reset** email sent (FR-AUTH-007); toast **`Password reset email sent.`** |
| AC-USR-006-02 | Administrator without **Users.Manage**                                                                 | User views **User details** header actions       | **Send password reset** is **not shown**                                                                                                   |
| AC-USR-006-03 | Administrator on **User details**; target **Deactivated** is true                                      | User views header actions                        | **Send password reset** is **not shown**                                                                                                   |
| AC-USR-006-04 | Administrator on **User details**; target is **awaiting invitation acceptance**                        | User views header actions                        | **Send password reset** is **not shown**; administrator uses **Resend invitation** (FR-INV-003) instead                                    |
| AC-USR-006-05 | Administrator sends **Send password reset** twice for the same user                                    | Second send completes                            | Previous unused reset tokens for that user are invalidated                                                                                 |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
