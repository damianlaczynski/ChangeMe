# Auth requirements — changelog

Pending changes only. See `docs/requirements-change-process.md`. Remove entries after implementation.

---

## 2026-06-04 — External linking policy and profile email

### Why

Linking providers only from **My account** avoids surprise account merges on login. **Profile email** is the single notification destination; provider emails may differ after linking or **Change email**.

### Requirements touched

| REQ            | Action                                                                                                                                                                                                                                   |
| -------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `REQ-AUTH-014` | **Updated** — no guest auto-link on **Login**/**Register**; link only from **My account** without provider email match; **Profile email** vs **Provider email**; invitation OIDC remains the only guest email-match link (REQ-AUTH-010). |
| `REQ-AUTH-010` | **Updated** — invitation external onboarding requires verified provider email **matching invited Profile email**; explicit error when mismatch.                                                                                          |
| `REQ-AUTH-007` | **Updated** — all auth notification emails sent to **Profile email**; provider email never used.                                                                                                                                         |
| `REQ-AUTH-014` | **Updated** — **External provider linking enabled** deployment flag (separate from **External providers enabled**).                                                                                                                      |
| `REQ-AUTH-015` | **Updated** — **Profile email** / external notices; **Self-service email change enabled** deployment flag.                                                                                                                               |

### Behavior delta

- **Before:** Guest OIDC could auto-link when provider email matched an existing account (**Link external account**, external-only second provider auto-link). Notification destination was implicit.
- **After:** Guest OIDC signs in only when **External login** exists, creates a **new** account (public registration), or completes **Accept invitation** with matching invited email. Existing users link providers from **My account** after step-up; provider email may differ from **Profile email**. All auth emails use **Profile email**. Guest **Link external account** screen removed.

### Relates

- `docs/req/invitations-requirements-changelog.md` — `2026-06-04 — External linking policy and profile email`
- `docs/req/users-requirements-changelog.md` — `2026-06-04 — Self-service email change` (Profile email notices on **Edit user**)

---

## 2026-06-04 — Self-service email change

### Why

Users need a secure way to update their sign-in email without administrator involvement. Confirmation via the new mailbox reduces account takeover risk.

### Requirements touched

| REQ            | Action                                                                                                         |
| -------------- | -------------------------------------------------------------------------------------------------------------- |
| `REQ-AUTH-015` | **New** — signed-in user requests email change, confirms from new mailbox, can resend or cancel while pending. |
| `REQ-AUTH-007` | **Updated** — five new auth notification email types for email change flows.                                   |
| `REQ-AUTH-011` | **Updated** — cross-reference to self-service email change; removed prior out-of-scope note.                   |
| `REQ-AUTH-013` | **Updated** — **Change email** submit and **Cancel email change** added to sensitive account actions.          |
| `REQ-AUTH-014` | **Updated** — external-provider notice on **Change email**; external logins retained after change.             |

### Behavior delta

- **Before:** **Email** on **My account** was read-only with no self-service change path (explicitly out of scope in REQ-USR-001 / REQ-AUTH-011). Administrators could change **Email** immediately on **Edit user** without additional notification or session rules.
- **After:** Signed-in users open **Change email**, pass step-up authentication, and receive a confirmation link on the **new** address; **current email** stays active for sign-in until confirmation. **My account** shows a **Pending email change** panel with resend and cancel. Guests use **Confirm email change** from the link. Successful confirmation revokes all sessions and requires sign-in with the new address. Administrators gain explicit rules when saving **Email** on **Edit user** (cancel pending change, revoke sessions, verification, notification emails).

### Relates

- `docs/req/users-requirements-changelog.md` — `2026-06-04 — Self-service email change`

---
