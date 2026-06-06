---
id: REQ-BIL-002
title: Position Catalog
domain: billing
status: active
depends_on: [REQ-BIL-001]
---

## Goal

An authorized administrator must be able to maintain a catalog of organizational **positions** used on employment contracts.

## Features

### Positions list

- Screen: **Positions**
- Requires **Billing.ManageEmployment** or **Billing.ViewAny**; users with only **Billing.ViewAny** see a read-only list without create or edit actions.
- Page title: **`Positions`**.

### List columns

| Column         | Behavior                                                               |
| -------------- | ---------------------------------------------------------------------- |
| **Name**       | Position name; link to **Position details** when the user can view.    |
| **Department** | Department or team name; **`—`** when empty.                           |
| **Active**     | Badge **`Active`** or **`Inactive`**.                                  |
| **Contracts**  | Count of employment contracts that reference this position (all time). |

- Default sort: **Name** ascending.
- Search filters rows by **Name** or **Department** (case-insensitive contains).
- Empty state: **`No positions defined.`**
- Header action **Add position** (requires **Billing.ManageEmployment**).

### Position details

- Screen: **Position details**
- Requires **Billing.ViewAny** or **Billing.ManageEmployment**.
- Read-only fields: **Name**, **Department**, **Description**, **Active**, **Contracts** count.
- **Description** shows **`—`** when empty.
- Header action **Edit** when the user has **Billing.ManageEmployment**.
- **Back** returns to **Positions**.

### Create position

- Screen: **Create position**
- Requires **Billing.ManageEmployment**.

| Field           | Behavior                                                 |
| --------------- | -------------------------------------------------------- |
| **Name**        | Required; **2–100** characters; unique case-insensitive. |
| **Department**  | Not required; max **100** characters.                    |
| **Description** | Not required; max **500** characters.                    |
| **Active**      | Toggle; default **true** (**`Active`**).                 |

- **Save**: validate, save, show message **`Position created.`**, open **Position details**.
- **Cancel**: return to **Positions** without saving.

### Edit position

- Screen: **Edit position**
- Same fields and validation as **Create position**, pre-filled.
- **Save changes**: on success show message **`Position saved.`** and open **Position details**.
- **Cancel**: return to **Position details** without saving.

### Validation

- **Name**: required; **2–100** characters; unique case-insensitive; inline error on duplicate: **`A position with this name already exists.`**
- **Department**: max **100** characters when not empty.
- **Description**: max **500** characters when not empty.

### States and business rules

- Setting **Active** to **false** hides the position from new contract forms; existing contracts keep their position reference.
- A position referenced by at least one contract cannot be deleted; **Delete position** is not shown in that case.
- **Delete position** (unreferenced positions only): confirmation **`Delete position "{name}"?`**; on confirm show **`Position deleted.`** and return to **Positions**.

### Permissions and visibility

- **Billing.ManageEmployment**: required for **Add position**, **Create position**, **Edit position**, and **Delete position**.
- **Billing.ViewAny**: sufficient for read-only **Positions** and **Position details**.

---
