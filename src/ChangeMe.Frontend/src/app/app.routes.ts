import { Routes } from '@angular/router';
import { AcceptInvitationComponent } from '@features/auth/components/accept-invitation/accept-invitation.component';
import { ChangeEmailComponent } from '@features/auth/components/change-email/change-email.component';
import { ChangePasswordComponent } from '@features/auth/components/change-password/change-password.component';
import { ConfirmEmailChangeComponent } from '@features/auth/components/confirm-email-change/confirm-email-change.component';
import { EditMyAccountComponent } from '@features/auth/components/edit-my-account/edit-my-account.component';
import { ExternalSignInCallbackComponent } from '@features/auth/components/external-sign-in-callback/external-sign-in-callback.component';
import { ForgotPasswordComponent } from '@features/auth/components/forgot-password/forgot-password.component';
import { LoginComponent } from '@features/auth/components/login/login.component';
import { MyAccountComponent } from '@features/auth/components/my-account/my-account.component';
import { OptionalPasskeyEnrollmentComponent } from '@features/auth/components/optional-passkey-enrollment/optional-passkey-enrollment.component';
import { RegisterComponent } from '@features/auth/components/register/register.component';
import { RequiredPasskeySetupComponent } from '@features/auth/components/required-passkey-setup/required-passkey-setup.component';
import { RequiredPasswordChangeComponent } from '@features/auth/components/required-password-change/required-password-change.component';
import { RequiredTwoFactorSetupComponent } from '@features/auth/components/required-two-factor-setup/required-two-factor-setup.component';
import { ResetPasswordComponent } from '@features/auth/components/reset-password/reset-password.component';
import { SetPasswordComponent } from '@features/auth/components/set-password/set-password.component';
import { TwoFactorVerificationComponent } from '@features/auth/components/two-factor-verification/two-factor-verification.component';
import { VerifyEmailComponent } from '@features/auth/components/verify-email/verify-email.component';
import { authGuard } from '@features/auth/guards/auth.guard';
import { externalSignInCallbackGuard } from '@features/auth/guards/external-sign-in-callback.guard';
import { guestGuard } from '@features/auth/guards/guest.guard';
import {
  optionalPasskeyEnrollmentGuard,
  passkeySetupRequiredGuard
} from '@features/auth/guards/passkey.guard';
import { passwordChangeRequiredGuard } from '@features/auth/guards/password-change-required.guard';
import {
  anyPermissionsGuard,
  permissionGuard,
  permissionsGuard
} from '@features/auth/guards/permission.guard';
import { registerGuard } from '@features/auth/guards/register.guard';
import {
  twoFactorChallengeGuard,
  twoFactorSetupRequiredGuard
} from '@features/auth/guards/two-factor.guard';
import { AvailabilityCalendarComponent } from '@features/billing/components/availability-calendar/availability-calendar.component';
import { BillingReportsComponent } from '@features/billing/components/billing-reports/billing-reports.component';
import { CreateEmploymentContractComponent } from '@features/billing/components/create-employment-contract/create-employment-contract.component';
import { CreateLeaveRequestComponent } from '@features/billing/components/create-leave-request/create-leave-request.component';
import { CreatePositionComponent } from '@features/billing/components/create-position/create-position.component';
import { EditEmploymentContractComponent } from '@features/billing/components/edit-employment-contract/edit-employment-contract.component';
import { EditPositionComponent } from '@features/billing/components/edit-position/edit-position.component';
import { EmploymentContractDetailsComponent } from '@features/billing/components/employment-contract-details/employment-contract-details.component';
import { LeaveRequestDetailsComponent } from '@features/billing/components/leave-request-details/leave-request-details.component';
import { LeaveRequestsListComponent } from '@features/billing/components/leave-requests-list/leave-requests-list.component';
import { MyAvailabilityComponent } from '@features/billing/components/my-availability/my-availability.component';
import { MyBillingComponent } from '@features/billing/components/my-billing/my-billing.component';
import { MyLeaveComponent } from '@features/billing/components/my-leave/my-leave.component';
import { PositionDetailsComponent } from '@features/billing/components/position-details/position-details.component';
import { PositionsListComponent } from '@features/billing/components/positions-list/positions-list.component';
import { SettlementsListComponent } from '@features/billing/components/settlements-list/settlements-list.component';
import { UserSettlementDetailsComponent } from '@features/billing/components/user-settlement-details/user-settlement-details.component';
import { CreateIssueComponent } from '@features/issues/components/create-issue/create-issue.component';
import { EditIssueComponent } from '@features/issues/components/edit-issue/edit-issue.component';
import { IssueDetailsComponent } from '@features/issues/components/issue-details/issue-details.component';
import { IssuesComponent } from '@features/issues/components/issues-list/issues-list.component';
import { CreateProjectComponent } from '@features/projects/components/create-project/create-project.component';
import { EditProjectComponent } from '@features/projects/components/edit-project/edit-project.component';
import { ProjectDetailsComponent } from '@features/projects/components/project-details/project-details.component';
import { ProjectsListComponent } from '@features/projects/components/projects-list/projects-list.component';
import { CreateRoleComponent } from '@features/roles/components/create-role/create-role.component';
import { EditRoleComponent } from '@features/roles/components/edit-role/edit-role.component';
import { RoleDetailsComponent } from '@features/roles/components/role-details/role-details.component';
import { RolesListComponent } from '@features/roles/components/roles-list/roles-list.component';
import { MyTimeComponent } from '@features/time/components/my-time/my-time.component';
import { TimeReportsComponent } from '@features/time/components/time-reports/time-reports.component';
import { EditUserComponent } from '@features/users/components/edit-user/edit-user.component';
import { InviteUserComponent } from '@features/users/components/invite-user/invite-user.component';
import { UserDetailsComponent } from '@features/users/components/user-details/user-details.component';
import { UsersListComponent } from '@features/users/components/users-list/users-list.component';
import { PermissionCodes } from '@shared/authorization/permission-codes';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'issues',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'external-sign-in/callback',
    component: ExternalSignInCallbackComponent,
    canActivate: [externalSignInCallbackGuard]
  },
  {
    path: 'confirm-email-change',
    component: ConfirmEmailChangeComponent
  },
  {
    path: 'register',
    component: RegisterComponent,
    canActivate: [registerGuard]
  },
  {
    path: 'verify-email',
    component: VerifyEmailComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'accept-invitation',
    component: AcceptInvitationComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'forgot-password',
    component: ForgotPasswordComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'reset-password',
    component: ResetPasswordComponent,
    canActivate: [guestGuard]
  },
  {
    path: 'two-factor-verification',
    component: TwoFactorVerificationComponent,
    canActivate: [twoFactorChallengeGuard]
  },
  {
    path: 'required-password-change',
    component: RequiredPasswordChangeComponent,
    canActivate: [passwordChangeRequiredGuard]
  },
  {
    path: 'required-two-factor-setup',
    component: RequiredTwoFactorSetupComponent,
    canActivate: [twoFactorSetupRequiredGuard]
  },
  {
    path: 'required-passkey-setup',
    component: RequiredPasskeySetupComponent,
    canActivate: [passkeySetupRequiredGuard]
  },
  {
    path: 'add-passkey-prompt',
    component: OptionalPasskeyEnrollmentComponent,
    canActivate: [optionalPasskeyEnrollmentGuard]
  },
  {
    path: 'issues',
    component: IssuesComponent,
    canActivate: [authGuard]
  },
  {
    path: 'issues/create',
    component: CreateIssueComponent,
    canActivate: [authGuard]
  },
  {
    path: 'issues/:id',
    component: IssueDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'issues/:id/edit',
    component: EditIssueComponent,
    canActivate: [authGuard]
  },
  {
    path: 'projects',
    component: ProjectsListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'projects/create',
    component: CreateProjectComponent,
    canActivate: [authGuard]
  },
  {
    path: 'projects/:id',
    component: ProjectDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'projects/:id/edit',
    component: EditProjectComponent,
    canActivate: [authGuard]
  },
  {
    path: 'my-time',
    component: MyTimeComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.timeViewOwn)]
  },
  {
    path: 'time-reports',
    component: TimeReportsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.timeViewReports)]
  },
  {
    path: 'my-leave',
    component: MyLeaveComponent,
    data: { title: 'My leave' },
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingViewOwn)]
  },
  {
    path: 'my-availability',
    component: MyAvailabilityComponent,
    data: { title: 'My availability' },
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingViewOwn)]
  },
  {
    path: 'my-billing',
    component: MyBillingComponent,
    data: { title: 'My billing' },
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingViewOwn)]
  },
  {
    path: 'leave-requests',
    component: LeaveRequestsListComponent,
    data: { title: 'Leave requests' },
    canActivate: [
      authGuard,
      anyPermissionsGuard([
        PermissionCodes.billingViewAny,
        PermissionCodes.billingManageLeave,
        PermissionCodes.billingApproveLeave
      ])
    ]
  },
  {
    path: 'leave-requests/create',
    component: CreateLeaveRequestComponent,
    data: { title: 'Create leave request' },
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingManageLeave)]
  },
  {
    path: 'leave-requests/:id',
    component: LeaveRequestDetailsComponent,
    data: { title: 'Leave request details' },
    canActivate: [
      authGuard,
      anyPermissionsGuard([
        PermissionCodes.billingViewAny,
        PermissionCodes.billingManageLeave,
        PermissionCodes.billingApproveLeave,
        PermissionCodes.billingViewOwn
      ])
    ]
  },
  {
    path: 'availability-calendar',
    component: AvailabilityCalendarComponent,
    data: { title: 'Availability calendar' },
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingViewAny)]
  },
  {
    path: 'billing/positions',
    component: PositionsListComponent,
    canActivate: [authGuard]
  },
  {
    path: 'billing/positions/create',
    component: CreatePositionComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingManageEmployment)]
  },
  {
    path: 'billing/positions/:id',
    component: PositionDetailsComponent,
    canActivate: [authGuard]
  },
  {
    path: 'billing/positions/:id/edit',
    component: EditPositionComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingManageEmployment)]
  },
  {
    path: 'settlements',
    component: SettlementsListComponent,
    data: { title: 'Settlements' },
    canActivate: [
      authGuard,
      anyPermissionsGuard([
        PermissionCodes.billingViewReports,
        PermissionCodes.billingManageSettlements
      ])
    ]
  },
  {
    path: 'user-settlements/:id',
    component: UserSettlementDetailsComponent,
    data: { title: 'User settlement details' },
    canActivate: [
      authGuard,
      anyPermissionsGuard([
        PermissionCodes.billingViewReports,
        PermissionCodes.billingManageSettlements,
        PermissionCodes.billingViewOwn
      ])
    ]
  },
  {
    path: 'billing-reports',
    component: BillingReportsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.billingViewReports)]
  },
  {
    path: 'account',
    component: MyAccountComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/edit',
    component: EditMyAccountComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/change-password',
    component: ChangePasswordComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/change-email',
    component: ChangeEmailComponent,
    canActivate: [authGuard]
  },
  {
    path: 'account/set-password',
    component: SetPasswordComponent,
    canActivate: [authGuard]
  },
  {
    path: 'users',
    component: UsersListComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersView)]
  },
  {
    path: 'users/invite',
    component: InviteUserComponent,
    canActivate: [
      authGuard,
      permissionsGuard(
        [PermissionCodes.usersManage, PermissionCodes.rolesManage],
        '/users'
      )
    ]
  },
  {
    path: 'users/:id',
    component: UserDetailsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersView)]
  },
  {
    path: 'users/:id/edit',
    component: EditUserComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersManage)]
  },
  {
    path: 'users/:id/employment/contracts/create',
    component: CreateEmploymentContractComponent,
    canActivate: [
      authGuard,
      permissionsGuard(
        [PermissionCodes.usersView, PermissionCodes.billingManageEmployment],
        '/users'
      )
    ]
  },
  {
    path: 'users/:id/employment/contracts/:contractId',
    component: EmploymentContractDetailsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersView)]
  },
  {
    path: 'users/:id/employment/contracts/:contractId/edit',
    component: EditEmploymentContractComponent,
    canActivate: [
      authGuard,
      permissionsGuard(
        [PermissionCodes.usersView, PermissionCodes.billingManageEmployment],
        '/users'
      )
    ]
  },
  {
    path: 'roles',
    component: RolesListComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.rolesView)]
  },
  {
    path: 'roles/create',
    component: CreateRoleComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.rolesManage)]
  },
  {
    path: 'roles/:id',
    component: RoleDetailsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.rolesView)]
  },
  {
    path: 'roles/:id/edit',
    component: EditRoleComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.rolesManage)]
  }
];
