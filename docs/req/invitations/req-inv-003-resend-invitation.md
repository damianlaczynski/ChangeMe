---
id: REQ-INV-003
title: Resend Invitation
domain: invitations
status: active
depends_on: [REQ-AUTH-007, REQ-INV-002]
---
## Goal

An administrator must be able to send a **new** invitation email when the previous link is missing, expired, or should be rotated.

## Features

- Available only from the **Invitation** panel on **User details** (REQ-INV-002).
- Shown when **Status** is **`Invited`** and user has **Users.Manage**.
- Confirmation: **`Resend invitation to "{email}"? A new invitation link will be sent. Previous unused links will stop working.`**
- On confirm:
  - invalidate unused invitation tokens (REQ-AUTH-007);
  - issue new token and send **Account invitation** email;
  - revoke previous **pending** account invitation rows and create a new pending row (**sent at** = now);
  - message **`Invitation resent.`**;
  - refresh **User details** in place.
- Does not change roles or **Email verified**.
- Does not apply when user already has a local password (no pending invitation).

### Permissions and visibility

- **Users.Manage**: required.

---
