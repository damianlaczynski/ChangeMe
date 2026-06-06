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

| ID | Title | File |
| -- | ----- | ---- |
| FR-ROL-001 | Permission Catalog and Effective Permissions | [fr-rol-001-permission-catalog-and-effective-permissions.md](functional/access/fr-rol-001-permission-catalog-and-effective-permissions.md) |
| FR-ROL-002 | Roles List | [fr-rol-002-roles-list.md](functional/access/fr-rol-002-roles-list.md) |
| FR-ROL-003 | Create and Edit Role | [fr-rol-003-create-and-edit-role.md](functional/access/fr-rol-003-create-and-edit-role.md) |
| FR-ROL-004 | Role Details | [fr-rol-004-role-details.md](functional/access/fr-rol-004-role-details.md) |
| FR-ROL-005 | Role and User Assignments | [fr-rol-005-role-and-user-assignments.md](functional/access/fr-rol-005-role-and-user-assignments.md) |
| FR-ROL-006 | Initial Administrator and System Roles | [fr-rol-006-initial-administrator-and-system-roles.md](functional/access/fr-rol-006-initial-administrator-and-system-roles.md) |

## Identity (`functional/identity/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-AUTH-001 | Login and Registration with Sessions | [fr-auth-001-login-and-registration-with-sessions.md](functional/identity/fr-auth-001-login-and-registration-with-sessions.md) |
| FR-AUTH-002 | Staying Signed In | [fr-auth-002-staying-signed-in.md](functional/identity/fr-auth-002-staying-signed-in.md) |
| FR-AUTH-003 | Logout | [fr-auth-003-logout.md](functional/identity/fr-auth-003-logout.md) |
| FR-AUTH-004 | My Sessions | [fr-auth-004-my-sessions.md](functional/identity/fr-auth-004-my-sessions.md) |
| FR-AUTH-005 | Change Password | [fr-auth-005-change-password.md](functional/identity/fr-auth-005-change-password.md) |
| FR-AUTH-006 | Forgot and Reset Password (Self-Service) | [fr-auth-006-forgot-and-reset-password-self-service.md](functional/identity/fr-auth-006-forgot-and-reset-password-self-service.md) |
| FR-AUTH-007 | Auth Email Notifications | [fr-auth-007-auth-email-notifications.md](functional/identity/fr-auth-007-auth-email-notifications.md) |
| FR-AUTH-008 | Password Policy | [fr-auth-008-password-policy.md](functional/identity/fr-auth-008-password-policy.md) |
| FR-AUTH-009 | Password Expiration | [fr-auth-009-password-expiration.md](functional/identity/fr-auth-009-password-expiration.md) |
| FR-AUTH-010 | Accept Account Invitation | [fr-auth-010-accept-account-invitation.md](functional/identity/fr-auth-010-accept-account-invitation.md) |
| FR-AUTH-011 | Email Verification | [fr-auth-011-email-verification.md](functional/identity/fr-auth-011-email-verification.md) |
| FR-AUTH-012 | Public Registration Policy | [fr-auth-012-public-registration-policy.md](functional/identity/fr-auth-012-public-registration-policy.md) |
| FR-AUTH-013 | Two-Factor Authentication | [fr-auth-013-two-factor-authentication.md](functional/identity/fr-auth-013-two-factor-authentication.md) |
| FR-AUTH-014 | External Identity Providers | [fr-auth-014-external-identity-providers.md](functional/identity/fr-auth-014-external-identity-providers.md) |
| FR-AUTH-015 | Self-Service Email Change | [fr-auth-015-self-service-email-change.md](functional/identity/fr-auth-015-self-service-email-change.md) |

## Invitations (`functional/invitations/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-INV-001 | Invite User (Administrator) | [fr-inv-001-invite-user-administrator.md](functional/invitations/fr-inv-001-invite-user-administrator.md) |
| FR-INV-002 | Pending Invitation Banner (User Details) | [fr-inv-002-pending-invitation-banner-user-details.md](functional/invitations/fr-inv-002-pending-invitation-banner-user-details.md) |
| FR-INV-003 | Resend Invitation | [fr-inv-003-resend-invitation.md](functional/invitations/fr-inv-003-resend-invitation.md) |
| FR-INV-004 | Cancel Invitation | [fr-inv-004-cancel-invitation.md](functional/invitations/fr-inv-004-cancel-invitation.md) |
| FR-INV-005 | User Status | [fr-inv-005-user-status.md](functional/invitations/fr-inv-005-user-status.md) |
| FR-INV-006 | Invitation History Retention | [fr-inv-006-invitation-history-retention.md](functional/invitations/fr-inv-006-invitation-history-retention.md) |
| FR-INV-007 | Accept Invitation — Guest Screen Presentation | [fr-inv-007-accept-invitation-guest-screen-presentation.md](functional/invitations/fr-inv-007-accept-invitation-guest-screen-presentation.md) |

