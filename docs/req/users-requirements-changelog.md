# Users requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-25 — Passkeys cross-references (pending)

### Why

Passkeys self-service and admin UX are specified in REQ-PKY-003 and REQ-PKY-005; Users REQs reference **My account** and **User details** sections.

### Requirements touched

| REQ            | Action                                                                                     |
| -------------- | ------------------------------------------------------------------------------------------ |
| `REQ-USR-001`  | **Updated** — **Passkeys** section on **My account** (REQ-PKY-003).                        |
| `REQ-USR-004`  | **Updated** (pending detail in REQ-PKY-005) — **Passkeys** section and **Reset passkeys**. |
| Business terms | **Updated** — **Passkey enrolled**, **Passkey-only account**, **Passkey credential**.      |

### Behavior delta

**Before:** No passkey terms or UI on user screens.

**After:** Documented placeholders; UI ships with passkeys implementation.

### Relates

- `docs/req/passkeys-requirements-changelog.md` — `2026-05-25 — Passkeys / WebAuthn requirements (new area)`

---

## 2026-05-24 — Account invitations area and admin UX

### Why

Invitation admin flows and pending-invitation presentation move to `docs/req/invitations-requirements.md`. Users REQs keep profile, list shell, edit, sessions, and deactivation.

### Requirements touched

| REQ           | Action                                                                                       |
| ------------- | -------------------------------------------------------------------------------------------- |
| `REQ-USR-002` | **Updated** — **Status** column/filter (`Invited`, `Active`, `Deactivated`) per REQ-INV-005. |
| `REQ-USR-003` | **Updated** — **Edit user** only; invite per REQ-INV-001.                                    |
| `REQ-USR-004` | **Updated** — invitation panel per REQ-INV-002; no resend in header.                         |
| `REQ-USR-008` | **Removed** — see `docs/req/invitations-requirements.md`.                                    |

### Behavior delta

See `docs/req/invitations-requirements-changelog.md` (same date).

### Relates

- `docs/req/invitations-requirements-changelog.md` — `2026-05-24 — Account invitations area and admin UX`

---
