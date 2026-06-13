# Authentication operations guide

> **Audience:** operators, administrators, and developers deploying or supporting **ChangeMe**.
> **Scope:** deployment settings under `AuthOptions` in backend configuration, how each option affects sign-in and accounts, and how to connect external identity providers (OIDC). **Not** an HTTP API catalog — use Swagger and `docs/guides/` for routes and handlers.
> **Related:** product behaviour is defined in `docs/requirements/functional/identity/` and `docs/requirements/functional/users/` (see `docs/requirements/README.md`). **Passkeys (WebAuthn):** `docs/requirements/functional/passkeys/`. Cross-cutting terms: `docs/requirements/_shared/reference/`. This guide explains **operations**, not formal requirements.

---

## 1. Where authentication is configured

| Location                                                                     | Purpose                                                                                                                                                                       |
| ---------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.json`             | Production defaults (secrets via environment / secret store in real deployments).                                                                                             |
| `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.Development.json` | Local development overrides.                                                                                                                                                  |
| Environment variables                                                        | Override any setting using `AuthOptions__` prefix and `__` for nesting (see [.NET configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)). |
| `InitialAdministratorOptions` section                                        | Bootstrap admin account on first run (separate from `AuthOptions`).                                                                                                           |
| `EmailOptions` section                                                       | SMTP for invitations, verification, password reset, and auth notification emails.                                                                                             |
| `CorsOptions:AllowedOrigins`                                                 | Must include the frontend origin or the browser cannot call the API.                                                                                                          |

**Restart required:** Auth policy is read when the API starts and on each request for compliance flags (password expiration, mandatory 2FA). Changing `appsettings` requires an application restart (or configuration reload if you add that in hosting).

**Public settings API:** `GET /api/auth/settings` exposes password policy, registration flags, 2FA flags, **self-service email change enabled**, **external provider linking enabled**, and external provider **keys and display names only**—never client secrets.

---

## 2. End-to-end sign-in flow

After primary authentication (password, passkey, or external provider), the API may apply **compliance gates** (account active, email verified, password expiration, 2FA verification/setup, mandatory passkeys, etc.) before issuing a full session.

**Canonical gate order and passkey/2FA interaction:** [`docs/requirements/_shared/reference/compliance-gates.md`](../requirements/_shared/reference/compliance-gates.md).

This guide covers **configuration** that enables or disables each gate (`AuthOptions` below).

---

## 3. Application screens (auth area)

Auth-related routes and screen behaviour are defined in product requirements, not duplicated here:

- **Routes:** `src/ChangeMe.Frontend/src/app/app.routes.ts`
- **Identity flows:** [`docs/requirements/functional/identity/`](../requirements/functional/identity/) (see [`docs/requirements/README.md`](../requirements/README.md))
- **Passkeys UI:** [`docs/requirements/functional/passkeys/`](../requirements/functional/passkeys/)

Administrators manage other users under **Users** (invitations, deactivate, reset 2FA, unlink external logins, etc.) — see [`docs/requirements/functional/users/`](../requirements/functional/users/).

---

## 4. `AuthOptions` settings reference

**Defaults:** see `appsettings.json` and `appsettings.Development.json` in `ChangeMe.Backend.Web`.

### 4.1 General

| Setting           | What it does                      | Impact                                                                                                                      |
| ----------------- | --------------------------------- | --------------------------------------------------------------------------------------------------------------------------- |
| `FrontendBaseUrl` | Canonical URL of the Angular app. | Used in email links (reset, verify, invite) and to build the **OIDC redirect URI**. Must match the URL users actually open. |

### 4.2 JWT (`AuthOptions:Jwt`)

| Setting               | What it does                                        | Impact                                                                                             |
| --------------------- | --------------------------------------------------- | -------------------------------------------------------------------------------------------------- |
| `Issuer`              | JWT issuer claim.                                   | Must match validation configuration.                                                               |
| `Audience`            | JWT audience.                                       | Tokens rejected if audience mismatches.                                                            |
| `SigningKey`          | Symmetric key for signing access tokens.            | **Must be at least 32 characters** and unique per environment. Compromise = full account takeover. |
| `ExpirationMinutes`   | Access token lifetime.                              | After expiry, client refreshes session via refresh token.                                          |
| `SessionLifetimeDays` | Max refresh/session lifetime from **signed in at**. | Long-lived session until revoked or expired; client stores credentials in **local storage**.       |

Every successful sign-in creates a session with this lifetime. The frontend always persists the session in **local storage**.

Users can revoke sessions on **My account**; admins can revoke on **User details**.

### 4.3 Password policy (`AuthOptions:PasswordPolicy`)

| Setting                   | What it does                           | Impact                                                         |
| ------------------------- | -------------------------------------- | -------------------------------------------------------------- |
| `MinimumLength`           | Minimum password length.               | Register, invite accept, reset, change password, set password. |
| `MaximumLength`           | Maximum password length.               | Same surfaces as above.                                        |
| `RequireUppercase`        | Requires an uppercase letter.          | Validation on all password set/change flows.                   |
| `RequireLowercase`        | Requires a lowercase letter.           | Same.                                                          |
| `RequireDigit`            | Requires a digit.                      | Same.                                                          |
| `RequireSpecialCharacter` | Requires a non-alphanumeric character. | Same.                                                          |

Policy is exposed to the frontend via `GET /api/auth/settings` so forms can show requirements before submit.

### 4.4 Password expiration (`AuthOptions:PasswordExpiration`)

| Setting                  | What it does                           | Impact                                                                                                         |
| ------------------------ | -------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `Enabled`                | Enables maximum password age.          | When enabled, expired passwords trigger **Required password change** after sign-in instead of the main app.    |
| `MaximumPasswordAgeDays` | Days after `password last changed at`. | Applies to password-based accounts; external-only users without a password are not subject until they set one. |

### 4.5 Email verification (`AuthOptions:EmailVerification`)

| Setting             | What it does                            | Impact                                                                                                         |
| ------------------- | --------------------------------------- | -------------------------------------------------------------------------------------------------------------- |
| `Enabled`           | Requires verified email before sign-in. | Register sends verification email; Login blocked until verified. Admins can mark verified on **User details**. |
| `LinkLifetimeHours` | Validity of verification links.         | Expired links require resend from **Verify email**.                                                            |

**Requires working `EmailOptions` configuration** (or MailHog in Docker for local dev).

### 4.6 Registration (`AuthOptions:Registration`)

| Setting         | What it does                                          | Impact                                                                                                           |
| --------------- | ----------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| `PublicEnabled` | Allows `/register` and self-service account creation. | When `false`, route is blocked; **external OIDC auto-registration** also fails unless an account already exists. |

### 4.7 Password reset (`AuthOptions:PasswordReset`)

| Setting             | What it does         | Impact                              |
| ------------------- | -------------------- | ----------------------------------- |
| `LinkLifetimeHours` | Reset link validity. | Used by forgot/reset password flow. |

### 4.8 Invitations (`AuthOptions:Invitations`)

| Setting                       | What it does              | Impact                                               |
| ----------------------------- | ------------------------- | ---------------------------------------------------- |
| `InvitationLinkLifetimeHours` | Invitation link validity. | Used when admins **Invite user** without a password. |

##### Retention (`AuthOptions:Invitations:Retention`)

| Setting                          | What it does                                                                | Impact                                                                                                   |
| -------------------------------- | --------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `RevokedInvitationRetentionDays` | Delete **revoked** / **cancelled** `AccountInvitation` rows after this age. | **Pending** and **accepted** rows are never removed. Age uses `RevokedAtUtc`, or `SentAtUtc` if missing. |
| `CleanupCronExpression`          | Hangfire schedule for the cleanup job.                                      | See [Hangfire and background jobs](database-and-docker.md#hangfire-and-background-jobs).                 |

Example:

```json
"AuthOptions": {
  "Invitations": {
    "InvitationLinkLifetimeHours": 72,
    "Retention": {
      "RevokedInvitationRetentionDays": 7
    }
  }
}
```

### 4.9 Two-factor authentication (`AuthOptions:TwoFactor`)

| Setting                    | What it does                                          | Impact                                                                                                                                                                                                        |
| -------------------------- | ----------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Enabled`                  | Master switch for TOTP/recovery codes.                | When `false`, 2FA UI and enforcement are off; stored secrets remain in DB but are inactive.                                                                                                                   |
| `Required`                 | Every account must enroll in 2FA.                     | When `true` (and 2FA enabled), users without TOTP get **strict two-factor setup** after sign-in or on next API call (`twoFactorSetupRequired`). Invite-pending users are exempt until they accept invitation. |
| `TrustIdentityProviderMfa` | Trust IdP MFA assertion on **external** sign-in only. | Effective only when **both** 2FA and external providers are enabled. See §6.10.                                                                                                                               |

