---
id: REQ-ISS-006
title: Issue Attachments
domain: issues
status: active
depends_on: [REQ-ISS-003, REQ-ISS-004]
---
## Goal

Authenticated users must be able to attach files to an issue, review them on **Issue details**, download them securely, and remove attachments they uploaded.

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

### Validation

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

- Deleting an issue (REQ-ISS-003) removes all attachment metadata and **deletes stored files** for that issue.

### Notifications

- Attachment add and remove trigger watcher notifications per REQ-ISS-004 (excluding the acting user).

### Out of scope

- Attachment upload on **Create issue** / **Edit issue** screens (attachments are managed only on **Issue details**).
- Virus scanning, image thumbnails, or inline preview in the browser.
- Replacing an existing attachment in place (upload always adds a new row).

---
