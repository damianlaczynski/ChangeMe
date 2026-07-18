import { AuthService } from '@features/auth/services/auth.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';

export interface IssuePermissionContext {
  createdBy: string;
  assignedToUserId?: string | null;
}

export function canAccessIssuesArea(authService: AuthService): boolean {
  return (
    authService.hasPermission(PermissionCodes.issuesView) ||
    authService.hasPermission(PermissionCodes.issuesCreate)
  );
}

export function canViewIssues(authService: AuthService): boolean {
  return authService.hasPermission(PermissionCodes.issuesView);
}

export function canCreateIssues(authService: AuthService): boolean {
  return authService.hasPermission(PermissionCodes.issuesCreate);
}

export function canDeleteIssues(authService: AuthService): boolean {
  return authService.hasPermission(PermissionCodes.issuesDelete);
}

export function canManageIssueAttachments(authService: AuthService): boolean {
  return authService.hasPermission(PermissionCodes.issuesManageAttachments);
}

export function isIssueAuthor(
  authService: AuthService,
  issue: IssuePermissionContext
): boolean {
  const userId = authService.currentUser()?.id;
  return Boolean(userId && issue.createdBy === userId);
}

export function isIssueAssignee(
  authService: AuthService,
  issue: IssuePermissionContext
): boolean {
  const userId = authService.currentUser()?.id;
  return Boolean(userId && issue.assignedToUserId && issue.assignedToUserId === userId);
}

export function canCommentOnIssue(
  authService: AuthService,
  issue: IssuePermissionContext
): boolean {
  return (
    authService.hasPermission(PermissionCodes.issuesComment) ||
    (canViewIssues(authService) && isIssueAuthor(authService, issue))
  );
}

export function canEditAnyIssueField(
  authService: AuthService,
  issue: IssuePermissionContext
): boolean {
  return (
    authService.hasPermission(PermissionCodes.issuesEdit) ||
    (canViewIssues(authService) &&
      (isIssueAuthor(authService, issue) || isIssueAssignee(authService, issue)))
  );
}

export function canWatchIssues(authService: AuthService): boolean {
  return canViewIssues(authService);
}
