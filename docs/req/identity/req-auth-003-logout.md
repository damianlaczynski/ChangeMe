---
id: REQ-AUTH-003
title: Logout
domain: identity
status: active
depends_on: [REQ-USR-001]
---
## Goal

The user must be able to sign out from the current browser or from all devices.

## Features

### Logout (current browser)

- **Logout** button in the application header signs the user out of the **current session**.
- The user is redirected to **Login**.
- Protected screens are no longer accessible until the user signs in again.

### Sign out everywhere

- **Sign out everywhere** button is a header action on **My account** (REQ-USR-001).
- Clicking **Sign out everywhere** opens confirmation dialog: **`Sign out from all devices? You will be signed out on every browser and device.`**
- On confirm, the system revokes **all active sessions** for the user, signs out the current browser, and redirects to **Login**.

### States and business rules

- Repeating logout when already signed out redirects to **Login** without error.
- A revoked session cannot access protected screens or renew credentials.

---
