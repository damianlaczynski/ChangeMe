---
id: FR-INV-001
title: User Invitations
domain: invitations
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-008,
    FR-ROL-001,
    FR-ROL-005,
    FR-ROL-006,
    FR-USR-003,
    FR-USR-005,
  ]
inherits_conventions:
  [STD-ACC-001, STD-FRM-001, STD-LST-001, STD-MSG-001, STD-OP-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

> Invitation terms: `docs/requirements/_shared/domain/glossary.md`.
> Role assignment rules: `FR-ROL-005`. Password rules: `FR-AUTH-008`.

## Goal

An authorized administrator must be able to invite a person by email to create their own account, and the invitee must be able to accept the invitation and sign in.

## Functional requirements

### Authorization

**Administrator flows**

- **Users.Invite**: required to view the invitations list, create invitations, resend invitations, and revoke pending invitations.
- **Roles.Manage**: required to view and edit **Roles** on the create invitation form.
- The invitations area is visible only with **Users.Invite**.

**Acceptance flow**

- Available to guests with a valid invitation link; does not require sign-in.
- A signed-in user who opens an acceptance link is signed out before the acceptance form is shown.

### Data

**Invitation**

| Field           | Constraints                                                                                          |
| --------------- | ---------------------------------------------------------------------------------------------------- |
| **Email**       | **Required**; valid email; max **320** characters.                                                   |
| **First name**  | **Not required** on create; max **100** characters when provided.                                    |
| **Last name**   | **Not required** on create; max **100** characters when provided.                                    |
| **Roles**       | **Required** when **Roles** is visible; at least one role per FR-ROL-005.                           |
| **Invited by**  | Acting administrator; set by the system on create.                                                     |
| **Created at**  | Set by the system on create.                                                                           |
| **Expires at**  | **Created at** plus **7 days**; updated on resend.                                                   |
| **Status**      | One of **Pending**, **Accepted**, **Expired**, **Revoked** (see Business rules).                     |
| **Accepted at** | Set when acceptance succeeds; **`—`** until then.                                                    |

- The invitation token is single-use, stored hashed, and never shown in administrator lists.

**Invitations list** exposes: **Email**, **Status**, **Roles**, **Invited by**, **Created at**, **Expires at**, **Accepted at**.

- **Filterable** attributes: **Email**, **Status**, **Created at**, **Expires at**.
- Default sort: **Created at**, descending.

**Accept invitation form** (guest):

| Field                | Constraints                                                        |
| -------------------- | ------------------------------------------------------------------ |
| **Email**            | Read-only; from the invitation.                                      |
| **First name**       | **Required**; max **100** characters.                              |
| **Last name**        | **Required**; max **100** characters.                              |
| **Password**         | **Required**; follows password policy (FR-AUTH-008).               |
| **Confirm password** | **Required**; must match **Password**.                             |

- Pre-filled **First name** and **Last name** from the invitation when present; the invitee may change them before submit.

### Operations

**Administrator**

- Create an invitation with **Email**, optional name fields, and **Roles**.
- Browse and filter the invitations list.
- **Resend** a **Pending** invitation: sends a new email, generates a new token, invalidates the previous token, and resets **Expires at** to seven days from resend.
- **Revoke** a **Pending** invitation after confirmation; status becomes **Revoked**; the token cannot be used.

**Invitee**

- Open the acceptance link from email while **Pending** and before **Expires at**.
- Submit the acceptance form to create an account with **Deactivated** false, assigned **Roles** from the invitation, and the profile from the form.
- On success, the invitation status becomes **Accepted**, **Accepted at** is recorded, and the invitee is redirected to sign-in.

**Email**

- On create and resend, the system sends one email to **Email** containing a link to the acceptance form with the active token.
- Email subject: **`You are invited to ChangeMe`**.
- Email body includes: invited **Email**, **Expires at** (date and time), and the acceptance link.

### Validation

- **Create invitation** — email already belongs to an active user: rejection message **`A user with this email already exists.`**
- **Create invitation** — another **Pending** invitation exists for the same **Email**: rejection message **`An invitation for this email is already pending.`**
- **Roles**: per FR-ROL-005.
- **Revoke invitation**: confirmation message **`Revoke invitation for "{email}"? The link will stop working.`**
- **Accept invitation** — token missing or unknown: rejection message **`This invitation link is not valid.`**
- **Accept invitation** — status **Revoked**: rejection message **`This invitation is no longer valid.`**
- **Accept invitation** — status **Accepted**: rejection message **`This invitation has already been used.`**
- **Accept invitation** — status **Expired** or past **Expires at**: rejection message **`This invitation has expired. Contact an administrator for a new invitation.`**
- **Accept invitation** — **Email** now belongs to an active user: rejection message **`A user with this email already exists.`**

### Business rules

**Status transitions**

- **Pending** → **Accepted** when acceptance succeeds.
- **Pending** → **Revoked** when an administrator revokes.
- **Pending** → **Expired** when current time is after **Expires at** (evaluated on list load, acceptance open, and acceptance submit).
- **Accepted**, **Expired**, and **Revoked** are terminal; they do not return to **Pending**.

**Role assignment on create**

- When the administrator has **Roles.Manage**, **Roles** is visible and editable on create; at least one role is required.
- When the administrator lacks **Roles.Manage**, **Roles** is hidden and the invitation assigns the **User** system role (FR-ROL-006).

**Consistency**

- Acceptance creates the user through the same uniqueness and profile rules as FR-USR-003, except the administrator does not set a password.
- The new account does **not** receive **Administrator** unless that role was explicitly selected on the invitation (FR-ROL-006).
- After acceptance, the invitee signs in with **Email** and **Password** per FR-AUTH-001.
- Direct user creation with password (FR-USR-003) and invitation remain separate paths; an accepted invitation does not create a duplicate account.

**Administrator restrictions**

- An administrator **cannot** revoke an invitation that is not **Pending**; rejection message **`Only pending invitations can be revoked.`**
- An administrator **cannot** resend an invitation that is not **Pending**; rejection message **`Only pending invitations can be resent.`**

**Success feedback**

- After create: toast **`Invitation sent.`**
- After resend: toast **`Invitation resent.`**
- After revoke: toast **`Invitation revoked.`**
- After acceptance: inline success message on sign-in screen **`Account created. Sign in with your email and password.`**

## Out of scope

- Self-service registration without an invitation.
- Bulk invitation import.
- Custom expiry per invitation (fixed at seven days).
- Editing a **Pending** invitation in place (revoke and create a new one instead).
- Forced password change on first sign-in after acceptance.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
