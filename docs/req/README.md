# Requirements index

> Manifest of atomic REQ files. Process: `docs/requirements-change-process.md`. Validate: `npm run req:validate`.

## Shared cross-cutting docs

| Document | Purpose |
| -------- | ------- |
| [_shared/glossary.md](_shared/glossary.md) | Business terms (account, sign-in, invitations) |
| [_shared/account-model.md](_shared/account-model.md) | Observable account attributes |
| [_shared/compliance-gates.md](_shared/compliance-gates.md) | Post-sign-in gate ordering |
| [_shared/permissions.md](_shared/permissions.md) | Permission catalog summary |

## Pending changes

See [changes/](changes/) for open requirement deltas.


## Identity (auth) (`identity/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-AUTH-001 | Login and Registration with Sessions | [req-auth-001-login-and-registration-with-sessions.md](identity/req-auth-001-login-and-registration-with-sessions.md) |
| REQ-AUTH-002 | Staying Signed In | [req-auth-002-staying-signed-in.md](identity/req-auth-002-staying-signed-in.md) |
| REQ-AUTH-003 | Logout | [req-auth-003-logout.md](identity/req-auth-003-logout.md) |
| REQ-AUTH-004 | My Sessions | [req-auth-004-my-sessions.md](identity/req-auth-004-my-sessions.md) |
| REQ-AUTH-005 | Change Password | [req-auth-005-change-password.md](identity/req-auth-005-change-password.md) |
| REQ-AUTH-006 | Forgot and Reset Password (Self-Service) | [req-auth-006-forgot-and-reset-password-self-service.md](identity/req-auth-006-forgot-and-reset-password-self-service.md) |
| REQ-AUTH-007 | Auth Email Notifications | [req-auth-007-auth-email-notifications.md](identity/req-auth-007-auth-email-notifications.md) |
| REQ-AUTH-008 | Password Policy | [req-auth-008-password-policy.md](identity/req-auth-008-password-policy.md) |
| REQ-AUTH-009 | Password Expiration | [req-auth-009-password-expiration.md](identity/req-auth-009-password-expiration.md) |
| REQ-AUTH-010 | Accept Account Invitation | [req-auth-010-accept-account-invitation.md](identity/req-auth-010-accept-account-invitation.md) |
| REQ-AUTH-011 | Email Verification | [req-auth-011-email-verification.md](identity/req-auth-011-email-verification.md) |
| REQ-AUTH-012 | Public Registration Policy | [req-auth-012-public-registration-policy.md](identity/req-auth-012-public-registration-policy.md) |
| REQ-AUTH-013 | Two-Factor Authentication | [req-auth-013-two-factor-authentication.md](identity/req-auth-013-two-factor-authentication.md) |
| REQ-AUTH-014 | External Identity Providers | [req-auth-014-external-identity-providers.md](identity/req-auth-014-external-identity-providers.md) |
| REQ-AUTH-015 | Self-Service Email Change | [req-auth-015-self-service-email-change.md](identity/req-auth-015-self-service-email-change.md) |

## Users (`users/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-USR-001 | My Account Profile | [req-usr-001-my-account-profile.md](users/req-usr-001-my-account-profile.md) |
| REQ-USR-002 | User List | [req-usr-002-user-list.md](users/req-usr-002-user-list.md) |
| REQ-USR-003 | Edit User (Admin) | [req-usr-003-edit-user-admin.md](users/req-usr-003-edit-user-admin.md) |
| REQ-USR-004 | User Details and Session Administration | [req-usr-004-user-details-and-session-administration.md](users/req-usr-004-user-details-and-session-administration.md) |
| REQ-USR-005 | Deactivate and Activate Accounts | [req-usr-005-deactivate-and-activate-accounts.md](users/req-usr-005-deactivate-and-activate-accounts.md) |
| REQ-USR-006 | Admin Send Password Reset | [req-usr-006-admin-send-password-reset.md](users/req-usr-006-admin-send-password-reset.md) |
| REQ-USR-007 | Admin Confirm Email | [req-usr-007-admin-confirm-email.md](users/req-usr-007-admin-confirm-email.md) |

