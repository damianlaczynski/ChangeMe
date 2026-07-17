---
id: FR-AUTH-002
title: Staying Signed In
domain: identity
type: functional
status: active
depends_on: [FR-ISS-004, FR-ROL-001, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-VAL-001]
inherits_quality: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must remain signed in during normal application use without repeated manual sign-in while the session remains valid.

## Functional requirements

### Data

| Setting                         | Default        |
| ------------------------------- | -------------- |
| **Short-lived credential**      | **30 minutes** |
| **Long-lived session lifetime** | **14 days**    |

- Every successful sign-in (FR-AUTH-001) creates a session with the **long-lived session lifetime** of **14 days**, counted from **signed in at**.
- Session credentials persist in **browser local storage** so the user remains signed in after closing and reopening the browser, within the configured lifetime.
- The **short-lived credential** expires **30 minutes** after issuance.

### Operations

- **60 seconds** before the short-lived credential expires, the system renews it automatically without user action while the session is active.
- On each successful renewal, the system updates **last activity** on the current session and refreshes effective permissions (FR-ROL-001).
- Sessions can be renewed after the browser is closed and reopened, within the **14-day** lifetime.
- When a signed-in user triggers an action that fails because the short-lived credential expired, the system attempts **one** automatic renewal and repeats the action once.
- After successful credential renewal, open real-time views (notifications per FR-ISS-004) continue to receive updates without manual page reload.

### Business rules

- A **revoked** session cannot be renewed.
- If renewal fails because the session expired, was revoked, or **Deactivated** is **true**, the user is signed out and must sign in again.
- Deactivating a user revokes all active sessions immediately (FR-USR-005); renewal attempts for that user fail.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
