---
id: FR-AUTH-015
title: Self-Service Email Change
domain: identity
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-006,
    FR-AUTH-007,
    FR-AUTH-010,
    FR-AUTH-011,
    FR-AUTH-013,
    FR-AUTH-014,
    FR-PKY-001,
    FR-USR-001,
    FR-USR-003,
    FR-USR-004,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The signed-in user must be able to change the email address on their account by confirming control of the new mailbox. The current email address remains active for sign-in until the change is confirmed.

## Functional requirements

### Deployment policy

- Deployment settings include **Self-service email change enabled** (`AuthOptions:EmailChange:Enabled`); default **true**.
- When **Self-service email change enabled** is **false**, **Change email** is hidden on **My account**, APIs to start a new email change are unavailable, and direct navigation to **Change email** redirects to **My account** with no new request allowed.
- When **Self-service email change enabled** is **false** and a **pending email change** already exists, **My account** still shows the **Pending email change** panel (resend, cancel) until the pending change is cleared or confirmed.
- Administrator **Edit user** email change (FR-USR-003) is unaffected by this flag.

### Business terms

| Term                     | Meaning                                                                                                                                                                                                      |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Pending email change** | A self-service request to replace the current email with a new address. The new address is stored as pending until the user opens the confirmation link sent to that address or an administrator cancels it. |
| **Current email**        | The account **Profile email** used for sign-in, display, and notifications until a pending change is confirmed or cancelled (FR-AUTH-014).                                                                   |
| **New email**            | The target address in a **pending email change**; not used for sign-in until confirmation succeeds.                                                                                                          |

### My account — entry and pending state

- Header action **Change email** on **My account** (FR-USR-001) opens **Change email**.
- **Change email** is **not shown** when:
  - **Self-service email change enabled** is **false**;
  - the user is **awaiting invitation acceptance**;
  - a **pending email change** already exists.
- When a **pending email change** exists, **My account** shows a **Pending email change** panel above the profile summary with:
  - read-only line **`New email: {new email}`**;
  - read-only line **`Requested at: {date and time}`**;
  - message **`Sign in with your current email until you confirm the change from the new mailbox.`**
  - action **Resend confirmation email**;
  - action **Cancel email change** (requires **Sensitive account actions** step-up authentication — FR-AUTH-013).
- **Resend confirmation email** sends a new **Confirm email change** message to the **new email** (FR-AUTH-007), invalidates the previous unused confirmation link for this pending change, and shows message **`If the pending change is still valid, a new confirmation link has been sent to the new email address.`**
- **Cancel email change** opens confirmation dialog **`Cancel the pending email change to "{new email}"? Your current email will stay unchanged.`** On confirm: clears the pending change, sends **Email change cancelled** to the **current email** (FR-AUTH-007), shows message **`Email change cancelled.`**, and refreshes **My account** in place.

### Change email screen

- Screen: **Change email**
- Linked from **My account** via header action **Change email**; **Back to my account** at the top.
- Only authenticated users with **Deactivated** false who are **not** **awaiting invitation acceptance** and have **no pending email change** can open this screen.

| Field             | Behavior                                                                                                  |
| ----------------- | --------------------------------------------------------------------------------------------------------- |
| **New email**     | Text field, **required**; valid email format; max **320** characters; must differ from **current email**. |
| **Current email** | Read-only; shows the signed-in user's **current email**.                                                  |

- When **External providers enabled** is **true** and the user has at least one **External login** row, show persistent notice: **`External sign-in methods stay linked. Notifications stay on your profile email ({current email}). Provider addresses may differ.`**

### Step-up before submit

- **Change email** button opens the **Sensitive account actions** step-up dialog (FR-AUTH-013) before the request is submitted.
- Step-up collects **Current password** when the user **has a local password**, **TOTP** or **recovery code** when **two-factor enrolled**, and **step-up external sign-in** when the user is **external-only**.
- When step-up succeeds, the system validates **New email** and creates the **pending email change**.

### Validation (change email)

- **New email** identical to **current email** shows inline field error: **`New email must differ from your current email.`**
- Duplicate **new email** (already used as **Profile email** by another account) shows form-level error: **`An account with this email already exists.`**
- Another user's **pending email change** to the same address does **not** block this request or **public registration** / **Invite user**; only a confirmed **Profile email** on another account counts as taken at submit time.
- Required and format errors are inline on **New email**.

