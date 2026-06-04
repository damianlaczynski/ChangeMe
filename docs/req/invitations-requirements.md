# Requirements — Account invitations

This document covers administrator **invite**, **resend**, and **cancel** flows, **membership status** on **Users list** and **User details**, the **pending invitation** panel, invitation retention, and the guest **Accept invitation** presentation delta.

Cross-references:

- **Edit user** (profile, roles, deactivation): `docs/req/users-requirements.md` (REQ-USR-003).
- **User details** shell (sessions, permissions): `docs/req/users-requirements.md` (REQ-USR-004).
- **Accept invitation** (password, OIDC completion): `docs/req/auth-requirements.md` (REQ-AUTH-010).
- **Invitation email and tokens**: `docs/req/auth-requirements.md` (REQ-AUTH-007).
- **External sign-in during invite**: `docs/req/auth-requirements.md` (REQ-AUTH-014).
- **Passkeys after onboarding**: `docs/req/passkeys-requirements.md` (REQ-PKY-002, REQ-PKY-003) — not part of invitation acceptance.

---

## Business terms

| Term                               | Meaning                                                                                                                                                                                                                                                                                                                                                                                  |
| ---------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Account invitation**             | One invitation send recorded for a user: **sent at**, lifecycle **pending** (active), **revoked** (superseded by **Resend invitation** or **Cancel invitation** — both set **RevokedAtUtc**), or **accepted** (onboarding complete; kept for history, never removed by retention). **Revoked** / **cancelled** rows are purged after **RevokedInvitationRetentionDays** (default **7**). |
| **Awaiting invitation acceptance** | The user has a **pending** account invitation and has **not** completed onboarding. They are not yet an active application user (no local password and no completed external onboarding).                                                                                                                                                                                                |
| **Invitation expired**             | The current invitation **token** is missing, already used, or past its **ExpiresAtUtc** (`UserAuthToken`). This is independent of whether a pending `AccountInvitation` row still exists.                                                                                                                                                                                                |
| **Invite user**                    | Administrator action that creates an account and sends the first **Account invitation** email. UI label **`Invite user`** (not **Create user**).                                                                                                                                                                                                                                         |
| **Cancel invitation**              | Administrator action that ends the pending invitation without sending a replacement: pending invitation rows are closed, unused invitation tokens are invalidated, and the user remains in the directory without access until invited again or otherwise managed.                                                                                                                        |

**Email verified** for invited users: set when **Invite user** succeeds (mailbox chosen by admin). See REQ-AUTH-011 and REQ-USR-007.

**Status** on **Users list** and **User details**: **`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** per REQ-INV-005.

---

## Account invitation (implementation model)

Each send creates an `AccountInvitation` row on the user aggregate:

| Field                | Stored | Purpose                                                                                         |
| -------------------- | ------ | ----------------------------------------------------------------------------------------------- |
| **SentAtUtc**        | Yes    | When the invitation email was sent.                                                             |
| **LinkExpiresAtUtc** | Yes    | Expiry recorded for the invitation link issued for that send; aligns with the associated token. |
| **AcceptedAtUtc**    | Yes    | Set when onboarding completes (password link or matching external sign-in).                     |
| **RevokedAtUtc**     | Yes    | Set when superseded by **Resend invitation**, **Cancel invitation**, or a newer send.           |

Link expiry is stored on `AccountInvitation` as `LinkExpiresAtUtc` for the send record, while invitation validity is enforced using the active **invitation token** (`UserAuthToken` type `Invitation`) and its expiry.

**Pending invitation** (API: `pendingInvitation` on **User details**): present when the user has at least one account invitation with **pending** lifecycle (`AcceptedAtUtc` and `RevokedAtUtc` both null). When absent (`null`), the user is not **awaiting invitation acceptance** (accepted, cancelled, or never invited). Summary fields:

| Field             | Source                                                                                                                                                                                       |
| ----------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **lastSentAtUtc** | **Sent at** of the current pending invitation (latest pending send).                                                                                                                         |
| **expiresAtUtc**  | **ExpiresAtUtc** of the current valid unused invitation token when one exists; otherwise **lastSentAtUtc** + current `AuthOptions:Invitations:InvitationLinkLifetimeHours` for display only. |
| **isLinkExpired** | **`true`** when there is no valid unused invitation token for the user at request time, or that token is past expiry. **`false`** when a valid unused token exists.                          |

**Invitation pending** (API: `invitationPending` on **Users list** and **User details**): **`true`** when the user has a pending account invitation (same lifecycle rule as above). Drives **Status** **`Invited`** when **Deactivated** is false (REQ-INV-005).

**After invitation acceptance** (REQ-AUTH-010, REQ-AUTH-014): the current pending invitation is **utilized** (**AcceptedAtUtc** set); `pendingInvitation` becomes **null** and the **Invitation** panel is hidden. The **accepted** row is **kept permanently** for history. Earlier **revoked** rows remain until **retention** removes them (REQ-INV-006). No rows are deleted at accept time.

**API rules (not a separate REQ):**

- **invitationPending** reflects domain pending lifecycle (not accepted, not revoked), independent of **isLinkExpired**.
- **isLinkExpired** reflects **token** validity only; UI may show **`Expired`** while **Resend invitation** remains available (REQ-INV-003).

---

# REQ-INV-001: Invite User (Administrator)

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

# REQ-INV-002: Pending Invitation Banner (User Details)

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

# REQ-INV-003: Resend Invitation

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

# REQ-INV-004: Cancel Invitation

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

# REQ-INV-005: User Status

## Goal

Administrators must see one **Status** per user on **Users list** and **User details**, covering the full invitation and membership lifecycle. On **Users list**, the **Account** column is **replaced** by **Status**; the **Account state** column is **removed** (not renamed).

Values: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`**.

