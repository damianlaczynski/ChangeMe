# Requirements - Issues

This document covers six REQs for the **Issues** area:
issue list, issue create/edit flow, issue details page, issue attachments, watching and notifications, and the in-app notification dropdown in the top bar.

The scope includes comments, attachments, change history, issue deletion, push notifications in real time, and email notifications.

---

# REQ-ISS-001: Issue List

## Goal

The user must be able to browse all issues, search, filter, sort, navigate to details, manage watches, and quickly start creating a new issue.

## Features

### Access

- Screen: **Issues list**
- Available only to authenticated users. Guests are redirected to **Login** (REQ-AUTH-001).

### Search and actions bar

- **Add issue** button opens **Create issue** (REQ-ISS-002).

### Issues table — columns

| Column            | Description                                                                                       |
| ----------------- | ------------------------------------------------------------------------------------------------- |
| **Title**         | Short issue title; clickable link to **Issue details**.                                           |
| **Status**        | Issue status badge (**New**, **In Progress**, **Resolved**, **Closed**).                          |
| **Priority**      | Priority badge (**Low**, **Medium**, **High**, **Critical**).                                     |
| **Assigned to**   | Assignee as **`{name} ({email})`** or **email** when name is missing; **`Unassigned`** when none. |
| **Created at**    | Issue creation date and time.                                                                     |
| **Last activity** | Date and time of the most recent change, comment, or other activity on the issue.                 |
| **Actions**       | Watch control and overflow menu for row actions (see below).                                      |

### Sorting

- **Title**: sortable alphabetically ascending and descending.
- **Created at**: sortable chronologically ascending and descending.
- **Last activity**: sortable chronologically ascending and descending.
- Default sort: **Last activity**, descending (most recent first).

### Row actions and watch control

- **Watch / Unwatch**: compact button shows **watcher count** as label and bell / bell-slash icon for current watch state. Tooltip format: `**Watch this issue ({n} watchers)`** or `**Unwatch this issue ({n} watchers)\*\*`where`{n}` is the count.
- Overflow menu: **Open details**, **Edit issue**, **Delete issue**.
- **Delete issue** confirmation: `**Delete "{issue title}"? This action cannot be undone.`\*\*

### Search and filters

- Toggleable **Filters** panel (collapsed by default).
- **Status** filter: multi-select; empty selection means no restriction.
- **Priority** filter: multi-select; empty selection means no restriction.
- **Assigned to** filter: single-select user list from assignable users (REQ-USR-005); clearable.
- **Watched by me** filter: checkbox; when selected, shows only issues watched by the current user.
- **My issues** filter: checkbox; when selected, shows only issues **created by** or **assigned to** the current user.
- All filters combine with search text using **AND** logic.
- **Apply filters** submits the filter panel with the current search text.
- **Clear filters** resets the filter form and removes all filter constraints (search text included).
- Applied filters list

### Pagination

- The issues table is **server-paginated** with **10** rows per page by default.
- A paginator below the table shows the current page and total count; changing page or page size reloads the list.
- Search, filters, and sort reset to **page 1**.

### Loading

- While the table is loading, a loading indicator is shown in the table area; the screen layout remains visible.

---

# REQ-ISS-002: Issue Create and Edit Flow

## Goal

The user must be able to create a new issue and edit an existing one by providing the required core data, and after saving be taken to **Issue details**.

## Features

### Access

- Screens: **Create issue**, **Edit issue**
- Available only to authenticated users.

### "Issue details" section (create and edit)

| Field                    | Behavior                                                                                                                  |
| ------------------------ | ------------------------------------------------------------------------------------------------------------------------- |
| **Title**                | Text field, **required**; **3–255** characters.                                                                           |
| **Description**          | Multiline text area, **required**; up to **2000** characters.                                                             |
| **Status**               | Issue status dropdown; **required**; default on create: **New**.                                                          |
| **Priority**             | Priority dropdown; **required**; default on create: **Medium**.                                                           |
| **Assigned to**          | User selector from assignable users (REQ-USR-005); options show **Display label**; **not required** (clear = unassigned). |
| **Watch after creation** | Checkbox on create only; selected by default; adds the author as a watcher when checked.                                  |

### "Acceptance criteria" section

