---
id: FR-USR-007
title: Admin Confirm Email
domain: users
type: functional
status: active
depends_on: [FR-AUTH-007, FR-AUTH-011, FR-INV-001, FR-USR-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When email verification is enabled, an authorized administrator must be able to mark a user's email as verified without the user clicking the verification link — for example after self-registration.

## Functional requirements

### User details action

- **Confirm email** header action on **User details** (FR-USR-004).
- Requires permission **Users.Manage**.
- Shown only when email verification is enabled (FR-AUTH-011) and the user's **Email verified** is false (typically self-registered accounts).
- **Not shown** when the user was invited via **Invite user** and is already verified from the invitation email (FR-INV-001).
- Shown for users with an email address on record regardless of **Deactivated**.
- Confirmation dialog: **`Mark email as verified for "{full name}"?`**
- On confirm:
  - **Email verified** becomes true;
  - **Email verified at** is set to the current time;
  - show message **`Email marked as verified.`**;
  - refresh **User details** in place.
- The action is **not shown** when **Email verified** is already true.

### Business rules

- **Confirm email** does not sign the user in and does not revoke or create sessions.
- Admin-invited users are already **email verified** when the invitation is sent; they must still complete invitation acceptance (via the email link **or** external sign-in) before they can use the application.
- Manual confirmation does not send email (FR-AUTH-007).

### Permissions and visibility

- **Users.Manage**: required for **Confirm email**.

## Acceptance scenarios

| ID            | Given                                                                                                                     | When                                       | Then                                                                                                                                                                                                                |
| ------------- | ------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-USR-007-01 | Email verification **enabled**; administrator with **Users.Manage**; target **Email verified** is false (self-registered) | User views **User details** header         | **Confirm email** is shown                                                                                                                                                                                          |
| AC-USR-007-02 | Administrator with **Users.Manage**; target was invited via **Invite user** and is already verified (FR-INV-001)          | User views **User details** header         | **Confirm email** is **not shown**                                                                                                                                                                                  |
| AC-USR-007-03 | Administrator with **Users.Manage**; target **Email verified** is already true                                            | User views **User details** header         | **Confirm email** is **not shown**                                                                                                                                                                                  |
| AC-USR-007-04 | Administrator without **Users.Manage**                                                                                    | User views **User details** header         | **Confirm email** is **not shown**                                                                                                                                                                                  |
| AC-USR-007-05 | Administrator with **Users.Manage**; target **Email verified** is false                                                   | User clicks **Confirm email** and confirms | Dialog **`Mark email as verified for "{full name}"?`**; **Email verified** true; **Email verified at** set; toast **`Email marked as verified.`**; **User details** refreshes in place; no email sent (FR-AUTH-007) |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
