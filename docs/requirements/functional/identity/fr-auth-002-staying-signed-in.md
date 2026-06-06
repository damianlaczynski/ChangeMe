---
id: FR-AUTH-002
title: Staying Signed In
domain: identity
type: functional
status: active
depends_on: [FR-ISS-004, FR-ROL-001, FR-USR-005]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

The user must remain signed in during normal application use without repeated manual sign-in while the session remains valid.

## Functional requirements

### Session lifetime

| Setting                         | Default        |
| ------------------------------- | -------------- |
| **Short-lived credential**      | **30 minutes** |
| **Long-lived session lifetime** | **14 days**    |

- Every successful sign-in (password, external provider, or registration when email verification is disabled) creates a session with the **long-lived session lifetime** of **14 days**, counted from **signed in at**.
- The client stores session credentials in **browser local storage** so the user remains signed in after closing and reopening the browser, within the configured lifetime.
- The **short-lived credential** expires **30 minutes** after issuance.

### Automatic renewal

- **60 seconds** before the short-lived credential expires, the system renews it automatically without user action while the session is active.
- On each successful renewal, the system updates **last activity** on the current session.
- The user’s effective permissions are refreshed on renewal (FR-ROL-001).
- Sessions can be renewed after the browser is closed and reopened, within the **14-day** lifetime.
- If renewal fails because the session expired, was revoked, or **Deactivated** is **true**, the user is signed out and redirected to **Login**.

### Renewal after failed action

- When a signed-in user triggers an action that fails because the short-lived credential expired, the system attempts **one** automatic renewal and repeats the action once.
- If renewal fails, the user is signed out and redirected to **Login**.

### Real-time updates

- After successful credential renewal, open real-time views (issue list refresh and notifications per FR-ISS-004) continue to receive updates without manual page reload.

### States and business rules

- A **revoked** session cannot be renewed.
- Deactivating a user revokes all active sessions immediately (FR-USR-005); renewal attempts for that user fail and redirect to **Login**.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-AUTH-002-01 | Signed-in user with active session; **short-lived credential** within **30 minutes** of issuance | **60 seconds** before credential expiry | System renews credential automatically; updates **last activity** on current session; refreshes effective permissions (FR-ROL-001) |
| AC-AUTH-002-02 | Signed-in user; session within **14-day** long-lived lifetime; browser closed and reopened | User returns to the application | Session can be renewed without manual sign-in |
| AC-AUTH-002-03 | Signed-in user; **short-lived credential** expired; user triggers an authenticated action | Action fails due to expired credential | System attempts **one** automatic renewal and repeats the action once |
| AC-AUTH-002-04 | Signed-in user; session **revoked** or past **14-day** lifetime, or user **Deactivated** is **true** (FR-USR-005) | Automatic renewal is attempted | Renewal fails; user is signed out and redirected to **Login** |
| AC-AUTH-002-05 | Signed-in user with active session | Short-lived credential is renewed successfully | Open real-time views (issue list refresh and notifications per FR-ISS-004) continue without manual page reload |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
