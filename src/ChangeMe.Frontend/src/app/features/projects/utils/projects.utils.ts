import {
  ProjectMemberRole,
  ProjectStatus,
  ProjectVisibility
} from '@features/projects/models/project.model';

export const ProjectConstraints = {
  NAME_MIN_LENGTH: 2,
  NAME_MAX_LENGTH: 100,
  KEY_MIN_LENGTH: 2,
  KEY_MAX_LENGTH: 10,
  DESCRIPTION_MAX_LENGTH: 1000,
  DEFAULT_COLOR: '#3B82F6'
} as const;

export const projectStatuses = [
  { label: 'Active', value: ProjectStatus.ACTIVE },
  { label: 'On hold', value: ProjectStatus.ON_HOLD },
  { label: 'Archived', value: ProjectStatus.ARCHIVED }
];

export const projectVisibilities = [
  {
    label: 'Internal',
    value: ProjectVisibility.INTERNAL,
    description: 'Visible to every user with project access.'
  },
  {
    label: 'Private',
    value: ProjectVisibility.PRIVATE,
    description: 'Visible only to project members.'
  }
];

export const projectMemberRoles = [
  { label: 'Owner', value: ProjectMemberRole.OWNER },
  { label: 'Member', value: ProjectMemberRole.MEMBER },
  { label: 'Viewer', value: ProjectMemberRole.VIEWER }
];

export const ProjectMessages = {
  projectCreated: 'Project created',
  projectUpdated: 'Project updated',
  projectDeleted: 'Project deleted',
  memberAdded: 'Member added',
  memberRemoved: 'Member removed',
  memberRoleUpdated: 'Member role updated'
} as const;

export function getProjectMemberRoleLabel(role: ProjectMemberRole): string {
  return projectMemberRoles.find((item) => item.value === role)?.label ?? role;
}

export function getRemoveProjectMemberConfirmMessage(
  displayLabel: string,
  projectName: string
): string {
  return `Remove <strong>${displayLabel}</strong> from project <strong>${projectName}</strong>?`;
}

export function formatProjectDescription(description?: string | null): string {
  return description?.trim() || 'No description';
}

export function getDeleteProjectConfirmMessage(name: string): string {
  return `Delete project <strong>${name}</strong>? This action cannot be undone. Projects with issues cannot be deleted.`;
}

export function getProjectStatusSeverity(
  status: ProjectStatus
): 'success' | 'warn' | 'secondary' {
  switch (status) {
    case ProjectStatus.ACTIVE:
      return 'success';
    case ProjectStatus.ON_HOLD:
      return 'warn';
    case ProjectStatus.ARCHIVED:
    default:
      return 'secondary';
  }
}

export function getProjectStatusLabel(status: ProjectStatus): string {
  return projectStatuses.find((item) => item.value === status)?.label ?? status;
}

export function getProjectVisibilityLabel(visibility: ProjectVisibility): string {
  return (
    projectVisibilities.find((item) => item.value === visibility)?.label ?? visibility
  );
}

export function normalizeProjectKey(value: string): string {
  return value
    .replace(/[^a-zA-Z0-9]/g, '')
    .toUpperCase()
    .slice(0, ProjectConstraints.KEY_MAX_LENGTH);
}

export function canManageProjectResource(
  currentUserRole?: ProjectMemberRole | null
): boolean {
  return currentUserRole === ProjectMemberRole.OWNER;
}
