---
id: FR-ISS-004
title: Watching Issues and Push / Email Notifications
domain: issues
type: functional
status: active
depends_on: [FR-ISS-002, FR-ISS-006]
inherits_nfr: [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---
## Goal

The user must be able to watch selected issues and receive in-app push notifications and email about related activity.

## Functional requirements

### Watch management

- Start watching from **Issues list** and **Issue details**.
- Stop watching from the same places.
- Watch state is stored per user and per issue; duplicate watches for the same user and issue are not created.
- Watch button shows **watcher count** and whether the current user watches the issue.

### Events that generate notifications

Notifications are sent to watchers (excluding the acting user) for:

- comment creation,
- status change (including **issue closed** and **issue reopened**),
- priority change,
- assignee change,
- title edit,
- description edit,
- acceptance-criterion add, update, and remove,
- attachment add and remove (FR-ISS-006).
- Watch and unwatch update watcher counts on the screen where the action was taken; they do **not** create in-app notifications.

### Push notifications (real time)

- A watching user receives new in-app notifications without manually reloading the page.
- Each notification includes: **notification id**, **event type**, **issue id**, **issue title**, **message**, **event time**, and **link** to the issue.
- When signed in, new notifications update the top-bar bell badge and, when the notification panel is open, the list inside the panel.
- After connection loss, the user can continue working; when the push connection resumes, the notification bell and open panel resynchronize.
- **Issues list** and **Issue details** do not auto-refresh from push events; the user refreshes or navigates to see current issue data.

### Email notifications

- The system sends an email for every in-app notification event listed above.
- Email contains: **issue title**, **change type**, **short summary**, **event time**, and **link to issue details**.
- Every emailed event is also stored as an in-app notification.
- After sign-in, the user sees notifications in the bell dropdown and can open the linked issue.
- Duplicate notification records for the same history entry and recipient are not created.

### Business rules

- When **Watch after creation** is selected (FR-ISS-002), the author becomes a watcher on the new issue.
- A user who unwatches stops receiving new notifications for that issue from that moment.

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ISS-004-01 | Authenticated user on **Issue details** who is not watching the issue                     | User clicks **Watch**                             | User becomes a watcher; watcher count on the button increases; no in-app notification is created for the acting user                              |
| AC-ISS-004-02 | Authenticated user on **Issues list** who is watching the issue                           | User clicks **Unwatch**                           | User stops watching; watcher count decreases; no in-app notification is created                                                                 |
| AC-ISS-004-03 | User A watches an issue; User B (not User A) adds a comment to that issue                 | Comment is saved                                  | User A receives an in-app push notification with **issue title**, **message**, **event time**, and **link**; User B does not receive one        |
| AC-ISS-004-04 | User A watches an issue; User B changes the issue **Status** from **In Progress** to **Closed** | Status change is saved                      | User A receives an email with **issue title**, **change type**, **short summary**, **event time**, and **link to issue details**                 |
| AC-ISS-004-05 | Authenticated user on **Create issue** with **Watch after creation** checked              | User successfully creates the issue               | The author is added as a watcher on the new issue                                                                                                 |
| AC-ISS-004-06 | User watches an issue, then unwatches it; another user later adds a comment               | Comment is saved                                  | The unwatched user does not receive a new in-app notification or email for that comment                                                           |
| AC-ISS-004-07 | Authenticated user on **Issues list**; a push notification arrives for a watched issue    | User remains on **Issues list** without refreshing | Issue row data (for example **Status** or **Last activity**) does not auto-update from the push event                                          |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
