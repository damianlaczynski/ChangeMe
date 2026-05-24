# Passkeys requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-25 — Passkeys / WebAuthn requirements (new area)

### Why

Product needs phishing-resistant sign-in and step-up aligned with existing password, TOTP two-factor, and OIDC flows. Requirements are split into a dedicated area so auth REQs stay focused and cross-links stay explicit.

### Requirements touched

| REQ            | Action                                                                                    |
| -------------- | ----------------------------------------------------------------------------------------- |
| `REQ-PKY-001`  | **New** — deployment policy, RP settings, interaction with 2FA and passkey-only accounts. |
| `REQ-PKY-002`  | **New** — guest passkey sign-in on **Login** and post-auth gates.                         |
| `REQ-PKY-003`  | **New** — **My account** enrollment, rename, remove, strict setup enrollment.             |
| `REQ-PKY-004`  | **New** — passkey step-up for sensitive actions.                                          |
| `REQ-PKY-005`  | **New** — administrator view, per-credential remove, **Reset passkeys**.                  |
| `REQ-PKY-006`  | **New** — combined compliance gate order and cross-auth interaction.                      |
| `REQ-PKY-007`  | **New** — passkey notification emails.                                                    |
| `REQ-AUTH-001` | **Updated** (pending) — **Login** passkey entry; cross-ref REQ-PKY-002.                   |
| `REQ-AUTH-007` | **Updated** (pending) — notification catalog for passkey events.                          |
| `REQ-AUTH-013` | **Updated** (pending) — remove WebAuthn from out-of-scope; step-up table extension.       |
| `REQ-USR-001`  | **Updated** (pending) — **My account** Passkeys section.                                  |
| `REQ-USR-004`  | **Updated** (pending) — **User details** Passkeys section and **Reset passkeys**.         |

### Behavior delta

**Before:** No passkey or WebAuthn support; REQ-AUTH-013 explicitly excluded passkeys.

**After:** Optional deployment-enabled passkeys for primary sign-in and step-up; optional mandatory passkey enrollment; configurable substitution for two-factor on passkey sign-in; admin reset and user self-service per REQ-PKY-001–007. Implementation not started until changelog entry is removed.

### Relates

- `docs/req/auth-requirements-changelog.md` — `2026-05-25 — Passkeys cross-references (pending)`
- `docs/req/users-requirements-changelog.md` — `2026-05-25 — Passkeys cross-references (pending)`

---