| Setting                                 | What it does                                            | Impact                                                                                          |
| --------------------------------------- | ------------------------------------------------------- | ----------------------------------------------------------------------------------------------- |
| `TotpTimeStepSeconds`                   | TOTP time step.                                         | Authenticator app codes rotate every 30s.                                                       |
| `TotpValidationWindowSteps`             | Accept ±1 step around current time.                     | Tolerates clock skew between server and phone.                                                  |
| `VerificationCodeLength`                | Expected TOTP length.                                   | UI and validation.                                                                              |
| `RecoveryCodeCount`                     | Codes generated at setup/regeneration.                  | Single-use backup codes; shown once.                                                            |
| `PendingSignInChallengeLifetimeMinutes` | Pending 2FA challenge during sign-in.                   | User redirected to Login if expired.                                                            |
| `MaxFailedVerificationAttempts`         | Failed TOTP/recovery attempts per challenge/step-up.    | Challenge invalidated; user must sign in again.                                                 |
| `StepUpExternalSignInValidityMinutes`   | How long OIDC **step-up** counts for sensitive actions. | External-only users must re-authenticate with linked provider before unlink, set password, etc. |
| `TotpIssuerName`                        | Issuer shown in authenticator apps and QR label.        | Branding in Google Authenticator, etc.                                                          |

