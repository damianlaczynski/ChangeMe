---
id: FR-PKY-003
title: Passkey Enrollment and My Account Management
domain: passkeys
type: functional
status: active
depends_on: [FR-AUTH-014, FR-PKY-004, FR-PKY-006, FR-PKY-007, FR-USR-001]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Signed-in users must be able to register, name, and remove passkeys on their account when the feature is enabled, without compromising the rule that every account retains at least one viable sign-in method.

## Functional requirements

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

- Collapsible section **Passkeys** on **My account** (FR-USR-001); shown only when **Passkeys authentication enabled** is **true**; default **collapsed**.
- Section lists all **Passkey credential** rows: **Name** (editable inline or via rename dialog), **Created at**, **Last used at**, **Authenticator type**, **Remove** action.
- Empty state: **`No passkeys registered.`** and description **`Passkeys let you sign in with your device PIN, fingerprint, or face.`**
- Primary action **Add passkey** opens **Add passkey** flow.

### Add passkey flow

- **Add passkey** requires an active full session (not enrollment bootstrap for two-factor or passkey only).
- Before starting the WebAuthn registration ceremony, the user must complete **Sensitive account actions** step-up (FR-PKY-004) when any step-up rule applies.
- The system issues a **registration challenge**; the browser creates the credential; the server verifies attestation per deployment **Attestation conveyance** and stores the **Passkey credential**.
- After success, show dialog **Name your passkey** with field **Name** (**required**; max **100** characters; default as above); **Save** persists the name and shows message **`Passkey added.`**
- Sends **Passkey added** email (FR-PKY-007).
- When the user already has **Maximum passkeys per user** credentials, **Add passkey** is disabled with message **`Maximum number of passkeys reached. Remove one before adding another.`**

### Rename passkey

- **Rename** action on each row; requires **Sensitive account actions** step-up (FR-PKY-004).
- **Name** max **100** characters; on success message **`Passkey renamed.`**

### Remove passkey

- **Remove** opens confirmation **`Remove passkey "{name}" from your account?`**
- Requires **Sensitive account actions** step-up (FR-PKY-004).
- **Remove** is blocked when it would leave the user with **no sign-in method**:
  - **No local password**, **no external login**, and this is the **only** passkey → message **`Add a password or external sign-in before removing your only pass-in method.`** with links to **Set password** (FR-AUTH-014) when applicable and external linking when enabled.
  - When **Passkeys authentication required** is **true** and this is the **only** passkey but other sign-in methods exist → message **`At least one passkey is required. Add another passkey before removing this one.`**
- On success: message **`Passkey removed.`**; sends **Passkey removed** email (FR-PKY-007).

### Strict passkey setup (signed-in enrollment)

- When **Passkeys authentication required** is **true** and the user has zero passkeys, **strict passkey setup** (FR-PKY-006) uses the same **Add passkey** ceremony without step-up (first credential).
- After the first passkey is registered during strict setup, the application opens **Issues list** subject to remaining gates (two-factor, password expiration).

### States and business rules

- Adding a passkey does not disable two-factor, change password, or unlink external providers.
- Removing all passkeys while signed in is allowed only when another sign-in method remains and passkeys are not required.
- **Out of scope:** administrator registering a passkey on behalf of another user; exporting private keys; shared team passkeys.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
