import { Routes } from '@angular/router';
import { AcceptInvitationComponent } from '@features/auth/components/accept-invitation/accept-invitation.component';
import { ChangePasswordComponent } from '@features/auth/components/change-password/change-password.component';
import { EditMyAccountComponent } from '@features/auth/components/edit-my-account/edit-my-account.component';
import { ExternalSignInCallbackComponent } from '@features/auth/components/external-sign-in-callback/external-sign-in-callback.component';
import { ForgotPasswordComponent } from '@features/auth/components/forgot-password/forgot-password.component';
import { LinkExternalAccountComponent } from '@features/auth/components/link-external-account/link-external-account.component';
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
  permissionGuard,
  permissionsGuard
} from '@features/auth/guards/permission.guard';
import { registerGuard } from '@features/auth/guards/register.guard';
import {
  twoFactorChallengeGuard,
  twoFactorSetupRequiredGuard
} from '@features/auth/guards/two-factor.guard';
import { CreateIssueComponent } from '@features/issues/components/create-issue/create-issue.component';
import { EditIssueComponent } from '@features/issues/components/edit-issue/edit-issue.component';
import { IssueDetailsComponent } from '@features/issues/components/issue-details/issue-details.component';
import { IssuesComponent } from '@features/issues/components/issues-list/issues-list.component';
import { CreateRoleComponent } from '@features/roles/components/create-role/create-role.component';
import { EditRoleComponent } from '@features/roles/components/edit-role/edit-role.component';
import { RoleDetailsComponent } from '@features/roles/components/role-details/role-details.component';
import { RolesListComponent } from '@features/roles/components/roles-list/roles-list.component';
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
    path: 'link-external-account',
    component: LinkExternalAccountComponent,
    canActivate: [guestGuard]
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
