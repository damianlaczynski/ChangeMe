import { signal } from '@angular/core';
import {
  ProjectMembershipHistoryEventType,
  ProjectOperationHistoryEventType,
  ProjectRole
} from '@features/projects/models/project.model';
import { destructiveMenuItemDangerClasses } from '@shared/ui/utils/confirmation-dialog.utils';
import { highlightDialogValue } from '@shared/ui/utils/dialog-message.utils';

export const ProjectPermissionCodes = {
  issuesManage: 'Project.Issues.Manage',
  issuesView: 'Project.Issues.View'
} as const;

export const ProjectConstraints = {
  NAME_MIN_LENGTH: 2,
  NAME_MAX_LENGTH: 100,
  DESCRIPTION_MAX_LENGTH: 500
};

export const ProjectMessages = {
  duplicateName: 'A project with this name already exists.',
  descriptionTooLong: 'Description cannot exceed 500 characters.',
  projectCreated: 'Project created.',
  projectSaved: 'Project saved.',
  projectDeleted: 'Project deleted.',
  systemProjectCannotBeModified: 'System projects cannot be modified.',
  hasIssuesDelete:
    'Project has one or more issues. Move or delete all issues before deleting this project.',
  noProjects: 'You are not a member of any project.',
  noMembers: 'No members in this project.',
  noMembershipHistory: 'No membership history yet.',
  noOperationHistory: 'No operations history yet.',
  noIssuesInProject: 'No issues in this project.',
  memberAdded: 'Member added.',
  memberRemoved: 'Member removed.',
  memberRoleUpdated: 'Member role updated.',
  userAlreadyMember: 'User is already a member of this project.',
  memberAlreadyHasRole: 'Member already has this role.',
  mustHaveOwner: 'Project must have at least one owner.',
  assignOwnerBeforeRemove: 'Assign another owner before removing yourself.',
  assignOwnerBeforeRoleChange: 'Assign another owner before changing your own role.',
  emptyDescription: '—',
  issuesCount: (count: number) => `${count} issues`
};

export type ProjectBadgeSeverity = 'secondary' | 'success' | 'info' | 'warn' | 'danger';

export type ProjectLabeledOption<T> = { value: T; label: string };

type ProjectBadgeMeta<T> = ProjectLabeledOption<T> & { severity: ProjectBadgeSeverity };

const PROJECT_ROLE_META: ProjectBadgeMeta<ProjectRole>[] = [
  { value: ProjectRole.OWNER, label: 'Owner', severity: 'success' },
  { value: ProjectRole.MEMBER, label: 'Member', severity: 'info' },
  { value: ProjectRole.VIEWER, label: 'Viewer', severity: 'secondary' }
];

const projectRoleMetaByValue = new Map(
  PROJECT_ROLE_META.map((item) => [item.value, item])
);

export const projectRoles = signal<ProjectLabeledOption<ProjectRole>[]>(
  PROJECT_ROLE_META.map(({ value, label }) => ({ value, label }))
);

export function getProjectRoleLabel(role: ProjectRole): string {
  return projectRoleMetaByValue.get(role)?.label ?? role;
}

export function getProjectRoleSeverity(role: ProjectRole): ProjectBadgeSeverity {
  return projectRoleMetaByValue.get(role)?.severity ?? 'secondary';
}

export function formatDescription(description: string | null | undefined): string {
  return description?.trim() ? description : ProjectMessages.emptyDescription;
}

export function getDeleteProjectConfirmMessage(projectName: string): string {
  return `Delete project ${highlightDialogValue(projectName)}? This cannot be undone.`;
}

export function getRemoveMemberConfirmMessage(
  memberReference: string,
  projectName: string
): string {
  return `Remove ${highlightDialogValue(memberReference)} from project ${highlightDialogValue(projectName)}?`;
}

export const projectDeleteMenuItemDangerClasses = destructiveMenuItemDangerClasses;

export type ProjectHistoryEventVisual = {
  icon: string;
  markerClass: string;
  tagSeverity: ProjectBadgeSeverity;
};

const MEMBERSHIP_HISTORY_EVENT_VISUALS: Record<
  ProjectMembershipHistoryEventType,
  ProjectHistoryEventVisual
> = {
  MEMBER_ADDED: {
    icon: 'pi pi-user-plus',
    markerClass: 'bg-green-600 text-white dark:bg-green-500',
    tagSeverity: 'success'
  },
  MEMBER_REMOVED: {
    icon: 'pi pi-user-minus',
    markerClass: 'bg-red-600 text-white dark:bg-red-500',
    tagSeverity: 'danger'
  },
  MEMBER_ROLE_CHANGED: {
    icon: 'pi pi-sync',
    markerClass: 'bg-sky-600 text-white dark:bg-sky-500',
    tagSeverity: 'info'
  }
};

const OPERATION_HISTORY_EVENT_VISUALS: Record<
  ProjectOperationHistoryEventType,
  ProjectHistoryEventVisual
> = {
  PROJECT_CREATED: {
    icon: 'pi pi-plus',
    markerClass: 'bg-green-600 text-white dark:bg-green-500',
    tagSeverity: 'success'
  },
  NAME_CHANGED: {
    icon: 'pi pi-pencil',
    markerClass: 'bg-sky-600 text-white dark:bg-sky-500',
    tagSeverity: 'info'
  },
  DESCRIPTION_CHANGED: {
    icon: 'pi pi-align-left',
    markerClass: 'bg-zinc-500 text-white dark:bg-zinc-400',
    tagSeverity: 'secondary'
  }
};

const defaultHistoryEventVisual: ProjectHistoryEventVisual = {
  icon: 'pi pi-history',
  markerClass: 'bg-primary text-primary-contrast',
  tagSeverity: 'secondary'
};

export function getMembershipHistoryEventVisual(
  eventType: ProjectMembershipHistoryEventType
): ProjectHistoryEventVisual {
  return MEMBERSHIP_HISTORY_EVENT_VISUALS[eventType] ?? defaultHistoryEventVisual;
}

export function getOperationHistoryEventVisual(
  eventType: ProjectOperationHistoryEventType
): ProjectHistoryEventVisual {
  return OPERATION_HISTORY_EVENT_VISUALS[eventType] ?? defaultHistoryEventVisual;
}

export function shouldShowMembershipBeforeAfter(
  eventType: ProjectMembershipHistoryEventType
): boolean {
  return eventType === ProjectMembershipHistoryEventType.MEMBER_ROLE_CHANGED;
}

export function shouldShowOperationBeforeAfter(
  eventType: ProjectOperationHistoryEventType
): boolean {
  return eventType === ProjectOperationHistoryEventType.NAME_CHANGED;
}
