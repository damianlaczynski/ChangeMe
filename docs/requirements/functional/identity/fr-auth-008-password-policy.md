---
id: FR-AUTH-008
title: Password Policy
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Password strength rules must be consistent for admin user creation and configurable per deployment.

## Functional requirements

### Default rules (when not overridden in deployment settings)

- Minimum length **8**, maximum length **128**.
- At least one uppercase letter, one lowercase letter, and one digit.
- Special characters are **not required** by default.

### User-visible validation

- Violations show inline on the password field with a specific message (for example **`Password must contain at least one uppercase letter.`**).
- All password entry forms load policy hints from the server on screen open.

### Configuration

- Deployment settings define minimum length, maximum length, and each character-class requirement.
- Changing settings affects new password entry only; existing passwords are not re-validated on sign-in.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
