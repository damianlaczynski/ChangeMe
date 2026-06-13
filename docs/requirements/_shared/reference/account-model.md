# Account model

> Observable account attributes used across Users and Auth REQs.

Administrative enablement is separate from how the user signs in.

## Attributes

| Concept                      | Shown in UI / admin              | Meaning                                                                 |
| ---------------------------- | -------------------------------- | ----------------------------------------------------------------------- |
| **Deactivated**              | **Status** **`Deactivated`**     | Whether an administrator disabled the account.                          |
| **Deactivated at**           | **User details**                 | When the account was last deactivated, if applicable.                   |
| **Password last changed at** | **User details**                 | When the local password was last set or changed.                        |

## Status (UI only, read-only)

**`Active`** or **`Deactivated`** on **Users list** and **User details**.

- **`Active`**: **Deactivated** is **false**.
- **`Deactivated`**: **Deactivated** is **true**.

## Other rules

- **First name** and **Last name** are **required** on **Create user** and **Edit user** (FR-USR-003).
- **Password last changed at** is **not shown** on **My account** (FR-USR-001).
