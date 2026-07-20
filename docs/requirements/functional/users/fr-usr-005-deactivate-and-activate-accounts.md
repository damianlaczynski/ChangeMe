---
id: FR-USR-005
title: Deactivate and Activate Accounts
domain: users
type: functional
status: active
depends_on: [FR-ISS-002, FR-ROL-006]
inherits_conventions: [STD-ACC-001, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to set **Deactivated** to **true** or **false**, immediately removing or restoring sign-in access.

## Functional requirements

### Authorization

- **Users.Deactivate**: required to deactivate and activate accounts.

### Operations

**Deactivate** (requires confirmation):

- **Deactivated** becomes **true**.
- **Deactivated at** is set to the current date and time.
- All active sessions for that user are revoked immediately.
- Success message: **`User deactivated.`**

**Activate** (requires confirmation):

- **Deactivated** becomes **false**.
- **Deactivated at** is cleared.
- Activation does **not** restore previously revoked sessions.
- Success message: **`User activated.`**

### Data

- **Assignable users** (FR-ISS-002): only users with **Deactivated** false.
- Each assignable user option uses **Display label** (`displayLabel`): **`{first name} {last name} ({email})`** or **Email** only when both names are empty.

### Validation

- An administrator **cannot** set their own **Deactivated** to **true**; rejection message: **`You cannot deactivate your own account.`**
- **Deactivate**: confirmation message **`Deactivate {full name}? The user will be signed out and cannot sign in until reactivated.`**
- **Activate**: confirmation message **`Activate {full name}? The user will be able to sign in again.`**

### Business rules

- Deactivating the first seeded administrator requires another user with **Deactivated** false, **Users.Deactivate**, and the **Administrator** role (FR-ROL-006).
- Deactivating the last active administrator is rejected; rejection message: **`You cannot deactivate the last active administrator.`**
- Deactivation does **not** delete the user record, issue authorship, or comments.
- Users with **Deactivated** true cannot sign in (FR-AUTH-001) and are excluded from assignable-user lists (FR-ISS-002).

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