- List of acceptance criteria; the user can add multiple items and remove rows.
- Each item is a multiline **criterion** field, **required** when the row is present; up to **2000** characters per item.
- Zero acceptance-criteria rows are allowed on create.

**System fields (read-only on edit):**

- **Author**, **Created at**, and **Last activity** in a read-only summary block.
- **Issue identifier** is assigned by the system on first save; not editable in the form.
- **Author** is the currently signed-in user on create.
- **Created at** and **Last activity** are maintained by the system.

### Validation

- **Title**: required; **3–255** characters.
- **Description**: required; max **2000** characters.
- **Status**: required; one of **New**, **In Progress**, **Resolved**, **Closed**.
- **Priority**: required; one of **Low**, **Medium**, **High**, **Critical**.
- **Assigned to**: when selected, must be an assignable user with **Deactivated** false (REQ-USR-005).
- **Acceptance criterion**: when a row exists, its text is required; max **2000** characters.
- Validation errors are inline next to the relevant field; the form stays open on failure.

### Form actions

- **Back to issues list** button navigates to **Issues list** without saving.
- **Cancel** (create): same as **Back to issues list** — leaves without saving.
- **Create issue** / **Save changes**: on success save and open **Issue details**; on failure keep the form open with validation messages.

### Back navigation (create and edit)

| Screen           | Back button label         | Destination                            |
| ---------------- | ------------------------- | -------------------------------------- |
| **Create issue** | **Back to issues list**   | **Issues list**                        |
| **Edit issue**   | **Back to issue details** | **Issue details** for the edited issue |

### Consistency between create and edit

- Edit uses the same core fields and validation as create, plus read-only system metadata.
- On edit, the user can change **Title**, **Description**, **Status**, **Priority**, **Assigned to**, and all **acceptance criteria** rows.
- After create, the author is added to watchers when **Watch after creation** is checked.
- Every create and edit writes entries to change history (REQ-ISS-003), including acceptance-criterion add, update, and remove events.

### Loading

- Before the first load on create/edit, a loading state covers the form area until initial data arrives.

---

# REQ-ISS-003: Issue Details, Comments, and Change History

## Goal

**Issue details** is the central view where the user reviews full issue data, adds comments, tracks change history, watches or unwatches, edits, or deletes the issue.

## Features

### Access

- Screen: **Issue details**
- Available only to authenticated users.

### Issue header and metadata

- Page header shows issue **title**, or `**Issue Details`\*\* while loading.
- Metadata block: **Author**, **Assigned to**, **Status**, **Priority**, **Created at**, **Last activity** (status and priority as badges).
- Issue identifier is not shown as a separate labeled field; it is used in navigation and search (REQ-ISS-001).
- Watch state is shown by the watch button icon, watcher count label, and tooltip — not by separate **Watching** / **Not watching** text.
- **Edit** opens **Edit issue** (REQ-ISS-002).
- **Delete** confirmation: `**Delete "{issue title}"? This action cannot be undone.`** On confirm, delete the issue and navigate to **Issues list\*\*.

### Description section

- Full **Description** as read-only content in a toggleable panel.

### Acceptance criteria section

- Lists all **acceptance criteria** for the issue.
- Empty state: `**No acceptance criteria defined`\*\*

### Comments, attachments, and history tabs

- Separate tabs: **Comments**, **Attachments**, and **History**.

### Comments section

- Layout order (top to bottom): **Add a comment** form (textarea and **Add comment** button), then the comments list, then **Show more** when more comments exist.
- The comment form is **always above** the list, including when the list is empty or loading.
- Users add comments to an issue.
- Each comment shows **author**, **date and time**, and **full content**.
- Comments are loaded **server-paginated**, sorted by **date and time** descending (**newest first**).
- The first page shows up to **10** most recent comments.
- When older comments exist beyond the loaded pages, a **Show more** control appears below the list; each activation loads the next page of **older** comments and **appends** them without leaving the screen.
- **Show more** is hidden when all comments are loaded.
- While the first page loads, a loading indicator is shown in the list area; the comment form stays visible.
- While **Show more** is loading, the button shows a loading state; already loaded comments remain visible.
- Adding a comment updates **Last activity**, reloads comments from the first page, and shows the new comment without leaving the screen.
- Adding a comment triggers notifications for watchers (REQ-ISS-004).

