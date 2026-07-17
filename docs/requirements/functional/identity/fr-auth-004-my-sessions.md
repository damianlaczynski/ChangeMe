---
id: FR-AUTH-004
title: My Sessions
domain: identity
type: functional
status: active
depends_on: [FR-AUTH-003, FR-USR-001]
inherits_conventions: [STD-ACC-001, STD-LST-002, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to review active sign-in sessions and revoke sessions they no longer trust.

## Functional requirements

### Authorization

- **Sessions.ViewOwn**: required to view active sessions.
- **Sessions.ManageOwn**: required to revoke non-current sessions and use **Sign out everywhere** (FR-AUTH-003).

### Data

- Each active session exposes **device / browser label** (format **`{Browser} on {Platform}`**), **IP address** (or **`Unknown`**), **signed in at**, **last activity**, and whether it is the **current session**.
- Only **active** sessions are shown; revoked sessions do not appear.
- Inherits STD-LST-002 unless stated below.

### Operations

- View all active sessions on **My account** (FR-USR-001).
- Revoke a non-current session after confirmation.
- The current session cannot be revoked individually; the user signs out the current browser via logout (FR-AUTH-003).

### Validation

- **Revoke session**: confirmation message **`Revoke this session? That device will be signed out.`**

### Business rules

- Revoking a session signs out that device on next activity.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