Mailbox verification is **not** part of **Status** — use **Email verified** (REQ-AUTH-011) for self-registration and similar.

## Features

### Status values

| Status                    | When shown                                                                                                         | Meaning                                                                                                                                                                                                                                                          |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`Deactivated`**         | **Deactivated** **true**                                                                                           | Administrator disabled the account.                                                                                                                                                                                                                              |
| **`Invited`**             | Not deactivated and `invitationPending` **true**                                                                   | Invitation outstanding; onboarding not complete. Expired link is shown in the **Invitation** panel (REQ-INV-002), not as a separate status.                                                                                                                      |
| **`Invitation canceled`** | Not deactivated, `invitationPending` **false**, user **has no local password**, and **no** linked external sign-in | Directory account exists but the user cannot sign in with email/password yet (typical after **Cancel invitation**). Administrator may **Send invitation** (REQ-INV-004). The invitee may still self-onboard when **public registration** is enabled (see below). |
| **`Active`**              | Not deactivated, not **`Invited`**, not **`Invitation canceled`**                                                  | User can use the application (local password and/or completed external onboarding). Includes self-registered users awaiting email verification — they remain **`Active`** here; see **Email verified**.                                                          |

### Evaluation order

1. **Deactivated** → **`Deactivated`**
2. **`invitationPending`** → **`Invited`**
3. No local password and no linked external provider → **`Invitation canceled`**
4. Otherwise → **`Active`**

### What is not a separate status

| Situation                                       | **Status**    | Where shown                                         |
| ----------------------------------------------- | ------------- | --------------------------------------------------- |
| Invitation link expired, pending row still open | **`Invited`** | **Invitation** panel: **Expired** tag (REQ-INV-002) |
| Self-registration, mailbox not verified         | **`Active`**  | **Email verified** column / badge                   |
| Fully onboarded member                          | **`Active`**  | —                                                   |

### Users list

- **Account** column → **Status** (one tag per row).
- **Account state** column → **removed**.
- Tag severity: **`Deactivated`** — danger; **`Invited`** — warn; **`Invitation canceled`** — warn (secondary/muted acceptable); **`Active`** — success.

### Filters (replaces **Account** filter)

