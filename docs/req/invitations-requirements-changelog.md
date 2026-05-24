# Account invitations — requirements changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-24 — Account invitations area and admin UX

### Why

Invitation lifecycle was spread across Users and Auth REQs and mixed with “create user” wording. Administrators need a clear **invite / resend / cancel** model, accurate **expired** signalling from invitation tokens, list filtering, and a dedicated informational banner on **User details**.

### Requirements touched

| REQ            | Action                                                                                                  |
| -------------- | ------------------------------------------------------------------------------------------------------- |
| `REQ-INV-001`  | **New** — **Invite user** screen and copy (replaces **Create user** for admin onboarding).              |
| `REQ-INV-002`  | **New** — **Invitation** panel at top of **User details**; resend/cancel only here.                     |
| `REQ-INV-003`  | **New** — **Resend invitation** (moved from REQ-USR-008).                                               |
| `REQ-INV-004`  | **New** — **Cancel invitation**; **Send invitation** in header after cancel.                            |
| `REQ-INV-005`  | **New** — **Status** (`Invited`, `Invitation canceled`, `Active`, `Deactivated`) + filter.              |
| `REQ-INV-006`  | **New** — **`Auth:Invitations:Retention`**; purge **revoked** rows only; **accepted** kept for history. |
| `REQ-INV-007`  | **New** — guest email line on **Accept invitation** (see also REQ-AUTH-010).                            |
| `REQ-USR-002`  | **Updated** — **Invite user**; **Status** column/filter (REQ-INV-005).                                  |
| `REQ-USR-003`  | **Updated** — edit-only scope; invite moved to REQ-INV-001.                                             |
| `REQ-USR-004`  | **Updated** — invitation UI deferred to REQ-INV-002; header without resend.                             |
| `REQ-USR-008`  | **Removed** — superseded by REQ-INV-003 / REQ-INV-004.                                                  |
| `REQ-AUTH-010` | **Updated** — cross-ref REQ-INV-007 for guest email line.                                               |

### Behavior delta

**Before**

- Admin **Create user** wording; invitation details and **Resend** in **User details** header and collapsible section including redundant **Email verified**.
- No **Cancel invitation**; pending state did not expose token-based **expired**.
- No list filter for awaiting invitation.
- **Accept invitation** did not show invitee email on screen.
- **Invitations sent** count on admin panel; no retention of closed invitation rows.

**After**

- **Invite user** / **Send invitation** language; top **Invitation** informational panel for pending invites; **Resend** and **Cancel** only in that panel.
- API `pendingInvitation`: **lastSentAtUtc**, **expiresAtUtc**, **isExpired** (no **sentCount**); **expires at** prefers active token expiry.
- **Status** includes **`Invitation canceled`**; public **Register** / OIDC may complete onboarding (REQ-INV-005); route **`/users/invite`**.
- **`Auth:Invitations:Retention`** (default **7** days) removes **revoked** / **cancelled** rows only; **accepted** rows kept for history; on accept, panel hidden and invitation utilized.

### Relates

- `docs/req/users-requirements-changelog.md` — `2026-05-24 — Account invitations area and admin UX`
- `docs/req/auth-requirements-changelog.md` — `2026-05-24 — Account invitations area and admin UX`

---
