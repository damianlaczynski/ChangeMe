# Requirements index

> Functional specifications (`FR-*`), non-functional requirements (`NFR-*`), and reference docs.
> Process: `docs/requirements-change-process.md`. Validate: `npm run requirements:validate`.

## Reference documents (`_shared/reference/`)

| Document | File |
| -------- | ---- |
| account-model.md | [account-model.md](_shared/reference/account-model.md) |
| compliance-gates.md | [compliance-gates.md](_shared/reference/compliance-gates.md) |
| glossary.md | [glossary.md](_shared/reference/glossary.md) |
| permissions.md | [permissions.md](_shared/reference/permissions.md) |

## Shared functional patterns (`_shared/functional/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-UI-001 | Shared UI and UX Patterns | [ui-patterns.md](_shared/functional/ui-patterns.md) |

## Non-functional requirements (`_shared/non-functional/`)

| ID | Title | File |
| -- | ----- | ---- |
| NFR-A11Y-001 | Accessibility | [accessibility.md](_shared/non-functional/accessibility.md) |
| NFR-I18N-001 | Internationalization and Copy | [internationalization.md](_shared/non-functional/internationalization.md) |
| NFR-PERF-001 | Performance and Scale | [performance-and-scale.md](_shared/non-functional/performance-and-scale.md) |
| NFR-QUAL-001 | Product Quality (Index) | [product-quality.md](_shared/non-functional/product-quality.md) |
| NFR-RSP-001 | Responsiveness and Layout | [responsiveness.md](_shared/non-functional/responsiveness.md) |

## Pending changes

See [changes/](changes/) for open requirement deltas.


## Access (`functional/access/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-ROL-001 | Permission Catalog and Effective Permissions | 9 | [fr-rol-001-permission-catalog-and-effective-permissions.md](functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md) |
| FR-ROL-002 | Roles List | 11 | [fr-rol-002-roles-list.md](functional/access/fr-rol-002-roles-list.md) |
| FR-ROL-003 | Create and Edit Role | 16 | [fr-rol-003-create-and-edit-role.md](functional/access/fr-rol-003-create-and-edit-role.md) |
| FR-ROL-004 | Role Details | 13 | [fr-rol-004-role-details.md](functional/access/fr-rol-004-role-details.md) |
| FR-ROL-005 | Role and User Assignments | 12 | [fr-rol-005-role-and-user-assignments.md](functional/access/fr-rol-005-role-and-user-assignments.md) |
| FR-ROL-006 | Initial Administrator and System Roles | 8 | [fr-rol-006-initial-administrator-and-system-roles.md](functional/access/fr-rol-006-initial-administrator-and-system-roles.md) |

## Identity (`functional/identity/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-AUTH-001 | Login and Registration with Sessions | 14 | [fr-auth-001-login-and-registration-with-sessions.md](functional/identity/fr-auth-001-login-and-registration-with-sessions.md) |
| FR-AUTH-002 | Staying Signed In | 5 | [fr-auth-002-staying-signed-in.md](functional/identity/fr-auth-002-staying-signed-in.md) |
| FR-AUTH-003 | Logout | 5 | [fr-auth-003-logout.md](functional/identity/fr-auth-003-logout.md) |
| FR-AUTH-004 | My Sessions | 7 | [fr-auth-004-my-sessions.md](functional/identity/fr-auth-004-my-sessions.md) |
| FR-AUTH-005 | Change Password | 6 | [fr-auth-005-change-password.md](functional/identity/fr-auth-005-change-password.md) |
| FR-AUTH-006 | Forgot and Reset Password (Self-Service) | 4 | [fr-auth-006-forgot-and-reset-password-self-service.md](functional/identity/fr-auth-006-forgot-and-reset-password-self-service.md) |
| FR-AUTH-007 | Auth Email Notifications | 7 | [fr-auth-007-auth-email-notifications.md](functional/identity/fr-auth-007-auth-email-notifications.md) |
| FR-AUTH-008 | Password Policy | 5 | [fr-auth-008-password-policy.md](functional/identity/fr-auth-008-password-policy.md) |
| FR-AUTH-009 | Password Expiration | 9 | [fr-auth-009-password-expiration.md](functional/identity/fr-auth-009-password-expiration.md) |
| FR-AUTH-010 | Accept Account Invitation | 7 | [fr-auth-010-accept-account-invitation.md](functional/identity/fr-auth-010-accept-account-invitation.md) |
| FR-AUTH-011 | Email Verification | 6 | [fr-auth-011-email-verification.md](functional/identity/fr-auth-011-email-verification.md) |
| FR-AUTH-012 | Public Registration Policy | 6 | [fr-auth-012-public-registration-policy.md](functional/identity/fr-auth-012-public-registration-policy.md) |
| FR-AUTH-013 | Two-Factor Authentication | 12 | [fr-auth-013-two-factor-authentication.md](functional/identity/fr-auth-013-two-factor-authentication.md) |
| FR-AUTH-014 | External Identity Providers | 10 | [fr-auth-014-external-identity-providers.md](functional/identity/fr-auth-014-external-identity-providers.md) |
| FR-AUTH-015 | Self-Service Email Change | 10 | [fr-auth-015-self-service-email-change.md](functional/identity/fr-auth-015-self-service-email-change.md) |

