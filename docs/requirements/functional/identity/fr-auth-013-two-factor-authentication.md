---
id: FR-AUTH-013
title: Two-Factor Authentication
domain: identity
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

When two-factor authentication is enabled in deployment settings, users must be able to protect their account with a time-based one-time password (TOTP) from an authenticator app. Deployments may require two-factor for every account or allow voluntary opt-in.

## Functional requirements

### Two-factor authentication policy

- Deployment settings include **Two-factor authentication enabled**; default **false**.
- When **Two-factor authentication enabled** is **false**, two-factor authentication is not offered or enforced; existing enrollment data may remain in storage but is inactive until the setting is turned on again (see **Deployment policy changes**).
- When **Two-factor authentication enabled** is **true**, deployment settings include **Two-factor authentication required**; default **false**.
- When **two-factor authentication required** is **enabled**, every active account must enroll in two-factor before using the application, except users **awaiting invitation acceptance** (they complete **Accept invitation** first).
- When **Two-factor authentication required** is **false**, two-factor authentication is **optional**; users enable or disable it on **My account** (FR-USR-001).
- Deployment settings include **Trust identity provider MFA**; default **false**. Applies only when **Two-factor authentication enabled** and **External providers enabled** (FR-AUTH-014) are both **true**. When **true**, external sign-in may satisfy two-factor requirements using the IdP’s MFA assertion instead of app TOTP (see **Trust identity provider MFA** and FR-AUTH-014).

### Trust identity provider MFA

- When **Trust identity provider MFA** is **false**, external sign-in follows the same two-factor rules as password sign-in (verification or **strict two-factor setup**); IdP MFA is ignored.
- When **Trust identity provider MFA** is **true**, after external sign-in the system inspects the IdP assertion for provider MFA. For **Google** and **Microsoft** templates, MFA is recognized when the OIDC **`amr`** claim includes **`mfa`** (provider-specific rules are documented in deployment configuration; generic OIDC providers use the same **`amr`** convention when present).
- When provider MFA is **asserted** on external sign-in:
  - Skip **Two-factor verification** for that sign-in, even when **Two-factor enabled** is **true**.
  - When **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**, allow full application access without **strict two-factor setup** on that sign-in path.
- When provider MFA is **not asserted** on external sign-in, normal two-factor rules apply (**Two-factor verification** or **strict two-factor setup**). When the user has **Two-factor enabled** **true** and signs in via IdP without MFA assertion, **Two-factor verification** is **required** — IdP sign-in alone is not sufficient.
- **Trust identity provider MFA** never affects password sign-in: password sign-in always requires app TOTP when **Two-factor enabled** is **true**, and **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**.
- Voluntarily enrolled app TOTP remains stored; password sign-in continues to use it regardless of **Trust identity provider MFA**.

### Sign-in challenge types

Two mechanisms cover post-primary-auth flows; they are not interchangeable:

| Mechanism                        | When used                                                                                                                                                                 | Session issued?                                                                                                    |
| -------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| **Pending sign-in challenge**    | Primary auth succeeded; **Two-factor enabled** true; verification needed                                                                                                  | **No** — short-lived server-side challenge only (guest **Two-factor verification**).                               |
| **Enrollment bootstrap session** | Primary auth succeeded; two-factor required but **Two-factor enabled** false (and IdP MFA does not satisfy policy); or policy change forces setup while already signed in | **Yes** — limited JWT; middleware allows only two-factor setup, logout, and session refresh until setup completes. |

- **Pending sign-in challenge** expires after **10 minutes**; no application JWT is created until verification succeeds.
- **Enrollment bootstrap session** uses the same restriction pattern as password expiration (FR-AUTH-009): **strict two-factor setup** on the client; server rejects other application APIs with **403 Forbidden** until **Two-factor enabled** becomes **true** or policy no longer requires enrollment.

### Account model

| Concept                   | Storage                               | Meaning                                                                                                                                                                  |
| ------------------------- | ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Two-factor enabled**    | Boolean; default **false**            | **true** — password sign-in requires TOTP or recovery code after primary authentication unless an external path satisfies **Trust identity provider MFA** (FR-AUTH-013). |
| **Two-factor enabled at** | Date and time; empty when disabled    | Set when **Two-factor enabled** becomes **true**; cleared when two-factor is disabled or reset.                                                                          |
| **Two-factor secret**     | Encrypted secret; empty when disabled | TOTP shared secret for authenticator apps; never exposed after initial setup except during enrollment QR display.                                                        |
| **Recovery codes**        | Hashed one-time codes                 | Single-use backup codes generated at enrollment and on regeneration; plain codes are shown only once.                                                                    |

