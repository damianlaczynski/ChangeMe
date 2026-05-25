# Auth requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-25 — Passkeys cross-references (pending)

### Why

New passkeys requirements area; Auth REQs need explicit **Login** and REQ-AUTH-013 cross-links until passkeys are implemented.

### Requirements touched

| REQ            | Action                                                                                 |
| -------------- | -------------------------------------------------------------------------------------- |
| `REQ-AUTH-001` | **Updated** — **Login** passkey entry; compliance gates reference REQ-PKY-006.         |
| `REQ-AUTH-013` | **Updated** — passkeys removed from out-of-scope; cross-ref REQ-PKY-001 / REQ-PKY-006. |

### Behavior delta

**Before:** No passkey sign-in on **Login**; WebAuthn listed as out of scope in REQ-AUTH-013.

**After:** Documented integration points only; behaviour ships when `docs/req/passkeys-requirements-changelog.md` entry is implemented and removed.

### Relates

- `docs/req/passkeys-requirements-changelog.md` — `2026-05-25 — Passkeys / WebAuthn requirements (new area)`

---
