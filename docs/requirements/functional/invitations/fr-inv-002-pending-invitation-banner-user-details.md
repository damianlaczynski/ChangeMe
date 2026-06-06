---
id: FR-INV-002
title: Pending Invitation Banner (User Details)
domain: invitations
type: functional
status: active
depends_on: [FR-INV-003, FR-INV-004, FR-USR-004]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

On **User details**, administrators must immediately see that the person was **invited but has not joined yet**. The block is **informational** (not the primary profile summary). It explains that the account exists in the directory but the invitee is not an active user of the application yet.

## Functional requirements

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
| **Resend invitation** | **Users.Manage**, **Status** **`Invited`** | Per FR-INV-003. |
| **Cancel invitation** | **Users.Manage**, **Status** **`Invited`** | Per FR-INV-004. |

- **Resend invitation** and **Cancel invitation** are **not** shown in the **User details** page header (card toolbar). Header keeps **Edit**, **Deactivate** / **Activate**, sessions, password reset, etc., per FR-USR-004.

### Permissions and visibility

- **Users.View**: see the panel when data is present.
- **Users.Manage**: **Resend** and **Cancel**.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-INV-002-01 | Administrator with **Users.View**; user **Status** **`Invited`** with `pendingInvitation` present | User opens **User details** | **Invitation** panel is the **first content block** above profile summary; section is **expanded by default**; intro explains the user was invited and cannot sign in until acceptance |
| AC-INV-002-02 | Administrator with **Users.View**; user has accepted, cancelled, or was never invited (`pendingInvitation` **null**) | User opens **User details** | **Invitation** panel is **not shown** |
| AC-INV-002-03 | **Invitation** panel visible; `pendingInvitation.isLinkExpired` is **true** | User views the panel | Tag **`Expired`** (warn severity) and message **`This invitation link may no longer work. Resend or cancel the invitation.`** are shown |
| AC-INV-002-04 | **Invitation** panel visible; **First name** and **Last name** are both empty | User views **Profile name** in the panel | **`Not set`** is shown |
| AC-INV-002-05 | Administrator with **Users.Manage**; user **Status** **`Invited`** | User views **User details** | **Resend invitation** and **Cancel invitation** appear in the **Invitation** panel only (not in the page header) |
| AC-INV-002-06 | User with `pendingInvitation` present | User views the **Invitation** panel | **Last sent at**, **Link expires at**, and expiry note **`Based on the active invitation link. Changing Auth:Invitations:InvitationLinkLifetimeHours does not change an already-issued token.`** are shown; **Email verified** is **not shown** in this panel |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
