import { ProjectMemberRole } from '@features/projects/models/project.model';
import { describe, expect, it } from 'vitest';
import { canManageProjectResource } from './projects.utils';

describe('projects.utils', () => {
  it('allows stewardship only for project owners', () => {
    expect(canManageProjectResource(ProjectMemberRole.OWNER)).toBe(true);
    expect(canManageProjectResource(ProjectMemberRole.MEMBER)).toBe(false);
    expect(canManageProjectResource(ProjectMemberRole.VIEWER)).toBe(false);
    expect(canManageProjectResource(null)).toBe(false);
    expect(canManageProjectResource(undefined)).toBe(false);
  });
});
