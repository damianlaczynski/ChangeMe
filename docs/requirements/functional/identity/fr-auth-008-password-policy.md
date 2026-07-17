---
id: FR-AUTH-008
title: Password Policy
domain: identity
type: functional
status: active
depends_on: []
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

Password strength rules must be consistent for admin user creation and configurable per deployment.

## Functional requirements

### Data

Default rules (when not overridden in deployment settings):

- Minimum length **8**, maximum length **128**.
- At least one uppercase letter, one lowercase letter, and one digit.
- Special characters are **not required** by default.

### Operations

- All password entry forms load policy rules from the server.
- Deployment settings define minimum length, maximum length, and each character-class requirement.

### Validation

- Violations are rejected with a specific message per failed rule (for example **`Password must contain at least one uppercase letter.`**).

### Business rules

- Changing deployment settings affects new password entry only; existing passwords are not re-validated on sign-in.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
