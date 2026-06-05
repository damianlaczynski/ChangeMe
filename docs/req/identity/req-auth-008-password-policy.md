---
id: REQ-AUTH-008
title: Password Policy
domain: identity
status: active
depends_on: []
---
## Goal

Password strength rules must be consistent across registration, user creation flows, password change, and reset, and configurable per deployment.

## Features

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
