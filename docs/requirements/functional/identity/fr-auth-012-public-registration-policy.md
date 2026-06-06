---
id: FR-AUTH-012
title: Public Registration Policy
domain: identity
type: functional
status: active
depends_on:
  [
    FR-AUTH-001,
    FR-AUTH-006,
    FR-AUTH-010,
    FR-AUTH-011,
    FR-INV-001,
    FR-ROL-006,
    FR-USR-003,
  ]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Deployments must be able to turn off self-service account registration so new users are onboarded only by administrators.

## Functional requirements

### Public registration policy

- Deployment settings include **Public registration enabled**; default **true**.
- When **Public registration enabled** is **true**, behavior matches FR-AUTH-001 (**Register** screen, **Create an account** link, registration API).
- When **Public registration enabled** is **false**, guests cannot create accounts; administrators onboard users via **Invite user** (FR-INV-001).

### Login screen

- Footer link **Create an account** → **Register** is **not shown** when **Public registration enabled** is **false**.

### Register screen and registration API

- When **Public registration enabled** is **false**:
  - the **Register** route is not available to guests;
  - a guest who opens the register URL is redirected to **Login** with message **`Registration is disabled. Contact an administrator.`**;
  - registration API requests are rejected (for example **403 Forbidden**) with a clear error; the UI does not expose registration when disabled.

### Unaffected flows when registration is disabled

| Flow                                                   | Still available                                        |
| ------------------------------------------------------ | ------------------------------------------------------ |
| **Login** (FR-AUTH-001)                                | Yes                                                    |
| **Forgot password** / **Reset password** (FR-AUTH-006) | Yes                                                    |
| **Accept invitation** (FR-AUTH-010)                    | Yes                                                    |
| **Verify email** (FR-AUTH-011)                         | Yes — for existing unverified self-registered accounts |
| **Admin create user** (FR-USR-003)                     | Yes                                                    |

### Interaction with other auth flows

| Flow                                         | When **Public registration enabled** is **false**                                     |
| -------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Register** (FR-AUTH-001)                   | Unavailable                                                                           |
| **Verify email** (FR-AUTH-011)               | Still available for accounts created before disable or while registration was enabled |
| **Email verification enabled** (FR-AUTH-011) | Independent; admin invite still sets **Email verified** at send (FR-USR-003)          |
| **Default User role** (FR-ROL-006)           | Assigned on admin invite and on registration when enabled                             |

### States and business rules

- Disabling public registration does not affect existing accounts, sessions, or pending invitation or verification links.
- **Out of scope:** admin UI to change **Public registration enabled** at runtime (setting is deployment configuration only, same as other auth policy flags).

---

## Acceptance scenarios

| ID             | Given                                                              | When                                                                                    | Then                                                                                                          |
| -------------- | ------------------------------------------------------------------ | --------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------- |
| AC-AUTH-012-01 | **Public registration enabled** is **false**; guest on **Login**   | User views footer links                                                                 | **Create an account** link to **Register** is **not shown**                                                   |
| AC-AUTH-012-02 | **Public registration enabled** is **false**; guest user           | Guest opens the **Register** URL directly                                               | Redirected to **Login** with message `Registration is disabled. Contact an administrator.`                    |
| AC-AUTH-012-03 | **Public registration enabled** is **false**                       | Guest submits registration API request                                                  | Request rejected (for example **403 Forbidden**); UI does not expose registration                             |
| AC-AUTH-012-04 | **Public registration enabled** is **true**                        | Guest uses **Register** (FR-AUTH-001)                                                   | Registration behavior matches FR-AUTH-001 (**Register** screen, **Create an account** link, registration API) |
| AC-AUTH-012-05 | **Public registration enabled** is **false**                       | Guest uses **Login**, **Forgot password**, **Reset password**, or **Accept invitation** | Those flows remain available per unaffected-flows table                                                       |
| AC-AUTH-012-06 | **Public registration enabled** changed from **true** to **false** | Existing accounts, sessions, and pending invitation or verification links               | Unaffected; disabling does not invalidate existing state                                                      |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
