# Users requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-22 — Admin invite, password reset, and email confirmation

### Why

Admins onboard users by invitation; support password recovery; when verification is on, admins can confirm email manually.

### Requirements touched

| REQ           | Action                                                                                             |
| ------------- | -------------------------------------------------------------------------------------------------- |
| `REQ-USR-003` | **Updated** — Invite; optional name on create/edit; user confirms on accept                        |
| `REQ-USR-004` | **Updated** — Profile summary, **Invitation**, **Resend invitation**, **Password expires at** (UI) |
| `REQ-USR-002` | **Updated** — **`Pending profile`** when name incomplete; email verified filter                    |
| `REQ-USR-001` | **Updated** — **My account** minimal (no verification / invitation detail)                         |
| `REQ-USR-005` | **Updated** — **Deactivated at**; assignable rules unchanged                                       |
| `REQ-USR-006` | **New** — Admin send password reset                                                                |
| `REQ-USR-007` | **New** — Admin confirm email                                                                      |
| `REQ-USR-008` | **New** — Resend invitation; invalidates prior links; updates **Invitation sent at**               |

### Behavior delta

- **Before:** Create user with password and full name; **Status** Active/Inactive enum; assignable = Active only; no resend invitation.
- **After:** **Deactivated** + **Deactivated at**; admin invite with optional name; user confirms name on accept; **Resend invitation**; **Invitation sent at** tracked; **User details** shows invitation, verification, password-age fields, and calculated **Password expires at** when expiry is enabled; **My account** unchanged (minimal).

### Relates

- `docs/req/auth-requirements-changelog.md` — 2026-05-22 — Password lifecycle, invites, policy, and email verification

---
