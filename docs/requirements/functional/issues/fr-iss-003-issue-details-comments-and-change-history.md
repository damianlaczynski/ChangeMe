---
id: FR-ISS-003
title: Issue Details, Comments, and Change History
domain: issues
type: functional
status: active
depends_on: [FR-ISS-001, FR-ISS-002, FR-ISS-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

**Issue details** is the central view where the user reviews full issue data, adds comments, tracks change history, watches or unwatches, edits, or deletes the issue.

## Functional requirements

### Access

- Screen: **Issue details**
- Available only to authenticated users.

### Issue header and metadata

- Page header shows issue **title**, or `**Issue Details`\*\* while loading.
- Metadata block: **Author**, **Assigned to**, **Status**, **Priority**, **Created at**, **Last activity** (status and priority as badges).
- Issue identifier is not shown as a separate labeled field; it is used in navigation and search (FR-ISS-001).
- Watch state is shown by the watch button icon, watcher count label, and tooltip — not by separate **Watching** / **Not watching** text.
- **Edit** opens **Edit issue** (FR-ISS-002).
- **Delete** confirmation: `**Delete "{issue title}"? This action cannot be undone.`** On confirm, delete the issue and navigate to **Issues list\*\*.

### Description section

- Full **Description** as read-only content in a toggleable panel.

### Acceptance criteria section

- Lists all **acceptance criteria** for the issue.
- Empty state: `**No acceptance criteria defined`\*\*

### Comments, attachments, and history tabs

- Separate tabs: **Comments**, **Attachments**, and **History**.

### Comments section

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) for default page size (**10**) and section loading; overrides pagination with **Show more** append loading (below).
- Layout order (top to bottom): **Add a comment** form (textarea and **Add comment** button), then the comments list, then **Show more** when more comments exist.
- The comment form is **always above** the list, including when the list is empty or loading.
- Users add comments to an issue.
- Each comment shows **author**, **date and time**, and **full content**.
- Sorted by **date and time** descending (**newest first**).
- When older comments exist beyond the loaded pages, a **Show more** control appears below the list; each activation loads the next page of **older** comments and **appends** them without leaving the screen.
- **Show more** is hidden when all comments are loaded.
- While the first page loads, a loading indicator is shown in the list area; the comment form stays visible.
- While **Show more** is loading, the button shows a loading state; already loaded comments remain visible.
- Adding a comment updates **Last activity**, reloads comments from the first page, and shows the new comment without leaving the screen.
- Adding a comment triggers notifications for watchers (FR-ISS-004).

### Comment validation

- **Comment content**: required; max **4000** characters.
- Empty comment cannot be saved; error inline on the comment field.
- After validation error, the comment form stays open.

### Change history section

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) for default page size (**10**) and section loading; overrides pagination with **Show more** append loading (below).
- **History** tab shows an activity timeline.
- History includes: issue creation, status change, priority change, assignee change, title edit, description edit, acceptance-criterion add, update, and remove, attachment add, and attachment remove.
- Each entry: **summary** (event type), **acting user**, **date and time**, and **Before** / **After** when values apply.
- **Description** changes show summary only (no before/after inline).
- History is read-only.
- Event types use distinct timeline markers (icons and colors).
- Sorted by **date and time** descending (**newest first**), consistent with comments.
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

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ISS-003-01 | Guest or unauthenticated user                                      | User navigates to **Issue details**               | User is redirected to **Login** (FR-AUTH-001)                                                                                                     |
| AC-ISS-003-02 | Authenticated user opening **Issue details** while data is loading | User views the page header                        | Header shows **`Issue Details`** until the issue title loads                                                                                      |
| AC-ISS-003-03 | Authenticated user on **Issue details** for a loaded issue         | User views the metadata block                     | **Author**, **Assigned to**, **Status**, **Priority**, **Created at**, and **Last activity** are shown; issue identifier is not a labeled field  |
| AC-ISS-003-04 | Authenticated user on **Issue details** for an issue with no acceptance criteria | User views the acceptance criteria section | Empty state shows **`No acceptance criteria defined`**                                                                                            |
| AC-ISS-003-05 | Authenticated user on **Issue details**                            | User clicks header **Edit**                       | **Edit issue** opens (FR-ISS-002)                                                                                                                 |
| AC-ISS-003-06 | Authenticated user on **Issue details**                            | User clicks **Delete** and confirms the dialog    | Confirmation shows **`Delete "{issue title}"? This action cannot be undone.`**; issue is deleted; user navigates to **Issues list**               |
| AC-ISS-003-07 | Authenticated user on **Issue details** **Comments** tab           | User submits the **Add a comment** form with empty content | Inline error on the comment field; the comment form stays open                                                                            |
| AC-ISS-003-08 | Authenticated user on **Issue details** **Comments** tab           | User submits a valid comment via **Add comment**  | User stays on **Issue details**; the new comment appears at the top of the list; **Last activity** updates                                        |
| AC-ISS-003-09 | Authenticated user on **Issue details**                            | User clicks **Back to issues list**               | User navigates to **Issues list**                                                                                                                 |
| AC-ISS-003-10 | Authenticated user on **Issue details** **Comments** tab           | User views the section layout                     | **Add a comment** form (textarea and **Add comment** button) is above the comments list                                                           |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
