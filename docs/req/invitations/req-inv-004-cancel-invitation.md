---
id: REQ-INV-004
title: Cancel Invitation
domain: invitations
status: active
depends_on: [REQ-INV-002]
---
## Goal

An administrator must be able to **withdraw** a pending invitation when the invite should no longer be valid, without deleting the user account from the directory.

## Features

- Available only from the **Invitation** panel on **User details** (REQ-INV-002).
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
