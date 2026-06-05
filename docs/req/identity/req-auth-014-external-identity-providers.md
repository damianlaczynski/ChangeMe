---
id: REQ-AUTH-014
title: External Identity Providers
domain: identity
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-005, REQ-AUTH-007, REQ-AUTH-008, REQ-AUTH-009, REQ-AUTH-010, REQ-AUTH-011, REQ-AUTH-012, REQ-AUTH-013, REQ-AUTH-015, REQ-INV-001, REQ-USR-003, REQ-USR-004]
---
## Goal

Deployments must be able to allow sign-in through configured external identity providers (OpenID Connect) in addition to email and password. Users must be able to link and unlink providers from their account when permitted.

## Features

### External providers policy

- Deployment settings include **External providers enabled**; default **false**.
- Deployment settings include **External provider linking enabled**; default **true**. Effective only when **External providers enabled** is **true**.
- When **External providers enabled** is **false**, external sign-in buttons, **Link**/**Unlink** UI on **My account**, and external sign-in APIs are unavailable.
- When **External providers enabled** is **true** and **External provider linking enabled** is **false**, **Continue with {Display name}** on **Login**, **Register**, and **Accept invitation** remains available; existing **External login** rows remain usable for sign-in and step-up; **Link {Display name}** on **My account** and OIDC **link mode** APIs are unavailable; **Unlink** on **My account** remains available subject to last-sign-in-method rules.
- When **External providers enabled** is **true**, the deployment configures one or more providers. Each provider entry includes at minimum:
  - **Provider key** — stable identifier (for example **`google`**, **`microsoft`**).
  - **Display name** — button label (for example **`Google`**, **`Microsoft`**).
  - **Authority** — OIDC issuer URL.
  - **Client id** and **Client secret** — deployment secrets, not editable from the application UI.
  - **Allowed email domains** — optional list (for example **`example.com`**). When non-empty, external sign-in and linking succeed only when the normalized provider email ends with `@` + one listed domain; otherwise redirect to **Login** with **`Sign-in with this account is not allowed.`**
  - **Issuer validation mode** — optional deployment setting. **Discovery** (default): accept only the issuer published in the provider’s OIDC metadata (typical for Google, single-tenant Microsoft, generic OIDC). **Microsoft multi-tenant**: accept sign-in from any Microsoft Entra tenant; **required** when the configured authority uses `/common` or `/organizations`. Operator detail: `docs/auth-operations-guide.md`.
  - **Trust IdP email without email verified** — optional per provider. When **enabled**, treat the identity provider’s email as verified even when the token does not include an explicit **email verified** flag (common for Microsoft Entra). When **disabled**, new account creation via OIDC registration and **Accept invitation** external onboarding require a verified email assertion from the provider.
- Supported built-in provider templates in documentation and default configuration: **Google** and **Microsoft** (Entra ID / Microsoft identity platform). Additional providers may use the same generic OIDC configuration shape.
- The frontend loads the list of enabled providers (key and display name only) from a public auth settings endpoint; secrets remain server-side. The same endpoint exposes **External providers enabled**, **External provider linking enabled**, and **Self-service email change enabled** (REQ-AUTH-015).
- When **Trust identity provider MFA** is **true** (REQ-AUTH-013), the backend evaluates the IdP **`amr`** claim (and provider-specific equivalents) on each external sign-in callback.

### OIDC protocol security

- Authorization requests use **authorization code** flow with **PKCE** (S256).
- Each authorization request generates a cryptographically random **`state`**; the callback rejects mismatched or missing **state** (CSRF protection).
- Each request includes **`nonce`**; the backend validates **`nonce`** in the ID token on callback (replay protection).
- On callback, the backend discovers authorize and token endpoints from `{Authority}/.well-known/openid-configuration`. ID token **issuer** validation follows **Issuer validation mode** (default: metadata **issuer**); **audience** (client id), **expiry**, and **signature** are always validated against provider metadata before trusting claims.
- For **new account creation** via OIDC on **Register** or **Login**, and for **Accept invitation** external onboarding (REQ-AUTH-010), the provider email is used only when the identity provider asserts it is verified, when **Trust IdP email without email verified** is **enabled** for that provider, or when the token includes provider-specific verified email claims (for example **verified primary email**). If the email is missing or not treated as verified, the system does not create an account by email; an existing **External login** for this provider subject may still sign the user in.
- Authorization codes are single-use; exchanged server-side only (secrets never sent to the frontend).

### External providers disabled at runtime

- When **external providers** are **disabled**, **External login** rows are **retained** but **inactive** (not usable for sign-in or step-up until re-enabled).
- When **external provider linking** is **disabled** (and external providers remain enabled), **Login**, **Register**, and **Accept invitation** still show **Continue with {Display name}**; **My account** hides **Link {Display name}** only.
- **Login**, **Register**, and **My account** hide all external provider UI when **External providers enabled** is **false** (on next settings load).
- Users who **have a local password** continue signing in with email and password (and two-factor when applicable).
- **External-only** users cannot sign in until an administrator re-enables external providers or the user sets a local password; show **`External sign-in is unavailable. Contact an administrator or set a password when sign-in is available.`** on **Login** when detected (edge case for accounts that relied solely on external sign-in while providers were turned off).

### Trust identity provider MFA (external sign-in)

See REQ-AUTH-013 for deployment flag and password-sign-in rules. External sign-in behavior:

| IdP MFA asserted | **Two-factor enabled** | **Two-factor required** | Outcome after provider success                                  |
| ---------------- | ---------------------- | ----------------------- | --------------------------------------------------------------- |
| Yes, trust on    | false                  | true                    | Full session; no **strict two-factor setup**.                   |
| Yes, trust on    | true                   | any                     | Full session; skip **Two-factor verification**.                 |
| No or trust off  | true                   | any                     | **Pending sign-in challenge** → **Two-factor verification**.    |
| No or trust off  | false                  | true                    | **Enrollment bootstrap session** → **strict two-factor setup**. |
| No or trust off  | false                  | false                   | Full session.                                                   |

### Account model

| Concept                      | Storage                   | Meaning                                                                                         |
| ---------------------------- | ------------------------- | ----------------------------------------------------------------------------------------------- |
| **External login**           | Row per linked provider   | Associates **Provider key** and **Provider subject** (stable id from the issuer) with one user. |
| **External login linked at** | Date and time on each row | When the link was created.                                                                      |

- A user may have zero or more external logins.
- The pair (**Provider key**, **Provider subject**) is unique across all users.
- A user may link at most one account per **Provider key**.

### Profile email and provider email

| Term               | Meaning                                                                                                                                                                                                                                                                              |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Profile email**  | The **current email** on the ChangeMe user account. Used for email/password sign-in, uniqueness (REQ-USR-003), display on **My account**, and **every** application and auth notification (REQ-AUTH-007, issue notifications).                                                       |
| **Provider email** | The email address returned by the identity provider on an OIDC callback. Used for **allowed email domains**, **Accept invitation** external onboarding (must match invited **Profile email**), and optional display on **My account**. **Never** used as a notification destination. |

**Profile email** changes only through: registration, **Invite user**, first-time OIDC account creation (initial **Profile email** = verified provider email), **Change email** (REQ-AUTH-015), or administrator **Edit user** (REQ-USR-003).

**Linking** a provider from **My account** does **not** change **Profile email**, even when **Provider email** differs.

Subsequent OIDC sign-ins do **not** update **Profile email** from provider claims.

### Guest external sign-in (Login and Register)

The system maintains **at most one user account per normalized Profile email** (REQ-USR-003).

Guest **Continue with {Display name}** on **Login** or **Register** does **not** link a provider to an existing account except **Accept invitation** external onboarding (REQ-AUTH-010). To add Google or Microsoft to an existing account, the user signs in to ChangeMe first, then uses **Link {Display name}** on **My account**.

Evaluate rows **in table order** (first match wins). **Allowed email domains** are evaluated on **Provider email** before other rows.

| Condition                                                                                                         | Outcome                                                                                                                                                                                                                                                     |
| ----------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Provider email not allowed by domain allowlist (when configured)                                                  | **`Sign-in with this account is not allowed.`** on **Login** or **Register**.                                                                                                                                                                               |
| **External login** exists for this (**Provider key**, **Provider subject**)                                       | Sign in the linked user (subject to deactivation, email verification, two-factor, and password expiration below). **Provider email** may differ from **Profile email**.                                                                                     |
| User is **awaiting invitation acceptance**; verified **Provider email** matches invited **Profile email**         | Complete invitation and link provider per REQ-AUTH-010 (only email-match link on guest OIDC).                                                                                                                                                               |
| User is **awaiting invitation acceptance**; **Provider email** does not match invited **Profile email**           | **`The external account email does not match the invited email address.`** on **Login** or **Accept invitation**.                                                                                                                                           |
| **Provider subject** not linked; a user account already exists with the same normalized **Provider email**        | **`An account already exists for this email. Sign in with your password, then link {Display name} from My account.`** on **Login** or **Register**. Does **not** open a linking screen.                                                                     |
| **Provider subject** not linked; no account with that **Provider email**; **public registration** is **enabled**  | Create **one** new user; set initial **Profile email** from verified **Provider email**; link provider; create as **external-only**; issue full session, enrollment bootstrap session, or pending two-factor challenge per **Trust identity provider MFA**. |
| **Provider subject** not linked; no account with that **Provider email**; **public registration** is **disabled** | **`No account exists for this email. Contact an administrator.`** on **Login**.                                                                                                                                                                             |
| Matched user’s account is **deactivated**                                                                         | **`This account has been deactivated. Contact an administrator.`** on **Login**.                                                                                                                                                                            |
| Email verification **enabled**; matched user’s **Profile email** **not verified**                                 | **`Verify your email before signing in.`** on **Login** (verified **Provider email** alone does not override an unverified local registration).                                                                                                             |

- New users created via external sign-in receive **First name** and **Last name** from provider claims when present; otherwise empty (user may complete profile on **Edit profile**).
- External sign-in never bypasses app TOTP when **Trust identity provider MFA** is **disabled**, or when the IdP does not assert MFA. When **Trust identity provider MFA** is **enabled** and MFA is asserted, external sign-in may bypass **Two-factor verification** and **strict two-factor setup** per REQ-AUTH-013.

### Linking external providers (signed-in only)

Linking adds an **External login** row to the **signed-in** user’s account. The user must already have an active ChangeMe session (email/password, an already-linked provider, or passkey per other REQs).

| Step | Behavior                                                                                                                                                                                                                                                     |
| ---- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1    | User opens **My account** → **Link {Display name}** for an enabled provider not yet linked. Requires **External provider linking enabled** is **true**.                                                                                                      |
| 2    | **Sensitive account actions** step-up (REQ-AUTH-013) completes before OIDC starts.                                                                                                                                                                           |
| 3    | When **Provider email** differs from **Profile email**, show confirmation before OIDC: **`Link {Display name} to your account? Your profile email is {profile email}. The provider may use a different address. Notifications stay on your profile email.`** |
| 4    | OIDC callback in **link mode** attaches (**Provider key**, **Provider subject**) to the signed-in user. **Provider email match is not required.**                                                                                                            |
| 5    | **Allowed email domains** (when configured) are evaluated against **Provider email** on callback.                                                                                                                                                            |
| 6    | (**Provider key**, **Provider subject**) must not already belong to another user — **`This external account is already linked to another user.`**                                                                                                            |
| 7    | On success: message **`External sign-in method linked.`**; **External account linked** email to **Profile email** (REQ-AUTH-007); refresh **My account**.                                                                                                    |

- A user may link multiple providers (for example Google and Microsoft) from **My account**; each requires its own **Link** action and step-up. **Provider email** values may differ from each other and from **Profile email**.
- **Out of scope for this REQ:** guest **Link external account** screen; auto-linking a provider during **Login** or **Register** when **Provider email** matches an existing account (except **Accept invitation** per REQ-AUTH-010).

### Login screen — external sign-in

- When **External providers enabled** is **true** and at least one provider is configured, **Login** shows **Continue with {Display name}** for each provider (REQ-AUTH-001).
- Clicking a provider starts the OIDC authorization code flow with PKCE; the user is redirected to the provider and returns to **External sign-in callback** (guest route).
- On provider success the system reads the provider email and subject id.

### External sign-in callback (guest)

- Screen/route: **External sign-in callback**; processes the OIDC redirect, shows a loading state, then continues sign-in logic without manual input when possible.
- On unrecoverable error (provider error, invalid state, denied consent): redirect to **Login** with form-level error **`External sign-in failed. Try again or use email and password.`**

### My account — external sign-in methods

- Section **External sign-in methods** on **My account** when **External providers enabled** is **true**; collapsible; default **collapsed**.
- Persistent notice at the top of the section: **`Notifications are sent to your profile email ({profile email}), not to provider addresses.`**
- Lists each linked provider as a row: **Provider** (display name), **Provider email** (last known from the most recent OIDC callback for that link, or **`—`** when unknown), **Linked at**, **Unlink** action.
- When **Provider email** differs from **Profile email**, show inline hint on the row: **`May differ from your profile email.`**
- Lists enabled providers not yet linked with **Link {Display name}** when **External provider linking enabled** is **true**; action starts OIDC in **link mode** (returns to signed-in **My account** on success).
- **Link {Display name}** (when linking is enabled) and **Unlink** require **Sensitive account actions** step-up authentication (REQ-AUTH-013) before the action completes.
- **Unlink** requires confirmation: **`Remove {Display name} sign-in from your account?`**
- **Unlink** is blocked when it would leave the user with no sign-in method (**external-only** with only one linked provider) — show message **`Set a password before removing your only sign-in method.`** with link to **Set password** (see below).
- On successful link: message **`External sign-in method linked.`** and **External account linked** email (REQ-AUTH-007).
- On successful unlink: message **`External sign-in method removed.`** and **External account unlinked** email (REQ-AUTH-007).

### Set password (signed-in)

- Screen: **Set password**; linked from **My account** when the signed-in user is **external-only**.
- **Set password** requires **Sensitive account actions** step-up authentication (REQ-AUTH-013) before submit.
- Same fields and validation as **Change password** except **Current password** is omitted (REQ-AUTH-008).
- On success establishes a **local password** and sets **password last changed at**; does not revoke other sessions.
- Enables **Change password** (REQ-AUTH-005) and **Forgot password** self-service thereafter.

### Register screen

- When **External providers enabled** is **true** and **Public registration enabled** is **true**, **Register** shows the same **Continue with {Display name}** buttons as **Login** (shared behavior: creates account when email is new).

### Password and expiration interaction

- **External-only** users do not use **Forgot password** until they set a local password; external sign-in remains available when linked.
- Password expiration (REQ-AUTH-009) applies only when the user **has a local password**; **external-only** users are not redirected to **Required password change** for age reasons.
- **Change password** (REQ-AUTH-005) requires a **local password**; **external-only** users use **Set password** first.

### Administrator unlink (external sign-in)

- Administrators may **Unlink** a provider from **User details** (REQ-USR-004); requires **Users.Manage**.
- Confirmation: **`Remove {Display name} sign-in from this account?`**
- On success removes the **External login** row. **Unlink** is blocked when it would leave the user with no sign-in method (**external-only** with only one provider), same as self-service unlink.
- Sends **External account unlinked** email (REQ-AUTH-007).

### User details (admin read-only)

- When **External providers enabled** is **true**, **User details** shows section **External sign-in methods**: linked **Provider** display names, **Linked at**, and per-row **Unlink** for administrators (REQ-USR-004).

### Admin invite user and invitation

- **Invite user** (REQ-INV-001): administrators onboard by **Profile email**; the invitee must use that address for **Accept invitation** external onboarding (REQ-AUTH-010).

### Interaction with other auth flows

| Flow                                   | Behavior when external providers enabled                                                                                |
| -------------------------------------- | ----------------------------------------------------------------------------------------------------------------------- |
| **Login** (REQ-AUTH-001)               | Email/password unchanged; provider buttons when configured.                                                             |
| **Register** (REQ-AUTH-001)            | Provider buttons when public registration enabled.                                                                      |
| **Two-factor** (REQ-AUTH-013)          | Applies after provider success unless **Trust identity provider MFA** and IdP MFA assertion apply.                      |
| **Email verification** (REQ-AUTH-011)  | Provider-asserted verified email sets **Email verified** true on new account; existing unverified users remain blocked. |
| **Public registration** (REQ-AUTH-012) | When disabled, provider sign-in for unknown emails fails with contact administrator message.                            |
| **Password expiration** (REQ-AUTH-009) | Not evaluated for users **without a local password**.                                                                   |

### States and business rules

- **Link {Display name}** and **Unlink** require a signed-in session and **Sensitive account actions** step-up (REQ-AUTH-013), except administrator **Unlink** on **User details**.
- After linking, all **External login** rows for the user share one **Profile email**, roles, sessions, and two-factor settings.
- Admin **Edit user** **Profile email** change (REQ-USR-003): when **External providers enabled** is **true** and the user has at least one **External login**, **Edit user** shows notice **`External sign-in stays linked. Profile email is used for notifications; provider addresses may differ.`** Saving does **not** remove **External login** rows; administrators may **Unlink** providers from **User details** when appropriate.
- Self-service **Change email** (REQ-AUTH-015): **External login** rows are retained; notice on **Change email** when the user has linked providers: **`External sign-in methods stay linked. Notifications stay on your profile email.`**
- **Out of scope for this REQ:** SAML 2.0; social providers without email claim; automatic admin provisioning rules; SCIM; admin UI to configure providers at runtime; merging two user records with **different Profile emails** into one account; guest **Link external account** screen.

---
