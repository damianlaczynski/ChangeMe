---
id: REQ-PKY-007
title: Passkey Notification Emails
domain: passkeys
status: active
depends_on: [REQ-AUTH-007]
---
## Goal

Users must receive email notification when passkeys are added, removed, or administratively reset, consistent with other auth notification emails.

## Features

### Email types

| Event                       | Trigger                              | Subject line (approximate)          |
| --------------------------- | ------------------------------------ | ----------------------------------- |
| **Passkey added**           | User completes **Add passkey**       | `Passkey added to your account`     |
| **Passkey removed**         | User **Remove passkey** self-service | `Passkey removed from your account` |
| **Passkeys reset by admin** | Administrator **Reset passkeys**     | `Passkeys reset on your account`    |

- Each email includes **account email**, **event time** (UTC), **passkey name** when applicable, and guidance **`If you did not perform this action, contact your administrator immediately.`**
- Emails require working **Email** configuration (same as REQ-AUTH-007).
- Admin per-credential **Remove** sends **Passkey removed** with the credential name.

### Auth notification list (REQ-AUTH-007)

- Extend REQ-AUTH-007 notification catalog with the three rows above when passkeys are implemented.

### States and business rules

- **Out of scope for this REQ:** email on every passkey sign-in (high volume); push notifications.
