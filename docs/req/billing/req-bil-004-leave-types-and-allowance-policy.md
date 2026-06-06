---
id: REQ-BIL-004
title: Leave Types and Allowance Policy
domain: billing
status: active
depends_on: [REQ-BIL-001]
---

## Goal

The system must define **leave types**, annual allowance rules, and administrator settings that govern how leave balances are calculated.

## Features

### Leave type

A **leave type** classifies absence and determines whether it consumes annual allowance.

| Attribute             | Rule                                                                                             |
| --------------------- | ------------------------------------------------------------------------------------------------ |
| **Name**              | Required; **2–100** characters; unique case-insensitive (for example **`Vacation`**).            |
| **Code**              | Required short code; **2–20** characters; unique case-insensitive (for example **`VAC`**).       |
| **Counts as paid**    | Boolean; when **true**, approved leave of this type reduces **Expected minutes** in settlements. |
| **Uses allowance**    | Boolean; when **true**, approved days reduce the user's **Leave allowance** balance.             |
| **Requires approval** | Boolean; when **true**, requests must reach **`Approved`** through the workflow (REQ-BIL-005).   |
| **Active**            | Boolean; inactive types are hidden from new requests.                                            |

### Seeded leave types

On first startup, the system ensures these leave types exist:

| **Name**         | **Code** | **Counts as paid** | **Uses allowance** | **Requires approval** |
| ---------------- | -------- | ------------------ | ------------------ | --------------------- |
| **Vacation**     | **VAC**  | true               | true               | true                  |
| **Sick leave**   | **SICK** | true               | false              | true                  |
| **Unpaid leave** | **UNPD** | false              | false              | true                  |
| **Other paid**   | **OTH**  | true               | false              | true                  |

- Seeded types can be edited but not deleted.

### Billing settings

Application settings on **Billing reports** — **`Settings`** tab (REQ-BIL-008), section title **`Billing settings`**.

#### Leave allowance

| Setting                       | Rule                                                                                               |
| ----------------------------- | -------------------------------------------------------------------------------------------------- |
| **Default annual leave days** | Required decimal; **0–365** with one fractional digit; default **`26.0`** (Polish full-time norm). |
| **Allow half-day leave**      | Boolean; default **true**. When **false**, **Day portion** is not offered on request forms.        |

- **Default annual leave days** applies to users with an active **`Employment`** contract type and **FTE** **`1.00`**; other users receive **Default annual leave days** × **FTE** rounded to one decimal place.

#### Default work hours

Organization-wide defaults for new **weekly recurring patterns** and half-day leave windows (REQ-BIL-010):

| Setting                         | Rule                                                                                                                                                                                         |
| ------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Default workday start**       | Required time **`HH:mm`**; default **`09:00`**.                                                                                                                                              |
| **Default workday end**         | Required time **`HH:mm`**; must be after **Default workday start**; default **`17:00`**.                                                                                                     |
| **Half-day split time**         | Required time **`HH:mm`**; must be between **Default workday start** and **Default workday end**; default **`13:00`**. Used as end of **`First half`** and start of **`Second half`** leave. |
| **Default workdays**            | Multi-select weekdays; default **Monday**, **Tuesday**, **Wednesday**, **Thursday**, **Friday**.                                                                                             |
| **Default availability status** | Required; one of **`Available`**, **`Remote`**, **`On-site`**; default **`On-site`**.                                                                                                        |

- **Default workday duration** (derived, not stored): minutes between **Default workday start** and **Default workday end** (default **480** minutes).
- When a user's first employment contract is saved (REQ-BIL-003), the system creates a **weekly recurring pattern** from these defaults: each selected **Default workdays** row is **Enabled** with **Default workday start**, scaled **End time**, and **Default availability status**.
- Scaled **End time** for contract **FTE** **`1.00`**: **Default workday end**.
- Scaled **End time** for **FTE** below **`1.00`**: **Default workday start** plus (**Default workday duration** × **FTE**), rounded down to the previous **15**-minute boundary (for example FTE **`0.50`** → **`09:00`–`13:00`** when defaults are **`09:00`–`17:00`**).
- Changing **Default work hours** does **not** alter existing user patterns; it applies only to newly created patterns.

- Editing **Billing settings** requires **Billing.ManageSettlements**.
- On save success, show message **`Billing settings saved.`**
- Validation: inline error **`Default workday end must be after Default workday start.`**; **`Half-day split time must be between workday start and end.`**; **`Select at least one default workday.`**

### Leave balance

A user's **leave allowance** for a calendar year:

- **Entitled days** = **Default annual leave days** × **FTE** of the active contract on the last day of the year (or today's date when viewing the current year), rounded to one decimal.
- When the user had multiple contracts in the year, **Entitled days** uses the contract active on the calculation date; mid-year changes do not retroactively split entitlement in v1.
- **Used days** = sum of **approved** leave days (including half-days as **0.5**) for leave types with **Uses allowance** **true** in that year.
- **Remaining days** = **Entitled days** − **Used days**.

### Leave types administration

- Screen section: **Leave types** on **Billing reports** — **`Settings`** tab.
- Requires **Billing.ManageSettlements** to edit; **Billing.ViewReports** to view read-only.
- Table columns: **Name**, **Code**, **Counts as paid**, **Uses allowance**, **Requires approval**, **Active**.
- Header action **Add leave type**.
- **Create leave type** and **Edit leave type** dialogs use the fields from the **Leave type** table above.
- Custom leave types can be deleted when no leave request references them; confirmation: **`Delete leave type "{name}"?`**
- Seeded leave types cannot be deleted.

### Validation

- **Name** and **Code** uniqueness: inline errors **`A leave type with this name already exists.`** and **`A leave type with this code already exists.`**
- **Default annual leave days**: inline error **`Enter a number from 0 to 365 with at most one decimal place.`**
- **Default workday start** / **Default workday end** / **Half-day split time**: inline error **`Enter a valid time.`**

### Out of scope for this REQ

- Carry-over of unused vacation to the next year.
- Per-user manual entitlement overrides.

---
