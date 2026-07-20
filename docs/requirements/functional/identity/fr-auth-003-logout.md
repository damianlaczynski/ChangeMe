---
id: FR-AUTH-003
title: Logout
domain: identity
type: functional
status: active
depends_on: [FR-USR-001]
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to sign out from the current browser or from all devices.

## Functional requirements

### Operations

- Sign out of the **current session**; protected actions are no longer available until the user signs in again.
- **Sign out everywhere** (FR-USR-001): revoke **all active sessions** for the user, including the current browser, after confirmation.

### Validation

- **Sign out everywhere**: confirmation message **`Sign out everywhere? You will be signed out on every device including this browser.`**

### Business rules

- Repeating logout when already signed out completes without error.
- A revoked session cannot access protected actions or renew credentials.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