- Recovery codes are stored hashed; each code is invalidated after one successful use.
- The system stores **10** recovery codes when enrollment completes and when the user regenerates codes.
- TOTP validation accepts the current time step and **±1** adjacent step (**30**-second window each) to tolerate clock skew between server and authenticator app.

### Sensitive account actions (step-up authentication)

The following actions require **step-up authentication** in addition to an active session:

| Action                                | Where                          |
| ------------------------------------- | ------------------------------ |
| **Disable two-factor authentication** | **My account**                 |
| **Regenerate recovery codes**         | **My account**                 |
| **Link {Display name}** (signed-in)   | **My account** (FR-AUTH-014)   |
| **Unlink** external provider          | **My account** (FR-AUTH-014)   |
| **Set password**                      | **My account** (FR-AUTH-014)   |
| **Change email** (submit)             | **Change email** (FR-AUTH-015) |
| **Cancel email change**               | **My account** (FR-AUTH-015)   |

Step-up rules (all that apply must succeed):

| Condition                                                      | Requirement                                                                                                                                                                                   |
| -------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| User **has a local password**                                  | **Current password** must match.                                                                                                                                                              |
| User is **two-factor enrolled**                                | Valid **TOTP** or unused **recovery code**.                                                                                                                                                   |
| User is **external-only** (no local password, linked provider) | **Step-up external sign-in**: complete a fresh OIDC flow with a **linked** provider within the last **15 minutes** before the action (server-stored step-up timestamp per user and provider). |

- **Enable two-factor authentication** and **Set up two-factor authentication** already require password (when applicable) and verification code as part of enrollment; they follow enrollment rules, not this table.
- Failed step-up attempts use the same rate limits as **Two-factor verification** where codes are involved.
- When a **recovery code** is consumed during step-up, sends **Recovery code used** email (FR-AUTH-007).
- Guest **Accept invitation** external onboarding uses invited **Profile email** match only (FR-AUTH-010); step-up session rules do not apply on that path.

### My account — two-factor section

- Collapsible section **Two-factor authentication** on **My account** (FR-USR-001); shown only when **Two-factor authentication enabled** is **true**.
- Section title: **Two-factor authentication**; default **collapsed**.

| State                    | Section content                                                                                                                              |
| ------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------- |
| **Disabled**             | Description of two-factor authentication; **Enable two-factor authentication** button.                                                       |
| **Enabled**              | Badge **`Enabled`**; **Two-factor enabled at** (read-only); **Regenerate recovery codes** and **Disable two-factor authentication** actions. |
| **Required, not set up** | Warning **`Two-factor authentication is required for your account.`** and **Set up two-factor authentication** button (same flow as enable). |

- **Enable two-factor authentication** and **Set up two-factor authentication** open **Set up two-factor authentication** (dialog or dedicated screen on **My account** route subtree).
- **Disable two-factor authentication** is available only when **Two-factor authentication required** is **false**; opens confirmation and requires **Sensitive account actions** step-up authentication (FR-AUTH-013).

### Set up two-factor authentication

| Step / element        | Behavior                                                                                                                   |
| --------------------- | -------------------------------------------------------------------------------------------------------------------------- |
| **Current password**  | **Required** when the user **has a local password**; must match the current password. Omitted for **external-only** users. |
| **Authenticator QR**  | Displays a QR code and manual entry key for the authenticator app; issuer name **`ChangeMe`**.                             |
| **Verification code** | **Required**; **6** digits; must match the current TOTP from the configured secret.                                        |

- **Confirm setup** button: on success sets **Two-factor enabled** true, **Two-factor enabled at**, stores the encrypted secret, generates **10** recovery codes, shows the recovery codes once in a read-only list with copy guidance, and shows message **`Two-factor authentication enabled.`**
- Sends **Two-factor enabled** email (FR-AUTH-007).
- The user must acknowledge **`I have saved my recovery codes`** before closing the recovery-code step.
- **Cancel** closes the flow without saving.

### Regenerate recovery codes

