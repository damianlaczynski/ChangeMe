# Issue model

> Observable issue attributes and participant roles used across Issues specifications.

## Core attributes

| Concept            | Meaning                                                                                                      |
| ------------------ | ------------------------------------------------------------------------------------------------------------ |
| **Issue**          | A tracked work item with title, description, status, priority, author, optional assignee, and related data. |
| **Issue identifier** | System-assigned stable id used in navigation and search (FR-ISS-002).                                      |
| **Author**         | User who created the issue; stored on first save.                                                            |
| **Assigned to**    | Optional assignable user responsible for the issue; **`Unassigned`** when none (FR-ISS-001).                 |
| **Last activity**  | System-maintained timestamp of the latest issue change or comment.                                           |

## Status and priority

| Concept      | Allowed values                                                          |
| ------------ | ----------------------------------------------------------------------- |
| **Status**   | **New**, **In Progress**, **Resolved**, **Closed** (FR-ISS-002).        |
| **Priority** | **Low**, **Medium**, **High**, **Critical** (FR-ISS-002).               |

## Participants

| Role on an issue | Meaning                                                                                   |
| ---------------- | ----------------------------------------------------------------------------------------- |
| **Author**       | Creator of the issue; may have participant overrides per FR-ISS-007.                      |
| **Assigned to**  | Current assignee; may have participant overrides on status and priority per FR-ISS-007.   |
| **Watcher**      | User watching the issue for notifications (FR-ISS-004).                                   |

## Related collections

| Concept                    | Meaning                                                        |
| -------------------------- | -------------------------------------------------------------- |
| **Acceptance criterion**   | Checklist item defining done for the issue (FR-ISS-002).       |
| **Comment**                  | User-authored note on the issue (FR-ISS-003).                  |
| **Change history entry**   | Read-only audit event on the issue timeline (FR-ISS-003).    |
| **Attachment**             | File linked to the issue (FR-ISS-006).                         |

## Access

Issue permissions and participant overrides: `docs/requirements/functional/issues/fr-iss-007-issue-permissions.md` (`FR-ISS-007`).
