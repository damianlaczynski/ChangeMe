# Requirements - Issues

This document covers five REQs for the **Issues** area:
issue list, issue create/edit flow, issue details page, watching and notifications, and the in-app notification dropdown in the top bar.

The scope also includes comments, change history, issue deletion, and delivering notifications in real time and by email.

## Shared UX and data-loading rules

- The issues list, issue details page, create flow, and edit flow are available only to authenticated users. Route guards enforce access; do not add guest or `isAuthenticated` UI branches inside issues screens.
- Screens and sections in the `issues` module that load data asynchronously should show a **local loading indicator** (for example `p-progressSpinner` or the built-in `p-table` loading state).
- A full-page blocking loader must not be used as the primary loading mechanism for `issues` screens, except as a brief initial load state on detail/form screens before the first payload arrives.
- Loading indicators should be displayed within the list, form, details panel, or notification dropdown panel depending on where data is being fetched.
- After data is loaded, the loading indicator disappears without a full view reload.
- The real-time refresh mechanism for the issues list and issue details page is independent from the watch mechanism. Watching is used only to deliver user notifications.
- In-app back navigation uses `app-back-button` and `NavigationHistoryService` (session-scoped stack in `sessionStorage`, including query params such as `tab`). After deleting an issue, call `removeIssue()` / `navigateAfterIssueRemoval()` before navigating away so stale detail URLs are not restored.

---

# REQ-ISS-001: Issue List

## Goal

The user must be able to browse all issues, search, filter, sort, navigate to details, manage watches, and quickly start creating a new issue.

## Features

### Search and actions bar

- A text field with the placeholder _Search issues..._ filters the list by a fragment of the **issue title**, **issue description**, or the full **issue identifier** when the search text is a valid GUID.
- Submitting the search form ( **Search** button or form submit) applies the current search text together with the filter panel values.
- The **Add issue** button navigates to the issue creation form (REQ-ISS-002).

### Issues table - columns

| Column            | Description                                                                       |
| ----------------- | --------------------------------------------------------------------------------- |
| **Title**         | Short issue title; clickable link to the issue details page.                      |
| **Status**        | Issue status badge (**New**, **In Progress**, **Resolved**, **Closed**).          |
| **Priority**      | Priority badge (**Low**, **Medium**, **High**, **Critical**).                     |
| **Assigned to**   | Full name of the assigned user or **Unassigned**.                                 |
| **Created at**    | Issue creation date and time.                                                     |
| **Last activity** | Date and time of the most recent change, comment, or other activity on the issue. |
| **Actions**       | Watch control and an overflow menu for row actions (see below).                   |

### Sorting

- The **Title** column is sortable alphabetically ascending and descending.
- The **Created at** column is sortable chronologically ascending and descending.
- The **Last activity** column is sortable chronologically ascending and descending.
- The default list sort is **Last activity**, descending (most recent first).

### Row actions and watch control

- **Watch / Unwatch**: a compact button shows the current **watcher count** as its label and uses a bell / bell-slash icon to reflect whether the current user watches the issue. A tooltip describes the action and watcher count (`Watch this issue (3 watchers)`).
- **Overflow menu** (ellipsis): **Open details**, **Edit issue**, **Delete issue** (with confirmation "Delete \"{issue title}\"? This action cannot be undone.").

### Filter panel

- Filters live in a toggleable **Filters** panel (collapsed by default).
- **Status** filter: multi-select; empty selection means no restriction.
- **Priority** filter: multi-select; empty selection means no restriction.
- **Assigned to** filter: single-select user list populated from assignable users; clearable (any assignee).
- **Watched by me** filter: checkbox; when selected, shows only issues watched by the current user.
- **My issues** filter: checkbox; when selected, shows only issues **created by** or **assigned to** the current user.
- All filters combine with the search text using **AND** logic.
- **Apply filters** submits the filter panel together with the current search text.
- **Clear filters** resets the filter form and removes filter constraints from the active query (search text included).

---

# REQ-ISS-002: Issue Create and Edit Flow

## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data, and after saving be taken to the issue details page.

## Features

### "Issue details" section (create and edit)

| Field                    | Behavior                                                                                 |
| ------------------------ | ---------------------------------------------------------------------------------------- |
| **Title**                | Text field, **required**; **3–255** characters.                                          |
| **Description**          | Multiline text area, **required**; up to **2000** characters.                            |
| **Status**               | Issue status dropdown; **required**; default in create flow: **New**.                    |
| **Priority**             | Priority dropdown; **required**; default in create flow: **Medium**.                     |
| **Assigned to**          | User selector populated from assignable users; **optional** (clear = unassigned).        |
| **Watch after creation** | Checkbox on create only; selected by default; adds the author as a watcher when checked. |

### "Acceptance criteria" section

- List of acceptance criteria; the user can add multiple items and remove rows.
- Each item is a multiline **criterion** field, **required** when the row is present; up to **2000** characters per item.
- Acceptance criteria are optional overall (zero rows allowed on create).

**System fields (read-only on edit):**

