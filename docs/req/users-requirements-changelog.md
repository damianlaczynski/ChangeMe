# Users requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-06-04 — Self-service email change

### Why

**My account** and administrator screens must expose email-change status and actions consistently with the new auth flow.

### Requirements touched

| REQ           | Action                                                                                                                                                                                       |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `REQ-USR-001` | **Updated** — **Change email** header action (gated by **Self-service email change enabled**); **Pending email change** panel on **My account**; removed out-of-scope note for email change. |
| `REQ-USR-003` | **Updated** — administrator **Email** save cancels pending self-service change, revokes sessions, sets **Email verified**, sends notification emails.                                        |
| `REQ-USR-004` | **Updated** — **Pending email change** panel and **Cancel pending email change** on **User details**.                                                                                        |

### Behavior delta

- **Before:** **My account** listed **Email** as read-only only. **Edit user** could save a new **Email** without documented side effects on sessions, verification, or pending user actions.
- **After:** **My account** links to **Change email** and surfaces pending state. **User details** shows pending changes for support. **Edit user** email saves follow explicit admin rules aligned with REQ-AUTH-015.

### Relates

- `docs/req/auth-requirements-changelog.md` — `2026-06-04 — Self-service email change`
- `docs/req/auth-requirements-changelog.md` — `2026-06-04 — External linking policy and profile email` (**Edit user** notice)

---