- Requires **Sensitive account actions** step-up authentication (FR-AUTH-013).
- On success invalidates all previous recovery codes, generates **10** new codes, shows them once, and shows message **`Recovery codes regenerated.`**
- Does not change **Two-factor enabled** or the TOTP secret.

### Disable two-factor authentication

- Available only when **Two-factor authentication required** is **false**.
- Requires **Sensitive account actions** step-up authentication (FR-AUTH-013).
- On success clears **Two-factor enabled**, secret, and recovery codes; shows message **`Two-factor authentication disabled.`**
- Sends **Two-factor disabled** email (FR-AUTH-007).

### Two-factor verification screen (guest)

- Screen: **Two-factor verification**; available to guests during sign-in when primary authentication succeeded and **Two-factor enabled** is **true**.
- Reached from **Login** (FR-AUTH-001) and from external provider sign-in (FR-AUTH-014) before a session is created.
- **Back to sign in** at the top → **Login** (clears the pending sign-in challenge).

| Field / element       | Behavior                                                                                                                       |
| --------------------- | ------------------------------------------------------------------------------------------------------------------------------ |
| **Verification code** | **Required**; **6** digits for TOTP, or a recovery code in the same field (recovery codes are alphanumeric, case-insensitive). |

- **Verify** button: on success creates the session per FR-AUTH-001 / FR-AUTH-014 and continues the normal post-sign-in flow (password expiration, **Projects list**, etc.). When a **recovery code** was used, sends **Recovery code used** email (FR-AUTH-007).
- Invalid code shows form-level error: **`Invalid verification code.`**
- **Use a recovery code** helper text explains that a recovery code may be entered instead of a TOTP code.
- Pending sign-in challenge expires after **10 minutes**; expired challenge redirects to **Login** with message **`Sign-in timed out. Try again.`**
- Applies only to the **pending sign-in challenge** path; it does not apply to users in **enrollment bootstrap session** (they already hold a limited JWT).

### Two-factor verification rate limiting

- Each **pending sign-in challenge** allows at most **5** failed verification attempts (invalid TOTP or recovery code).
- After **5** failures, the challenge is invalidated immediately; the user is redirected to **Login** with message **`Too many attempts. Sign in again.`**
- A new challenge requires successful primary authentication again.
- The same **5**-attempt limit applies per step-up action flow when **Two-factor enabled** true and a code is required (**Sensitive account actions**).
- Rate limits are enforced server-side; error messages do not indicate whether a recovery or TOTP format was expected.

### Mandatory enrollment

- When **Two-factor authentication required** is **true**, the user has **Two-factor enabled** false, and IdP MFA does not satisfy policy (**Trust identity provider MFA** is **false**, or external sign-in was not used, or the IdP did not assert MFA), the system issues an **enrollment bootstrap session** after successful primary authentication (password or external provider).
- The client enters **strict two-factor setup** mode: the user cannot navigate to other application screens until setup completes (except **Logout**); the application shows only minimal chrome.
- **Strict two-factor setup** uses the same **Set up two-factor authentication** flow as voluntary enrollment on **My account**.
- After successful setup, the application opens **Projects list**, subject to **Combined account compliance gates** (password expiration is resolved before two-factor setup when both apply on the same sign-in). See `docs/requirements/_shared/reference/compliance-gates.md`.

### Deployment policy changes

Deployment settings are read on each sign-in, session refresh, and authenticated API request (same evaluation model as **Password expiration enabled** in FR-AUTH-009). When settings change in `appsettings` and the application restarts or reloads configuration, the system applies the **current** policy immediately; users are not grandfathered out of new requirements.

| Setting change                                                                                               | Effect on existing signed-in users                                                                                                                                                                                                                       |
| ------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Two-factor authentication enabled** **false** → **true**, **Two-factor authentication required** **false** | No forced action; two-factor becomes available on **My account** only.                                                                                                                                                                                   |
| **Two-factor authentication required** turned **on** (while two-factor is enabled)                           | Users who are not yet **two-factor enrolled** (except those **awaiting invitation acceptance**) receive **`twoFactorSetupRequired`** on the next session refresh or blocked API response; client enters **strict two-factor setup** without signing out. |
| **Two-factor authentication enabled** **true** → **false**                                                   | Enforcement stops on next refresh; stored secrets remain but are inactive; **strict two-factor setup** ends immediately.                                                                                                                                 |
| **Two-factor authentication required** **true** → **false**                                                  | **Strict two-factor setup** ends on next refresh for users who had not enrolled; enrolled users keep two-factor until they disable it voluntarily.                                                                                                       |
| **Trust identity provider MFA** toggled                                                                      | Affects the next external sign-in only; does not disable existing app TOTP enrollment.                                                                                                                                                                   |