**Sensitive actions** (require step-up: password + TOTP if enabled, or recent external step-up for passwordless accounts):

- Disable 2FA, regenerate recovery codes
- Link external provider on **My account** (when `LinkingEnabled` is true); unlink external provider on **My account**
- Set password (external-only accounts)

**Administrator:** **Reset two-factor** on **User details** clears 2FA and revokes all sessions (requires `Users.Manage`).

### 4.10 Self-service email change (`AuthOptions:EmailChange`)

| Setting             | What it does                                       | Impact                                                                                                                                                                                                                |
| ------------------- | -------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Enabled`           | Gates **Change email** on **My account** and APIs. | When `false`, users cannot start a new email change; **Change email** is hidden. An existing **pending email change** may still be shown (resend/cancel) until cleared. Confirmation links already sent remain valid. |
| `LinkLifetimeHours` | Validity of confirmation links to the new mailbox. | Expired links require **Resend confirmation email** on **Change email**.                                                                                                                                              |

**Email uniqueness:** Only **Profile email** blocks duplicate accounts at registration, invite, and **Change email** submit. Another user's **pending new email** does not reserve the address. **Confirm email change** re-checks that the target is not already another account's **Profile email**; otherwise the pending change remains and the user is told to cancel it on **My account**.

Configuration path examples: `AuthOptions__EmailChange__Enabled=false`, `AuthOptions__EmailChange__LinkLifetimeHours=48`.

### 4.11 External identity providers (`AuthOptions:External`)

| Setting                  | What it does                                                                       | Impact                                                                                                                                                                                                                                                      |
| ------------------------ | ---------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `Enabled`                | Master switch for OIDC sign-in (**Continue with …** on Login/Register/invitation). | When `false`, no provider buttons; sign-in and step-up external APIs are unavailable. Existing `External login` rows are kept but unusable until re-enabled.                                                                                                |
| `LinkingEnabled`         | Gates **Link {Provider}** from **My account** (OIDC **link mode**).                | When `false` while `Enabled` is `true`, guest sign-in and first-time OIDC registration still work; users with existing links can sign in and **Unlink**; **Link** actions and link-mode APIs are hidden/forbidden. Requires `Enabled: true` to take effect. |
| `PendingLifetimeMinutes` | Lifetime of pending OIDC state rows.                                               | Expired pending flows must be restarted from Login or **My account**.                                                                                                                                                                                       |
| `SignInCallbackPath`     | Frontend path appended to `FrontendBaseUrl` for redirect URI.                      | Must match IdP redirect URI registration.                                                                                                                                                                                                                   |
| `Providers`              | List of provider configurations.                                                   | Each fully configured entry appears on **Login**, **Register**, and **Accept invitation** when `Enabled` is `true`. **My account** shows **Link** only when `LinkingEnabled` is `true`. Incomplete entries are ignored.                                     |

Per-provider fields:

| Field                               | Required | What it does                                                                               | Impact                                                                                                                                                                                                                                                                                  |
| ----------------------------------- | -------- | ------------------------------------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `ProviderKey`                       | Yes      | Stable id (e.g. `google`, `microsoft`).                                                    | Used in URLs (`/api/auth/external/{providerKey}/begin`) and storage; must be unique per provider.                                                                                                                                                                                       |
| `DisplayName`                       | Yes      | Button label (e.g. `Google`).                                                              | Shown on **Continue with {DisplayName}**.                                                                                                                                                                                                                                               |
| `Authority`                         | Yes      | OIDC issuer URL (metadata at `{Authority}/.well-known/openid-configuration`).              | Token validation and token endpoint discovery.                                                                                                                                                                                                                                          |
| `ClientId`                          | Yes      | OAuth client id.                                                                           | Public; sent in authorize request.                                                                                                                                                                                                                                                      |
| `ClientSecret`                      | Yes      | OAuth client secret.                                                                       | Server-side only; used at token exchange.                                                                                                                                                                                                                                               |
| `AllowedEmailDomains`               | No       | List of allowed domains (e.g. `example.com` or `@example.com`).                            | **Empty** = any verified email domain. **Non-empty** = only emails whose domain matches (case-insensitive) can sign in or link. Others see _Sign-in with this account is not allowed._                                                                                                  |
| `IssuerValidationMode`              | No       | How the API validates the ID token `iss` claim. Default: `Discovery`.                      | `Discovery` — issuer must match OIDC metadata from `{Authority}/.well-known/openid-configuration` (Google, single-tenant Microsoft, generic OIDC). `MicrosoftMultiTenant` — accept any Microsoft Entra tenant issuer; **required** when `Authority` uses `/common` or `/organizations`. |
| `TrustIdpEmailWithoutEmailVerified` | No       | Treat the IdP `email` claim as verified when `email_verified` is absent. Default: `false`. | Set `true` for Microsoft Entra (often omits `email_verified`). Enables `email`, `preferred_username`, and `upn` as verified email sources. Google and most OIDC providers usually leave this `false`.                                                                                   |

**Effective enablement:** `External:Enabled` must be `true` **and** at least one provider must pass `IsConfigured` (all required fields non-empty).

**OIDC discovery:** authorize URL, token endpoint, and (for `Discovery` issuer mode) expected issuer are loaded from `{Authority}/.well-known/openid-configuration`. Configure `Authority`, client credentials, and the optional fields above — not hard-coded endpoint paths.

### 4.12 Passkeys (WebAuthn) (`AuthOptions:Passkeys`)

Product behaviour (screens, gates, mandatory enrollment): `docs/requirements/functional/passkeys/` (start with FR-PKY-001). This section covers **deployment configuration** only.

| Setting                            | What it does                                                                     | Impact                                                                                                                                                                                                            |
| ---------------------------------- | -------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `PasskeysAuthenticationEnabled`    | Master switch.                                                                   | When `false`, passkey UI and ceremonies are off; stored credentials remain in the database.                                                                                                                       |
| `PasskeysAuthenticationRequired`   | Every active account must register at least one passkey.                         | When `true` (and passkeys enabled), compliance gate until enrolled (see FR-PKY-001).                                                                                                                              |
| `PasskeySatisfiesTwoFactor`        | Passkey with user verification satisfies 2FA on that sign-in path.               | Effective only when **both** passkeys and `TwoFactor:Enabled` are `true`.                                                                                                                                         |
| `AllowPasskeyOnlyAccounts`         | Accounts with passkeys but no local password may sign in with passkeys only.     | When `false`, passkey sign-in also requires a local password or linked external provider.                                                                                                                         |
| `DiscoverablePasskeySignInOnLogin` | **Sign in with a passkey** on Login without entering email first.                | When `false`, user must enter email before passkey sign-in.                                                                                                                                                       |
| `OfferPasskeyEnrollmentPrompt`     | Post-sign-in enrollment prompt when passkeys enabled and user has none.          | UI-only; does not block access unless `PasskeysAuthenticationRequired` is `true`.                                                                                                                                 |
| `RelyingPartyId`                   | WebAuthn **RP ID** (registrable domain).                                         | When empty, derived from the **host** of `FrontendBaseUrl` (for example `localhost` or `app.contoso.com`). Set explicitly when the app runs on a subdomain but credentials should be scoped to the parent domain. |
| `RelyingPartyDisplayName`          | Human-readable RP name in browser prompts.                                       | Exposed via `GET /api/auth/settings`.                                                                                                                                                                             |
| `MaximumPasskeysPerUser`           | Cap per account.                                                                 | Registration rejected when limit reached.                                                                                                                                                                         |
| `ChallengeLifetimeMinutes`         | Registration, sign-in, and step-up ceremony lifetime.                            | Expired ceremonies must be restarted.                                                                                                                                                                             |
| `UserVerificationRequired`         | Authenticator must perform user verification (PIN, biometrics, device password). | Maps to WebAuthn **required** user verification.                                                                                                                                                                  |
| `AllowedAuthenticatorAttachment`   | `Platform`, `CrossPlatform`, or `Any`.                                           | Restricts platform vs security-key authenticators.                                                                                                                                                                |
| `AttestationConveyance`            | `None`, `Indirect`, or `Direct`.                                                 | Enterprise attestation policies may use `Direct`.                                                                                                                                                                 |
| `PasskeyStepUpValidityMinutes`     | How long passkey step-up satisfies sensitive actions.                            | Same pattern as external OIDC step-up.                                                                                                                                                                            |
| `MaxFailedPasskeyAttempts`         | Failed attempts per ceremony before invalidation.                                | User must begin a new ceremony.                                                                                                                                                                                   |

#### Origin and RP ID (critical)

The backend builds Fido2 configuration from:

- **Origin** — `AuthOptions:FrontendBaseUrl` (trimmed, no trailing slash). Must match the URL in the browser address bar **exactly** (scheme, host, port).
- **RP ID** — `Passkeys:RelyingPartyId` when set, otherwise the **host** part of `FrontendBaseUrl`.

Examples:

| Frontend URL                                                 | RP ID (when `RelyingPartyId` empty) | Notes                                                                |
| ------------------------------------------------------------ | ----------------------------------- | -------------------------------------------------------------------- |
| `http://localhost:4200`                                      | `localhost`                         | Works for local dev; HTTPS not required on localhost.                |
| `https://app.contoso.com`                                    | `app.contoso.com`                   | Production must use **HTTPS** (browser WebAuthn requirement).        |
| `https://app.contoso.com` with `RelyingPartyId: contoso.com` | `contoso.com`                       | Share passkeys across `app.` and `www.` subdomains when appropriate. |

