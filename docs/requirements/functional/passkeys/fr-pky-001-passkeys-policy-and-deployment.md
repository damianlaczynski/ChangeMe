---
id: FR-PKY-001
title: Passkeys Policy and Deployment
domain: passkeys
type: functional
status: active
depends_on:
  [FR-AUTH-005, FR-AUTH-009, FR-AUTH-013, FR-AUTH-014, FR-PKY-003, FR-PKY-006]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Deployments must be able to enable passkey authentication, optionally require every active account to register at least one passkey, and control how passkeys interact with two-factor authentication and password-based sign-in.

## Functional requirements

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
| User is **external-only** with ≥1 passkey               | **Yes** (external sign-in remains available per FR-AUTH-014)              | **Yes**                                                                  |
| User has only passkeys (no password, no external login) | **No**                                                                    | **Yes**                                                                  |
| User is **awaiting invitation acceptance**              | **No** (complete invitation first)                                        | **No**                                                                   |
| User has zero passkeys                                  | **No** (enrollment only while signed in)                                  | **No**                                                                   |

- **Passkey-only account** means the user has at least one passkey, **no local password**, and **no external login** rows. Such accounts are allowed only when **Allow passkey-only accounts** is **true**.
- Registering the first passkey does **not** remove an existing **local password** or **external login** unless the user explicitly removes those methods per FR-AUTH-005, FR-AUTH-014, FR-PKY-003.

### Passkey satisfies two-factor

- When **Passkey satisfies two-factor** is **false**, passkey sign-in counts as **primary authentication** only (same role as password or external provider). If **Two-factor enabled** is **true**, the user proceeds to **Two-factor verification** after a successful passkey sign-in unless another gate applies first.
- When **Passkey satisfies two-factor** is **true** and the passkey assertion includes **user verification**:
  - Skip **Two-factor verification** for that sign-in when **Two-factor enabled** is **true**.
  - Skip **strict two-factor setup** when **Two-factor authentication required** is **true** and **Two-factor enabled** is **false**.
- When **Passkey satisfies two-factor** is **true** but the assertion does **not** include user verification, passkey sign-in counts as **primary authentication** only and **normal two-factor rules apply** (proceed to **Two-factor verification** when **Two-factor enabled** is **true** on the account, same as when **Passkey satisfies two-factor** is **false**).
- **Passkey satisfies two-factor** never disables stored TOTP enrollment; password sign-in continues to require app TOTP when **Two-factor enabled** is **true** (FR-AUTH-013).

### Deployment policy changes

Deployment settings are read on each sign-in, session refresh, and authenticated request (same model as FR-AUTH-009 and FR-AUTH-013).

| Setting change                                                               | Effect                                                                                                                                                                                      |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Passkeys authentication enabled** **false** → **true**, required **false** | Passkeys become available on **My account** and **Login** per policy; no forced enrollment.                                                                                                 |
| **Passkeys authentication required** turned **on**                           | Users without a passkey (except **awaiting invitation acceptance**) receive **`passkeySetupRequired`** on next refresh or blocked API; client enters **strict passkey setup** (FR-PKY-006). |
| **Passkeys authentication enabled** **true** → **false**                     | Enforcement stops; credentials remain stored but inactive; **strict passkey setup** ends immediately.                                                                                       |
| **Passkey satisfies two-factor** toggled                                     | Affects the next passkey sign-in only.                                                                                                                                                      |
| **Allow passkey-only accounts** **true** → **false**                         | Users who are passkey-only cannot sign in until they **Set password** (FR-AUTH-014) or link an external provider; existing passkeys remain on the account.                                  |

### States and business rules

- Disabling passkeys deployment-wide does not delete stored credentials; re-enabling restores usability.
- **Out of scope:** admin UI to change passkey deployment flags at runtime; per-role passkey requirement; hardware security key inventory UI beyond credential list; syncing passkeys across browsers without per-browser enrollment.

---

## Acceptance scenarios

| ID            | Given                                                                                                                                                  | When                                                                          | Then                                                                                                                 |
| ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ | ----------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| AC-PKY-001-01 | **Passkeys authentication enabled** is **true**; user has **local password** and at least one passkey                                                  | User views public auth settings or **Login**                                  | **Sign in with a passkey** and **Passkeys** section on **My account** are available per policy                       |
| AC-PKY-001-02 | **Passkeys authentication enabled** is **true**; **Allow passkey-only accounts** is **false**; user has only passkeys (no password, no external login) | User attempts passkey sign-in                                                 | Sign-in blocked with `Set a password or use external sign-in before using passkeys on this account.`                 |
| AC-PKY-001-03 | **Passkeys authentication enabled** is **true**; **Allow passkey-only accounts** is **true**; user has only passkeys                                   | User attempts passkey sign-in                                                 | Passkey sign-in allowed                                                                                              |
| AC-PKY-001-04 | **Passkeys authentication enabled** is **true**; **Passkey satisfies two-factor** is **true**; account **Two-factor enabled** is **true**              | User completes passkey sign-in with assertion including **user verification** | **Two-factor verification** skipped for that sign-in (FR-AUTH-013)                                                   |
| AC-PKY-001-05 | **Passkeys authentication enabled** is **true**; **Passkey satisfies two-factor** is **false**; account **Two-factor enabled** is **true**             | User completes passkey primary authentication                                 | User proceeds to **Two-factor verification** after passkey sign-in                                                   |
| AC-PKY-001-06 | **Passkeys authentication required** turned **on** while user has zero passkeys (not **awaiting invitation acceptance**)                               | Next session refresh or blocked API response                                  | Client receives **`passkeySetupRequired`** and enters **strict passkey setup** (FR-PKY-006)                          |
| AC-PKY-001-07 | **Passkeys authentication enabled** changed **true** → **false**                                                                                       | Signed-in user affected by policy                                             | Enforcement stops immediately; stored credentials remain but inactive; **strict passkey setup** ends                 |
| AC-PKY-001-08 | **Allow passkey-only accounts** changed **true** → **false**; user is passkey-only                                                                     | User attempts sign-in                                                         | Cannot sign in until **Set password** (FR-AUTH-014) or external provider linked; existing passkeys remain on account |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
