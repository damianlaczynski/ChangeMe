---
id: FR-INV-001
title: Invite User (Administrator)
domain: invitations
type: functional
status: active
depends_on:
  [
    FR-AUTH-007,
    FR-AUTH-010,
    FR-AUTH-011,
    FR-AUTH-012,
    FR-AUTH-014,
    FR-ROL-005,
    FR-USR-003,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

An authorized administrator must be able to **invite** a person by email: create their account, assign roles, optionally set profile name, and send the first invitation email. The flow is explicitly an **invite**, not a generic “create user” without onboarding.

## Functional requirements

### Invite user screen

- Screen title: **`Invite user`**
- Route: **`/users/invite`** (replaces **`/users/create`**); breadcrumb **`Invite user`**.
- Button on **Users list**: **`Invite user`** (replaces **`Add user`** / **`Create user`**); navigates to **`/users/invite`**.
- Menu remains under **Users**.
- Subheader (example): **`Send an invitation by email. The person completes setup from the link before they can sign in.`**
- Requires permission **Users.Manage**.

| Field          | Behavior                                                   |
| -------------- | ---------------------------------------------------------- |
| **First name** | **Optional**; max **100** characters.                      |
| **Last name**  | **Optional**; max **100** characters.                      |
| **Email**      | **Required**; valid email; unique; max **320** characters. |
| **Roles**      | Multi-select; assignment rules per FR-ROL-005.             |

- **Roles** and **Permissions** preview: same rules as former create flow (FR-USR-003).
- Inherits `FR-UI-001` (**Create and edit form screens**) for validation presentation and form-area loading; **Back** / **Cancel** navigate to **Users list**; primary submit label **`Send invitation`** (replaces **`Create user`**).
- On success:
  - message **`Invitation sent.`** (replaces **`User created. An invitation email has been sent.`**);
  - open **User details** for the new user;
  - record first **account invitation** and send **Account invitation** email (FR-AUTH-007);
  - set **Email verified** when email verification is enabled (FR-AUTH-011).

### Business rules

- Invited users start with **Deactivated** **false**.
- Invited users remain **awaiting invitation acceptance** until **Accept invitation** (FR-AUTH-010) or matching external sign-in (FR-AUTH-014).
- When **Public registration enabled** is **true**, self-registration remains available (FR-AUTH-012); when **false**, new accounts come only through **Invite user**.
- Administrator cannot remove their own **Administrator** role on invite (same rule as before).

### Permissions and visibility

- **Users.Manage**: required.
- **Roles.Manage**: required to assign roles on invite.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-INV-001-01 | Signed-in user without **Users.Manage** | User attempts to open **Invite user** | Access is denied; **Invite user** screen is not available |
| AC-INV-001-02 | Administrator with **Users.Manage** on **Users list** | User clicks **Invite user** | **Invite user** screen opens at **`/users/invite`**; breadcrumb **`Invite user`**; subheader explains email invitation setup |
| AC-INV-001-03 | Administrator with **Users.Manage** on **Invite user** with valid **Email** and optional **First name** / **Last name** | User clicks **Send invitation** | Toast **`Invitation sent.`**; **User details** opens for the new user; **Account invitation** email is sent (FR-AUTH-007); **Email verified** is set when email verification is enabled (FR-AUTH-011) |
| AC-INV-001-04 | Administrator on **Invite user** | User clicks **Back** or **Cancel** | **Users list** opens; no invitation is sent |
| AC-INV-001-05 | Administrator with **Users.Manage** inviting their own account and attempting to remove the **Administrator** role | User submits **Send invitation** | Submission is blocked; administrator cannot remove their own **Administrator** role on invite |
| AC-INV-001-06 | New user created via **Invite user** | Invitation is sent successfully | **Deactivated** is **false**; user remains **awaiting invitation acceptance** until **Accept invitation** (FR-AUTH-010) or matching external sign-in (FR-AUTH-014) |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