Also verify:

1. **`CorsOptions:AllowedOrigins`** includes the same frontend origin.
2. **`FrontendBaseUrl`** matches the deployed Angular URL (same as OIDC redirect setup in §6.1).
3. Database migrations applied (`PasskeyCredentials`, `WebAuthnCeremonyPending`, etc.).

Example (enable passkeys locally):

```json
"AuthOptions": {
  "FrontendBaseUrl": "http://localhost:4200",
  "Passkeys": {
    "PasskeysAuthenticationEnabled": true,
    "RelyingPartyDisplayName": "ChangeMe"
  }
}
```

Environment override example: `AuthOptions__Passkeys__PasskeysAuthenticationEnabled=true`.

---

## 5. How options interact (common scenarios)

| Scenario                                          | Recommended settings                                                            | Result                                                                                                                                               |
| ------------------------------------------------- | ------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------- |
| Internal app, passwords only                      | `External:Enabled: false`, 2FA optional or off                                  | Classic email/password only.                                                                                                                         |
| Enterprise SSO + optional password                | Enable one OIDC provider, domain allowlist, `Registration:PublicEnabled: false` | Only existing users or admin-created users link/sign in; new emails without account get _No account exists…_.                                        |
| High security                                     | `TwoFactor:Required: true`, 2FA enabled                                         | All users must set up authenticator after first sign-in (unless IdP MFA trusted on external path).                                                   |
| Google/Microsoft + skip app 2FA when IdP used MFA | `TwoFactor:TrustIdentityProviderMfa: true` + 2FA enabled                        | External sign-in with `amr` containing `mfa` skips app TOTP and mandatory setup; password sign-in still uses app TOTP.                               |
| Phishing-resistant sign-in                        | `Passkeys:PasskeysAuthenticationEnabled: true`                                  | Users enroll passkeys on **My account**; optional mandatory passkeys and 2FA substitution per FR-PKY-001.                                            |
| Public SaaS signup                                | `Registration:PublicEnabled: true`, email verification on                       | Register → verify email → login.                                                                                                                     |
| Lock registration, allow Google                   | `Registration:PublicEnabled: false`, Google OIDC                                | New users only via Google when no account exists; existing emails must sign in with password and link from **My account** if `LinkingEnabled` is on. |
| SSO sign-in, no self-service link                 | `External:Enabled: true`, `External:LinkingEnabled: false`                      | **Continue with …** on Login; no **Link** on **My account**; admins can still unlink on **User details**.                                            |
| No self-service email change                      | `EmailChange:Enabled: false`                                                    | **Change email** hidden; admin **Edit user** email change still available.                                                                           |

