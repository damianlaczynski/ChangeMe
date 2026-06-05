---
id: REQ-INV-002
title: Pending Invitation Banner (User Details)
domain: invitations
status: active
depends_on: [REQ-INV-003, REQ-INV-004, REQ-USR-004]
---
## Goal

On **User details**, administrators must immediately see that the person was **invited but has not joined yet**. The block is **informational** (not the primary profile summary). It explains that the account exists in the directory but the invitee is not an active user of the application yet.

## Features

### Placement and scope

- When `pendingInvitation` is present, show an **Invitation** panel as the **first content block** on **User details** (above profile summary, roles, sessions, and permissions).
- Section is **expanded by default** (not collapsed behind a toggle on first load).
- When `pendingInvitation` is **null** (accepted, cancelled, or never invited), the section is **not shown**.

### Content (read-only)

| Element             | Behavior                                                                                                                                                                                 |
| ------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Intro**           | Short text, for example: **`This user was invited and has not completed account setup yet. They cannot sign in until they accept the invitation.`**                                      |
| **Last sent at**    | `pendingInvitation.lastSentAtUtc` (date/time).                                                                                                                                           |
| **Link expires at** | `pendingInvitation.expiresAtUtc` (date/time).                                                                                                                                            |
| **Expiry note**     | Muted helper: **`Based on the active invitation link. Changing Auth:Invitations:InvitationLinkLifetimeHours does not change an already-issued token.`**                                  |
| **Expired state**   | When `pendingInvitation.isLinkExpired` is **`true`**: show tag **`Expired`** (warn severity) and message **`This invitation link may no longer work. Resend or cancel the invitation.`** |
| **Profile name**    | **First name** and **Last name** when set; **`Not set`** when both empty (admin may set on **Edit user**; invitee confirms on accept).                                                   |

- **Do not show** **Email verified** in this panel (badge already appears in profile summary).

### Actions (only in this panel)

| Action                | When shown                                 | Behavior         |
| --------------------- | ------------------------------------------ | ---------------- |
| **Resend invitation** | **Users.Manage**, **Status** **`Invited`** | Per REQ-INV-003. |
| **Cancel invitation** | **Users.Manage**, **Status** **`Invited`** | Per REQ-INV-004. |

- **Resend invitation** and **Cancel invitation** are **not** shown in the **User details** page header (card toolbar). Header keeps **Edit**, **Deactivate** / **Activate**, sessions, password reset, etc., per REQ-USR-004.

### Permissions and visibility

- **Users.View**: see the panel when data is present.
- **Users.Manage**: **Resend** and **Cancel**.

---