### Submit success

- On successful submit after step-up:
  - create **pending email change** with **new email** and **requested at** (current date and time);
  - send **Confirm email change** to the **new email** (FR-AUTH-007);
  - send **Email change requested** to the **current email** (FR-AUTH-007);
  - show message **`Check the new email address for a confirmation link.`**;
  - navigate to **My account**.
- Submit does **not** change **current email**, **Email verified**, or active sessions.

### Confirmation link lifetime

- **Email change confirmation link lifetime (hours)** applies to self-service email change; default **72** (same default as **Email verification link lifetime (hours)** in FR-AUTH-011).
- Each **Resend confirmation email** issues a new link and invalidates previous unused links for the same pending change.

### Confirm email change screen (guest)

- Screen: **Confirm email change**; available to guests (confirmation token in URL).
- **Back to sign in** at the top → **Login**.

| Outcome                                                                                                                                          | Behavior                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| ------------------------------------------------------------------------------------------------------------------------------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Valid link                                                                                                                                       | Applies the **new email** as **current email**; clears **pending email change**; sets **Email verified** true and **Email verified at** to the current date and time; revokes **all active sessions** for that user; sends **Email change completed** to the **previous current email** and to the **new email** (FR-AUTH-007). **Confirm email change** shows a **success** message and success toast; if the user was signed in when opening the link, also states that previous sessions were ended. User stays on the screen until they choose **Sign in now** or **Back to sign in** → **Login** with **`Email changed. Sign in with your new email address.`** (`emailChanged=1` on **Sign in now** only). |
| Valid link, but **new email** is already another account's **Profile email** (for example another user registered while this change was pending) | Does **not** apply the change; **pending email change** remains; form-level error **`An account with this email already exists. Cancel the pending email change on My account.`** with action **Back to sign in** → **Login**. When the requester is signed in and the pending change still exists, also show **Resend confirmation email** (same as on **My account**).                                                                                                                                                                                                                                                                                                                                         |
| Invalid or expired                                                                                                                               | Shows form-level error **`This confirmation link is invalid or has expired.`** with action **Back to sign in** → **Login**. When the user is signed in and a pending change still exists, also show **Resend confirmation email** (same behavior as on **My account**).                                                                                                                                                                                                                                                                                                                                                                                                                                          |

- Opening a valid confirmation link while signed in as a **different** user shows error **`This confirmation link belongs to another account. Sign out and open the link again, or sign in as the account that requested the change.`** with actions **Sign out** → **Login**, and **Back to my account** when the signed-in user has access to **My account**.

### Form actions (change email)

- **Cancel** button navigates to **My account** without saving.
- **Change email** button: collect step-up (FR-AUTH-013), then validate and submit as above.

### Interaction with other auth flows

| Flow                                 | Behavior                                                                                                                                                          |
| ------------------------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Login** (FR-AUTH-001)              | Sign-in uses **current email** until confirmation; **new email** cannot sign in while the change is pending.                                                      |
| **Register** (FR-AUTH-001)           | May create an account whose **Profile email** equals another user's **pending new email**; that pending change is unchanged until confirm or cancel.              |
| **Forgot password** (FR-AUTH-006)    | Available for **current email** while a change is pending.                                                                                                        |
| **Email verification** (FR-AUTH-011) | Pending change does not alter **Email verified** on **current email** until confirmation; after confirmation, **Email verified** is true on the **new email**.    |
| **Passkeys** (FR-PKY-001)            | Passkey credentials remain on the account; discoverable passkey sign-in is unchanged. Non-discoverable passkey sign-in uses **current email** until confirmation. |
| **External providers** (FR-AUTH-014) | **External login** rows retained; **Profile email** is notification destination; provider emails may differ.                                                      |
| **Admin edit user** (FR-USR-003)     | Administrator email change cancels any **pending email change**, applies immediately, and follows FR-USR-003 admin email rules.                                   |
| **Admin user details** (FR-USR-004)  | Shows **Pending email change** when present; administrator may **Cancel pending email change**.                                                                   |