## Invitations (`invitations/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-INV-001 | Invite User (Administrator) | [req-inv-001-invite-user-administrator.md](invitations/req-inv-001-invite-user-administrator.md) |
| REQ-INV-002 | Pending Invitation Banner (User Details) | [req-inv-002-pending-invitation-banner-user-details.md](invitations/req-inv-002-pending-invitation-banner-user-details.md) |
| REQ-INV-003 | Resend Invitation | [req-inv-003-resend-invitation.md](invitations/req-inv-003-resend-invitation.md) |
| REQ-INV-004 | Cancel Invitation | [req-inv-004-cancel-invitation.md](invitations/req-inv-004-cancel-invitation.md) |
| REQ-INV-005 | User Status | [req-inv-005-user-status.md](invitations/req-inv-005-user-status.md) |
| REQ-INV-006 | Invitation History Retention | [req-inv-006-invitation-history-retention.md](invitations/req-inv-006-invitation-history-retention.md) |
| REQ-INV-007 | Accept Invitation — Guest Screen Presentation | [req-inv-007-accept-invitation-guest-screen-presentation.md](invitations/req-inv-007-accept-invitation-guest-screen-presentation.md) |

## Access (roles & permissions) (`access/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-ROL-001 | Permission Catalog and Effective Permissions | [req-rol-001-permission-catalog-and-effective-permissions.md](access/req-rol-001-permission-catalog-and-effective-permissions.md) |
| REQ-ROL-002 | Roles List | [req-rol-002-roles-list.md](access/req-rol-002-roles-list.md) |
| REQ-ROL-003 | Create and Edit Role | [req-rol-003-create-and-edit-role.md](access/req-rol-003-create-and-edit-role.md) |
| REQ-ROL-004 | Role Details | [req-rol-004-role-details.md](access/req-rol-004-role-details.md) |
| REQ-ROL-005 | Role and User Assignments | [req-rol-005-role-and-user-assignments.md](access/req-rol-005-role-and-user-assignments.md) |
| REQ-ROL-006 | Initial Administrator and System Roles | [req-rol-006-initial-administrator-and-system-roles.md](access/req-rol-006-initial-administrator-and-system-roles.md) |

## Passkeys (`passkeys/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-PKY-001 | Passkeys Policy and Deployment | [req-pky-001-passkeys-policy-and-deployment.md](passkeys/req-pky-001-passkeys-policy-and-deployment.md) |
| REQ-PKY-002 | Sign-In with Passkey | [req-pky-002-sign-in-with-passkey.md](passkeys/req-pky-002-sign-in-with-passkey.md) |
| REQ-PKY-003 | Passkey Enrollment and My Account Management | [req-pky-003-passkey-enrollment-and-my-account-management.md](passkeys/req-pky-003-passkey-enrollment-and-my-account-management.md) |
| REQ-PKY-004 | Step-Up Authentication with Passkeys | [req-pky-004-step-up-authentication-with-passkeys.md](passkeys/req-pky-004-step-up-authentication-with-passkeys.md) |
| REQ-PKY-005 | Administrator Passkey Management | [req-pky-005-administrator-passkey-management.md](passkeys/req-pky-005-administrator-passkey-management.md) |
| REQ-PKY-006 | Combined Compliance Gates and Cross-Auth Interaction | [req-pky-006-combined-compliance-gates-and-cross-auth-interaction.md](passkeys/req-pky-006-combined-compliance-gates-and-cross-auth-interaction.md) |
| REQ-PKY-007 | Passkey Notification Emails | [req-pky-007-passkey-notification-emails.md](passkeys/req-pky-007-passkey-notification-emails.md) |

## Issues (`issues/`)

| ID | Title | File |
| -- | ----- | ---- |
| REQ-ISS-001 | Issue List | [req-iss-001-issue-list.md](issues/req-iss-001-issue-list.md) |
| REQ-ISS-002 | Issue Create and Edit Flow | [req-iss-002-issue-create-and-edit-flow.md](issues/req-iss-002-issue-create-and-edit-flow.md) |
| REQ-ISS-003 | Issue Details, Comments, and Change History | [req-iss-003-issue-details-comments-and-change-history.md](issues/req-iss-003-issue-details-comments-and-change-history.md) |
| REQ-ISS-004 | Watching Issues and Push / Email Notifications | [req-iss-004-watching-issues-and-push-email-notifications.md](issues/req-iss-004-watching-issues-and-push-email-notifications.md) |
| REQ-ISS-005 | Notification Bell and Dropdown | [req-iss-005-notification-bell-and-dropdown.md](issues/req-iss-005-notification-bell-and-dropdown.md) |
| REQ-ISS-006 | Issue Attachments | [req-iss-006-issue-attachments.md](issues/req-iss-006-issue-attachments.md) |

---

_Auto-generated from `.req-manifest.json` via `node scripts/req-readme.mjs` (also refreshed by `npm run req:validate`)._
