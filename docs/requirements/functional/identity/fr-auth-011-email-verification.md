---
id: FR-AUTH-011
title: Email Verification
domain: identity
type: functional
status: active
depends_on: [FR-AUTH-001, FR-AUTH-006, FR-AUTH-009, FR-AUTH-010, FR-AUTH-015, FR-INV-001, FR-ROL-006, FR-USR-005]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

When email verification is enabled in deployment settings, users must prove control of their email address before using the application. Administrators can manually confirm a user's email when needed.

## Functional requirements

### Email verification policy

- Deployment settings include **Email verification enabled**; default **false**.
- When **Email verification enabled** is **false**, every account is treated as **Email verified** true (including new registrations and the initial administrator).
- When **Email verification enabled** is **true**, each account has **Email verified** (true or false) and optional **Email verified at** (date and time when verification last succeeded).
- **Email verification link lifetime (hours)** applies when verification is enabled; default **72**.

### Sign-in gate

- A user with **Deactivated** false and **Email verified** false cannot sign in when verification is enabled (FR-AUTH-001).
- Password reset (FR-AUTH-006) and forgot password remain available for unverified users so they can recover access to the mailbox and complete verification.

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
| **Register** (FR-AUTH-001)                  | No session; **Verify email** screen; verification email sent                                                   |
| **Accept invitation** (FR-AUTH-010)         | **Email verified** on success (email link or external sign-in)                                                 |
| **Admin invite user** (FR-INV-001)          | **Email verified** true when invitation is sent to the user's email                                            |
| **Initial administrator** (FR-ROL-006)      | **Email verified** true at creation                                                                            |
| **Password expiration** (FR-AUTH-009)       | Evaluated only after a successful sign-in; sign-in requires **Email verified** true first                      |
| **Deactivated** (FR-USR-005)                | **Deactivated** true blocks sign-in regardless of verification                                                 |
| **Self-service email change** (FR-AUTH-015) | **Current email** remains for sign-in until confirmation; successful confirmation sets **Email verified** true |

### States and business rules

- Email verification status and whether the user **has a local password** are independent of deactivation; an **enabled** account may still be unverified or **awaiting invitation acceptance**.
- Assignable-user lists (FR-USR-005) include only users with **Deactivated** false; when verification is enabled they must also be verified and have a password set.
- Self-service email change follows FR-AUTH-015; completing a pending change sets **Email verified** true and **Email verified at** to the confirmation time regardless of the global **Email verification enabled** flag.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-AUTH-011-01 | **Email verification enabled** is **true**; user with **Deactivated** false and **Email verified** false | User attempts sign-in on **Login** (FR-AUTH-001) | Sign-in blocked with form-level error `Verify your email before signing in.` |
| AC-AUTH-011-02 | **Email verification enabled** is **false** | Any account is created or evaluated | Account treated as **Email verified** true (including new registrations and initial administrator) |
| AC-AUTH-011-03 | Guest opens valid verification link from email (token in URL) | Link is processed | **Email verified** set true; **Email verified at** set; token invalidated; redirected to **Login** with message `Email verified. You can sign in now.` |
| AC-AUTH-011-04 | Guest opens invalid or expired verification link | User views **Verify email** | Error `This verification link is invalid or has expired.` with **Resend verification email** on the same screen |
| AC-AUTH-011-05 | Guest on **Verify email** after registration when verification enabled | User submits **Resend verification email** with valid **Email** | Message `If an unverified account exists for this email, a verification link has been sent.` (does not reveal whether account exists) |
| AC-AUTH-011-06 | **Email verification enabled** is **true**; user with unverified mailbox | User uses **Forgot password** (FR-AUTH-006) | Flow remains available so user can recover mailbox access and complete verification |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
