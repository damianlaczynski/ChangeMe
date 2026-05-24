# Auth requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-24 — Account invitations area and admin UX

### Why

Invitation lifecycle terms and guest accept screen copy are aligned with the new invitations requirements area.

### Requirements touched

| REQ            | Action                                                                                                                            |
| -------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `REQ-AUTH-007` | **Updated** — email triggered by **Invite user** / resend / cancel per invitations REQs.                                          |
| `REQ-AUTH-010` | **Updated** — guest email line (REQ-INV-007); acceptance keeps **accepted** row; retention purges **revoked** only (REQ-INV-006). |
| `REQ-AUTH-001` | **Updated** — **Register** may complete **Invitation canceled** account (REQ-INV-005).                                            |
| `REQ-AUTH-014` | **Updated** — external sign-in for **Invitation canceled** (REQ-INV-005).                                                         |
| Auth config    | **Updated** — **`Auth:Invitations:Retention`** (REQ-INV-006).                                                                     |

### Behavior delta

See `docs/req/invitations-requirements-changelog.md` (same date).

### Relates

- `docs/req/invitations-requirements-changelog.md` — `2026-05-24 — Account invitations area and admin UX`

---
