---
id: FR-ISS-003
title: Issue Details, Comments, and Change History
domain: issues
type: functional
status: active
depends_on: [FR-ISS-002, FR-ISS-004, FR-ISS-006, FR-ISS-007]
inherits_conventions:
  [STD-ACC-001, STD-LST-002, STD-MSG-001, STD-OP-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to review full issue data, add comments, track change history, watch or unwatch, edit, and delete an issue.

## Functional requirements

### Authorization

- View details, comments, and history: **Issues.View** (FR-ISS-007).
- Edit: **Issues.Edit**, with author and assignee overrides per FR-ISS-007.
- Delete: **Issues.Delete**.
- Add comment: **Issues.Comment**, with author override per FR-ISS-007.
- Watch or unwatch: **Issues.View** (FR-ISS-004).

### Data

- Issue metadata: **title**, **author**, **assigned to**, **status**, **priority**, **created at**, **last activity**, **description**, and **acceptance criteria**.
- Issue identifier is used in navigation and search (FR-ISS-001) but is not a separate labeled field on details.
- **Comments**: each shows **author**, **date and time**, and **full content**; sorted newest first.
- **Change history**: read-only timeline of issue activity; sorted newest first.
- Inherits STD-LST-002 unless stated below.

### Operations

- View full issue data, description, and acceptance criteria.
- Edit an issue (FR-ISS-002).
- Delete an issue after confirmation; stored attachments are removed (FR-ISS-006).
- Watch or unwatch (FR-ISS-004).
- Add a comment; updates **Last activity** and triggers watcher notifications (FR-ISS-004).

### Validation

- **Comment content**: **required**; max **4000** characters.
- **Delete issue**: confirmation message **`Delete "{issue title}"? This cannot be undone.`**

### Business rules

**Change history** includes:

- Issue creation, status change, priority change, assignee change, title edit, description edit, acceptance-criterion add/update/remove, attachment add, and attachment remove.
- Each entry records **summary** (event type), **acting user**, **date and time**, and **Before** / **After** when values apply.
- **Description** changes record summary only (no before/after values).

- Adding a comment reloads the comment list and shows the new comment.
- Deleting an issue removes the issue from list and details contexts; list view refreshes in place.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
