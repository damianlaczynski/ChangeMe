# Requirements - Passkeys

This document covers seven REQs for the **Passkeys** area:
deployment policy for WebAuthn passkeys, guest sign-in with passkeys, signed-in enrollment and credential management, step-up authentication for sensitive actions, administrator oversight and reset, combined compliance gates with existing auth flows, and passkey-related notification emails.

**Passkeys** (FIDO2 / WebAuthn credentials) are an optional authentication method alongside email and password, two-factor authentication (REQ-AUTH-013), and external identity providers (REQ-AUTH-014). They do not replace deployment configuration of those features; they integrate with the same session model (REQ-AUTH-001, REQ-AUTH-002), compliance gates (REQ-AUTH-009, REQ-AUTH-013), and account lifecycle rules (REQ-AUTH-010, REQ-AUTH-011, REQ-AUTH-012, invitations, users).

Account lifecycle terms (**local password**, **external-only account**, **awaiting invitation acceptance**, **two-factor enrolled**) are defined in `docs/req/users-requirements.md` (**Business terms**) and `docs/req/invitations-requirements.md`.

---

# REQ-PKY-001: Passkeys Policy and Deployment

## Goal

Deployments must be able to enable passkey authentication, optionally require every active account to register at least one passkey, and control how passkeys interact with two-factor authentication and password-based sign-in.

## Features

### Passkeys authentication policy

| Deployment setting                        | Default   | Meaning                                                                                                                                                                                                                                                       |
| ----------------------------------------- | --------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Passkeys authentication enabled**       | **false** | Master switch. When **false**, passkey UI, enrollment, and passkey sign-in are unavailable; stored credentials remain inactive until re-enabled.                                                                                                              |
| **Passkeys authentication required**      | **false** | When **true** (and passkeys enabled), every active account must have at least one registered passkey before using the application, except users **awaiting invitation acceptance**.                                                                           |
| **Passkey satisfies two-factor**          | **false** | When **true** (and passkeys and two-factor both enabled), a passkey assertion with **user verification** satisfies **Two-factor verification** and **strict two-factor setup** on that sign-in path.                                                          |
| **Allow passkey-only accounts**           | **false** | When **true**, a user with at least one passkey and **no local password** may sign in with passkeys only (see **Sign-in method eligibility**). When **false**, passkey sign-in requires a local password or linked external provider in addition to passkeys. |
| **Discoverable passkey sign-in on Login** | **true**  | When **true**, **Login** offers **Sign in with a passkey** without entering email first (resident / discoverable credentials). When **false**, the user must enter **Email** before passkey sign-in.                                                          |

- Deployment settings include **Relying party id** (default: derived from **Frontend base URL** host, e.g. `localhost` for local dev, production hostname in production); **Relying party display name** (default **`ChangeMe`**).
- Deployment settings include **Maximum passkeys per user**; default **10**.
- Deployment settings include **Passkey challenge lifetime**; default **5 minutes** for registration, authentication, and step-up ceremonies.
- Deployment settings include **User verification** requirement for ceremonies; default **required** (authenticator must perform user verification — PIN, biometrics, or device password).
- Deployment settings include **Allowed authenticator attachment**; default **any** (platform and cross-platform security keys). Values: **platform**, **cross-platform**, **any**.
- Deployment settings include **Attestation conveyance** for registration; default **none** (privacy-preserving). Values: **none**, **indirect**, **direct** (enterprise deployments may use **direct** for inventory policies).
- The public auth settings response exposes **passkeys authentication enabled**, **passkeys authentication required**, **passkey satisfies two-factor**, **discoverable passkey sign-in on Login**, **relying party id**, **relying party display name**, and **maximum passkeys per user** — never private signing keys or credential secrets.

### Sign-in method eligibility

A user may use passkey sign-in when **Passkeys authentication enabled** is **true** and at least one of the following holds:

| Condition                                               | Passkey sign-in allowed when **Allow passkey-only accounts** is **false** | Passkey sign-in allowed when **Allow passkey-only accounts** is **true** |
| ------------------------------------------------------- | ------------------------------------------------------------------------- | ------------------------------------------------------------------------ |
| User has **local password** and ≥1 passkey              | **Yes**                                                                   | **Yes**                                                                  |
| User is **external-only** with ≥1 passkey               | **Yes** (external sign-in remains available per REQ-AUTH-014)             | **Yes**                                                                  |
| User has only passkeys (no password, no external login) | **No**                                                                    | **Yes**                                                                  |
| User is **awaiting invitation acceptance**              | **No** (complete invitation first)                                        | **No**                                                                   |
| User has zero passkeys                                  | **No** (enrollment only while signed in)                                  | **No**                                                                   |

