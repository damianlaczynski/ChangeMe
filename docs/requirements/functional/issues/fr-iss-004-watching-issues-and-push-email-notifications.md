---
id: FR-ISS-004
title: Watching Issues and Push / Email Notifications
domain: issues
type: functional
status: active
depends_on: [FR-ISS-002, FR-ISS-006, FR-ISS-007]
inherits_conventions: [STD-ACC-001, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to watch selected issues and receive in-app push notifications and email about related activity.

## Functional requirements

### Authorization

- Watch or unwatch: **Issues.View** (FR-ISS-007).
- Notifications are delivered only to watchers with **Issues.View** at delivery time (FR-ISS-007).

### Operations

- Start or stop watching an issue from the issue list or issue details.
- Watch state is stored per user and per issue; duplicate watches for the same user and issue are not created.
- When **Watch after creation** is selected (FR-ISS-002), the author becomes a watcher on the new issue.
- A user who unwatches stops receiving new notifications for that issue from that moment.
- Watch and unwatch update watcher counts; they do **not** create in-app notifications.

### Data

Each in-app notification includes: **notification id**, **event type**, **issue id**, **issue title**, **message**, **event time**, and **link** to the issue.

### Events that generate notifications

Notifications are sent to watchers (excluding the acting user) for:

- comment creation,
- status change (including issue closed and issue reopened),
- priority change,
- assignee change,
- title edit,
- description edit,
- acceptance-criterion add, update, and remove,
- attachment add and remove (FR-ISS-006).

### Push notifications

- A watching user receives new in-app notifications without manually reloading the page.
- After connection loss, notifications resynchronize when the push connection resumes.
- Issue list and issue details do **not** auto-refresh from push events.

### Email notifications

- The system sends an email for every in-app notification event listed above.
- Email contains: **issue title**, **change type**, **short summary**, **event time**, and **link to issue details**.
- Every emailed event is also stored as an in-app notification.
- Duplicate notification records for the same history entry and recipient are not created.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