- **Author**, **Created at**, and **Last activity** are shown in a read-only summary block on the edit form.
- **Issue identifier** is assigned by the system on first save (used in routes and search; not edited in the form).
- **Author** is set to the currently authenticated user during creation.
- **Created at** and **Last activity** are maintained by the system.

### Validation

- **Title**: required; minimum and maximum length enforced on the client and server.
- **Description**: required; maximum length enforced.
- **Status**: required; must be one of the allowed dictionary values.
- **Priority**: required; must be one of the allowed dictionary values.
- **Assigned to**: if provided, must come from the assignable-users list.
- **Acceptance criterion**: when a row exists, its text is required and length-limited.
- Validation errors are shown inline next to the relevant fields without closing the form.

### Form actions

- **Back**: returns using navigation history, falling back to the issues list.
- **Cancel** (create): same as back — leaves without saving.
- **Create issue** / **Save changes**: triggers validation; success saves and navigates to the issue details page; failure keeps the form open with validation messages.

### Consistency between create and edit

- The edit form uses the same core fields and validation rules as the create flow, plus read-only system metadata.
- In edit mode, the user can change **Title**, **Description**, **Status**, **Priority**, **Assigned to**, and all **acceptance criteria** rows (add, remove, update text).
- After issue creation, the author is added to watchers when **Watch after creation** is selected.
- Every issue creation and every issue edit writes entries to change history (REQ-ISS-003), including acceptance-criteria add, update, and remove events.

---

# REQ-ISS-003: Issue Details, Comments, and Change History

## Goal

The issue details page is the central detailed view where the user reviews the full issue data, adds comments, tracks change history, watches or unwatches, edits, or deletes the issue.

Access to the issue details page requires authentication.

## Features

### Issue header and metadata

- The page header shows the issue **title** (or _Issue Details_ while loading).
- A metadata block displays: **Author**, **Assigned to**, **Status**, **Priority**, **Created at**, and **Last activity** (status and priority as badges).
- The issue identifier is not duplicated as a separate labeled field; it is implied by the route and search behavior.
- Watch state is conveyed by the watch button icon, label (watcher count), and tooltip — not by a separate **Watching** / **Not watching** text label.
- **Edit** navigates to the edit form (REQ-ISS-002).
- **Delete** asks for confirmation, deletes the issue and returns to the issues list.

### Description section

- The full issue **Description** is displayed as read-only content in a toggleable panel.

### Acceptance criteria section

- The issue details page displays the list of **acceptance criteria** linked to the issue.
- Each item shows the full criterion text.
- If the issue has no acceptance criteria, the view shows a clear empty-state message (_No acceptance criteria defined_).

### Comments and history tabs

- **Comments** and **History** are separate tabs.

### Comments section

- Users can add comments to an issue.
- Each comment displays: **author**, **date and time**, and **full content**.
- Comments are sorted chronologically ascending.
- Adding a comment updates the issue **Last activity** and refreshes the details payload in place.
- Adding a comment triggers notifications for watchers (REQ-ISS-004).

### Comment validation

- **Comment content**: required; up to **4000** characters; an empty comment cannot be saved.
- After a validation error, the comment form remains open.

### Change history section

- The **History** tab shows an activity timeline for the issue.
- History includes at least: issue creation, status change, priority change, assignee change, title edit, description edit, and acceptance-criterion add, update, and remove.
- Each history entry contains: **summary** (event type), **acting user**, **date and time**, and optional **before** / **after** values when applicable.
- For **description** changes, before/after values are not shown inline (summary only); other field changes show **Before** and **After** when values are present.
- History is read-only and serves as an audit trail of work on the issue.
- Event types use distinct timeline markers (icons and colors) for quick scanning.

### Actions and navigation

- **Back** returns to the previous in-app URL (for example the issues list), not always a fixed “back to list” label.
- After saving an edit, the user returns to the issue details page with refreshed data.
- After adding a comment, the user stays on the issue page and sees the new comment without manually refreshing the view.
- Real-time issue events reload the open details page when the viewed issue is affected (REQ-ISS-004).

---

# REQ-ISS-004: Watching Issues and Real-Time / Email Notifications

## Goal

The user must be able to watch selected issues and receive notifications about activity related to those issues in real time and by email.

## Features

### Watch management

- The user can manually start watching an issue from the issues list and from the issue details page.
- The user can manually stop watching an issue from the same places.
- Watch state is stored per user and per issue.
- The system must not duplicate the same issue watch for the same user.
- The watch button displays the current **watcher count** and reflects whether the current user watches the issue.

### Events that generate notifications

- Notifications are generated for watchers (excluding the actor) at minimum for:
  - comment creation,
  - status change (including mapped **issue closed** and **issue reopened** events),
  - priority change,
  - assignee change,
  - title edit,
  - description edit,
  - acceptance-criterion add, update, and remove.
- Watch / unwatch actions update watcher counts and may publish real-time **watchers changed** events for list/details refresh; they do not create in-app notifications.

### Real-time notifications

