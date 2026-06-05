---
id: REQ-USR-006
title: Admin Send Password Reset
domain: users
status: active
depends_on: [REQ-AUTH-007, REQ-INV-003, REQ-USR-004]
---
## Goal

An authorized administrator must be able to send a password reset link to a user who forgot their password.

## Features

### User details action

- **Send password reset** header action on **User details** (REQ-USR-004).
- Requires permission **Users.Manage**.
- Shown only when the account is enabled and the user **has a local password** (completed invite or registration).
- Confirmation dialog: **`Send a password reset link to "{email}"?`**
- On confirm, the system sends a **Password reset** email (REQ-AUTH-007) and shows message **`Password reset email sent.`**
- The action can be repeated; each send invalidates previous unused reset tokens for that user.

### Business rules

- Users with **Deactivated** true cannot receive a reset link; the action is not shown.
- Users **awaiting invitation acceptance** cannot receive a password reset link; use **Resend invitation** (REQ-INV-003) instead.

### Permissions and visibility

- **Users.Manage**: required for **Send password reset**.

---