- **Status** multi-select: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`**.
- Empty selection = no restriction. Chips e.g. **`Status: Invitation canceled`**.

### API (users list query)

- Optional **`status`** filter array: **`Invited`**, **`InvitationCanceled`**, **`Active`**, **`Deactivated`** (wire format may use PascalCase enum; UI labels as above).
- **Users list** items expose at least: `deactivated`, `invitationPending`, `hasPasswordSet`, and whether the user has any **external login** (for **`Invitation canceled`**).
- Filter mapping:
  - **`Deactivated`** → `deactivated` = true
  - **`Invited`** → `deactivated` = false and `invitationPending` = true
  - **`InvitationCanceled`** → `deactivated` = false, `invitationPending` = false, `hasPasswordSet` = false, no external logins
  - **`Active`** → `deactivated` = false, `invitationPending` = false, and (`hasPasswordSet` = true or has external login)

### User details

- Single read-only **Status** field (same rules). **Send invitation** when **Status** is **`Invitation canceled`** (REQ-INV-004).

### Invitation canceled and public registration

When **Public registration enabled** is **true** (REQ-AUTH-012), **Cancel invitation** does **not** block the invitee from using guest self-service onboarding with the **same email** as the directory account:

| Path                                                           | Behavior                                                                                                                                                                                                                                                                                                    |
| -------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Register** — **Continue with {Display name}** (REQ-AUTH-014) | **Not** allowed to link or complete the canceled-invitation account via OIDC. Use **Register** with email and password (row below) or administrator **Send invitation**.                                                                                                                                    |
| **Register** — email and password (REQ-AUTH-001)               | Allowed to **complete** the existing account (set local password and profile fields) instead of returning duplicate-email conflict, when the account has **no local password**, is **not** **awaiting invitation acceptance**, and is **not** **deactivated**. On success, **Status** becomes **`Active`**. |
| **Login** — external sign-in                                   | Same as **Register** OIDC: no auto-link; use password registration path or administrator invitation.                                                                                                                                                                                                        |

- When **Public registration enabled** is **false**, the invitee cannot self-register; only **Send invitation** / **Resend invitation** (administrator) or a new **Invite user** path applies.
- **Cancel invitation** does not delete the user row or change assigned roles.

### Deactivated + invited

- **Status** is **`Deactivated`** when the account is deactivated (wins over pending invitation in storage).
- **Resend invitation** / **Cancel invitation** require **Status** **`Invited`** — REQ-INV-002, REQ-INV-003, REQ-INV-004.
- Invitation links remain invalid for sign-in while **Deactivated** (REQ-USR-005, REQ-AUTH-001).

### Permissions and visibility

- **Users.View**: required to see **Status** and use the **Status** filter.

---

# REQ-INV-006: Invitation History Retention

## Goal

**Revoked** and **cancelled** **account invitation** rows (same storage: **RevokedAtUtc** set) must not accumulate indefinitely. **Accepted** rows are **audit history** and are **never** deleted by retention. Configuration lives under **AuthOptions**, similar in spirit to notification retention (`NotificationRetentionOptions`).

## Configuration

Section: **`AuthOptions:Invitations:Retention`**

| Setting                            | Default                                          | Meaning                                                                                                                                                                    |
| ---------------------------------- | ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **RevokedInvitationRetentionDays** | **7**                                            | Delete **revoked** / **cancelled** invitation rows when older than this many days. Age is measured from **RevokedAtUtc**, or **SentAtUtc** if **RevokedAtUtc** is missing. |
| **CleanupCronExpression**          | Hangfire default (same pattern as notifications) | Schedule for the background cleanup job.                                                                                                                                   |

- Settings live in deployment configuration (for example `appsettings.json`) under the **AuthOptions** area, not under **Notifications**.
- **Pending** and **accepted** invitations are **never** removed by retention.

### Example configuration

```json
"AuthOptions": {
  "Invitations": {
    "InvitationLinkLifetimeHours": 72,
    "Retention": {
      "RevokedInvitationRetentionDays": 7,
      "CleanupCronExpression": "0 4 * * *"
    }
  }
}
```

- Operational reference: `docs/auth-operations-guide.md` (§ Invitation retention).

## Features

### Background cleanup

- Recurring job (Hangfire) deletes **revoked** / **cancelled** `AccountInvitation` rows ( **RevokedAtUtc** not null) that exceed **RevokedInvitationRetentionDays**.
- **Pending** and **accepted** rows are excluded.
- Cleanup does not delete the user account, sessions, or auth tokens (token invalidation remains on resend, cancel, and accept flows).

### On invitation acceptance

When onboarding completes (REQ-AUTH-010, REQ-AUTH-014):

- Set **AcceptedAtUtc** on the pending row (invitation **utilized**); unused invitation tokens are invalidated per REQ-AUTH-010.
- `pendingInvitation` becomes **null**; the **Invitation** panel is hidden.
- **Do not** delete any invitation rows at accept time. The **accepted** row is retained permanently. Earlier **revoked** rows remain until the retention job removes them.

### Permissions and visibility

- Not user-facing; administrators see no invitation history UI (out of scope).

---

# REQ-INV-007: Accept Invitation — Guest Screen Presentation

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
