# 2026-06-13 — Project resource-scoped permissions

**Status:** done (2026-06-13)

## Why

Project stewardship must be scoped to each project. Global **Projects.Manage** must not allow editing every project in the system.

## Functional specifications touched

| FR           | Action                                                                     | File                                                                           |
| ------------ | -------------------------------------------------------------------------- | ------------------------------------------------------------------------------ |
| `FR-PRJ-001` | **Updated** — settings/delete per **Owner**                                | `functional/projects/fr-prj-001-projects-list.md`                              |
| `FR-PRJ-002` | **Updated** — edit/delete per **Owner**; **Projects.Manage** = create only | `functional/projects/fr-prj-002-create-and-edit-project.md`                    |
| `FR-PRJ-004` | **Updated** — member management per **Owner**                              | `functional/projects/fr-prj-004-project-members-and-settings.md`               |
| `FR-ROL-001` | **Updated** — **Projects.Manage** description                              | `functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md` |

## Non-functional and shared docs touched

| ID / path                          | Action                                  |
| ---------------------------------- | --------------------------------------- |
| `_shared/reference/permissions.md` | **Updated** — **Projects.Manage** scope |

## Behavior delta

**Before**

- **Projects.Manage** allowed editing, deleting, and managing members on **any** project.

**After**

- **Projects.Manage** allows **Create project** only.
- **Project Owner** (project member role) manages profile, members, and deletion for **that project only**.
- **Member** and **Viewer** project roles cannot manage the project.
- Denied stewardship action: **`You do not have permission to manage this project.`**

## Implementation scope (optional)

- Backend: `Project.CanManage`, handler checks, endpoint permission downgrade to **Projects.View** for per-project mutations.
- Frontend: `currentUserRole` on project DTOs; UI uses **Owner** role instead of global **Projects.Manage** for settings/members/delete.
- Tests: domain, integration (owner vs non-owner), frontend unit (`projects-list`, `project-settings`, `projects.utils`), E2E smoke (`projects.smoke.spec.ts`, updated `auth.smoke.spec.ts`).
- E2E: **required** — post-login **Projects list**, workspace navigation, owner adds member in **Project settings**.
