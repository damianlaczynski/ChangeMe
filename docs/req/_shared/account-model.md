# Account model

> Observable account attributes used across Users, Auth, Invitations, and Passkeys REQs.

Administrative enablement is separate from onboarding and how the user signs in.

## Attributes

| Concept                      | Shown in UI / admin                                                                             | Meaning                                                                                                                                                            |
| ---------------------------- | ----------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Deactivated**              | **Status** **`Deactivated`**                                                                    | Whether an administrator disabled the account.                                                                                                                     |
| **Deactivated at**           | **User details**                                                                                | When the account was last deactivated, if applicable.                                                                                                              |
| **Local password**           | Implied by **Change password**, invitation state                                                | Whether the user has completed setting a ChangeMe password. Users **without** a local password are either **awaiting invitation acceptance** or **external-only**. |
| **Email verified**           | **Email verified** badge, filters                                                               | Whether the mailbox is considered confirmed when email verification is enabled (REQ-AUTH-011).                                                                     |
| **Email verified at**        | **User details**                                                                                | When verification last succeeded.                                                                                                                                  |
| **Password last changed at** | **User details**, password expiration                                                           | When the local password was last set or changed (REQ-AUTH-009).                                                                                                    |
| **Pending invitation**       | **Invitation** panel (REQ-INV-002)                                                              | Summary while **awaiting invitation acceptance**; hidden after acceptance or cancel.                                                                               |
| **Two-factor enabled**       | **My account**, **User details**                                                                | Whether the user enrolled in app TOTP when two-factor is enabled in deployment settings (REQ-AUTH-013).                                                            |
| **Two-factor enabled at**    | **User details**                                                                                | When two-factor enrollment last completed.                                                                                                                         |
| **External login**           | **External sign-in methods**                                                                    | A linked external provider identity (provider name, linked date).                                                                                                  |
| **Passkey credential**       | **Passkeys** (REQ-PKY-003, REQ-PKY-005)                                                         | A registered WebAuthn credential (name, created at, last used at, authenticator type).                                                                             |
| **Pending email change**     | **Pending email change** panel on **My account** (REQ-AUTH-015); **User details** (REQ-USR-004) | Self-service request to replace **current email** with a **new email** until confirmed or cancelled.                                                               |

## Email verified — when verification is enabled (REQ-AUTH-011)

- **Self-registration:** not verified until the user completes **Verify email**; verification time recorded on success.
- **Administrator invite user:** verified when **Invite user** succeeds (REQ-INV-001) — the invitation is sent to that email address, which is treated as confirmed.
- **Initial administrator:** verified at creation (REQ-ROL-006).
- **Accept invitation:** remains verified if already set at invite; otherwise verified on success (mailbox proof via the invitation link).

When email verification is **disabled** in deployment settings, every account is treated as verified for sign-in.

## Status (UI only, read-only)

**`Invited`**, **`Invitation canceled`**, **`Active`**, or **`Deactivated`** on **Users list** and **User details**. Rules: REQ-INV-005.

## Invitation lifecycle (observable)

- **Pending invitation** is present while the user is **awaiting invitation acceptance**.
- After **Accept invitation** (REQ-AUTH-010) or matching external sign-in (REQ-AUTH-014), the pending invitation is closed; the **Invitation** panel is hidden.
- **Invitation pending** on list and details drives **Status** **`Invited`** when **Deactivated** is false (REQ-INV-005).
- **Link expired** reflects link validity only; **Resend invitation** may remain available (REQ-INV-003).

## Other rules

- On admin invite, **First name** and **Last name** are **not required** on **Invite user** (REQ-INV-001) and **Edit user** (REQ-USR-003). On **Accept invitation** (REQ-AUTH-010), fields are pre-filled and the user may edit them before submit.
- **Password expires at (admin UI only):** Not stored. When password expiration is **enabled** (REQ-AUTH-009), **User details** shows **Password expires at** as **Password last changed at** plus **Maximum password age (days)**. Omitted when expiration is disabled; shown as **`—`** for users without a local password. Not shown on **My account** (REQ-USR-001).
