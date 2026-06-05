---
id: REQ-USR-007
title: Admin Confirm Email
domain: users
status: active
depends_on: [REQ-AUTH-007, REQ-AUTH-011, REQ-INV-001, REQ-USR-004]
---
## Goal

When email verification is enabled, an authorized administrator must be able to mark a user's email as verified without the user clicking the verification link — for example after self-registration.

## Features

### User details action

- **Confirm email** header action on **User details** (REQ-USR-004).
- Requires permission **Users.Manage**.
- Shown only when email verification is enabled (REQ-AUTH-011) and the user's **Email verified** is false (typically self-registered accounts).
- **Not shown** when the user was invited via **Invite user** and is already verified from the invitation email (REQ-INV-001).
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
- Manual confirmation does not send email (REQ-AUTH-007).

### Permissions and visibility

- **Users.Manage**: required for **Confirm email**.
