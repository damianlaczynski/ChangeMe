---
id: REQ-AUTH-004
title: My Sessions
domain: identity
status: active
depends_on: [REQ-AUTH-003, REQ-USR-001]
---
## Goal

The user must be able to review active sign-in sessions and revoke sessions they no longer trust.

## Features

### Active sessions on My account

- Section on **My account** (REQ-USR-001), not a separate screen.
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
- The sessions table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- While the list is loading, a loading indicator is shown in the list area; the rest of the screen remains visible.

### Actions

- **Revoke** on a non-current row opens confirmation dialog: **`Revoke this session? That device will be signed out.`**
- On confirm, that session is revoked and the row is removed from the list without reloading the entire screen.
- The **Revoke** button is **not shown** on the **Current session** row; the user signs out the current browser via **Logout** (REQ-AUTH-003).
- **Sign out everywhere** is a header action (REQ-AUTH-003); requires **Sessions.ManageOwn**.

### Permissions and visibility

- **Sessions.ViewOwn**: required to show the **Active sessions** section and view the list.
- **Sessions.ManageOwn**: required for **Revoke** on non-current rows and **Sign out everywhere**.

---
