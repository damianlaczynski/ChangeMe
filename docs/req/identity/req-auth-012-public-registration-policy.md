---
id: REQ-AUTH-012
title: Public Registration Policy
domain: identity
status: active
depends_on: [REQ-AUTH-001, REQ-AUTH-006, REQ-AUTH-010, REQ-AUTH-011, REQ-INV-001, REQ-ROL-006, REQ-USR-003]
---
## Goal

Deployments must be able to turn off self-service account registration so new users are onboarded only by administrators.

## Features

### Public registration policy

- Deployment settings include **Public registration enabled**; default **true**.
- When **Public registration enabled** is **true**, behavior matches REQ-AUTH-001 (**Register** screen, **Create an account** link, registration API).
- When **Public registration enabled** is **false**, guests cannot create accounts; administrators onboard users via **Invite user** (REQ-INV-001).

### Login screen

- Footer link **Create an account** → **Register** is **not shown** when **Public registration enabled** is **false**.

### Register screen and registration API

- When **Public registration enabled** is **false**:
  - the **Register** route is not available to guests;
  - a guest who opens the register URL is redirected to **Login** with message **`Registration is disabled. Contact an administrator.`**;
  - registration API requests are rejected (for example **403 Forbidden**) with a clear error; the UI does not expose registration when disabled.

### Unaffected flows when registration is disabled

| Flow                                                    | Still available                                        |
| ------------------------------------------------------- | ------------------------------------------------------ |
| **Login** (REQ-AUTH-001)                                | Yes                                                    |
| **Forgot password** / **Reset password** (REQ-AUTH-006) | Yes                                                    |
| **Accept invitation** (REQ-AUTH-010)                    | Yes                                                    |
| **Verify email** (REQ-AUTH-011)                         | Yes — for existing unverified self-registered accounts |
| **Admin create user** (REQ-USR-003)                     | Yes                                                    |

### Interaction with other auth flows

| Flow                                          | When **Public registration enabled** is **false**                                     |
| --------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Register** (REQ-AUTH-001)                   | Unavailable                                                                           |
| **Verify email** (REQ-AUTH-011)               | Still available for accounts created before disable or while registration was enabled |
| **Email verification enabled** (REQ-AUTH-011) | Independent; admin invite still sets **Email verified** at send (REQ-USR-003)         |
| **Default User role** (REQ-ROL-006)           | Assigned on admin invite and on registration when enabled                             |

### States and business rules

- Disabling public registration does not affect existing accounts, sessions, or pending invitation or verification links.
- **Out of scope for this REQ:** admin UI to change **Public registration enabled** at runtime (setting is deployment configuration only, same as other auth policy flags).

---
