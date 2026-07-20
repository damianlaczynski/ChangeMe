---
id: FR-AUTH-004
title: My Sessions
domain: identity
type: functional
status: active
depends_on: [FR-AUTH-003, FR-USR-001]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to review active sign-in sessions and revoke sessions they no longer trust.

## Functional requirements

### Active sessions on My account

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) unless stated below.
- Section on **My account** (FR-USR-001), not a separate screen.
- Section title: **Active sessions**; collapsible panel; default **collapsed**.
- Requires permission **Sessions.ViewOwn**.

| Column               | Description                                                                                  |
| -------------------- | -------------------------------------------------------------------------------------------- |
| **Device / browser** | Label in format **`{Browser} on {Platform}`** (for example **`Chrome on Windows`**).         |
| **IP address**       | Secondary line under device/browser; shows session IP or **`Unknown`**.                      |
| **Signed in at**     | Session start date and time.                                                                 |
| **Last activity**    | Date and time of last credential renewal or authenticated activity.                          |
| **Current**          | Badge **`Current session`** on the row for the browser the user is signed in with.           |
| **Actions**          | **Revoke** button on every row except the current session (requires **Sessions.ManageOwn**). |

- The list shows **active sessions only**; revoked sessions do not appear.
- Empty state: **`No active sessions.`**

### Actions

- **Revoke** on a non-current row opens confirmation dialog: **`Revoke this session? That device will be signed out.`**
- On confirm, that session is revoked and the row is removed from the list without reloading the entire screen.
- The **Revoke** button is **not shown** on the **Current session** row; the user signs out the current browser via **Logout** (FR-AUTH-003).
- **Sign out everywhere** is a header action (FR-AUTH-003); requires **Sessions.ManageOwn**.

### Permissions and visibility

- **Sessions.ViewOwn**: required to show the **Active sessions** section and view the list.
- **Sessions.ManageOwn**: required for **Revoke** on non-current rows and **Sign out everywhere**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