### Comment validation

- **Comment content**: required; max **4000** characters.
- Empty comment cannot be saved; error inline on the comment field.
- After validation error, the comment form stays open.

### Change history section

- **History** tab shows an activity timeline.
- History includes: issue creation, status change, priority change, assignee change, title edit, description edit, acceptance-criterion add, update, and remove, attachment add, and attachment remove.
- Each entry: **summary** (event type), **acting user**, **date and time**, and **Before** / **After** when values apply.
- **Description** changes show summary only (no before/after inline).
- History is read-only.
- Event types use distinct timeline markers (icons and colors).
- History entries are loaded **server-paginated**, sorted by **date and time** descending (**newest first**), consistent with comments.
- The first page shows up to **10** most recent entries.
- When older history exists beyond the loaded pages, a **Show more** control appears below the timeline; each activation loads the next page of **older** entries and **appends** them.
- **Show more** is hidden when all history entries are loaded.
- While the first page loads, a loading indicator is shown in the history area.
- While **Show more** is loading, the button shows a loading state; already loaded entries remain visible.

### Actions and navigation

- **Back to issues list** button navigates to **Issues list**.
- After edit save, the user returns to **Issue details** with refreshed data.
- After adding a comment, the user stays on **Issue details** and sees the new comment.

### Deletion navigation

- After deleting an issue from **Issue details**, the user is navigated to **Issues list**.
- After deleting an issue from **Issues list**, the user remains on **Issues list** with the list refreshed.

### Loading

- While issue header, description, and acceptance criteria load initially, a loading state covers those areas until initial data arrives.
- Comments and history load independently per tab (see above); tab content may load after the main issue body is visible.

---

# REQ-ISS-006: Issue Attachments

## Goal

Authenticated users must be able to attach files to an issue, review them on **Issue details**, download them securely, and remove attachments they uploaded. This REQ is the **reference implementation** for file handling in the application; later features should reuse the same storage, validation, and API patterns.

## Features

### Access

- Screen: **Issue details** — **Attachments** tab
- Available only to authenticated users (same as REQ-ISS-003).

### Attachments tab layout

- Layout order (top to bottom): **Upload file** control, then the attachments list, then **Show more** when more attachments exist.
- The upload control is **always above** the list, including when the list is empty or loading.

### Upload

- User selects **one file** at a time and uploads it to the current issue.
- **Upload** button starts the upload; while uploading, the control shows a loading state.
- On success: **Last activity** updates, the list reloads from the first page, and the new attachment appears without leaving the screen.
- On failure: inline error near the upload control; already uploaded files remain visible.

### Attachments list

- Each row shows: **file name**, **file size** (human-readable), **uploaded by**, **upload date and time**, and actions.
- Attachments are loaded **server-paginated**, sorted by **upload date and time** descending (**newest first**).
- The first page shows up to **10** most recent attachments.
- When older attachments exist beyond the loaded pages, a **Show more** control appears below the list; each activation loads the next page of **older** attachments and **appends** them.
- **Show more** is hidden when all attachments are loaded.
- While the first page loads, a loading indicator is shown in the list area; the upload control stays visible.
- While **Show more** is loading, the button shows a loading state; already loaded attachments remain visible.
- Empty state: **`No attachments yet`**

### Download

- **Download** action fetches the file through the authenticated API and saves it locally under the **original file name** shown in the list.
- While downloading, the row action shows a loading state.

### Delete

- **Delete** is available only to the user who **uploaded** the attachment.
- Confirmation: **`Delete "{file name}"? This action cannot be undone.`**
- On confirm: remove the attachment from the issue, delete stored content, write change history, update **Last activity**, and refresh the list from the first page.
- Other users do not see **Delete** on attachments they did not upload.

### Security policy (enforced server-side)

Deployment settings under **`FileStorage`** (see backend configuration) define upload limits for file size and allowed types. The per-issue attachment count is a fixed domain rule (not configurable).

