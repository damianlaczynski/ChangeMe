import { Routes } from '@angular/router';
import { EditMyAccountComponent } from '@features/auth/components/edit-my-account/edit-my-account.component';
import { LoginComponent } from '@features/auth/components/login/login.component';
import { MyAccountComponent } from '@features/auth/components/my-account/my-account.component';
import { authGuard } from '@features/auth/guards/auth.guard';
import { guestGuard } from '@features/auth/guards/guest.guard';
import {
  anyPermissionGuard,
  permissionGuard,
  permissionsGuard
} from '@features/auth/guards/permission.guard';
import { AcceptInvitationComponent } from '@features/invitations/components/accept-invitation/accept-invitation.component';
import { CreateInvitationComponent } from '@features/invitations/components/create-invitation/create-invitation.component';
import { InvitationsListComponent } from '@features/invitations/components/invitations-list/invitations-list.component';
import { CreateIssueComponent } from '@features/issues/components/create-issue/create-issue.component';
import { EditIssueComponent } from '@features/issues/components/edit-issue/edit-issue.component';
import { IssueDetailsComponent } from '@features/issues/components/issue-details/issue-details.component';
import { IssuesComponent } from '@features/issues/components/issues-list/issues-list.component';
import { CreateRoleComponent } from '@features/roles/components/create-role/create-role.component';
import { EditRoleComponent } from '@features/roles/components/edit-role/edit-role.component';
import { RoleDetailsComponent } from '@features/roles/components/role-details/role-details.component';
import { RolesListComponent } from '@features/roles/components/roles-list/roles-list.component';
import { CreateUserComponent } from '@features/users/components/create-user/create-user.component';
import { EditUserComponent } from '@features/users/components/edit-user/edit-user.component';
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
    path: 'invitations/accept/:token',
    component: AcceptInvitationComponent
  },
  {
    path: 'issues',
    component: IssuesComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.issuesView, '/account')]
  },
  {
    path: 'issues/create',
    component: CreateIssueComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.issuesCreate, '/issues')]
  },
  {
    path: 'issues/:id',
    component: IssueDetailsComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.issuesView, '/account')]
  },
  {
    path: 'issues/:id/edit',
    component: EditIssueComponent,
    canActivate: [
      authGuard,
      anyPermissionGuard(
        [PermissionCodes.issuesEdit, PermissionCodes.issuesView],
        '/issues'
      )
    ]
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
    path: 'invitations',
    component: InvitationsListComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersInvite)]
  },
  {
    path: 'invitations/create',
    component: CreateInvitationComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersInvite)]
  },
  {
    path: 'users',
    component: UsersListComponent,
    canActivate: [authGuard, permissionGuard(PermissionCodes.usersView)]
  },
  {
    path: 'users/create',
    component: CreateUserComponent,
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