- **Passkey-only account** means the user has at least one passkey, **no local password**, and **no external login** rows. Such accounts are allowed only when **Allow passkey-only accounts** is **true**.
- Registering the first passkey does **not** remove an existing **local password** or **external login** unless the user explicitly removes those methods per REQ-AUTH-005, REQ-AUTH-014, REQ-PKY-003.

### Passkey satisfies two-factor

- When **Passkey satisfies two-factor** is **false**, passkey sign-in counts as **primary authentication** only (same role as password or external provider). If **Two-factor enabled** is **true**, the user proceeds to **Two-factor verification** after a successful passkey sign-in unless another gate applies first.
- When **Passkey satisfies two-factor** is **true** and the passkey assertion includes **user verification**:
  - Skip **Two-factor verification** for that sign-in when **Two-factor enabled** is **true**.
  - Skip **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**.
- When **Passkey satisfies two-factor** is **true** but the assertion does **not** include user verification, passkey sign-in counts as **primary authentication** only and **normal two-factor rules apply** (proceed to **Two-factor verification** when **Two-factor enabled** is **true** on the account, same as when **Passkey satisfies two-factor** is **false**).
- **Passkey satisfies two-factor** never disables stored TOTP enrollment; password sign-in continues to require app TOTP when **Two-factor enabled** is **true** (REQ-AUTH-013).

### Deployment policy changes

Deployment settings are read on each sign-in, session refresh, and authenticated request (same model as REQ-AUTH-009 and REQ-AUTH-013).

| Setting change                                                               | Effect                                                                                                                                                                                       |
| ---------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Passkeys authentication enabled** **false** → **true**, required **false** | Passkeys become available on **My account** and **Login** per policy; no forced enrollment.                                                                                                  |
| **Passkeys authentication required** turned **on**                           | Users without a passkey (except **awaiting invitation acceptance**) receive **`passkeySetupRequired`** on next refresh or blocked API; client enters **strict passkey setup** (REQ-PKY-006). |
| **Passkeys authentication enabled** **true** → **false**                     | Enforcement stops; credentials remain stored but inactive; **strict passkey setup** ends immediately.                                                                                        |
| **Passkey satisfies two-factor** toggled                                     | Affects the next passkey sign-in only.                                                                                                                                                       |
| **Allow passkey-only accounts** **true** → **false**                         | Users who are passkey-only cannot sign in until they **Set password** (REQ-AUTH-014) or link an external provider; existing passkeys remain on the account.                                  |

### States and business rules

- Disabling passkeys deployment-wide does not delete stored credentials; re-enabling restores usability.
- **Out of scope for this REQ:** admin UI to change passkey deployment flags at runtime; per-role passkey requirement; hardware security key inventory UI beyond credential list; syncing passkeys across browsers without per-browser enrollment.

---

# REQ-PKY-002: Sign-In with Passkey

## Goal

When passkeys are enabled, guests must be able to sign in using a registered passkey instead of (or before) email and password, subject to the same account gates as other primary authentication methods.

## Features

### Login screen — passkey entry

- When **Passkeys authentication enabled** is **true**, **Login** shows action **Sign in with a passkey** below the email and password form (above external provider buttons when present).
- When **Discoverable passkey sign-in on Login** is **true**, **Sign in with a passkey** starts discoverable authentication (no email required).
- When **Discoverable passkey sign-in on Login** is **false**, the user must enter **Email** first; **Sign in with a passkey** is enabled only when **Email** is a valid format; the ceremony uses allow-list credentials for that account only.
- **Sign in with a passkey** is disabled with inline hint **`Enter your email to use a passkey.`** when discoverable sign-in is **false** and **Email** is empty or invalid.

### Passkey sign-in ceremony (guest)

1. User activates **Sign in with a passkey**.
2. The system issues a short-lived **authentication challenge** bound to the browser session (or email when non-discoverable).
3. The browser displays the platform or security-key UI; the user completes verification on the authenticator.
4. The system validates the assertion, resolves the credential to exactly one user, and continues the sign-in decision tree (below).