### States and business rules

- Only one **pending email change** per user at a time.
- **Pending new email** is not a **Profile email** and does not reserve that address for other accounts until confirmation succeeds.
- Immediately before applying a valid confirmation link, the system checks that no **other** account already uses the **new email** as **Profile email**.
- Users **awaiting invitation acceptance** cannot start self-service email change; they complete onboarding first (FR-AUTH-010).
- Confirmation success revokes **every** active session; the user must sign in again with the **new Profile email** (or a linked external provider / passkey per existing rules).
- **Out of scope:** changing email from **Edit profile**; administrator-initiated email change except cancellation of a pending change (FR-USR-003, FR-USR-004); changing email without confirmation of the new mailbox on self-service paths.

## Acceptance scenarios

| ID             | Given                                                                                                                                                                 | When                                                                                                                                                                    | Then                                                                                                                                                                                                                                                                                                                                    |
| -------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| AC-AUTH-015-01 | Signed-in user on **My account** (FR-USR-001); **Self-service email change enabled** is **true**; not **awaiting invitation acceptance**; no **pending email change** | User clicks header **Change email**                                                                                                                                     | **Change email** screen opens with **Back to my account** at top                                                                                                                                                                                                                                                                        |
| AC-AUTH-015-02 | Signed-in user **awaiting invitation acceptance** (FR-AUTH-010)                                                                                                       | User views **My account** header actions                                                                                                                                | **Change email** is **not shown**                                                                                                                                                                                                                                                                                                       |
| AC-AUTH-015-03 | Signed-in user with existing **pending email change**                                                                                                                 | User views **My account**                                                                                                                                               | **Pending email change** panel shown above profile summary with **New email**, **Requested at**, guidance, **Resend confirmation email**, and **Cancel email change**; header **Change email** hidden                                                                                                                                   |
| AC-AUTH-015-04 | Signed-in user on **Change email** with valid **New email** differing from **current email**                                                                          | User clicks **Change email** and completes **Sensitive account actions** step-up (FR-AUTH-013)                                                                          | **Pending email change** created; **Confirm email change** sent to **new email**; **Email change requested** sent to **current email** (FR-AUTH-007); message `Check the new email address for a confirmation link.`; navigates to **My account**                                                                                       |
| AC-AUTH-015-05 | Signed-in user on **Change email**; **New email** identical to **current email**                                                                                      | User submits the form                                                                                                                                                   | Inline field error `New email must differ from your current email.`                                                                                                                                                                                                                                                                     |
| AC-AUTH-015-06 | Signed-in user on **Change email**; **New email** already used as another account's **Profile email**                                                                 | User submits after step-up                                                                                                                                              | Form-level error `An account with this email already exists.`                                                                                                                                                                                                                                                                           |
| AC-AUTH-015-07 | Signed-in user on **My account** with **pending email change**                                                                                                        | User clicks **Cancel email change**, completes step-up, and confirms dialog `Cancel the pending email change to "{new email}"? Your current email will stay unchanged.` | Pending change cleared; **Email change cancelled** sent to **current email** (FR-AUTH-007); message `Email change cancelled.`; **My account** refreshes in place                                                                                                                                                                        |
| AC-AUTH-015-08 | Guest on **Confirm email change** with valid confirmation token                                                                                                       | Link is processed                                                                                                                                                       | **New email** becomes **Profile email**; pending change cleared; **Email verified** true; all sessions revoked; **Email change completed** sent to previous and new email (FR-AUTH-007); success shown until user chooses **Sign in now** or **Back to sign in** → **Login** with `Email changed. Sign in with your new email address.` |
| AC-AUTH-015-09 | Guest on **Confirm email change**; valid token but **new email** is now another account's **Profile email**                                                           | Link is processed                                                                                                                                                       | Change **not** applied; pending change remains; form-level error `An account with this email already exists. Cancel the pending email change on My account.`                                                                                                                                                                            |
| AC-AUTH-015-10 | **Self-service email change enabled** is **false**; no pending change                                                                                                 | Signed-in user navigates to **Change email** directly                                                                                                                   | Redirected to **My account**; new change cannot be started                                                                                                                                                                                                                                                                              |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