---

## 6. Connecting external providers (OIDC)

### 6.1 Prerequisites

1. **HTTPS in production** for frontend and IdP redirects (local dev may use `http://localhost:4200`).
2. **`FrontendBaseUrl`** matches the Angular origin exactly (scheme, host, port).
3. **`CorsOptions:AllowedOrigins`** includes that same origin.
4. Database migrations applied (`ExternalAuthPending`, `ExternalLogin`, etc.).
5. **`EmailOptions`** configured if you rely on link/unlink notification emails.

### 6.2 Redirect URI (critical)

Register this **exact** redirect URI at every identity provider:

```text
{FrontendBaseUrl}{External:SignInCallbackPath}
```

Example (local):

```text
http://localhost:4200/external-sign-in/callback
```

Example (production):

```text
https://app.contoso.com/external-sign-in/callback
```

The backend sends this URI in the authorization request and again at token exchange; a mismatch causes _External sign-in failed_.

### 6.3 Enable providers in configuration

**Step 1 — Turn on the feature:**

```json
"AuthOptions": {
  "FrontendBaseUrl": "http://localhost:4200",
  "EmailChange": {
    "Enabled": true,
    "LinkLifetimeHours": 72
  },
  "External": {
    "Enabled": true,
    "LinkingEnabled": true,
    "Providers": [ ]
  }
}
```

**Step 2 — Add one object per provider** (array index `0`, `1`, … or environment variables `AuthOptions__External__Providers__0__ClientId`, etc.). Use the matching recipe below for your IdP type.

**Step 3 — Restart the API** and open Login; you should see **Continue with {Display name}** for each configured provider.

**Step 4 — Verify** `GET /api/auth/settings` returns `externalProvidersEnabled: true` and the provider list (keys and display names only).

### 6.4 Provider types — quick reference

ChangeMe supports any standards-compliant OIDC provider via discovery. Built-in documentation covers four configuration patterns:

