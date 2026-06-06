---
id: REQ-BIL-003
title: User Employment Profile and Contracts
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-002, REQ-USR-004]
---

## Goal

An authorized administrator must be able to view and maintain each user's **employment profile** and **employment contracts** from **User details**.

## Features

### Employment section on User details

- On **User details** (REQ-USR-004), add collapsible section **Employment** below **Roles**.
- Section is visible when the viewer has **Billing.ViewAny** or **Billing.ManageEmployment**.
- Default state: **collapsed** when the user has no employment profile and no contracts; **expanded** when at least one contract exists.

### Employment profile summary

Read-only summary at the top of **Employment**:

| Field                | Behavior                                                                                                                                                  |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Employee ID**      | Internal identifier; **`—`** when not set.                                                                                                                |
| **National ID**      | National identification number (for example PESEL); **`—`** when not set.                                                                                 |
| **Tax ID**           | Tax identifier (for example NIP); **`—`** when not set.                                                                                                   |
| **Bank account**     | IBAN or domestic account number; masked as **`\*\***{last 4}`** for viewers with **Billing.ViewAny** only; full value for **Billing.ManageEmployment\*\*. |
| **Employment notes** | Administrator notes; **`—`** when empty.                                                                                                                  |

- Header action **Edit employment profile** (requires **Billing.ManageEmployment**) opens **Edit employment profile** dialog.

### Edit employment profile dialog

| Field                | Behavior                                                               |
| -------------------- | ---------------------------------------------------------------------- |
| **Employee ID**      | Not required; max **50** characters; unique case-insensitive when set. |
| **National ID**      | Not required; max **20** characters.                                   |
| **Tax ID**           | Not required; max **20** characters.                                   |
| **Bank account**     | Not required; max **34** characters.                                   |
| **Employment notes** | Not required; max **500** characters.                                  |

- **Save changes**: on success show **`Employment profile saved.`**, refresh **Employment** in place.
- **Cancel**: close without saving.
- Inline validation errors stay on the dialog.

### Contracts table

Below the profile summary:

- Section title: **`Contracts`**
- Table columns: **Position**, **Contract type**, **Start date**, **End date**, **FTE**, **Monthly hours norm**, **Rate / salary**, **Status**.
- **Rate / salary** shows **`{hourly rate}/h`** when **Hourly rate** is set, otherwise **`{monthly salary}/mo`**.
- **Status**: badge **`Active`**, **`Future`**, or **`Ended`** derived from **Start date**, **End date**, and today's date.
- **Monthly hours norm** displays as **`{hours}h {minutes}m`** (whole minutes from REQ-BIL-001).
- Empty state: **`No contracts defined.`**
- Row click or **View** opens **Contract details** (read-only) when the viewer has **Billing.ViewAny**.
- Header action **Add contract** (requires **Billing.ManageEmployment**).

### Create contract

- Screen: **Create contract** (reachable from **User details** — **Add contract**)
- Requires **Billing.ManageEmployment**.
- **User** is pre-filled and read-only (the user from **User details**).
- Fields per REQ-BIL-001 **Employment contract** table; **Position** is a dropdown of **Active** positions only.

| Field                  | Validation                                                                               |
| ---------------------- | ---------------------------------------------------------------------------------------- |
| **Contract type**      | Required selection from **`Employment`**, **`Mandate`**, **`Work contract`**, **`B2B`**. |
| **Start date**         | Required.                                                                                |
| **End date**           | Not required; must be on or after **Start date** when set.                               |
| **FTE**                | Required; **0.01–1.00**, two decimal places.                                             |
| **Monthly hours norm** | Required; whole minutes **60–10080**.                                                    |
| **Hourly rate**        | Not required; min **0.01** when set.                                                     |
| **Monthly salary**     | Not required; min **0.01** when set.                                                     |
| **Notes**              | Not required; max **500** characters.                                                    |

- At least one of **Hourly rate** or **Monthly salary** must be filled; form-level error: **`Enter an hourly rate or a monthly salary.`**
- Overlap with existing contracts for the same user: form-level error: **`Contract dates overlap an existing contract.`**
- **Save**: on success show **`Contract created.`**; when this is the user's **first** employment contract, create the **weekly recurring pattern** from organization **Default work hours** (REQ-BIL-004, REQ-BIL-010); return to **User details** with **Employment** expanded.
- **Cancel**: return to **User details** without saving.

### Edit contract

- Screen: **Edit contract**
- Requires **Billing.ManageEmployment**.
- Same fields as **Create contract**, pre-filled.
- **User** and **Position** remain editable.
- Overlap validation excludes the contract being edited.
- **Save changes**: on success show **`Contract saved.`** and return to **User details**.
- Contracts with **Status** **`Ended`** can be edited (for corrections); changing dates may change **Status**.

### Contract details (read-only)

- Screen: **Contract details**
- All contract fields read-only.
- **Back** returns to **User details**.

### My account — employment summary

- On **My account** (REQ-USR-001), when the signed-in user has **Billing.ViewOwn** and an active contract exists, show collapsible section **Employment summary** below **Roles**.
- Read-only fields: **Position**, **Contract type**, **Start date**, **End date** (or **`—`**), **FTE**, **Monthly hours norm**.
- Does **not** show **Employee ID**, **National ID**, **Tax ID**, **Bank account**, rates, or administrator notes.
- When no active contract exists, the section is **not shown**.

### Permissions and visibility

- **Billing.ViewAny**: read-only **Employment** on **User details**; masked **Bank account**.
- **Billing.ManageEmployment**: full view and all edit actions.
- **Billing.ViewOwn**: **Employment summary** on **My account** only (active contract fields listed above).

### Out of scope for this REQ

- Contract document upload and e-signature.
- Automatic contract renewal.

---