- A watching user receives a notification in the UI without manually refreshing the page.
- A real-time notification payload includes at least: **notification id**, **event type**, **issue id**, **issue title**, **message**, **event time**, and **link** to the issue.
- If the user is authenticated and active in the application, new notifications update the top-bar bell badge and, when the dropdown is open, the notification list inside the panel.
- The frontend keeps an active real-time connection to the notifications hub for the authenticated user.
- The frontend listens to at least two classes of real-time events:
  - **NotificationCreated** for newly created in-app notifications,
  - **IssueChanged** (and related issue activity) to refresh the issues list and open issue details.
- After receiving a notification event, the frontend updates at least the unread notification counter and notification list without reloading the page.
- If the issues list is open, the frontend refreshes the current page of results when an issue change event arrives, regardless of whether the current user watches the issue.
- If an issue details page is open for the affected issue, the frontend reloads issue details (comments, history, watch state, last activity, and other fields) without a manual refresh, regardless of watch state.
- On hub reconnect, list and details views resynchronize from the server.

### Email notifications

- The system also sends an email notification for every in-app notification event described above.
- The email contains at least: **issue title**, **change type**, **short summary**, **event time**, and **link to issue details**.
- An event covered by such an email must also be stored as an in-app notification so the user can see it after signing in.
- After signing in, the user sees the notification in the bell dropdown and can navigate directly to the issue by opening it.
- The system avoids creating duplicate notification records for the same history entry and recipient.

### Business rules

- The issue author may be a default watcher of a newly created issue when **Watch after creation** is selected.
- A user who stops watching an issue stops receiving new notifications about that issue from the moment they opt out.

---

# REQ-ISS-005: Notification Bell and Dropdown

## Goal

The user must be able to review new and historical notifications related to watched issues from a bell control in the top application bar, without leaving the current screen.

There is no separate **Notifications** route or sidebar entry.

## Features

### Notification bell in the top bar

- A **bell** icon control is available in the authenticated shell header (top bar), next to theme and account actions.
- The bell shows a **badge** with the unread notification count when greater than zero.
- Clicking the bell toggles a **dropdown panel** anchored to the control; it does not navigate to another page.
- Clicking outside the panel or pressing Escape closes the dropdown.
- New notifications update the badge and the open panel list in real time via real-time without reloading the page.

### Notification dropdown panel

- The panel header shows **Notifications**, **unread count**, and **total count** when loaded.
- **Refresh** reloads notifications from the API.
- **Mark all as read** marks every unread notification as read when any unread items exist.
- The panel body is scrollable and uses tabs: **Unread** and **Read**.
- Each notification displays at least: **issue title**, **message** (summary), **event time** (`occurredAt`), and actions.
- **Open** navigates using the notification **link**, marks unread items as read when opened, and closes the dropdown.
- **Mark read** is available per unread item without opening the issue.
- Empty states are shown when a tab has no items.
- A local loading indicator is shown inside the panel while the first load is in progress.

### Mark as read

- The user can mark a single notification as read.
- The user can mark all visible unread notifications as read (**Mark all as read**).
- Marking notifications as read updates the bell badge without reloading the page.

### States and retention

- A notification does not disappear after being read; it moves to the **Read** tab.
- Historical notifications remain available to the user for a period defined by the system retention policy.
- The system distinguishes at least two states: **Unread** and **Read**.
- The notification retention policy is configurable on the backend.
- Default retention policy:
  - an **unread** notification remains available for **90 days** from the event time,
  - a **read** notification remains available for **30 days** from the moment it is marked as read,
  - regardless of state, a single notification must not be stored longer than **180 days** from the event time.
- After the retention period expires, the notification disappears from the user's dropdown list and may be physically removed from the database.
- Retention applies only to the notification record in the in-app notification list; it does not remove comments, change history, or the issue itself.
- The expired-notification cleanup mechanism runs automatically on the system side and must not require user action.
- Reading the notifications list must not return expired records even if physical cleanup has not yet run.

### Consistency with issues

- Opening a notification from the dropdown navigates to the linked issue details page (and preserves or sets detail context as appropriate).
- After navigating from a notification to an issue, the user sees the current issue state, comments, and change history.
- The notification dropdown is a source of information about new events, but it does not replace the change history on the issue page.

---

## Cross-cutting acceptance criteria

- The list, details page, and `issues` forms display a local **loading indicator** while fetching data instead of a full-page blocking spinner for routine updates.
- The user can create an issue with **acceptance criteria**, and after saving sees those criteria on the issue details page.
- The user can edit existing issue **acceptance criteria**, and after saving sees the updated list and corresponding history entries on the issue details page.
- The user can start and stop watching an issue from both the list and the issue details page.
- The user can delete an issue from the list overflow menu and from the issue details page, with confirmation.
- If a watched issue changes in a notification-eligible way, the system sends an email and stores an in-app notification at the same time.
- After signing in again, the user sees unread notifications on the bell badge and can open the dropdown to navigate to the correct issue.
- Adding a comment updates the issue last activity and is visible without manually refreshing the page.
- The frontend keeps the issues list, open issue details page, and open notification dropdown up to date through a real-time mechanism without forcing a full manual page refresh.
- Loss of the real-time connection must not block the core application flow; after reconnection the frontend resumes listening and resynchronizes UI state.