| Pattern                     | Typical IdP                              | `Authority` example                                        | `IssuerValidationMode` | `TrustIdpEmailWithoutEmailVerified`                                |
| --------------------------- | ---------------------------------------- | ---------------------------------------------------------- | ---------------------- | ------------------------------------------------------------------ |
| **Google**                  | Google Cloud OAuth                       | `https://accounts.google.com`                              | `Discovery` (default)  | `false` (default)                                                  |
| **Microsoft single-tenant** | Entra ID — one directory                 | `https://login.microsoftonline.com/<tenant-id>/v2.0`       | `Discovery`            | `true`                                                             |
| **Microsoft multi-tenant**  | Entra ID — `/common` or `/organizations` | `https://login.microsoftonline.com/common/v2.0`            | `MicrosoftMultiTenant` | `true`                                                             |
| **Generic OIDC**            | Keycloak, Auth0, Okta, Duende, etc.      | Issuer URL from the IdP (often ends with `/realms/<name>`) | `Discovery` (default)  | Usually `false`; set `true` only if the IdP omits `email_verified` |

All patterns use the same **redirect URI** (§6.2) and request scopes `openid`, `profile`, `email` automatically. Authorize and token endpoints are discovered from `{Authority}/.well-known/openid-configuration`.

---

### 6.5 Provider configuration examples

For every provider: register the redirect URI from §6.2, create a confidential web/OIDC client, enable **authorization code** flow (ChangeMe sends PKCE S256), allow scopes **`openid`**, **`profile`**, **`email`**, copy **client id** and **client secret** into `appsettings`. Use your IdP's official documentation for console-specific steps — UI changes frequently.