- While **`twoFactorSetupRequired`** is **true**, the user **stays on the current screen and route** when the flag is raised during an active session (same UX principle as password expiration during an active session in FR-AUTH-009); the application shows a **sticky toast** with summary **`Two-factor authentication required`** and detail **`Set up two-factor authentication to continue saving your work to the server.`** with action **`Set up now`** opening **Set up two-factor authentication**.
- Until setup completes, server requests for application data (except sign-out, session refresh, and two-factor setup endpoints) are rejected; purely local UI state on the current screen remains available.
- On the next sign-in after a policy change, users without required two-factor follow **Mandatory enrollment** (bootstrap session) or **Two-factor verification** when already enrolled.
- Auth responses (sign-in, refresh) include **`twoFactorSetupRequired`** when enrollment is required under current deployment settings and **Two-factor enabled** is **false**, analogous to **`passwordChangeRequired`**.

### Combined account compliance gates

See `docs/requirements/_shared/reference/compliance-gates.md` for the full ordered gate list.

### Sign-in order with other auth rules

Primary authentication (password on **Login** or external provider per FR-AUTH-014) is evaluated first in this order before two-factor verification:

1. Unknown credentials / provider failure — fail without revealing account existence where applicable.
2. Account is **deactivated** — **`This account has been deactivated. Contact an administrator.`**
3. User is **awaiting invitation acceptance** (password sign-in) — **`Complete your account setup using the invitation link sent to your email.`**
4. Email verification is **enabled** and the mailbox is **not verified** (FR-AUTH-011) — **`Verify your email before signing in.`**
5. Password expiration after session would otherwise be issued (FR-AUTH-009) — **Required password change**; two-factor is **not** required until after the password is updated on that sign-in path.

When **Two-factor enabled** is **true**, step 5 applies to password sign-in only after a valid TOTP or recovery code. External provider sign-in applies two-factor after provider success and before session creation unless **Trust identity provider MFA** satisfies the step, or password expiration redirects first.

When **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**, password sign-in issues an **enrollment bootstrap session** (**strict two-factor setup**). External sign-in issues a bootstrap session only when **Trust identity provider MFA** does not apply or the IdP did not assert MFA; otherwise a normal full session is issued.

### Administrator reset

- **Reset two-factor** header action on **User details** (FR-USR-004); requires **Users.Manage**.
- Shown only when **Two-factor authentication enabled** is **true** and the user's **Two-factor enabled** is **true**.
- Confirmation dialog: **`Reset two-factor authentication for "{full name}"? They will need to set it up again on next sign-in.`**
- On success clears **Two-factor enabled**, secret, and recovery codes; revokes **all active sessions** for the user; success message **`Two-factor authentication reset.`**
- Sends **Two-factor reset by admin** email (FR-AUTH-007).
- When **Two-factor authentication required** is **true**, the user's next sign-in enters **strict two-factor setup** after primary authentication.

### User details (admin read-only)

- When **Two-factor authentication enabled** is **true**, **User details** shows **Two-factor authentication** badge **`Enabled`** or **`Disabled`** and **Two-factor enabled at** when enabled (FR-USR-004).

### States and business rules

- Changing password (FR-AUTH-005) does not disable two-factor authentication.
- **Sign out everywhere** and session revoke (FR-AUTH-003, FR-AUTH-004) do not disable two-factor authentication.
- Deactivating a user (FR-USR-005) does not clear two-factor settings; reactivation preserves them.
- A used recovery code cannot be reused; the system sends **Recovery code used** email (FR-AUTH-007).
- **Out of scope:** SMS or email one-time codes as the second factor; per-role two-factor requirement; trusted devices that skip two-factor; admin UI to change deployment two-factor flags at runtime.
- Passkeys (WebAuthn): `docs/requirements/functional/passkeys/` (FR-PKY-001 through FR-PKY-007). **Passkey satisfies two-factor** deployment setting interacts with FR-AUTH-013; see FR-PKY-001 and FR-PKY-006.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
