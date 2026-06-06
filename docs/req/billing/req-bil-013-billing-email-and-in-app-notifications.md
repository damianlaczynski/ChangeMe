---
id: REQ-BIL-013
title: Billing Email and In-App Notifications
domain: billing
status: active
depends_on: [REQ-AUTH-007, REQ-BIL-010, REQ-BIL-011, REQ-BIL-012, REQ-ISS-005]
---

## Goal

The system must notify users by email and in the notification bell when their availability changes or when an administrator updates availability on their behalf.

## Features

### Notification destination

- Every email in this REQ is sent to the affected user's **Profile email** (REQ-AUTH-007).
- In-app notifications are delivered to the affected user's notification bell (REQ-ISS-005).
- Failed email delivery does not roll back the triggering action; the UI still shows the success message for the action.

### Events that generate notifications

| Event                               | When sent                                                                                                   | Email subject (exact)                          | In-app message (exact)                                                         |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------- | ---------------------------------------------- | ------------------------------------------------------------------------------ |
| **Availability updated by admin**   | An administrator with **Billing.ManageAvailability** saves or deletes a **`Manual`** entry for another user | `Your availability was updated`                | `Your availability was updated by {actor full name} for {date or date range}.` |
| **Weekly pattern updated by admin** | An administrator saves another user's **weekly recurring pattern**                                          | `Your weekly availability pattern was updated` | `Your weekly availability pattern was updated by {actor full name}.`           |
| **Availability updated by self**    | The signed-in user saves or deletes their own **`Manual`** entry or **weekly recurring pattern**            | not sent                                       | `Your availability was updated for {date or date range}.`                      |

- **Availability updated by admin** email body includes: actor full name, affected date or range, **Availability status**, time range (**`All day`** or **`{start}–{end}`**), and button **`View my availability`** linking to **My availability**.
- **Weekly pattern updated by admin** email body includes actor full name and button **`View my availability`**.
- **Availability updated by self** creates an in-app notification only; no email is sent.
- **`Leave`** overlay changes from leave approval or cancellation do **not** trigger notifications in this REQ (leave has its own workflow screens).
- Duplicate in-app notification records for the same availability entry revision and recipient are not created.

### In-app notification presentation

- Billing notifications appear in the same bell dropdown as issue notifications (REQ-ISS-005).
- Each billing notification shows: **message**, **event time**, and actions **Open** and **Mark read**.
- **Open** navigates to **My availability** with the calendar focused on the affected date when a single date is known; otherwise opens **My availability** on today's month.
- When the viewer has **Billing.ViewAny** and the notification is about another user (not used in v1 — all billing notifications target the affected user only), **Open** would navigate to **Availability calendar**; v1 notifications are always for the signed-in affected user.

### Notification recipients

| Trigger actor                                          | Recipient     | Email | In-app |
| ------------------------------------------------------ | ------------- | ----- | ------ |
| Administrator (**ManageAvailability**) on another user | Affected user | Yes   | Yes    |
| User on own availability (**ManageOwnAvailability**)   | Same user     | No    | Yes    |

- The acting user does **not** receive a notification for changes they perform on their own availability.

### Permissions and visibility

- Receiving notifications requires the user to be able to sign in; **Deactivated** users receive no new notifications.
- Reading in-app notifications requires sign-in; the bell is available to all authenticated users per REQ-ISS-005.

### Out of scope for this REQ

- Email notifications for leave approval decisions (future REQ).
- Digest or weekly summary emails.
- Configurable per-user notification opt-out.

---