- On unsupported browser: form-level error **`Passkeys are not supported in this browser. Use email and password or try another browser.`**
- On user cancel: remain on **Login** with no error (ceremony aborted).
- On invalid or expired challenge: form-level error **`Passkey sign-in timed out. Try again.`**
- On unknown credential: form-level error **`No passkey matched. Sign in with email and password or use a different passkey.`** (does not reveal whether an email exists when discoverable flow was used).
- On credential linked to **deactivated** account: **`This account has been deactivated. Contact an administrator.`**
- On **awaiting invitation acceptance**: **`Complete your account setup using the invitation link sent to your email.`**
- On email verification **enabled** and mailbox **not verified**: **`Verify your email before signing in.`** (same as REQ-AUTH-001).
- On **Passkeys authentication enabled** but user has no passkeys: **`No passkey is registered for this account.`** when email was supplied; discoverable flow uses the generic no-match message above.
- On **Allow passkey-only accounts** **false** and user would be passkey-only: **`Set a password or use external sign-in before using passkeys on this account.`**

### Post-passkey primary authentication

After successful passkey primary authentication, evaluate gates in **Combined account compliance gates** (REQ-PKY-006):

1. **Required password change** when the user **has a local password** and password expiration applies (REQ-AUTH-009).
2. **Two-factor verification** when **Two-factor enabled** is **true** and **Passkey satisfies two-factor** did not apply on this assertion.
3. **Strict two-factor setup** when required and not satisfied (REQ-AUTH-013).
4. **Strict passkey setup** when **Passkeys authentication required** is **true** and the user has zero passkeys (should not occur after successful passkey sign-in; included for completeness).
5. **Issues list** (full session) when no gate applies.

- Passkey sign-in creates a **new session** per REQ-AUTH-001 when a full session is issued; records **signed in at**, **device / browser label**, **IP address**, and **Sign-in method** badge **`Passkey`** on session list (REQ-AUTH-004, REQ-USR-004).
- Failed passkey verification attempts per challenge: maximum **5**; after **5** failures, invalidate the challenge and show **`Too many passkey attempts. Try again.`**

### Interaction with other Login methods

| Other method                        | Coexistence on **Login**                                                                                        |
| ----------------------------------- | --------------------------------------------------------------------------------------------------------------- |
| Email and password (REQ-AUTH-001)   | Always available when the user is eligible for password sign-in.                                                |
| External providers (REQ-AUTH-014)   | Shown when enabled; independent of passkeys.                                                                    |
| **Forgot password?** (REQ-AUTH-006) | Available; irrelevant for passkey-only users until they set a password.                                         |
| **Register** (REQ-AUTH-012)         | Unchanged; new self-registered users do not receive passkeys until they enroll on **My account** after sign-in. |

### Accept invitation and Register

- **Accept invitation** (REQ-AUTH-010) does **not** register a passkey automatically; after successful acceptance the user may add passkeys on **My account** when signed in.
- After successful **Register** when email verification is disabled, the success path may show optional prompt **`Add a passkey for faster sign-in`** with action **Add passkey now** → **Add passkey** flow (REQ-PKY-003) before navigating to **Issues list**; declining navigates to **Issues list** without enrollment.
- When email verification is enabled, passkey enrollment prompt appears on first successful sign-in after **Verify email**, not on **Verify email** itself.

### States and business rules

- Passkey sign-in never bypasses **Deactivated**, **awaiting invitation acceptance**, or unverified-email rules.
- Passkey sign-in does not create an account for unknown credentials (no self-registration via passkey).
- **Out of scope for this REQ:** passwordless username enumeration beyond existing email-first flows; NFC-specific UX; conditional UI autofill (may be added later without changing security rules).

---

# REQ-PKY-003: Passkey Enrollment and My Account Management

## Goal

Signed-in users must be able to register, name, and remove passkeys on their account when the feature is enabled, without compromising the rule that every account retains at least one viable sign-in method.

## Features

### Account model

| Concept                   | Shown in UI              | Meaning                                                                                                                                  |
| ------------------------- | ------------------------ | ---------------------------------------------------------------------------------------------------------------------------------------- |
| **Passkey credential**    | Row in **Passkeys** list | One WebAuthn credential bound to the user: **Name**, **Created at**, **Last used at**, **Authenticator type** (platform / security key). |
| **Passkey credential id** | Not shown                | Stable identifier used server-side; never displayed.                                                                                     |
| **Backup eligible**       | Badge when applicable    | Indicates the credential may be synced by the platform (e.g. iCloud Keychain, Google Password Manager) — informational only.             |
| **Backup state**          | Badge when applicable    | Indicates whether the credential is currently backed up — informational only.                                                            |