| IdP type        | Official setup docs                                                                                          |
| --------------- | ------------------------------------------------------------------------------------------------------------ |
| Google          | [Google OAuth 2.0 for Web server apps](https://developers.google.com/identity/protocols/oauth2/web-server)   |
| Microsoft Entra | [Register an application](https://learn.microsoft.com/en-us/entra/identity-platform/quickstart-register-app) |
| Generic OIDC    | Your IdP's OIDC/OAuth client guide (Keycloak, Auth0, Okta, Duende, …)                                        |

#### Google

**IdP checklist:** Web OAuth client; redirect URI from §6.2; OAuth consent screen (add test users while app is in **Testing** mode). Google sends `email_verified` on the ID token — leave `TrustIdpEmailWithoutEmailVerified` at default (`false`).

```json
{
  "ProviderKey": "google",
  "DisplayName": "Google",
  "Authority": "https://accounts.google.com",
  "ClientId": "<your-google-client-id>",
  "ClientSecret": "<your-google-client-secret>",
  "AllowedEmailDomains": []
}
```

**Verify:** Restart API → **Login** → **Continue with Google** → confirm email on **My account**.

#### Microsoft Entra ID — single tenant

**IdP checklist:** App registration — **single tenant**; redirect URI from §6.2; client secret; delegated Graph permissions **`openid`**, **`profile`**, **`email`**, **`User.Read`**; optional ID token claim **`email`**. **Authority:** `https://login.microsoftonline.com/<tenant-id>/v2.0` (directory tenant GUID, not `/common`).

```json
{
  "ProviderKey": "microsoft",
  "DisplayName": "Microsoft",
  "Authority": "https://login.microsoftonline.com/<tenant-id>/v2.0",
  "ClientId": "<application-client-id>",
  "ClientSecret": "<client-secret>",
  "AllowedEmailDomains": ["contoso.com"],
  "IssuerValidationMode": "Discovery",
  "TrustIdpEmailWithoutEmailVerified": true
}
```

`AllowedEmailDomains` is optional; use `[]` to allow any verified domain.

**Verify:** Sign in with a tenant user. If _No account exists for this email_, add optional **`email`** claim and confirm `TrustIdpEmailWithoutEmailVerified: true`.

#### Microsoft Entra ID — multi-tenant (`/common` or `/organizations`)

Same checklist as single tenant, except:

- **Supported account types** must match your audience (multitenant and/or personal Microsoft accounts).
- **Authority:** `https://login.microsoftonline.com/common/v2.0` or `.../organizations/v2.0`.
- **`IssuerValidationMode` must be `MicrosoftMultiTenant`** — the ID token `iss` contains the user's tenant GUID, not the `/common` metadata issuer.

```json
{
  "ProviderKey": "microsoft",
  "DisplayName": "Microsoft",
  "Authority": "https://login.microsoftonline.com/common/v2.0",
  "ClientId": "<application-client-id>",
  "ClientSecret": "<client-secret>",
  "AllowedEmailDomains": [],
  "IssuerValidationMode": "MicrosoftMultiTenant",
  "TrustIdpEmailWithoutEmailVerified": true
}
```

| Error                                                      | Fix                                                                                                    |
| ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------------ |
| AADSTS50194 / single-tenant app with `/common`             | Use tenant-specific authority (above) **or** change app to multi-tenant.                               |
| _External sign-in failed_ after successful Microsoft login | Set `IssuerValidationMode` to `MicrosoftMultiTenant` for `/common` / `/organizations`.                 |
| _No account exists for this email_                         | Set `TrustIdpEmailWithoutEmailVerified: true`; add **`email`** optional claim on the app registration. |

#### Generic OIDC (Keycloak, Auth0, Okta, …)

**IdP checklist:** Confidential web client; redirect URI from §6.2; authorization code + PKCE; scopes **`openid`**, **`profile`**, **`email`**; issuer URL exposes `/.well-known/openid-configuration`. ID token should include **`email`** and preferably **`email_verified: true`**. If the IdP never sends `email_verified`, set `TrustIdpEmailWithoutEmailVerified: true`.

**Authority examples:** Keycloak `https://<host>/realms/<realm>`; Auth0 `https://<tenant>.auth0.com/` (or custom domain issuer from app settings).

When the API runs in Docker and the IdP runs on the host or another container, set **Authority** to a URL the **backend container** can reach (e.g. `http://keycloak:8080/realms/my-realm` on the Compose network, not `localhost`).

```json
{
  "ProviderKey": "keycloak",
  "DisplayName": "Keycloak",
  "Authority": "https://idp.example.com/realms/my-realm",
  "ClientId": "<client-id>",
  "ClientSecret": "<client-secret>",
  "AllowedEmailDomains": []
}
```

**Verify:** Restart API → **Continue with {Display name}** → confirm sign-in completes and email appears on the account.

---

### 6.9 Email domain allowlist

```json
"AllowedEmailDomains": ["contoso.com", "subsidiary.com"]
```

- Only addresses ending with `@contoso.com` or `@subsidiary.com` (case-insensitive) can complete external sign-in or link.
- Use for workforce tenants where the IdP may return many domains but the app should only accept corporate email.

### 6.10 Trust identity provider MFA

Set `TrustIdentityProviderMfa` to `true` when:

- 2FA is **enabled** in ChangeMe, and
- You want users who already completed MFA at Google/Microsoft to **skip** app TOTP and mandatory app enrollment on that sign-in.

**Detection:** ID token claim `amr` must include `mfa` (Google and Microsoft commonly send this).

**Does not apply to:** password sign-in (app TOTP always required when user has 2FA enabled).

### 6.11 Disabling external providers later

Set `External:Enabled` to `false` and restart.

| User type                   | Effect                                                                                     |
| --------------------------- | ------------------------------------------------------------------------------------------ |
| Has password                | Can still use **Login** with email/password.                                               |
| External-only (no password) | Cannot sign in until providers re-enabled or an admin helps set a password / local access. |
| Linked providers in DB      | Rows retained; sign-in and linking UI hidden until re-enabled.                             |

To keep OIDC sign-in but stop users from adding providers themselves, leave `External:Enabled` **true** and set `External:LinkingEnabled` to **false**.

### 6.12 External sign-in and linking (operator summary)

Guest **Continue with {Provider}** on **Login**, **Register**, or **Accept invitation** does **not** attach a provider to an existing account (except invitation onboarding when provider email matches the invited **Profile email** — FR-AUTH-010). Linking is only from **My account** when `External:LinkingEnabled` is **true**.

| Event                                                                   | Behaviour                                                                                                                                         |
| ----------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| (**Provider key**, **subject**) already linked                          | Signs in that user. **Provider email** from the token may differ from **Profile email**; sign-in does not change **Profile email**.               |
| Subject not linked; account exists with same normalized provider email  | Redirect to **Login** with: sign in with password, then link from **My account**. No guest **Link external account** screen.                      |
| Subject not linked; no account; public registration enabled             | Creates a new **external-only** user; initial **Profile email** = verified provider email.                                                        |
| Subject not linked; no account; public registration disabled            | _No account exists for this email…_                                                                                                               |
| **Accept invitation**; provider email matches invited **Profile email** | Completes invitation and links provider (FR-AUTH-010).                                                                                            |
| **Accept invitation**; provider email differs from invited email        | _The external account email does not match the invited email address._                                                                            |
| Signed-in user links another provider (`LinkingEnabled: true`)          | **My account** → step-up → optional confirmation when provider email ≠ **Profile email** → OIDC link mode. Provider email match **not** required. |
| `LinkingEnabled: false`                                                 | **Link** hidden; existing links still sign in; **Unlink** still available (subject to last-method rules).                                         |
| Same (**Provider key**, **subject**) on another user                    | _This external account is already linked to another user._                                                                                        |
| Different addresses (e.g. `jan@firma.pl` vs `jan@gmail.com`)            | Treated as **different** emails: separate accounts or “account already exists” per table above — not merged automatically.                        |

Changing **Profile email** in **Edit user** or via self-service **Change email** (`EmailChange:Enabled`) does **not** remove **External login** rows. Sign-in stays tied to (**Provider key**, **subject**), not email match.

### 6.13 Secrets and production

- Store `ClientSecret` and `Jwt:SigningKey` in a secret manager or environment variables, not in source control.
- Example override: `AuthOptions__External__Providers__0__ClientSecret=<secret>`.
- Rotate secrets at the IdP and update configuration together.

---

## 7. Initial administrator

Configured under `InitialAdministratorOptions` (not inside `AuthOptions`):

| Field                                        | Purpose                                      |
| -------------------------------------------- | -------------------------------------------- |
| `Email`, `Password`, `FirstName`, `LastName` | Created on first startup if no admin exists. |

The initial admin follows the same auth rules as other users (2FA, password expiration, etc.) when those features are enabled.

---

## 8. Email dependency

Auth flows that send email:

| Flow                               | Email template area           |
| ---------------------------------- | ----------------------------- |
| Email verification                 | Register with verification on |
| Password reset                     | Forgot password               |
| User invitation                    | Admin **Invite user**         |
| 2FA enabled / disabled / reset     | My account and admin reset    |
| Recovery code used                 | 2FA verification or step-up   |
| External account linked / unlinked | My account / admin unlink     |

Local Docker stack uses **MailHog** (see `docs/technical/database-and-docker.md`). Without SMTP, users stay unverified or never receive links.

---

## 9. Troubleshooting

| Symptom                                                 | Likely cause                                         | Action                                                                                                                 |
| ------------------------------------------------------- | ---------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| No **Continue with …** buttons                          | `External:Enabled` false or provider incomplete      | Check settings; call `GET /api/auth/settings`.                                                                         |
| _External sign-in failed_                               | Redirect URI mismatch, wrong secret, clock skew      | Compare IdP redirect URI with §6.2; check client secret and authority URL.                                             |
| _Sign-in with this account is not allowed_              | Email domain not in allowlist                        | Adjust `AllowedEmailDomains` or user’s IdP email.                                                                      |
| _External sign-in failed_ (after Microsoft login)       | Wrong `IssuerValidationMode` for `/common` authority | Set `IssuerValidationMode` to `MicrosoftMultiTenant` when using `/common` or `/organizations`.                         |
| _No account exists for this email_                      | Registration disabled and no user row                | **Invite user** in admin UI or enable public registration.                                                             |
| _No account exists for this email_ (Microsoft)          | Email claim not verified / missing optional claims   | Set `TrustIdpEmailWithoutEmailVerified: true`; add **`email`** optional claim (§6.5).                                  |
| _Complete your account setup using the invitation link_ | Pending account invitation (no password sign-in yet) | User must complete **Accept invitation** (email link) or external sign-in with matching verified email when enabled.   |
| _Verify your email before signing in_                   | `EmailVerification:Enabled`                          | User verifies email or admin marks verified.                                                                           |
| Stuck on two-factor setup                               | `TwoFactor:Required`                                 | User completes setup on **Required two-factor setup** or **My account**.                                               |
| External-only cannot unlink                             | Last sign-in method                                  | User must **Set password** first (after external step-up).                                                             |
| CORS errors on login                                    | Frontend origin not allowed                          | Add origin to `CorsOptions:AllowedOrigins`.                                                                            |
| Passkey ceremony fails / _NotAllowedError_              | Origin or RP ID mismatch                             | Match `FrontendBaseUrl` to browser URL; set `RelyingPartyId` for subdomain scenarios (§4.12). Use HTTPS in production. |
| Passkey option missing on Login                         | `PasskeysAuthenticationEnabled: false`               | Enable in config; confirm `GET /api/auth/settings` → `passkeys.passkeysAuthenticationEnabled`.                         |
| Passkey works on one machine only                       | Platform authenticator bound to device               | Expected for device passkeys; use cross-platform security key or enroll on each device.                                |
| Settings change not visible                             | Cached app / no restart                              | Hard refresh frontend; restart API.                                                                                    |

---

## 10. Security notes

- Authorization code flow with **PKCE (S256)**, **state**, and **nonce** are enforced for OIDC.
- ID tokens are validated (issuer, audience, signature, expiry, nonce) before trusting claims.
- Auto-registration and email matching use **verified** email from the IdP (`email_verified`, or `TrustIdpEmailWithoutEmailVerified` when configured); unverified emails are not used to find or create accounts.
- Client secrets and signing keys must never be committed to git or exposed to the browser.
- Use TLS in production for frontend, API, and IdP redirects.

---

## 11. Local verification checklist

1. Start stack: `docker compose up` (or `npm run start:all`).
2. Apply migrations (`npm run ef:database:update` or `Database:ApplyMigrationsOnStartup: true` in Development).
3. Configure `AuthOptions` and `EmailOptions` in `appsettings.Development.json`.
4. Open `http://localhost:4200/login` — confirm UI matches flags (register link, external buttons, etc.).
5. Call `GET https://localhost:<port>/api/auth/settings` — confirm JSON matches configuration.
6. Complete one password sign-in and, if configured, one external sign-in through the IdP test tenant.

For automated regression, run `npm run test:backend:integration` (requires Docker for Testcontainers).
