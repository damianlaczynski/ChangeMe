---
id: FR-ISS-001
title: Issue List
domain: issues
type: functional
status: active
depends_on: [FR-AUTH-001, FR-USR-005, FR-ISS-007]
inherits_conventions: [STD-ACC-001, STD-LST-001, STD-MSG-001, STD-OP-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The user must be able to browse all issues, search, filter, sort, open details, manage watches, and create new issues.

## Functional requirements

### Authorization

- **Issues** navigation: **Issues.View** or **Issues.Create** (FR-ISS-007).
- Browse, search, filter, sort, and open details: **Issues.View**.
- Create: **Issues.Create** (FR-ISS-002).
- Edit: **Issues.Edit**, with author and assignee overrides per FR-ISS-007.
- Delete: **Issues.Delete**.
- Watch or unwatch: **Issues.View** (FR-ISS-004).

### Data

- Each issue exposes **title**, **status**, **priority**, **assigned to**, **created at**, and **last activity**.
- **Assigned to** displays assignee name and email, or **Unassigned** when none.
- Inherits STD-LST-001 unless stated below.
- **Filterable** attributes: title, status, priority, assigned to (assignable users per FR-USR-005), created at, last activity.
- Global search matches issue identifier, **title**, and **description**.
- Default sort: **last activity**, descending.

### Operations

- Browse all issues.
- Create a new issue (FR-ISS-002).
- Open issue details (FR-ISS-003).
- Edit an issue (FR-ISS-002).
- Delete an issue after confirmation; deletion cannot be undone.
- Watch or unwatch an issue (FR-ISS-004).

### Validation

- **Delete issue**: confirmation message **`Delete "{issue title}"? This cannot be undone.`**

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
