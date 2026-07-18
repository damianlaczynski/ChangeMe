# 2026-07-17 — Issue permissions and user invitations

**Status:** pending

## Why

Introduce two substantial capabilities to exercise the five-layer requirements model end to end: cross-cutting issue access control (L1 + L4 updates across existing specs) and a new invitations domain with guest and administrator flows.

## Functional specifications touched

| FR            | Action  | File                                                                           |
| ------------- | ------- | ------------------------------------------------------------------------------ |
| `FR-ISS-007`  | **New** | `functional/issues/fr-iss-007-issue-permissions.md`                            |
| `FR-INV-001`  | **New** | `functional/invitations/fr-inv-001-user-invitations.md`                        |
| `FR-ISS-001`  | Updated | `functional/issues/fr-iss-001-issue-list.md`                                   |
| `FR-ISS-002`  | Updated | `functional/issues/fr-iss-002-issue-create-and-edit-flow.md`                   |
| `FR-ISS-003`  | Updated | `functional/issues/fr-iss-003-issue-details-comments-and-change-history.md` |
| `FR-ISS-004`  | Updated | `functional/issues/fr-iss-004-watching-issues-and-push-email-notifications.md` |
| `FR-ISS-005`  | Updated | `functional/issues/fr-iss-005-notification-bell-and-dropdown.md`               |
| `FR-ISS-006`  | Updated | `functional/issues/fr-iss-006-issue-attachments.md`                            |
| `FR-ROL-001`  | Updated | `functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md` |
| `FR-ROL-006`  | Updated | `functional/access/fr-rol-006-initial-administrator-and-system-roles.md`       |
| `FR-USR-003`  | Updated | `functional/users/fr-usr-003-edit-user-admin.md`                                 |

## Shared documents touched

| ID / path                         | Action                                                                 |
| --------------------------------- | ---------------------------------------------------------------------- |
| `_shared/domain/issue-model.md`   | **New** — issue attributes, participants, link to FR-ISS-007           |
| `_shared/domain/glossary.md`      | **Updated** — invitation terms and cross-references                    |
| `_shared/domain/permissions.md`  | **Updated** — Issues.* and Users.Invite in catalog reference           |
| `_shared/domain/README.md`        | **Updated** — index entry for issue-model.md                           |

## Behavior delta

**Before**

- Every signed-in user with **Deactivated** false had full access to all issue operations.
- New accounts were created only by administrators with a password (FR-USR-003).
- Permission catalog covered Users, Roles, and Sessions only.

**After**

- Issue access is governed by six **Issues.*** permissions plus **author** and **assignee** participant overrides (FR-ISS-007).
- Seeded **User** role receives **Issues.View**, **Issues.Create**, and **Issues.Comment**; **Administrator** receives all catalog permissions including the new ones.
- Administrators with **Users.Invite** can send time-limited email invitations; invitees complete account setup on a guest acceptance flow (FR-INV-001).
- **Users.Invite** is a separate permission from **Users.Manage**; invitation role selection follows the same **Roles.Manage** visibility rules as direct user creation.

## Implementation scope

- **Backend:** extend `PermissionCatalog` / seeding; authorization on issue endpoints; invitation aggregate (token hash, status, expiry); acceptance command creating `User`; email on invite/resend; migrate existing deployments to grant new permissions to **Administrator** and **User** roles per FR-ROL-006.
- **Frontend:** permission-gated Issues navigation and actions; invitations list and create/resend/revoke; guest accept-invitation form; sign-in success banner after acceptance; update role permission preview groups.
- **Tests:**
  - **Integration (required):** permission matrix per FR-ISS-007 (403 + STD-ACC-001 message); author/assignee overrides; invitation create/resend/revoke/accept/expired/revoked paths; duplicate email rules.
  - **Frontend unit (required):** hidden actions without permission; invitation form validation; accept form; notification bell hidden without **Issues.View**.
  - **E2E (required):** invite → accept → sign-in journey; optional smoke for author editing own issue without **Issues.Edit** if role fixture is practical in Playwright.
