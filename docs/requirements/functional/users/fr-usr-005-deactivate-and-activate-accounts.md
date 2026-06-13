---
id: FR-USR-005
title: Deactivate and Activate Accounts
domain: users
type: functional
status: active
depends_on: [FR-ISS-002, FR-ROL-006]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to set **Deactivated** to **true** or **false**, immediately removing or restoring sign-in access.

## Functional requirements

### Deactivate

- Available from **Users list** overflow **Deactivate**, **User details** **Deactivate**, and **Edit user** when **Deactivated** is set to **true** (requires **Users.Deactivate**).
- Confirmation dialog: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- On confirm:
  - **Deactivated** becomes **true**;
  - **Deactivated at** is set to the current date and time;
  - all active sessions for that user are revoked;
  - show message **`User deactivated.`**;
  - refresh the current screen in place.

### Activate

- Available from **Users list** overflow **Activate**, **User details** **Activate**, and **Edit user** when **Deactivated** is set to **false** (requires **Users.Deactivate**).
- Confirmation dialog: **`Activate "{full name}"? The user will be able to sign in again.`**
- On confirm:
  - **Deactivated** becomes **false**;
  - **Deactivated at** is cleared;
  - show message **`User activated.`**;
  - refresh the current screen in place.
- Activation does **not** restore previously revoked sessions.

### Business rules

- An administrator **cannot** set their own **Deactivated** to **true**; the action is rejected with message **`You cannot deactivate your own account.`**
- Deactivating the first seeded administrator requires another user with **Deactivated** false, **Users.Deactivate**, and the **Administrator** role (FR-ROL-006).
- Deactivation does **not** delete the user record, issue authorship, or comments.
- Users with **Deactivated** true are excluded from assignable-user selectors (FR-ISS-002).

### Assignable users

- Assignable-user lists include only users with **Deactivated** false.
- Each option shows **Display label** (`displayLabel`): **`{first name} {last name} ({email})`** or **Email** only when both names are empty.

### Permissions and visibility

- **Users.Deactivate** is required for **Deactivate** and **Activate** actions.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
