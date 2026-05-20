import { Routes } from '@angular/router';
import { IssuesComponent } from '@features/issues/components/issues-list/issues-list.component';
import { CreateIssueComponent } from '@features/issues/components/create-issue/create-issue.component';
import { IssueDetailsComponent } from '@features/issues/components/issue-details/issue-details.component';
import { EditIssueComponent } from '@features/issues/components/edit-issue/edit-issue.component';
import { LoginComponent } from '@features/auth/components/login/login.component';
import { RegisterComponent } from '@features/auth/components/register/register.component';
import { authGuard } from '@features/auth/guards/auth.guard';
import { guestGuard } from '@features/auth/guards/guest.guard';

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
    path: 'register',
    component: RegisterComponent,
    canActivate: [guestGuard]
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
  }
];
