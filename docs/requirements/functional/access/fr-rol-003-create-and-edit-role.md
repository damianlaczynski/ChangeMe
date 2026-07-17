---
id: FR-ROL-003
title: Create and Edit Role
domain: access
type: functional
status: active
depends_on: [FR-ROL-001, FR-ROL-005]
inherits_conventions:
  [STD-ACC-001, STD-FRM-001, STD-MSG-001, STD-OP-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

An authorized administrator must be able to create custom roles and edit their name, description, and permissions.

## Functional requirements

### Authorization

- **Roles.Manage**: required to create, edit, and delete custom roles.

### Data

| Field           | Constraints                                                                                               |
| --------------- | --------------------------------------------------------------------------------------------------------- |
| **Name**        | **Required**; **2–100** characters; unique case-insensitive.                                              |
| **Description** | **Not required**; max **500** characters.                                                                 |
| **Permissions** | At least **one** permission from the catalog (FR-ROL-001); grouped by **Users**, **Roles**, **Sessions**. |

### Operations

- Create a custom role with name, description, and selected permissions.
- Edit name, description, and permissions on custom roles only.
- Delete a custom role that has no user assignments.
- **System** roles (**Administrator**, **User**) cannot be created, edited, or deleted.

| Role              | Editable | Deletable            | Permissions                                               |
| ----------------- | -------- | -------------------- | --------------------------------------------------------- |
| **Administrator** | No       | No                   | All catalog permissions; fixed.                           |
| **User**          | No       | No                   | **Sessions.ViewOwn**, **Sessions.ManageOwn** only; fixed. |
| Custom roles      | Yes      | Yes, when unassigned | Selected from catalog at create/edit time.                |

### Validation

- **Name**: required; **2–100** characters; unique case-insensitive; rejection message: **`A role with this name already exists.`**
- **Description**: max **500** characters when not empty; rejection message: **`Description cannot exceed 500 characters.`**
- **Permissions**: at least one selected; rejection message: **`At least one permission is required.`**
- **Delete role**: confirmation message **`Delete role {role name}? Users will lose permissions granted only through this role.`**
- A role assigned to **one or more users** cannot be deleted; rejection message: **`Role is assigned to one or more users. Remove all user assignments before deleting this role.`**
- Attempting to edit a system role is rejected; rejection message: **`System roles cannot be modified.`**

### Business rules

- Creating or editing a role does **not** change user assignments; assignments are managed per FR-ROL-005.
- Permission changes on a role take effect for assigned users after their next credential renewal or sign-in.
- Deleting a role removes permissions granted only through that role from affected users after their next credential renewal or sign-in.

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) unless stated above.
- Document only overrides in this section when this specification differs from inherited quality or convention standards.
