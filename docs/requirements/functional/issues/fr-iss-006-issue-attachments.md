---
id: FR-ISS-006
title: Issue Attachments
domain: issues
type: functional
status: active
depends_on: [FR-ISS-003, FR-ISS-004]
inherits_nfr:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
inherits_fr: [FR-UI-001]
---

## Goal

Authenticated users must be able to attach files to an issue, review them on **Issue details**, download them securely, and remove attachments they uploaded.

## Functional requirements

### Access

- Screen: **Issue details** — **Attachments** tab
- Available only to authenticated users (same as FR-ISS-003).

### Attachments tab layout

- Layout order (top to bottom): **Upload file** control, then the attachments list, then **Show more** when more attachments exist.
- The upload control is **always above** the list, including when the list is empty or loading.

### Upload

- User selects **one file** at a time and uploads it to the current issue.
- **Upload** button starts the upload; while uploading, the control shows a loading state.
- On success: **Last activity** updates, the list reloads from the first page, and the new attachment appears without leaving the screen.
- On failure: inline error near the upload control; already uploaded files remain visible.

### Attachments list

- Inherits `FR-UI-001` (**Detail and section screens** → **Embedded lists**) for default page size (**10**) and section loading; overrides pagination with **Show more** append loading (below).
- Each row shows: **file name**, **file size** (human-readable), **uploaded by**, **upload date and time**, and actions.
- Sorted by **upload date and time** descending (**newest first**).
- When older attachments exist beyond the loaded pages, a **Show more** control appears below the list; each activation loads the next page of **older** attachments and **appends** them.
- **Show more** is hidden when all attachments are loaded.
- While the first page loads, a loading indicator is shown in the list area; the upload control stays visible.
- While **Show more** is loading, the button shows a loading state; already loaded attachments remain visible.
- Empty state: **`No attachments yet`**

### Download

- **Download** saves the file to the user's device under the **original file name** shown in the list; the user must be signed in.
- While downloading, the row action shows a loading state.

### Delete

- **Delete** is available only to the user who **uploaded** the attachment.
- Confirmation: **`Delete "{file name}"? This action cannot be undone.`**
- On confirm: remove the attachment from the issue, delete stored content, write change history, update **Last activity**, and refresh the list from the first page.
- Other users do not see **Delete** on attachments they did not upload.

### Upload constraints

| Rule                   | Limit                                                                                                         |
| ---------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Max file size**      | **5 MB** per file                                                                                             |
| **Max attachments**    | **10** per issue                                                                                              |
| **Allowed file types** | **`.pdf`**, **`.png`**, **`.jpg`**, **`.jpeg`**, **`.gif`**, **`.txt`**, **`.csv`**, **`.docx`**, **`.xlsx`** |

### Upload validation

- Upload, list, and download require a signed-in user.
- Empty files are rejected.
- Files exceeding **5 MB** are rejected; inline error: **`File cannot exceed 5.0 MB.`**
- When the issue already has **10** attachments, further uploads are rejected.
- Files without a file extension are rejected.
- Files whose extension is not in the allowed list are rejected.
- File content must match the declared file type; content that does not match the file extension is rejected.
- **`.txt`** and **`.csv`** files must be plain text; binary content in a text file is rejected.
- The file name shown in the list is derived from the name supplied by the user; path segments, control characters, and unsafe characters are removed; the displayed name cannot exceed **255** characters.
- Invalid or empty file names after sanitization are rejected.
- Validation errors from the server are shown inline near the upload control.

### Secure storage and download

- Stored files are not directly accessible by guessing a URL from the file name.
- The system stores each file under an internal identifier; the user-supplied file name is used for display and download only.
- **Download** saves the file to the user's device; the browser does not open the file inline on the **Attachments** tab.
- Downloaded files use the **original file name** shown in the list and the correct file type for the saved file.

### Upload outcome

- An upload succeeds only when the attachment is fully stored and recorded on the issue; on success the attachment appears in the list.
- A failed upload does not add an entry to the attachments list.
- The user never sees partial or in-progress attachments in the list.
- **List**, **Download**, and **Delete** apply only to attachments that completed upload successfully.
- If the system cannot complete an upload, no attachment record remains visible to the user for that attempt.

### Change history

- Adding an attachment writes an **attachment added** history entry (summary includes file name).
- Removing an attachment writes an **attachment removed** history entry (summary includes file name).
- History entries show **Before** / **After** when values apply (file name).

### Issue deletion

- Deleting an issue (FR-ISS-003) removes all attachment metadata and **deletes stored files** for that issue.

### Notifications

- Attachment add and remove trigger watcher notifications per FR-ISS-004 (excluding the acting user).

### Out of scope

- Attachment upload on **Create issue** / **Edit issue** screens (attachments are managed only on **Issue details**).
- Virus scanning, image thumbnails, or inline preview in the browser.
- Replacing an existing attachment in place (upload always adds a new row).

---

## Acceptance scenarios

| ID | Given | When | Then |
| -- | ----- | ---- | ---- |
| AC-ISS-006-01 | Guest or unauthenticated user                                      | User navigates to **Issue details** **Attachments** tab | User is redirected to **Login** (FR-AUTH-001)                                                                                             |
| AC-ISS-006-02 | Authenticated user on **Issue details** **Attachments** tab with no files | User views the section                      | Empty state shows **`No attachments yet`**; **Upload file** control is above the attachments list                                                 |
| AC-ISS-006-03 | Authenticated user on **Attachments** tab; issue has fewer than 10 attachments | User uploads a valid allowed file under 5 MB and clicks **Upload** | Attachment appears in the list; **Last activity** updates; user stays on **Attachments** tab without leaving the screen              |
| AC-ISS-006-04 | Authenticated user on **Attachments** tab                          | User selects a file larger than **5 MB** and clicks **Upload** | Inline error **`File cannot exceed 5.0 MB.`** near the upload control; no new row appears in the list                                           |
| AC-ISS-006-05 | Authenticated user on **Attachments** tab; issue already has **10** attachments | User attempts another upload               | Upload is rejected with an inline error near the upload control; attachment count remains **10**                                                  |
| AC-ISS-006-06 | Authenticated user who uploaded an attachment on **Attachments** tab | User clicks **Delete** and confirms the dialog | Confirmation shows **`Delete "{file name}"? This action cannot be undone.`**; attachment is removed; change history is written; **Last activity** updates; list reloads from the first page |
| AC-ISS-006-07 | Authenticated user viewing an attachment uploaded by another user on **Attachments** tab | User views row actions              | **Delete** is not shown; **Download** is available                                                                                                |
| AC-ISS-006-08 | Authenticated user on **Attachments** tab                          | User clicks **Download** on an attachment row     | File is saved to the device under the **original file name** shown in the list                                                                    |

## Non-functional requirements

- Inherits `docs/requirements/_shared/non-functional/product-quality.md` (`NFR-QUAL-001`) and linked NFR documents.
- Inherits `docs/requirements/_shared/functional/ui-patterns.md` (`FR-UI-001`) for shared list, form, and feedback behavior unless stated above.
- Document only overrides in this section when this specification differs from inherited NFR or UI patterns.