- Each user may have at most **Maximum passkeys per user** credentials.
- Credential **Name** defaults to **`Passkey`** plus sequence on first registration (e.g. **`Passkey 1`**); the user may rename.

### My account — Passkeys section

- Collapsible section **Passkeys** on **My account** (REQ-USR-001); shown only when **Passkeys authentication enabled** is **true**; default **collapsed**.
- Section lists all **Passkey credential** rows: **Name** (editable inline or via rename dialog), **Created at**, **Last used at**, **Authenticator type**, **Remove** action.
- Empty state: **`No passkeys registered.`** and description **`Passkeys let you sign in with your device PIN, fingerprint, or face.`**
- Primary action **Add passkey** opens **Add passkey** flow.

### Add passkey flow

- **Add passkey** requires an active full session (not enrollment bootstrap for two-factor or passkey only).
- Before starting the WebAuthn registration ceremony, the user must complete **Sensitive account actions** step-up (REQ-PKY-004) when any step-up rule applies.
- The system issues a **registration challenge**; the browser creates the credential; the server verifies attestation per deployment **Attestation conveyance** and stores the **Passkey credential**.
- After success, show dialog **Name your passkey** with field **Name** (**required**; max **100** characters; default as above); **Save** persists the name and shows message **`Passkey added.`**
- Sends **Passkey added** email (REQ-PKY-007).
- When the user already has **Maximum passkeys per user** credentials, **Add passkey** is disabled with message **`Maximum number of passkeys reached. Remove one before adding another.`**

### Rename passkey

- **Rename** action on each row; requires **Sensitive account actions** step-up (REQ-PKY-004).
- **Name** max **100** characters; on success message **`Passkey renamed.`**

### Remove passkey

- **Remove** opens confirmation **`Remove passkey "{name}" from your account?`**
- Requires **Sensitive account actions** step-up (REQ-PKY-004).
- **Remove** is blocked when it would leave the user with **no sign-in method**:
  - **No local password**, **no external login**, and this is the **only** passkey → message **`Add a password or external sign-in before removing your only pass-in method.`** with links to **Set password** (REQ-AUTH-014) when applicable and external linking when enabled.
  - When **Passkeys authentication required** is **true** and this is the **only** passkey but other sign-in methods exist → message **`At least one passkey is required. Add another passkey before removing this one.`**
- On success: message **`Passkey removed.`**; sends **Passkey removed** email (REQ-PKY-007).

### Strict passkey setup (signed-in enrollment)

- When **Passkeys authentication required** is **true** and the user has zero passkeys, **strict passkey setup** (REQ-PKY-006) uses the same **Add passkey** ceremony without step-up (first credential).
- After the first passkey is registered during strict setup, the application opens **Issues list** subject to remaining gates (two-factor, password expiration).

### States and business rules

- Adding a passkey does not disable two-factor, change password, or unlink external providers.
- Removing all passkeys while signed in is allowed only when another sign-in method remains and passkeys are not required.
- **Out of scope for this REQ:** administrator registering a passkey on behalf of another user; exporting private keys; shared team passkeys.

---

# REQ-PKY-004: Step-Up Authentication with Passkeys

## Goal

Sensitive account actions must accept a recent passkey assertion as step-up proof, consistent with password and two-factor step-up rules.

## Features

### Passkey step-up

- A **passkey step-up** is a successful authentication ceremony (allow-listed credentials for the signed-in user only) completed within **15 minutes** before the sensitive action.
- The server stores **passkey step-up completed at** per user (single timestamp; any registered passkey satisfies step-up).

### Sensitive account actions (updated)

The following actions require **step-up authentication** per REQ-AUTH-013, extended with passkey step-up:

| Action                                | Where               | Step-up options (all that apply must succeed)                                                                                                                                                            |
| ------------------------------------- | ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Disable two-factor authentication** | **My account**      | Password (if set) + TOTP/recovery **or** recent passkey step-up + TOTP/recovery when **Two-factor enabled**                                                                                              |
| **Regenerate recovery codes**         | **My account**      | Same as above                                                                                                                                                                                            |
| **Link {Display name}**               | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Unlink** external provider          | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Set password**                      | **My account**      | REQ-AUTH-014 + step-up rules                                                                                                                                                                             |
| **Add passkey**                       | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Rename passkey**                    | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Remove passkey**                    | **My account**      | REQ-PKY-003                                                                                                                                                                                              |
| **Change password**                   | **Change password** | REQ-AUTH-005 — **Current password** **or** passkey step-up when user has passkeys and no password is set is **not** allowed; password change always requires **Current password** when a password exists |

