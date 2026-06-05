---
id: REQ-USR-005
title: Deactivate and Activate Accounts
domain: users
status: active
depends_on: [REQ-AUTH-011, REQ-ISS-002, REQ-ROL-006]
---
## Goal

An authorized administrator must be able to set **Deactivated** to **true** or **false**, immediately removing or restoring sign-in access.

## Features

### Deactivate

- Available from **Users list** overflow **Deactivate**, **User details** **Deactivate**, and **Edit user** when **Deactivated** is set to **true** (requires **Users.Deactivate**).
- Confirmation dialog: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- On confirm:
  - **Deactivated** becomes **true**;
  - **Deactivated at** is set to the current date and time;
  - all active sessions for that user are revoked;
  - show message **`User deactivated.`**;
  - refresh the current screen in place.

### Activate

- Available from **Users list** overflow **Activate**, **User details** **Activate**, and **Edit user** when **Deactivated** is set to **false** (requires **Users.Deactivate**).
- Confirmation dialog: **`Activate "{full name}"? The user will be able to sign in again.`**
- On confirm:
  - **Deactivated** becomes **false**;
  - **Deactivated at** is cleared;
  - show message **`User activated.`**;
  - refresh the current screen in place.
- Activation does **not** restore previously revoked sessions and does **not** by itself complete invitation or email verification.

### Business rules

- An administrator **cannot** set their own **Deactivated** to **true**; the action is rejected with message **`You cannot deactivate your own account.`**
- Deactivating the first seeded administrator requires another user with **Deactivated** false, **Users.Deactivate**, and the **Administrator** role (REQ-ROL-006).
- Deactivation does **not** delete the user record, issue authorship, or comments.
- Users with **Deactivated** true are excluded from assignable-user selectors (REQ-ISS-002).

### Assignable users

- Assignable-user lists include only users with **Deactivated** false.
- When email verification is enabled (REQ-AUTH-011), assignable users must also have a **verified email** and a **local password**.
- Each option shows **Display label** (`displayLabel`): **`{first name} {last name} ({email})`** or **Email** only when both names are empty.

### Permissions and visibility

- **Users.Deactivate** is required for **Deactivate** and **Activate** actions.

---
