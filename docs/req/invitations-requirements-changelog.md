# Account invitations — requirements changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-06-04 — External linking policy and profile email

### Why

Align invitation flows with REQ-AUTH-014: OIDC on **Login**/**Register** no longer auto-links to existing accounts except **Accept invitation** with matching invited email.

### Requirements touched

| REQ           | Action                                                                                                                           |
| ------------- | -------------------------------------------------------------------------------------------------------------------------------- |
| `REQ-INV-005` | **Updated** — **Invitation canceled** + public registration: OIDC cannot complete account; password **Register** path unchanged. |

### Behavior delta

- **Before:** After **Cancel invitation**, invitee could use **Continue with {Provider}** on **Register**/**Login** to link and sign in when provider email matched the directory account.
- **After:** Same scenario requires **Register** with email/password or a new administrator invitation; OIDC does not auto-link.

### Relates

- `docs/req/auth-requirements-changelog.md` — `2026-06-04 — External linking policy and profile email`

---
