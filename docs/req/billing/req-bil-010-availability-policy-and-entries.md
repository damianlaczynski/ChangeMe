---
id: REQ-BIL-010
title: Availability Policy and Entries
domain: billing
status: active
depends_on: [REQ-BIL-001, REQ-BIL-004, REQ-BIL-005, REQ-ROL-001]
---

## Goal

The system must define **availability entries** and a **weekly recurring pattern** per user, overlay **approved leave** on the availability calendar, and extend billing permissions for viewing and editing availability.

## Features

### Availability status

Each availability record uses exactly one **Availability status**:

| Status            | Meaning                                                | Calendar color semantic |
| ----------------- | ------------------------------------------------------ | ----------------------- |
| **`Available`**   | The user can be scheduled for work on that time.       | success                 |
| **`Unavailable`** | The user cannot be scheduled (outside leave workflow). | danger                  |
| **`Remote`**      | The user works remotely on that time.                  | info                    |
| **`On-site`**     | The user works on-site on that time.                   | primary                 |

### Availability entry

An **availability entry** declares availability for one user over a date or time range.

| Attribute               | Rule                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------- |
| **User**                | Required. The account the entry applies to.                                                                   |
| **Start date**          | Required. First calendar day of the entry.                                                                    |
| **End date**            | Required. Last calendar day; must be on or after **Start date**.                                              |
| **All day**             | Boolean. Default **true**. When **false**, allowed only when **Start date** equals **End date**.              |
| **Start time**          | Required when **All day** is **false**; format **`HH:mm`** (24-hour).                                         |
| **End time**            | Required when **Start time** is set; must be after **Start time** on the same day.                            |
| **Availability status** | Required. One of **`Available`**, **`Unavailable`**, **`Remote`**, **`On-site`**.                             |
| **Notes**               | Not required; max **500** characters when provided.                                                           |
| **Source**              | **`Manual`**, **`Recurring`**, or **`Leave`**. Immutable after create except **Manual** entries are editable. |
| **Created at**          | System timestamp when the entry was saved.                                                                    |
| **Updated at**          | System timestamp when the entry was last edited.                                                              |

- **Manual** entries are created and edited by users with **Billing.ManageOwnAvailability** (own user) or **Billing.ManageAvailability** (any user).
- **Recurring** entries are generated from the **weekly recurring pattern** (below); they are replaced when the pattern is saved.
- **Leave** entries are generated from **`Approved`** leave requests (REQ-BIL-005); they are read-only on availability screens.

### Weekly recurring pattern

Each user has at most one **weekly recurring pattern** — a seven-row template (Monday through Sunday).

| Row field               | Rule                                                                                                                                            |
| ----------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| **Day of week**         | Fixed **`Monday`** … **`Sunday`**.                                                                                                              |
| **Enabled**             | Boolean. When **false**, that day has no recurring baseline.                                                                                    |
| **Start time**          | Required when **Enabled** is **true**; format **`HH:mm`**.                                                                                      |
| **End time**            | Required when **Enabled** is **true**; must be after **Start time**.                                                                            |
| **Availability status** | Required when **Enabled** is **true**; one of **`Available`**, **`Remote`**, **`On-site`**. **`Unavailable`** is not allowed on recurring rows. |

- When the user's **first** employment contract is saved (REQ-BIL-003), the system creates a **weekly recurring pattern** from **Default work hours** in billing settings (REQ-BIL-004): each **Default workdays** weekday is **Enabled** with **Default workday start**, FTE-scaled end time, and **Default availability status**; all other weekdays are **Enabled** **false**.
- When a user already has a pattern and a new contract starts, the existing pattern is **not** replaced automatically.
- Users without any pattern and without an active contract: all days **Enabled** **false**.
- Saving a pattern deletes prior **`Recurring`** entries for that user from **today** through **365** calendar days ahead and regenerates them from the new template.
- **`Recurring`** entries do not replace **`Manual`** entries or **`Leave`** entries on overlapping dates; display priority is **`Leave`** > **`Manual`** > **`Recurring`**.

### Leave overlay

- When a leave request becomes **`Approved`**, the system creates **`Leave`** **availability entries** for each calendar day in the request range.
- **`Leave`** entries use **Availability status** **`Unavailable`**, **All day** **true**, and **Notes** **`{Leave type name}`** (for example **`Vacation`**).
- Single-day leave with **Day portion** **`First half`**: **All day** **false**; **Start time** = **Default workday start** from billing settings; **End time** = **Half-day split time** from billing settings.
- Single-day leave with **Day portion** **`Second half`**: **All day** **false**; **Start time** = **Half-day split time**; **End time** = **Default workday end** from billing settings.
- When billing settings are unavailable at generation time, use **`09:00`**, **`13:00`**, and **`17:00`** respectively.
- When **`Approved`** leave is **`Cancelled`** or **`Rejected`** after approval, corresponding **`Leave`** entries are removed.
- **`Leave`** entries cannot be edited or deleted from availability screens.

### Overlap validation

- **`Manual`** entries for the same user must not overlap another **`Manual`** entry on the same date and time range.
- Overlap error: **`Availability overlaps an existing entry.`**
- Overlap with **`Recurring`** or **`Leave`** is allowed; higher-priority sources win in display only.

### Global permission catalog

The global catalog from REQ-ROL-001 is extended with exactly these permissions:

| Permission                        | Label (exact)            | Description                                                                            | Group   |
| --------------------------------- | ------------------------ | -------------------------------------------------------------------------------------- | ------- |
| **Billing.ManageOwnAvailability** | Manage own availability  | Create, edit, delete own **Manual** entries and save own **weekly recurring pattern**. | Billing |
| **Billing.ManageAvailability**    | Manage user availability | Create, edit, delete **Manual** entries and **weekly recurring pattern** for any user. | Billing |

- Viewing own availability calendar requires **Billing.ViewOwn** (extended in REQ-BIL-011).
- Viewing the team availability calendar requires **Billing.ViewAny** (extended in REQ-BIL-012).

### Default role assignments

- The seeded **User** system role (REQ-ROL-006) includes **Billing.ManageOwnAvailability** in addition to **Billing.ViewOwn**.
- The seeded **Administrator** role includes **Billing.ManageOwnAvailability** and **Billing.ManageAvailability**.

### Authorization rules

- **Billing.ManageOwnAvailability**: create, edit, and delete own **`Manual`** entries; save own **weekly recurring pattern**; cannot edit **`Leave`** entries or other users' data.
- **Billing.ManageAvailability**: same actions for any user; cannot edit **`Leave`** entries.
- Access denial uses the standard global message from REQ-ROL-001.

### Change notifications

- Email and in-app notifications for availability changes follow REQ-BIL-013.
- Saving a **`Manual`** entry or **weekly recurring pattern** triggers notifications per REQ-BIL-013 rules.

### Out of scope for this REQ

- Booking meetings or resource reservations.
- Automatic availability inference from logged time entries.
- iCal export and external calendar sync.

---