## Issues (`functional/issues/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-ISS-001 | Issue List | [fr-iss-001-issue-list.md](functional/issues/fr-iss-001-issue-list.md) |
| FR-ISS-002 | Issue Create and Edit Flow | [fr-iss-002-issue-create-and-edit-flow.md](functional/issues/fr-iss-002-issue-create-and-edit-flow.md) |
| FR-ISS-003 | Issue Details, Comments, and Change History | [fr-iss-003-issue-details-comments-and-change-history.md](functional/issues/fr-iss-003-issue-details-comments-and-change-history.md) |
| FR-ISS-004 | Watching Issues and Push / Email Notifications | [fr-iss-004-watching-issues-and-push-email-notifications.md](functional/issues/fr-iss-004-watching-issues-and-push-email-notifications.md) |
| FR-ISS-005 | Notification Bell and Dropdown | [fr-iss-005-notification-bell-and-dropdown.md](functional/issues/fr-iss-005-notification-bell-and-dropdown.md) |
| FR-ISS-006 | Issue Attachments | [fr-iss-006-issue-attachments.md](functional/issues/fr-iss-006-issue-attachments.md) |

## Passkeys (`functional/passkeys/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-PKY-001 | Passkeys Policy and Deployment | [fr-pky-001-passkeys-policy-and-deployment.md](functional/passkeys/fr-pky-001-passkeys-policy-and-deployment.md) |
| FR-PKY-002 | Sign-In with Passkey | [fr-pky-002-sign-in-with-passkey.md](functional/passkeys/fr-pky-002-sign-in-with-passkey.md) |
| FR-PKY-003 | Passkey Enrollment and My Account Management | [fr-pky-003-passkey-enrollment-and-my-account-management.md](functional/passkeys/fr-pky-003-passkey-enrollment-and-my-account-management.md) |
| FR-PKY-004 | Step-Up Authentication with Passkeys | [fr-pky-004-step-up-authentication-with-passkeys.md](functional/passkeys/fr-pky-004-step-up-authentication-with-passkeys.md) |
| FR-PKY-005 | Administrator Passkey Management | [fr-pky-005-administrator-passkey-management.md](functional/passkeys/fr-pky-005-administrator-passkey-management.md) |
| FR-PKY-006 | Combined Compliance Gates and Cross-Auth Interaction | [fr-pky-006-combined-compliance-gates-and-cross-auth-interaction.md](functional/passkeys/fr-pky-006-combined-compliance-gates-and-cross-auth-interaction.md) |
| FR-PKY-007 | Passkey Notification Emails | [fr-pky-007-passkey-notification-emails.md](functional/passkeys/fr-pky-007-passkey-notification-emails.md) |

## Users (`functional/users/`)

| ID | Title | File |
| -- | ----- | ---- |
| FR-USR-001 | My Account Profile | [fr-usr-001-my-account-profile.md](functional/users/fr-usr-001-my-account-profile.md) |
| FR-USR-002 | User List | [fr-usr-002-user-list.md](functional/users/fr-usr-002-user-list.md) |
| FR-USR-003 | Edit User (Admin) | [fr-usr-003-edit-user-admin.md](functional/users/fr-usr-003-edit-user-admin.md) |
| FR-USR-004 | User Details and Session Administration | [fr-usr-004-user-details-and-session-administration.md](functional/users/fr-usr-004-user-details-and-session-administration.md) |
| FR-USR-005 | Deactivate and Activate Accounts | [fr-usr-005-deactivate-and-activate-accounts.md](functional/users/fr-usr-005-deactivate-and-activate-accounts.md) |
| FR-USR-006 | Admin Send Password Reset | [fr-usr-006-admin-send-password-reset.md](functional/users/fr-usr-006-admin-send-password-reset.md) |
| FR-USR-007 | Admin Confirm Email | [fr-usr-007-admin-confirm-email.md](functional/users/fr-usr-007-admin-confirm-email.md) |

---

_Auto-generated from `.requirements-manifest.json` via `node scripts/requirements-readme.mjs` (also refreshed by `npm run requirements:validate`)._
