---
id: FR-ISS-006
title: Issue Attachments
domain: issues
type: functional
status: active
depends_on: [FR-ISS-003, FR-ISS-004, FR-ISS-007]
inherits_conventions:
  [STD-ACC-001, STD-LST-002, STD-MSG-001, STD-OP-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

Authenticated users must be able to attach files to an issue, review them, download them securely, and remove attachments they uploaded.

## Functional requirements

### Authorization

- Upload: **Issues.ManageAttachments** (FR-ISS-007).
- Download: **Issues.View** (FR-ISS-007).
- **Delete**: **Issues.ManageAttachments** and the acting user must be **uploaded by** on that attachment.

### Data

| Rule                   | Limit                                                                                                         |
| ---------------------- | ------------------------------------------------------------------------------------------------------------- |
| **Max file size**      | **5 MB** per file                                                                                             |
| **Max attachments**    | **10** per issue                                                                                              |
| **Allowed file types** | **`.pdf`**, **`.png`**, **`.jpg`**, **`.jpeg`**, **`.gif`**, **`.txt`**, **`.csv`**, **`.docx`**, **`.xlsx`** |

- Each attachment records **file name**, **file size**, **uploaded by**, and **upload date and time**; sorted newest first.
- Stored files use an internal identifier; the user-supplied file name is used for display and download only.
- Inherits STD-LST-002 unless stated below.

### Operations

- Upload **one file** at a time to an issue; on success, **Last activity** updates and the attachment appears in the list.
- Download an attachment under the **original file name** shown in the list; requires a signed-in user.
- Delete an uploaded attachment after confirmation; removes stored content, writes change history, and updates **Last activity**.
- Attachment add and remove trigger watcher notifications (FR-ISS-004), excluding the acting user.

### Validation

- Empty files are rejected.
- Files exceeding **5 MB** are rejected; rejection message: **`File cannot exceed 5.0 MB.`**
- When the issue already has **10** attachments, further uploads are rejected.
- Files without a file extension are rejected.
- Files whose extension is not in the allowed list are rejected.
- File content must match the declared file type; content that does not match the file extension is rejected.
- **`.txt`** and **`.csv`** files must be plain text; binary content in a text file is rejected.
- The display file name is sanitized from the user-supplied name; max **255** characters after sanitization; invalid or empty names after sanitization are rejected.
- **Delete attachment**: confirmation message **`Delete "{file name}"? This action cannot be undone.`**

### Business rules

- An upload succeeds only when the attachment is fully stored and recorded; failed uploads do not add a visible record.
- Stored files are not directly accessible by guessing a URL from the file name.
- Download saves the file to the user's device; the browser does not open the file inline.
- Adding an attachment writes an **attachment added** history entry; removing writes an **attachment removed** history entry (FR-ISS-003).
- Deleting an issue (FR-ISS-003) removes all attachment metadata and deletes stored files.

## Out of scope

- Attachment upload on issue create or edit (attachments are managed only on issue details).
- Virus scanning, image thumbnails, or inline preview.
- Replacing an existing attachment in place (upload always adds a new attachment).

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
