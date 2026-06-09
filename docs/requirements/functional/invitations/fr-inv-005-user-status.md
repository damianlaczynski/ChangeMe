---
id: FR-INV-005
title: User Status
domain: invitations
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-011,
    FR-AUTH-012,
    FR-AUTH-014,
    FR-INV-002,
    FR-INV-003,
    FR-INV-004,
    FR-USR-005,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Administrators must see one **Status** per user on **Users list** and **User details**, covering the full invitation and membership lifecycle. On **Users list**, the **Account** column is **replaced** by **Status**; the **Account state** column is **removed** (not renamed).

Values: **`Invited`**, **`Invitation canceled`**, **`Active`**, **`Deactivated`**.

Mailbox verification is **not** part of **Status** — use **Email verified** (FR-AUTH-011) for self-registration and similar.

## Functional requirements

### Status values

| Status                    | When shown                                                                                                         | Meaning                                                                                                                                                                                                                                                         |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **`Deactivated`**         | **Deactivated** **true**                                                                                           | Administrator disabled the account.                                                                                                                                                                                                                             |
| **`Invited`**             | Not deactivated and `invitationPending` **true**                                                                   | Invitation outstanding; onboarding not complete. Expired link is shown in the **Invitation** panel (FR-INV-002), not as a separate status.                                                                                                                      |
| **`Invitation canceled`** | Not deactivated, `invitationPending` **false**, user **has no local password**, and **no** linked external sign-in | Directory account exists but the user cannot sign in with email/password yet (typical after **Cancel invitation**). Administrator may **Send invitation** (FR-INV-004). The invitee may still self-onboard when **public registration** is enabled (see below). |
| **`Active`**              | Not deactivated, not **`Invited`**, not **`Invitation canceled`**                                                  | User can use the application (local password and/or completed external onboarding). Includes self-registered users awaiting email verification — they remain **`Active`** here; see **Email verified**.                                                         |

### Evaluation order

1. **Deactivated** → **`Deactivated`**
2. **`invitationPending`** → **`Invited`**
3. No local password and no linked external provider → **`Invitation canceled`**
4. Otherwise → **`Active`**

### What is not a separate status

| Situation                                       | **Status**    | Where shown                                        |
| ----------------------------------------------- | ------------- | -------------------------------------------------- |
| Invitation link expired, pending row still open | **`Invited`** | **Invitation** panel: **Expired** tag (FR-INV-002) |
| Self-registration, mailbox not verified         | **`Active`**  | **Email verified** column / badge                  |
| Fully onboarded member                          | **`Active`**  | —                                                  |

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
- Applied filter chips use the format **`Status: {value}`** (for example **`Status: Invited`**).
- Filter mapping:
  - **`Deactivated`** → `deactivated` = true
  - **`Invited`** → `deactivated` = false and `invitationPending` = true
  - **`InvitationCanceled`** → `deactivated` = false, `invitationPending` = false, `hasPasswordSet` = false, no external logins
  - **`Active`** → `deactivated` = false, `invitationPending` = false, and (`hasPasswordSet` = true or has external login)

### User details

- Single read-only **Status** field (same rules). **Send invitation** when **Status** is **`Invitation canceled`** (FR-INV-004).

### Invitation canceled and public registration

When **Public registration enabled** is **true** (FR-AUTH-012), **Cancel invitation** does **not** block the invitee from using guest self-service onboarding with the **same email** as the directory account:

| Path                                                          | Behavior                                                                                                                                                                                                                                                                                                    |
| ------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Register** — **Continue with {Display name}** (FR-AUTH-014) | **Not** allowed to link or complete the canceled-invitation account via OIDC. Use **Register** with email and password (row below) or administrator **Send invitation**.                                                                                                                                    |
| **Register** — email and password (FR-AUTH-001)               | Allowed to **complete** the existing account (set local password and profile fields) instead of returning duplicate-email conflict, when the account has **no local password**, is **not** **awaiting invitation acceptance**, and is **not** **deactivated**. On success, **Status** becomes **`Active`**. |
| **Login** — external sign-in                                  | Same as **Register** OIDC: no auto-link; use password registration path or administrator invitation.                                                                                                                                                                                                        |

- When **Public registration enabled** is **false**, the invitee cannot self-register; only **Send invitation** / **Resend invitation** (administrator) or a new **Invite user** path applies.
- **Cancel invitation** does not delete the user row or change assigned roles.

### Deactivated + invited

- **Status** is **`Deactivated`** when the account is deactivated (wins over pending invitation in storage).
- **Resend invitation** / **Cancel invitation** require **Status** **`Invited`** — FR-INV-002, FR-INV-003, FR-INV-004.
- Invitation links remain invalid for sign-in while **Deactivated** (FR-USR-005, FR-AUTH-001).

### Permissions and visibility

- **Users.View**: required to see **Status** and use the **Status** filter.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
