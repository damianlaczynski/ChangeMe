---
id: REQ-AUTH-011
title: Email Verification
domain: identity
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-006, REQ-AUTH-009, REQ-AUTH-010, REQ-AUTH-015, REQ-INV-001, REQ-ROL-006, REQ-USR-005]
---
## Goal

When email verification is enabled in deployment settings, users must prove control of their email address before using the application. Administrators can manually confirm a user's email when needed.

## Features

### Email verification policy

- Deployment settings include **Email verification enabled**; default **false**.
- When **Email verification enabled** is **false**, every account is treated as **Email verified** true (including new registrations and the initial administrator).
- When **Email verification enabled** is **true**, each account has **Email verified** (true or false) and optional **Email verified at** (date and time when verification last succeeded).
- **Email verification link lifetime (hours)** applies when verification is enabled; default **72**.

### Sign-in gate

- A user with **Deactivated** false and **Email verified** false cannot sign in when verification is enabled (REQ-AUTH-001).
- Password reset (REQ-AUTH-006) and forgot password remain available for unverified users so they can recover access to the mailbox and complete verification.

### Verify email screen (guest)

- Screen: **Verify email**; available to guests.
- Shown after registration when verification is enabled, and reachable from **Resend verification email** on **Login**.
- **Back to sign in** at the top → **Login**.

| Field / element               | Behavior                                                                                                                                                              |
| ----------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Email**                     | **Required** when resending; valid email; max **320** characters. Pre-filled when the user arrives from registration (read-only) or empty when opened from **Login**. |
| **Resend verification email** | Sends a new link if an unverified account exists; always shows **`If an unverified account exists for this email, a verification link has been sent.`**               |

- Opening a valid verification link from email (guest, token in URL) sets **Email verified** true and **Email verified at**, invalidates the token, and redirects to **Login** with message **`Email verified. You can sign in now.`**
- Invalid or expired link shows: **`This verification link is invalid or has expired.`** with **Resend verification email** on the same screen.

### Interaction with other auth flows

| Flow                                         | When verification enabled                                                                                      |
| -------------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| **Register** (REQ-AUTH-001)                  | No session; **Verify email** screen; verification email sent                                                   |
| **Accept invitation** (REQ-AUTH-010)         | **Email verified** on success (email link or external sign-in)                                                 |
| **Admin invite user** (REQ-INV-001)          | **Email verified** true when invitation is sent to the user's email                                            |
| **Initial administrator** (REQ-ROL-006)      | **Email verified** true at creation                                                                            |
| **Password expiration** (REQ-AUTH-009)       | Evaluated only after a successful sign-in; sign-in requires **Email verified** true first                      |
| **Deactivated** (REQ-USR-005)                | **Deactivated** true blocks sign-in regardless of verification                                                 |
| **Self-service email change** (REQ-AUTH-015) | **Current email** remains for sign-in until confirmation; successful confirmation sets **Email verified** true |

### States and business rules

- Email verification status and whether the user **has a local password** are independent of deactivation; an **enabled** account may still be unverified or **awaiting invitation acceptance**.
- Assignable-user lists (REQ-USR-005) include only users with **Deactivated** false; when verification is enabled they must also be verified and have a password set.
- Self-service email change follows REQ-AUTH-015; completing a pending change sets **Email verified** true and **Email verified at** to the confirmation time regardless of the global **Email verification enabled** flag.

---
