---
id: FR-ISS-005
title: Notification Bell and Dropdown
domain: issues
type: functional
status: active
depends_on: []
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

The user must be able to review new and historical notifications related to watched issues from a bell control in the top application bar, without leaving the current screen.

There is no separate **Notifications** screen or sidebar entry.

## Functional requirements

### Notification bell in the top bar

- **Bell** icon in the authenticated header, next to theme and account actions.
- Badge shows unread count when greater than zero.
- Clicking the bell toggles a dropdown panel anchored to the control; it does not navigate away.
- Clicking outside the panel or pressing Escape closes the dropdown.
- New push notifications update the badge and open panel list without full page reload.

### Notification dropdown panel

- Inherits `FR-UI-001` default page size (**10**) and section loading; overrides list control with tabbed **Show more** append loading (not a full-page paginator).
- Panel header: `**Notifications`**, **unread count**, and **total count\*\* when loaded.
- **Refresh** reloads the notification list.
- **Mark all as read** marks every unread notification as read when any unread items exist.
- Scrollable body with tabs: **Unread** and **Read**.
- Each tab loads notifications sorted **newest first** (`CreatedAt` descending).
- When more notifications exist beyond the loaded pages, a **Show more** control appears at the bottom of the active tab; each activation loads the next page and **appends** it to the list.
- **Show more** is hidden when all notifications in that tab are loaded; while loading, the button shows a loading state and already loaded items remain visible.
- Switching tabs reloads that tab from **page 1**.
- Each notification shows: **issue title**, **message**, **event time**, and actions.
- **Open** follows the notification **link**, marks unread items as read when opened, and closes the dropdown.
- **Mark read** per unread item without opening the issue.
- Empty state per tab when a tab has no items.
- Loading indicator inside the panel during first load.

### Mark as read

- Mark a single notification as read.
- **Mark all as read** for all unread notifications.
- Marking as read updates the bell badge without full page reload.

### States and retention

- Read notifications move to the **Read** tab; they do not disappear immediately.
- Notification states: **Unread** and **Read**.
- Retention policy defaults:
  - **Unread**: available **90 days** from event time,
  - **Read**: available **30 days** from mark-as-read time,
  - maximum lifetime **180 days** from event time regardless of state.
- After retention expires, the notification no longer appears in the dropdown.
- Retention applies only to notification records; it does not remove comments, history, or issues.
- Expired notifications are removed by automatic system cleanup; no user action is required.
- The notification list never shows expired items.

### Consistency with issues

- Opening a notification navigates to **Issue details** for the linked issue.
- After opening from a notification, the user sees current issue state, comments, and history.
- The notification dropdown does not replace change history on **Issue details**.

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