## Invitations (`functional/invitations/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-INV-001 | Invite User (Administrator) | 6 | [fr-inv-001-invite-user-administrator.md](functional/invitations/fr-inv-001-invite-user-administrator.md) |
| FR-INV-002 | Pending Invitation Banner (User Details) | 6 | [fr-inv-002-pending-invitation-banner-user-details.md](functional/invitations/fr-inv-002-pending-invitation-banner-user-details.md) |
| FR-INV-003 | Resend Invitation | 4 | [fr-inv-003-resend-invitation.md](functional/invitations/fr-inv-003-resend-invitation.md) |
| FR-INV-004 | Cancel Invitation | 4 | [fr-inv-004-cancel-invitation.md](functional/invitations/fr-inv-004-cancel-invitation.md) |
| FR-INV-005 | User Status | 9 | [fr-inv-005-user-status.md](functional/invitations/fr-inv-005-user-status.md) |
| FR-INV-006 | Invitation History Retention | 5 | [fr-inv-006-invitation-history-retention.md](functional/invitations/fr-inv-006-invitation-history-retention.md) |
| FR-INV-007 | Accept Invitation — Guest Screen Presentation | 2 | [fr-inv-007-accept-invitation-guest-screen-presentation.md](functional/invitations/fr-inv-007-accept-invitation-guest-screen-presentation.md) |

## Issues (`functional/issues/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-ISS-001 | Issue List | 9 | [fr-iss-001-issue-list.md](functional/issues/fr-iss-001-issue-list.md) |
| FR-ISS-002 | Issue Create and Edit Flow | 9 | [fr-iss-002-issue-create-and-edit-flow.md](functional/issues/fr-iss-002-issue-create-and-edit-flow.md) |
| FR-ISS-003 | Issue Details, Comments, and Change History | 10 | [fr-iss-003-issue-details-comments-and-change-history.md](functional/issues/fr-iss-003-issue-details-comments-and-change-history.md) |
| FR-ISS-004 | Watching Issues and Push / Email Notifications | 7 | [fr-iss-004-watching-issues-and-push-email-notifications.md](functional/issues/fr-iss-004-watching-issues-and-push-email-notifications.md) |
| FR-ISS-005 | Notification Bell and Dropdown | 8 | [fr-iss-005-notification-bell-and-dropdown.md](functional/issues/fr-iss-005-notification-bell-and-dropdown.md) |
| FR-ISS-006 | Issue Attachments | 8 | [fr-iss-006-issue-attachments.md](functional/issues/fr-iss-006-issue-attachments.md) |

## Passkeys (`functional/passkeys/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-PKY-001 | Passkeys Policy and Deployment | 8 | [fr-pky-001-passkeys-policy-and-deployment.md](functional/passkeys/fr-pky-001-passkeys-policy-and-deployment.md) |
| FR-PKY-002 | Sign-In with Passkey | 10 | [fr-pky-002-sign-in-with-passkey.md](functional/passkeys/fr-pky-002-sign-in-with-passkey.md) |
| FR-PKY-003 | Passkey Enrollment and My Account Management | 9 | [fr-pky-003-passkey-enrollment-and-my-account-management.md](functional/passkeys/fr-pky-003-passkey-enrollment-and-my-account-management.md) |
| FR-PKY-004 | Step-Up Authentication with Passkeys | 6 | [fr-pky-004-step-up-authentication-with-passkeys.md](functional/passkeys/fr-pky-004-step-up-authentication-with-passkeys.md) |
| FR-PKY-005 | Administrator Passkey Management | 5 | [fr-pky-005-administrator-passkey-management.md](functional/passkeys/fr-pky-005-administrator-passkey-management.md) |
| FR-PKY-006 | Combined Compliance Gates and Cross-Auth Interaction | 7 | [fr-pky-006-combined-compliance-gates-and-cross-auth-interaction.md](functional/passkeys/fr-pky-006-combined-compliance-gates-and-cross-auth-interaction.md) |
| FR-PKY-007 | Passkey Notification Emails | 5 | [fr-pky-007-passkey-notification-emails.md](functional/passkeys/fr-pky-007-passkey-notification-emails.md) |

## Users (`functional/users/`)

| ID | Title | Scenarios | File |
| -- | ----- | --------- | ---- |
| FR-USR-001 | My Account Profile | 7 | [fr-usr-001-my-account-profile.md](functional/users/fr-usr-001-my-account-profile.md) |
| FR-USR-002 | User List | 6 | [fr-usr-002-user-list.md](functional/users/fr-usr-002-user-list.md) |
| FR-USR-003 | Edit User (Admin) | 7 | [fr-usr-003-edit-user-admin.md](functional/users/fr-usr-003-edit-user-admin.md) |
| FR-USR-004 | User Details and Session Administration | 6 | [fr-usr-004-user-details-and-session-administration.md](functional/users/fr-usr-004-user-details-and-session-administration.md) |
| FR-USR-005 | Deactivate and Activate Accounts | 6 | [fr-usr-005-deactivate-and-activate-accounts.md](functional/users/fr-usr-005-deactivate-and-activate-accounts.md) |
| FR-USR-006 | Admin Send Password Reset | 5 | [fr-usr-006-admin-send-password-reset.md](functional/users/fr-usr-006-admin-send-password-reset.md) |
| FR-USR-007 | Admin Confirm Email | 5 | [fr-usr-007-admin-confirm-email.md](functional/users/fr-usr-007-admin-confirm-email.md) |

---

_Auto-generated from `.requirements-manifest.json` via `node scripts/requirements-readme.mjs` (also refreshed by `npm run requirements:validate`)._
