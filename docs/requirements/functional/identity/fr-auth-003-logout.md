---
id: FR-AUTH-003
title: Logout
domain: identity
type: functional
status: active
depends_on: [FR-USR-001]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

The user must be able to sign out from the current browser or from all devices.

## Functional requirements

### Logout (current browser)

- **Logout** button in the application header signs the user out of the **current session**.
- The user is redirected to **Login**.
- Protected screens are no longer accessible until the user signs in again.

### Sign out everywhere

- **Sign out everywhere** button is a header action on **My account** (FR-USR-001).
- Clicking **Sign out everywhere** opens confirmation dialog: **`Sign out from all devices? You will be signed out on every browser and device.`**
- On confirm, the system revokes **all active sessions** for the user, signs out the current browser, and redirects to **Login**.

### States and business rules

- Repeating logout when already signed out redirects to **Login** without error.
- A revoked session cannot access protected screens or renew credentials.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
