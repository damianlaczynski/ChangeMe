---
id: REQ-INV-001
title: Invite User (Administrator)
domain: invitations
status: active
depends_on: [REQ-AUTH-007, REQ-AUTH-010, REQ-AUTH-011, REQ-AUTH-012, REQ-AUTH-014, REQ-ROL-005, REQ-USR-003]
---
## Goal

An authorized administrator must be able to **invite** a person by email: create their account, assign roles, optionally set profile name, and send the first invitation email. The flow is explicitly an **invite**, not a generic “create user” without onboarding.

## Features

### Invite user screen

- Screen title: **`Invite user`**
- Route: **`/users/invite`** (replaces **`/users/create`**); breadcrumb **`Invite user`**.
- Button on **Users list**: **`Invite user`** (replaces **`Add user`** / **`Create user`**); navigates to **`/users/invite`**.
- Menu remains under **Users**.
- Subheader (example): **`Send an invitation by email. The person completes setup from the link before they can sign in.`**
- Requires permission **Users.Manage**.

| Field          | Behavior                                                   |
| -------------- | ---------------------------------------------------------- |
| **First name** | **Optional**; max **100** characters.                      |
| **Last name**  | **Optional**; max **100** characters.                      |
| **Email**      | **Required**; valid email; unique; max **320** characters. |
| **Roles**      | Multi-select; assignment rules per REQ-ROL-005.            |

- **Roles** and **Permissions** preview: same rules as former create flow (REQ-USR-003).
- **Back** / **Cancel** navigate to **Users list** without saving.
- Primary button: **`Send invitation`** (replaces **`Create user`**).
- On success:
  - message **`Invitation sent.`** (replaces **`User created. An invitation email has been sent.`**);
  - open **User details** for the new user;
  - record first **account invitation** and send **Account invitation** email (REQ-AUTH-007);
  - set **Email verified** when email verification is enabled (REQ-AUTH-011).

### Business rules

- Invited users start with **Deactivated** **false**.
- Invited users remain **awaiting invitation acceptance** until **Accept invitation** (REQ-AUTH-010) or matching external sign-in (REQ-AUTH-014).
- When **Public registration enabled** is **true**, self-registration remains available (REQ-AUTH-012); when **false**, new accounts come only through **Invite user**.
- Administrator cannot remove their own **Administrator** role on invite (same rule as before).

### Permissions and visibility

- **Users.Manage**: required.
- **Roles.Manage**: required to assign roles on invite.

---
