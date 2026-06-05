---
id: REQ-AUTH-010
title: Accept Account Invitation
domain: identity
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-007, REQ-AUTH-008, REQ-AUTH-013, REQ-AUTH-014, REQ-INV-001, REQ-INV-006, REQ-INV-007, REQ-USR-003]
---
## Goal

A user created by an administrator must complete onboarding before using the application. Onboarding may be completed **either** by setting a local password through the invitation email link **or** by signing in with an external identity provider when external sign-in is enabled (REQ-AUTH-014). Profile name may be supplied on the invitation screen or taken from the identity provider when the administrator did not set both names.

## Features

### Accept invitation screen (email link)

- Screen: **Accept invitation**; available to guests with valid token in the link.
- When invitation preview is **valid**, show read-only line above the form: **`Activating account for {email}`** (REQ-INV-007).
- Invalid or expired token shows: **`This invitation link is invalid or has expired. Contact your administrator.`**

| Field                    | Behavior                                                                                                                                   |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **First name**           | **Required**; max **100** characters; pre-filled from the account when the administrator already set a value (REQ-USR-003); user may edit. |
| **Last name**            | **Required**; max **100** characters; pre-filled when already set; user may edit.                                                          |
| **New password**         | **Required**; **Password policy** (REQ-AUTH-008).                                                                                          |
| **Confirm new password** | **Required**; must match **New password**.                                                                                                 |

- **Activate account** button: on success stores the submitted **First name** and **Last name** (replacing any admin-entered values), establishes a **local password**, sets **password last changed at**, marks the pending **account invitation** as accepted (invitation utilized; clears **Pending invitation** on **User details**; **accepted** row kept for history; **revoked** rows removed by retention — REQ-INV-006), redirects to **Login** with message **`Account activated. Sign in with your new password.`**
- When **external providers enabled** is **true**, the screen also shows **Continue with {Display name}** actions (same layout as **Login** and **Register**): an **or** divider and one button per enabled provider. These start the same OIDC flow as guest external sign-in (REQ-AUTH-014); they do not require the invitation token. External sign-in actions are shown only when the invitation link preview is **valid**; invalid or expired links show the error message only.
- If **Email verified** is not already true, sets **Email verified** true and **Email verified at** (mailbox proof via invitation link). Admin-invited users are already verified when the invitation was sent (REQ-INV-001).
- Sends **Password reset completed** email (same template as password set confirmation).

### Accept invitation via external sign-in

When **external providers enabled** is **true** and the user is **awaiting invitation acceptance**:

- On **Login** and **Accept invitation**, the user may choose **Continue with {Display name}** instead of setting a local password (email link) or signing in with email and password.
- The administrator invited a **specific email address**; external sign-in onboarding is allowed **only** when the provider returns a **verified email** that **exactly matches** the invited account **Profile email**. This is the **only** guest external sign-in path that links a provider to an existing account without a prior ChangeMe session (REQ-AUTH-014).
- When the verified provider email matches the invited account, the system **completes the invitation** in one step:
  - links the external provider to the account;
  - does **not** require a local password — the account becomes **external-only**;
  - marks the pending **account invitation** as accepted and clears **Pending invitation** (**accepted** row kept for history; **revoked** rows removed by retention — REQ-INV-006);
  - invalidates unused invitation tokens for that user;
  - applies **First name** and **Last name** from the provider when the administrator left both empty and the provider supplies both; otherwise keeps administrator-entered values;
  - signs the user in (subject to deactivation, two-factor, and other gates per REQ-AUTH-013 and REQ-AUTH-014).
- Sends **External account linked** email (REQ-AUTH-007) to the invited **Profile email**.
- If the provider email does **not** match the invited **Profile email**, is not verified, or external sign-in is disabled: do **not** complete the invitation; redirect to **Login** or **Accept invitation** with form-level error **`The external account email does not match the invited email address.`**

### Business rules (all invitation paths)

- Invitation link is valid for **72 hours** by default (configurable in deployment settings).
- Until invitation acceptance completes, the user cannot sign in with email and password (REQ-AUTH-001).
- When the administrator left **First name** and **Last name** empty, the user must supply both on **Accept invitation** (email link). When accepting via external sign-in, both names are taken from the provider when available; otherwise the user may complete the profile later on **Edit profile**.
- When the administrator set one or both names, those values are kept unless the user edits them on **Accept invitation** (email link).
- When email verification is enabled, invitation acceptance (either path) satisfies **Email verified**; the email-link path does not automatically sign the user in; the external path signs the user in immediately on success.

---
