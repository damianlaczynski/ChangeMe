# Auth requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-05-22 — Password lifecycle, invites, policy, and email verification

### Why

Account security and onboarding: recovery, invitation, configurable password rules, optional password expiry, optional proof of email ownership, and optional disable of public registration.

### Requirements touched

| REQ            | Action                                                                                                   |
| -------------- | -------------------------------------------------------------------------------------------------------- |
| `REQ-AUTH-006` | **New** — Forgot / reset password via email                                                              |
| `REQ-AUTH-007` | **Updated** — Adds verify-your-email notification                                                        |
| `REQ-AUTH-008` | **New** — Configurable password policy                                                                   |
| `REQ-AUTH-009` | **New** — Password expiration when enabled; **Password expires at** on **User details** (UI, calculated) |
| `REQ-AUTH-010` | **New** — Accept invitation; pre-fill admin name; user edits; password; email verified when enabled      |
| `REQ-USR-008`  | **New** — Resend invitation (AUTH-007 email)                                                             |
| `REQ-AUTH-011` | **New** — Optional email verification and guest verify flow                                              |
| `REQ-AUTH-012` | **New** — **Public registration enabled** (default true); hide **Register** when false                   |
| `REQ-AUTH-005` | **Updated** — Policy validation; email on change; password age                                           |
| `REQ-AUTH-001` | **Updated** — Register/login gates; **Create account** link when public registration enabled             |
| `REQ-USR-003`  | **Updated** — Admin invite; optional name on create/edit; user confirms on accept                        |
| `REQ-USR-006`  | **New** — Admin send password reset                                                                      |
| `REQ-USR-007`  | **New** — Admin confirm email                                                                            |

### Behavior delta

- **Before:** Register and login immediately open the app; no email proof; admin sets full profile and password on create; account **Status** enum Active/Inactive.
- **After:** **Deactivated** boolean replaces Status enum; admin invite (**Email** + **Roles**, optional name); user confirms or edits name on **Accept invitation**; admin can **Resend invitation**; optional **Public registration enabled** (default on); **User details** (not **My account**) holds admin-facing account metadata.

### Relates

- `docs/req/users-requirements-changelog.md` — 2026-05-22 — Admin invite, password reset, and email confirmation

---
