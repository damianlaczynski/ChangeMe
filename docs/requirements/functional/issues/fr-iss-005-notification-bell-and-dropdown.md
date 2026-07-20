---
id: FR-ISS-005
title: Notification Bell and Dropdown
domain: issues
type: functional
status: active
depends_on: [FR-ISS-004]
inherits_conventions: [STD-ACC-001, STD-LST-002, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to review new and historical notifications related to watched issues without leaving their current work context.

There is no separate notifications area in the application navigation.

## Functional requirements

### Data

- Notification states: **Unread** and **Read**.
- Each notification shows **issue title**, **message**, and **event time**.
- Unread count is tracked and shown on the notification entry point.
- Retention policy defaults:
  - **Unread**: available **90 days** from event time,
  - **Read**: available **30 days** from mark-as-read time,
  - maximum lifetime **180 days** from event time regardless of state.
- Expired notifications are removed by automatic system cleanup and never shown in the list.
- Retention applies only to notification records; it does not remove comments, history, or issues.

### Operations

- View unread and read notifications, sorted newest first.
- Mark a single notification as read or mark all unread notifications as read.
- Open a notification to navigate to the linked issue details; opening marks unread items as read.
- Refresh the notification list.
- New push notifications update the unread count and open list without full page reload (FR-ISS-004).
- Inherits STD-LST-002 unless stated below.

### Business rules

- Read notifications remain available until retention expires; they do not disappear immediately after being read.
- Opening a notification shows current issue state, comments, and history on issue details (FR-ISS-003).
- The notification list does not replace change history on issue details.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
