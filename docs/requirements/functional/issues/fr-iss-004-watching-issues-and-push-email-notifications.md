---
id: FR-ISS-004
title: Watching Issues and Push / Email Notifications
domain: issues
type: functional
status: active
depends_on: [FR-ISS-002, FR-ISS-006]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to watch selected issues and receive in-app push notifications and email about related activity.

## Functional requirements

### Watch management

- Start watching from **Project issues list** and **Issue details**.
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
- **Project issues list** and **Issue details** do not auto-refresh from push events; the user refreshes or navigates to see current issue data.

### Email notifications

- The system sends an email for every in-app notification event listed above.
- Email contains: **issue title**, **change type**, **short summary**, **event time**, and **link to issue details**.
- Every emailed event is also stored as an in-app notification.
- After sign-in, the user sees notifications in the bell dropdown and can open the linked issue.
- Duplicate notification records for the same history entry and recipient are not created.

### Business rules

- When **Watch after creation** is selected (FR-ISS-002), the author becomes a watcher on the new issue.
- A user who unwatches stops receiving new notifications for that issue from that moment.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
