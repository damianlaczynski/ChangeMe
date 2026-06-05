# Glossary — account and sign-in terms

> Single source of truth for cross-cutting business terms. REQ files reference this document instead of duplicating definitions.

The following terms describe observable account state, not implementation details.

## Account lifecycle

| Term                               | Meaning                                                                                                                                                                                                                                                                                                                             |
| ---------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Account enabled**                | The user is not deactivated by an administrator and may sign in when other rules allow.                                                                                                                                                                                                                                             |
| **Account deactivated**            | An administrator disabled the account; the user cannot sign in and has no effective permissions until reactivated.                                                                                                                                                                                                                  |
| **Account invitation**             | One invitation send recorded for a user: **sent at**, lifecycle **pending** (active), **revoked** (superseded by **Resend invitation** or **Cancel invitation**), or **accepted** (onboarding complete; kept for history). **Revoked** rows are purged after the configured retention period (default **7** days). See REQ-INV-006. |
| **Awaiting invitation acceptance** | The user has a **pending** account invitation and has **not** completed onboarding. They are not yet an active application user (no local password and no completed external onboarding). Sign-in and acceptance: REQ-AUTH-010, REQ-AUTH-014.                                                                                       |
| **Invitation expired**             | The current invitation **link** is missing, already used, or past its expiry. This is independent of whether a pending invitation still exists in the system. UI may show **`Expired`** while **Resend invitation** remains available (REQ-INV-003).                                                                                |
| **Invite user**                    | Administrator action that creates an account and sends the first **Account invitation** email. UI label **`Invite user`** (not **Create user**).                                                                                                                                                                                    |
| **Cancel invitation**              | Administrator action that ends the pending invitation without sending a replacement: the user remains in the directory without access until invited again or otherwise managed.                                                                                                                                                     |

## Sign-in methods

| Term                      | Meaning                                                                                                                                                                                                                                                                                    |
| ------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Local password**        | A password stored in ChangeMe for email/password sign-in. A user **with a local password** has completed invitation acceptance with a password, self-registration, or **Set password** on **My account**.                                                                                  |
| **External-only account** | The user can sign in through one or more linked external providers but **has no local password yet** and is **not** awaiting invitation acceptance (for example after invitation acceptance via OIDC, self-service registration via an IdP, or when no administrator invitation was sent). |
| **Two-factor enrolled**   | The user completed authenticator setup; password sign-in requires a verification code unless external **Trust identity provider MFA** applies on that sign-in.                                                                                                                             |
| **Passkey enrolled**      | The user has at least one **Passkey credential** (REQ-PKY-003). Passkey sign-in is available when **Passkeys authentication enabled** is **true** (REQ-PKY-001).                                                                                                                           |
| **Passkey-only account**  | The user has at least one passkey, **no local password**, and **no external login**; allowed only when **Allow passkey-only accounts** is **true** (REQ-PKY-001).                                                                                                                          |

## Email and profile

| Term               | Meaning                                                                                                                                                                                                                                                                |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Email verified** | When email verification is enabled in deployment settings, the user proved control of the mailbox (verification link, invitation acceptance, or administrator confirmation). When verification is disabled, every account is treated as verified for sign-in purposes. |
| **Profile email**  | The **current email** on the ChangeMe account; used for sign-in, display, and all notifications (REQ-AUTH-014). Shown as **Email** on **My account** and admin screens.                                                                                                |

## Cross-references

- Invitation flows: `docs/req/invitations/`
- Invitation acceptance: REQ-AUTH-010
- External sign-in: REQ-AUTH-014
- Email verification: REQ-AUTH-011
- Self-service email change: REQ-AUTH-015
- Passkeys: `docs/req/passkeys/`
- User **Status** rules: REQ-INV-005
- Account model (admin UI): `docs/req/_shared/account-model.md`
