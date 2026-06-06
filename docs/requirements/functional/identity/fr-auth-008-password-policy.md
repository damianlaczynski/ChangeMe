---
id: FR-AUTH-008
title: Password Policy
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

Password strength rules must be consistent across registration, user creation flows, password change, and reset, and configurable per deployment.

## Functional requirements

### Default rules (when not overridden in deployment settings)

- Minimum length **8**, maximum length **128**.
- At least one uppercase letter, one lowercase letter, and one digit.
- Special characters are **not required** by default.

### User-visible validation

- Violations show inline on the password field with a specific message (for example **`Password must contain at least one uppercase letter.`**).
- All password forms load policy hints from the server on screen open.

### Configuration

- Deployment settings define minimum length, maximum length, and each character-class requirement.
- Changing settings affects new password entry only; existing passwords are not re-validated until change.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-AUTH-008-01 | Default deployment settings (no policy override); guest on **Register** | User enters **Password** missing an uppercase letter | Inline field error such as `Password must contain at least one uppercase letter.` on the password field |
| AC-AUTH-008-02 | Default deployment settings; user on any password form (**Register**, **Change password**, **Reset password**, **Required password change**) | User opens the screen | Policy hints are loaded from the server on screen open |
| AC-AUTH-008-03 | Default deployment settings; guest on **Register** | User enters **Password** with length **7** characters | Inline validation rejects password below minimum length **8** |
| AC-AUTH-008-04 | Deployment settings changed to require special characters; existing user with password set under old policy | User signs in without changing password | Existing password remains valid until user enters a new password on change or reset |
| AC-AUTH-008-05 | Deployment settings changed to new minimum length; user on **Change password** (FR-AUTH-005) | User enters **New password** violating updated policy | Inline field error on **New password** with specific violation message |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