Passkey step-up rules:

| User state                                            | Passkey step-up alone sufficient?                                                                                                                                                                      |
| ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| Has passkeys; **Two-factor enabled** **false**        | **Yes** — replaces password requirement for external-only-style step-up.                                                                                                                               |
| Has passkeys; **Two-factor enabled** **true**         | **No** — passkey step-up **plus** valid **TOTP** or **recovery code** required (same as password + TOTP).                                                                                              |
| Has passkeys; action is **Add/Rename/Remove passkey** | For **Add** first passkey during voluntary enrollment: password if set, else passkey step-up not applicable (use password or external step-up per REQ-AUTH-013). Subsequent **Add** uses full step-up. |

- Step-up UI on **My account** and security dialogs offers **Verify with passkey** when the user has at least one passkey and **Passkeys authentication enabled** is **true**.
- **Verify with passkey** runs a WebAuthn authentication ceremony; on success sets **passkey step-up completed at** and returns the user to the pending action.
- Failed step-up: **`Passkey verification failed. Try again.`**; counts toward the same **5**-attempt limit per step-up session as REQ-AUTH-013.

### States and business rules

- Passkey step-up does not issue a new application session; it only unlocks the pending action.
- **Out of scope for this REQ:** step-up for **Accept invitation** external onboarding (invitation email match per REQ-AUTH-010 / REQ-AUTH-014).

---

# REQ-PKY-005: Administrator Passkey Management

## Goal

Administrators must be able to inspect passkeys registered to a user and revoke them when necessary, with the same session-revocation safeguards as other security resets.

## Features

### User details — Passkeys section

- When **Passkeys authentication enabled** is **true**, **User details** (REQ-USR-004) shows collapsible section **Passkeys**; default **collapsed**.
- Read-only table: **Name**, **Created at**, **Last used at**, **Authenticator type**, **Backup eligible**, **Backup state**.
- Empty state: **`No passkeys registered.`**
- Administrators cannot add passkeys for another user (device-bound ceremony).

### Reset all passkeys

- Header action **Reset passkeys** on **User details**; requires **Users.Manage**.
- Shown when **Passkeys authentication enabled** is **true** and the user has at least one **Passkey credential**.
- Confirmation: **`Remove all passkeys for "{full name}"? They will need to register a passkey again if required by policy.`**
- On success: deletes all **Passkey credential** rows for the user; revokes **all active sessions**; success message **`Passkeys reset.`**
- Sends **Passkeys reset by admin** email (REQ-PKY-007).
- When **Passkeys authentication required** is **true**, the user's next sign-in enters **strict passkey setup** (REQ-PKY-006) after primary authentication succeeds.

### Per-credential remove (admin)

- Row action **Remove** on **User details**; requires **Users.Manage**; confirmation **`Remove passkey "{name}" from this account?`**
- Same **no last sign-in method** rules as self-service **Remove passkey** (REQ-PKY-003).
- On success: message **`Passkey removed.`**; does **not** revoke all sessions unless that was the user's only passkey and policy triggers re-auth (session remains valid).

### Interaction with other admin actions

| Admin action (REQ-USR-004) | Interaction with passkeys                                         |
| -------------------------- | ----------------------------------------------------------------- |
| **Reset two-factor**       | Does not remove passkeys.                                         |
| **Deactivate**             | Passkeys remain; user cannot sign in until activated.             |
| **Unlink external**        | Independent; user may still sign in with passkey and/or password. |
| **Send password reset**    | Independent.                                                      |

### States and business rules

- **Out of scope for this REQ:** bulk export of passkey metadata for compliance archives beyond on-screen list; remote wipe of platform passkeys on user devices.

---

# REQ-PKY-006: Combined Compliance Gates and Cross-Auth Interaction

## Goal

Passkey requirements must compose predictably with password expiration, two-factor authentication, external sign-in, invitations, and session compliance flags already defined in Auth requirements.

## Features

### Combined account compliance gates (full order)

When multiple policies apply after **primary authentication** (password, external provider, or passkey per REQ-PKY-002), enforce **one gate at a time** in this order:

1. **Required password change** — **`passwordChangeRequired`** (REQ-AUTH-009); applies when the user **has a local password** and password is expired.
2. **Two-factor verification** — pending challenge (REQ-AUTH-013); skipped when **Passkey satisfies two-factor** applied on a passkey primary sign-in, or **Trust identity provider MFA** on external sign-in (REQ-AUTH-014).
3. **Strict two-factor setup** — **`twoFactorSetupRequired`** (REQ-AUTH-013).
4. **Strict passkey setup** — **`passkeySetupRequired`** when **Passkeys authentication required** is **true** and the user has zero passkeys.
5. **Full session** → **Issues list** (and other app screens).

