# 2026-07-17 — Business-capability requirements refocus

**Status:** done (2026-07-17)

## Why

Functional requirements and authoring instructions were reorganized to focus on key business value, application behavior, and implementation-relevant rules — not screen layout or presentation detail.

## Functional specifications touched

| FR            | Action  | File                                                                           |
| ------------- | ------- | ------------------------------------------------------------------------------ |
| `FR-ROL-001`  | Updated | `functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md` |
| `FR-ROL-002`  | Updated | `functional/access/fr-rol-002-roles-list.md`                                   |
| `FR-ROL-003`  | Updated | `functional/access/fr-rol-003-create-and-edit-role.md`                         |
| `FR-ROL-004`  | Updated | `functional/access/fr-rol-004-role-details.md`                                 |
| `FR-ROL-005`  | Updated | `functional/access/fr-rol-005-role-and-user-assignments.md`                    |
| `FR-ROL-006`  | Updated | `functional/access/fr-rol-006-initial-administrator-and-system-roles.md`       |
| `FR-AUTH-001` | Updated | `functional/identity/fr-auth-001-login-with-sessions.md`                       |
| `FR-AUTH-002` | Updated | `functional/identity/fr-auth-002-staying-signed-in.md`                         |
| `FR-AUTH-003` | Updated | `functional/identity/fr-auth-003-logout.md`                                    |
| `FR-AUTH-004` | Updated | `functional/identity/fr-auth-004-my-sessions.md`                               |
| `FR-AUTH-008` | Updated | `functional/identity/fr-auth-008-password-policy.md`                           |
| `FR-USR-001`  | Updated | `functional/users/fr-usr-001-my-account-profile.md`                            |
| `FR-USR-002`  | Updated | `functional/users/fr-usr-002-user-list.md`                                     |
| `FR-USR-003`  | Updated | `functional/users/fr-usr-003-edit-user-admin.md`                               |
| `FR-USR-004`  | Updated | `functional/users/fr-usr-004-user-details-and-session-administration.md`       |
| `FR-USR-005`  | Updated | `functional/users/fr-usr-005-deactivate-and-activate-accounts.md`              |
| `FR-ISS-001`  | Updated | `functional/issues/fr-iss-001-issue-list.md`                                   |
| `FR-ISS-002`  | Updated | `functional/issues/fr-iss-002-issue-create-and-edit-flow.md`                   |
| `FR-ISS-003`  | Updated | `functional/issues/fr-iss-003-issue-details-comments-and-change-history.md`    |
| `FR-ISS-004`  | Updated | `functional/issues/fr-iss-004-watching-issues-and-push-email-notifications.md` |
| `FR-ISS-005`  | Updated | `functional/issues/fr-iss-005-notification-bell-and-dropdown.md`               |
| `FR-ISS-006`  | Updated | `functional/issues/fr-iss-006-issue-attachments.md`                            |

## Non-functional and shared docs touched

| ID / path                               | Action                                                               |
| --------------------------------------- | -------------------------------------------------------------------- |
| `requirements-authoring-guide.md`       | **Updated** — capability-based structure; include/omit guidance      |
| `_functional-specification-template.md` | **Updated** — Authorization / Data / Operations / Validation / Rules |
| `requirements-change-process.md`        | **Updated** — FR definition is business capability, not per-screen   |

## Behavior delta

**Before:** Functional specifications were organized per screen (access, table columns, header actions, navigation labels) and mandated exact UI copy and layout detail alongside business rules.

**After:** Specifications are organized by business concern (authorization, data, operations, validation, business rules). Presentation defaults defer to L2 Conventions (`product-standards.md`). Screen layout, column tables, button placement, and generic navigation copy are omitted. Core behavior — permissions, field constraints, state transitions, side effects, rejection messages, and evaluation order — is preserved.

No intentional product behavior change; this is a documentation refocus. Implementation should continue to match existing behavior unless a trimmed bullet reveals an undocumented gap.

## Implementation scope (optional)

- Documentation only. No code changes required unless review finds a spec trim that no longer matches shipped behavior.
