---
id: REQ-INV-006
title: Invitation History Retention
domain: invitations
status: active
depends_on: [REQ-AUTH-010, REQ-AUTH-014]
---
## Goal

**Revoked** and **cancelled** **account invitation** rows (same storage: **RevokedAtUtc** set) must not accumulate indefinitely. **Accepted** rows are **audit history** and are **never** deleted by retention. Configuration lives under **AuthOptions**, similar in spirit to notification retention (`NotificationRetentionOptions`).

## Configuration

Section: **`AuthOptions:Invitations:Retention`**

| Setting                            | Default                                          | Meaning                                                                                                                                                                    |
| ---------------------------------- | ------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **RevokedInvitationRetentionDays** | **7**                                            | Delete **revoked** / **cancelled** invitation rows when older than this many days. Age is measured from **RevokedAtUtc**, or **SentAtUtc** if **RevokedAtUtc** is missing. |
| **CleanupCronExpression**          | Hangfire default (same pattern as notifications) | Schedule for the background cleanup job.                                                                                                                                   |

- Settings live in deployment configuration (for example `appsettings.json`) under the **AuthOptions** area, not under **Notifications**.
- **Pending** and **accepted** invitations are **never** removed by retention.

### Example configuration

```json
"AuthOptions": {
  "Invitations": {
    "InvitationLinkLifetimeHours": 72,
    "Retention": {
      "RevokedInvitationRetentionDays": 7,
      "CleanupCronExpression": "0 4 * * *"
    }
  }
}
```

- Operational reference: `docs/auth-operations-guide.md` (§ Invitation retention).

## Features

### Background cleanup

- Recurring job (Hangfire) deletes **revoked** / **cancelled** `AccountInvitation` rows ( **RevokedAtUtc** not null) that exceed **RevokedInvitationRetentionDays**.
- **Pending** and **accepted** rows are excluded.
- Cleanup does not delete the user account, sessions, or auth tokens (token invalidation remains on resend, cancel, and accept flows).

### On invitation acceptance

When onboarding completes (REQ-AUTH-010, REQ-AUTH-014):

- Set **AcceptedAtUtc** on the pending row (invitation **utilized**); unused invitation tokens are invalidated per REQ-AUTH-010.
- `pendingInvitation` becomes **null**; the **Invitation** panel is hidden.
- **Do not** delete any invitation rows at accept time. The **accepted** row is retained permanently. Earlier **revoked** rows remain until the retention job removes them.

### Permissions and visibility

- Not user-facing; administrators see no invitation history UI (out of scope).

---