- When both **`passwordChangeRequired`** and **`passkeySetupRequired`** are **true**, only **`passwordChangeRequired`** is active until the password is updated; **`passkeySetupRequired`** is evaluated immediately after successful required password change on the same sign-in path.
- When both **`twoFactorSetupRequired`** and **`passkeySetupRequired`** are **true**, **`twoFactorSetupRequired`** runs before **`passkeySetupRequired`**.
- Auth responses (sign-in, refresh) include **`passkeySetupRequired`** when enrollment is required under current deployment settings and the user has zero passkeys, analogous to **`twoFactorSetupRequired`** and **`passwordChangeRequired`**.
- The client shows **one** sticky toast for the active gate; passkey toast summary **`Passkey required`** detail **`Add a passkey to continue saving your work to the server.`** action **`Add now`** → **Add passkey** flow.

### Strict passkey setup mode

- Client mode mirrors **strict two-factor setup** (REQ-AUTH-013): minimal chrome; only **Add passkey**, **Logout**, and session refresh until at least one passkey exists.
- Server rejects other application APIs with access denied until the user registers a passkey or policy no longer requires it.
- Users **awaiting invitation acceptance** are exempt until they complete **Accept invitation** (REQ-AUTH-010).

### Primary authentication equivalence

| Method            | Role               | Creates session when gates pass?     |
| ----------------- | ------------------ | ------------------------------------ |
| Email + password  | Primary            | **Yes** (full or bootstrap)          |
| External OIDC     | Primary            | **Yes**                              |
| Passkey assertion | Primary            | **Yes**                              |
| TOTP / recovery   | Second factor only | **No** (completes pending challenge) |
| Passkey (step-up) | Step-up only       | **No**                               |

### External sign-in and passkeys

- External sign-in (REQ-AUTH-014) does not register passkeys automatically.
- A user may have **external login**, **local password**, **passkeys**, and **two-factor** concurrently.
- **Trust identity provider MFA** (REQ-AUTH-013) and **Passkey satisfies two-factor** are independent; both may be **true** in deployment settings.

### Password reset and invitation

- **Reset password** (REQ-AUTH-006) does not remove passkeys.
- **Accept invitation** (REQ-AUTH-010) does not remove passkeys on accounts that already had them (edge case: re-invited email).

### Deactivation and sessions

- Deactivating a user (REQ-USR-005) does not delete passkeys; reactivation preserves credentials.
- **Sign out everywhere** does not remove passkeys.

### REQ-AUTH-013 out of scope update

- WebAuthn / passkeys are **in scope** in this document (`docs/req/passkeys-requirements.md`), not REQ-AUTH-013.
- REQ-AUTH-013 **Out of scope** list must exclude passkeys (cross-reference REQ-PKY-001 through REQ-PKY-007).

### States and business rules

- Initial administrator (REQ-ROL-006) follows the same passkey rules when **Passkeys authentication required** is **true**.
- **Out of scope for this REQ:** per-tenant passkey policy; conditional passkey bypass for break-glass accounts.

---

# REQ-PKY-007: Passkey Notification Emails

## Goal

Users must receive email notification when passkeys are added, removed, or administratively reset, consistent with other auth notification emails.

## Features

### Email types

| Event                       | Trigger                              | Subject line (approximate)          |
| --------------------------- | ------------------------------------ | ----------------------------------- |
| **Passkey added**           | User completes **Add passkey**       | `Passkey added to your account`     |
| **Passkey removed**         | User **Remove passkey** self-service | `Passkey removed from your account` |
| **Passkeys reset by admin** | Administrator **Reset passkeys**     | `Passkeys reset on your account`    |

- Each email includes **account email**, **event time** (UTC), **passkey name** when applicable, and guidance **`If you did not perform this action, contact your administrator immediately.`**
- Emails require working **Email** configuration (same as REQ-AUTH-007).
- Admin per-credential **Remove** sends **Passkey removed** with the credential name.

### Auth notification list (REQ-AUTH-007)

- Extend REQ-AUTH-007 notification catalog with the three rows above when passkeys are implemented.

### States and business rules

- **Out of scope for this REQ:** email on every passkey sign-in (high volume); push notifications.
