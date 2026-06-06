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

## Issues and projects

| Term               | Meaning                                                                                                                                                                                    |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Project**        | A named container for issues. Every issue belongs to exactly one project (REQ-PRJ-001). The seeded **Default** project is a system project that cannot be edited or deleted.               |
| **Project member** | A user assigned to a project with exactly one **project role** (**Owner**, **Member**, or **Viewer**). Membership determines resource-scoped project permissions (REQ-PRJ-005).            |
| **Project role**   | Fixed role on one project: **Owner** (full project access), **Member** (view and manage issues), or **Viewer** (read-only). A user may hold different project roles on different projects. |
| **Time entry**     | A record of work performed by one user: required **Project**, optional **Issue**, **Work date**, **Duration** (whole minutes), and optional **Description**. See REQ-TIM-001.              |
| **Running timer**  | A single active stopwatch per signed-in user that pre-fills **Duration** on **Log time** when stopped. See REQ-TIM-002.                                                                    |

## Employment and billing

| Term                         | Meaning                                                                                                                                                                                                    |
| ---------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Position**                 | A named organizational role (for example **Developer**, **Project manager**) used on employment contracts. Managed in the position catalog (REQ-BIL-002).                                                  |
| **Employment contract**      | A date-bounded agreement linking a user to a **Position** with **Contract type**, **FTE**, **Monthly hours norm**, and compensation (**Hourly rate** or **Monthly salary**). See REQ-BIL-001, REQ-BIL-003. |
| **Contract type**            | One of **`Employment`**, **`Mandate`**, **`Work contract`**, or **`B2B`** — determines how the contract is labeled in settlements and reports.                                                             |
| **Leave type**               | A category of absence (for example **Vacation**, **Sick leave**) with rules for paid time and allowance consumption (REQ-BIL-004).                                                                         |
| **Leave request**            | A user's planned or taken absence with **Start date**, **End date**, optional half-day **Day portion**, and approval **Status** (REQ-BIL-005).                                                             |
| **Leave allowance**          | Annual vacation entitlement in days derived from **Default annual leave days** and contract **FTE** (REQ-BIL-004).                                                                                         |
| **Settlement period**        | One calendar month for which the system calculates **user settlements** from contracts, logged time, and approved leave (REQ-BIL-007).                                                                     |
| **User settlement**          | Per-user monthly summary: **Expected minutes**, **Logged minutes**, **Leave days**, and **Balance minutes** (overtime or undertime). See REQ-BIL-001.                                                      |
| **Availability entry**       | A date or time range declaring **Availability status** (**Available**, **Unavailable**, **Remote**, **On-site**) for one user. **Source** is **Manual**, **Recurring**, or **Leave** (REQ-BIL-010).        |
| **Weekly recurring pattern** | Seven-row template (Monday–Sunday) defining baseline availability; generates **Recurring** entries (REQ-BIL-010).                                                                                          |
| **Availability calendar**    | Combined view of **Leave**, **Manual**, and **Recurring** entries per user and day; **My availability** (REQ-BIL-011) or team **Availability calendar** (REQ-BIL-012).                                     |
| **Default work hours**       | Organization-wide billing settings (**Default workday start/end**, **Half-day split time**, **Default workdays**, **Default availability status**) used to seed new weekly patterns (REQ-BIL-004).         |

## Cross-references

- Invitation flows: `docs/req/invitations/`
- Invitation acceptance: REQ-AUTH-010
- External sign-in: REQ-AUTH-014
- Email verification: REQ-AUTH-011
- Self-service email change: REQ-AUTH-015
- Passkeys: `docs/req/passkeys/`
- Projects: `docs/req/projects/`
- Time tracking: `docs/req/time/`
- Billing and settlements: `docs/req/billing/`
- User **Status** rules: REQ-INV-005
- Account model (admin UI): `docs/req/_shared/account-model.md`