| Rule                       | Default / behavior                                                                                                                                                                                                                                                                                                                                                                       |
| -------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Max file size**          | **5 MB** per file                                                                                                                                                                                                                                                                                                                                                                        |
| **Max attachments**        | **10** per issue (fixed domain invariant)                                                                                                                                                                                                                                                                                                                                                |
| **Allowed extensions**     | **`.pdf`**, **`.png`**, **`.jpg`**, **`.jpeg`**, **`.gif`**, **`.txt`**, **`.csv`**, **`.docx`**, **`.xlsx`**                                                                                                                                                                                                                                                                            |
| **Content validation**     | Declared type and file extension must match an allowed type; content is verified with **[Mime-Detective](https://www.nuget.org/packages/Mime-Detective)** signature detection (scoped to allowed extensions). For **`.txt`** / **`.csv`**, plain-text rules apply and binary signatures that conflict with the declared extension are rejected. Mismatch or unknown content is rejected. |
| **File name sanitization** | Original name is stored for display only; path segments, control characters, and unsafe characters are stripped; max **255** characters.                                                                                                                                                                                                                                                 |
| **Storage key**            | Opaque server-generated identifier; user-supplied names are **never** used as storage paths.                                                                                                                                                                                                                                                                                             |
| **Storage location**       | Files live **outside** the web root in a configured root directory (subfolder per issue).                                                                                                                                                                                                                                                                                                |
| **Download response**      | **`Content-Disposition: attachment`**, validated **`Content-Type`**, and **`X-Content-Type-Options: nosniff`**.                                                                                                                                                                                                                                                                          |
| **Authorization**          | Upload, list, and download require authentication. Delete requires authentication **and** uploader identity.                                                                                                                                                                                                                                                                             |

### Upload consistency (DB-first)

- Upload uses attachment statuses **`Pending`** then **`Active`**:
  1. reserve metadata and an opaque **`StorageKey`** in the database as **`Pending`**,
  2. write file content to storage under that key,
  3. mark the attachment **`Active`**, update **Last activity**, and write history/notifications.
- If storage write or activation fails after the pending row exists, the server removes the pending row and deletes any partial stored file.
- **`Pending`** rows older than **`FileStorage:PendingRetentionMinutes`** (default **30**) are removed by automatic cleanup; orphaned files on disk with no matching metadata row are deleted by the same job.
- List, download, and delete operate only on **`Active`** attachments; **`Pending`** rows do not appear in the UI.

### Change history

- Adding an attachment writes an **attachment added** history entry (summary includes file name).
- Removing an attachment writes an **attachment removed** history entry (summary includes file name).
- History entries show **Before** / **After** when values apply (file name).

### Issue deletion

- Deleting an issue (REQ-ISS-003) removes all attachment metadata and **deletes stored files** for that issue.

### Notifications

- Attachment add and remove trigger watcher notifications per REQ-ISS-004 (excluding the acting user).

### Out of scope

- Attachment upload on **Create issue** / **Edit issue** screens (attachments are managed only on **Issue details**).
- Virus scanning, image thumbnails, or inline preview in the browser.
- Replacing an existing attachment in place (upload always adds a new row).

---

# REQ-ISS-004: Watching Issues and Push / Email Notifications

## Goal

The user must be able to watch selected issues and receive in-app push notifications and email about related activity.

## Features

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
- attachment add and remove (REQ-ISS-006).
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

- When **Watch after creation** is selected (REQ-ISS-002), the author becomes a watcher on the new issue.
- A user who unwatches stops receiving new notifications for that issue from that moment.

---

# REQ-ISS-005: Notification Bell and Dropdown

## Goal

The user must be able to review new and historical notifications related to watched issues from a bell control in the top application bar, without leaving the current screen.

There is no separate **Notifications** screen or sidebar entry.

## Features

### Notification bell in the top bar

- **Bell** icon in the authenticated header, next to theme and account actions.
- Badge shows unread count when greater than zero.
- Clicking the bell toggles a dropdown panel anchored to the control; it does not navigate away.
- Clicking outside the panel or pressing Escape closes the dropdown.
- New push notifications update the badge and open panel list without full page reload.

### Notification dropdown panel

- Panel header: `**Notifications`**, **unread count**, and **total count\*\* when loaded.
- **Refresh** reloads the notification list.
- **Mark all as read** marks every unread notification as read when any unread items exist.
- Scrollable body with tabs: **Unread** and **Read**.
- Each tab loads notifications **server-paginated** with **10** items per page by default, sorted **newest first** (`CreatedAt` descending).
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
