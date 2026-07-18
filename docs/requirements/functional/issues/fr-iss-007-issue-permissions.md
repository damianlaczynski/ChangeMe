---
id: FR-ISS-007
title: Issue Permissions and Access Rules
domain: issues
type: functional
status: active
depends_on: [FR-ROL-001, FR-ROL-006, FR-USR-005]
inherits_conventions: [STD-ACC-001, STD-MSG-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

> Issue terms and roles on an issue: `docs/requirements/_shared/domain/issue-model.md`.
> Permission catalog: `docs/requirements/functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md` (`FR-ROL-001`).

## Goal

The system must control who can view and change issues through a fixed permission catalog, including participant overrides for authors and assignees.

## Functional requirements

### Data

Issue permissions are defined in FR-ROL-001 under group **Issues**:

| Permission                   | Label (exact)            | Description                                                                                          |
| ---------------------------- | ------------------------ | ---------------------------------------------------------------------------------------------------- |
| **Issues.View**              | View issues              | View the issues list, issue details, comments, change history, watcher counts, and download attachments. |
| **Issues.Create**            | Create issues            | Create new issues.                                                                                   |
| **Issues.Edit**              | Edit issues              | Edit any issue's core fields and assignee.                                                           |
| **Issues.Delete**            | Delete issues            | Delete issues.                                                                                       |
| **Issues.Comment**           | Comment on issues        | Add comments on any issue.                                                                           |
| **Issues.ManageAttachments** | Manage issue attachments | Upload attachments and delete attachments the user uploaded.                                       |

### Authorization

**Navigation and read access**

- The **Issues** area is visible when the user has **Issues.View** or **Issues.Create**.
- Browsing the issues list, opening issue details, reading comments and change history, and downloading attachments require **Issues.View**.
- A signed-in user without **Issues.View** who opens an issues URL receives rejection message **`You do not have permission to perform this action.`**

**Create**

- Creating an issue requires **Issues.Create** (FR-ISS-002).

**Edit**

- Editing any issue's **title**, **description**, **status**, **priority**, **assigned to**, or acceptance criteria requires **Issues.Edit**, except:
  - **Author override**: the **Author** of an issue may edit **title**, **description**, **status**, **priority**, and acceptance criteria on that issue when they have **Issues.View**, even without **Issues.Edit**.
  - **Assignee override**: the user in **Assigned to** may edit **status** and **priority** on that issue when they have **Issues.View**, even without **Issues.Edit**.
- Changing **assigned to** requires **Issues.Edit**; author and assignee overrides do **not** apply to assignee changes.

**Delete**

- Deleting an issue requires **Issues.Delete** (FR-ISS-001, FR-ISS-003).

**Comments**

- Adding a comment requires **Issues.Comment**, except:
  - **Author override**: the **Author** may add comments when they have **Issues.View**, even without **Issues.Comment**.

**Watching and notifications**

- Watching or unwatching an issue requires **Issues.View** (FR-ISS-004).
- The notification bell and dropdown are available only when the user has **Issues.View** (FR-ISS-005).
- Push and email notifications are delivered only to watchers who have **Issues.View** at delivery time.

**Attachments**

- Uploading an attachment requires **Issues.ManageAttachments** (FR-ISS-006).
- Deleting an attachment requires **Issues.ManageAttachments** and the acting user must be **uploaded by** on that attachment (FR-ISS-006).
- Downloading an attachment requires **Issues.View**.

### Operations

- Effective permissions follow FR-ROL-001; issue permissions are included in the permission union from assigned roles.
- After an administrator changes role assignments (FR-ROL-005), new issue permissions apply when the affected user next renews credentials or signs in again (FR-ROL-001).
- Users with **Deactivated** true cannot sign in and have no effective permissions (FR-USR-005).

### Business rules

**Evaluation order** when multiple rules could apply:

1. User is not signed in → guest rules (FR-AUTH-001, STD-ACC-001).
2. User is **Deactivated** → cannot sign in (FR-USR-005).
3. User lacks the base permission and no participant override applies → **`You do not have permission to perform this action.`**
4. Participant override applies → action is allowed for the overridden fields or operations only.
5. Base permission grants access → action is allowed.

**Default role seeding** (FR-ROL-006):

- **Administrator** receives all issue permissions through **all permissions from FR-ROL-001**.
- **User** receives **Issues.View**, **Issues.Create**, and **Issues.Comment** only.

**UI visibility** (STD-ACC-001):

- Actions the user cannot perform, including those blocked by missing participant overrides, are **not shown**.
- Read-only issue details remain available with **Issues.View** when edit, comment, delete, watch, or attachment actions are hidden.

### Validation

- Protected issue operations without permission: rejection message **`You do not have permission to perform this action.`**

## Out of scope

- Project-scoped or row-level security beyond the permissions and overrides above.
- Issue permissions editable by administrators in the UI (catalog changes require a requirements update per FR-ROL-001).

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
