import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { IssueAssignableUserDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  ProjectDetailsDto,
  ProjectMemberRole,
  ProjectStatus,
  ProjectVisibility
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import { ProjectMessages } from '@features/projects/utils/projects.utils';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ProjectSettingsComponent } from './project-settings.component';

describe('ProjectSettingsComponent', () => {
  let fixture: ComponentFixture<ProjectSettingsComponent>;
  let component: ProjectSettingsComponent;
  let projectsService: {
    getProjectById: ReturnType<typeof vi.fn>;
    updateProject: ReturnType<typeof vi.fn>;
    addProjectMember: ReturnType<typeof vi.fn>;
    updateProjectMemberRole: ReturnType<typeof vi.fn>;
    removeProjectMember: ReturnType<typeof vi.fn>;
  };
  let issuesService: {
    getAssignableUsers: ReturnType<typeof vi.fn>;
  };
  let toastService: {
    success: ReturnType<typeof vi.fn>;
    error: ReturnType<typeof vi.fn>;
  };

  const projectDetails: ProjectDetailsDto = {
    id: 'project-1',
    name: 'Platform',
    key: 'PLAT',
    description: 'Workspace',
    status: ProjectStatus.ACTIVE,
    visibility: ProjectVisibility.INTERNAL,
    color: '#3B82F6',
    issueCount: 0,
    memberCount: 1,
    createdAt: '2026-06-13T10:00:00Z',
    currentUserRole: ProjectMemberRole.OWNER,
    members: [
      {
        userId: 'owner-1',
        displayLabel: 'Owner User (owner@example.com)',
        role: ProjectMemberRole.OWNER,
        joinedAt: '2026-06-13T10:00:00Z'
      }
    ]
  };

  const assignableUsers: IssueAssignableUserDto[] = [
    { id: 'user-2', displayLabel: 'Member User (member@example.com)' }
  ];

  beforeEach(async () => {
    projectsService = {
      getProjectById: vi.fn(() => of(projectDetails)),
      updateProject: vi.fn(() => of(projectDetails)),
      addProjectMember: vi.fn(() =>
        of({
          ...projectDetails,
          memberCount: 2,
          members: [
            ...projectDetails.members,
            {
              userId: 'user-2',
              displayLabel: assignableUsers[0].displayLabel,
              role: ProjectMemberRole.MEMBER,
              joinedAt: '2026-06-13T11:00:00Z'
            }
          ]
        })
      ),
      updateProjectMemberRole: vi.fn(() => of(projectDetails)),
      removeProjectMember: vi.fn(() =>
        of({
          ...projectDetails,
          memberCount: 0,
          members: []
        })
      )
    };

    issuesService = {
      getAssignableUsers: vi.fn(() => of(assignableUsers))
    };

    toastService = {
      success: vi.fn(),
      error: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [ProjectSettingsComponent],
      providers: [
        provideRouter([
          {
            path: 'projects/:projectId/settings',
            component: ProjectSettingsComponent
          }
        ]),
        MessageService,
        ConfirmationService,
        { provide: ProjectsService, useValue: projectsService },
        { provide: IssuesService, useValue: issuesService },
        {
          provide: AuthService,
          useValue: { hasPermission: vi.fn(() => false) }
        },
        {
          provide: ToastService,
          useValue: toastService
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads assignable users after the owner project context is available', () => {
    expect(issuesService.getAssignableUsers).toHaveBeenCalled();
    expect(component.availableUsers()).toEqual(assignableUsers);
  });

  it('does not add member when user is not selected', () => {
    component.onAddMember();

    expect(projectsService.addProjectMember).not.toHaveBeenCalled();
    expect(component.addMemberForm.controls.userId.touched).toBe(true);
  });

  it('adds selected user as project member', () => {
    component.addMemberForm.patchValue({
      userId: 'user-2',
      role: ProjectMemberRole.MEMBER
    });

    component.onAddMember();

    expect(projectsService.addProjectMember).toHaveBeenCalledWith('project-1', {
      userId: 'user-2',
      role: ProjectMemberRole.MEMBER
    });
    expect(toastService.success).toHaveBeenCalledWith(ProjectMessages.memberAdded);
  });

  it('updates member role when selection changes', () => {
    const member = projectDetails.members[0];

    component.onMemberRoleChange(member, ProjectMemberRole.VIEWER);

    expect(projectsService.updateProjectMemberRole).toHaveBeenCalledWith(
      'project-1',
      member.userId,
      { role: ProjectMemberRole.VIEWER }
    );
    expect(toastService.success).toHaveBeenCalledWith(
      ProjectMessages.memberRoleUpdated
    );
  });
});

describe('ProjectSettingsComponent read-only access', () => {
  let fixture: ComponentFixture<ProjectSettingsComponent>;
  let projectsService: {
    getProjectById: ReturnType<typeof vi.fn>;
    updateProject: ReturnType<typeof vi.fn>;
    addProjectMember: ReturnType<typeof vi.fn>;
    updateProjectMemberRole: ReturnType<typeof vi.fn>;
    removeProjectMember: ReturnType<typeof vi.fn>;
  };
  let issuesService: {
    getAssignableUsers: ReturnType<typeof vi.fn>;
  };

  const memberViewProject: ProjectDetailsDto = {
    id: 'project-1',
    name: 'Platform',
    key: 'PLAT',
    description: 'Workspace',
    status: ProjectStatus.ACTIVE,
    visibility: ProjectVisibility.INTERNAL,
    color: '#3B82F6',
    issueCount: 0,
    memberCount: 1,
    createdAt: '2026-06-13T10:00:00Z',
    currentUserRole: ProjectMemberRole.MEMBER,
    members: [
      {
        userId: 'owner-1',
        displayLabel: 'Owner User (owner@example.com)',
        role: ProjectMemberRole.OWNER,
        joinedAt: '2026-06-13T10:00:00Z'
      }
    ]
  };

  beforeEach(async () => {
    projectsService = {
      getProjectById: vi.fn(() => of(memberViewProject)),
      updateProject: vi.fn(() => of(memberViewProject)),
      addProjectMember: vi.fn(() => of(memberViewProject)),
      updateProjectMemberRole: vi.fn(() => of(memberViewProject)),
      removeProjectMember: vi.fn(() => of(memberViewProject))
    };

    issuesService = {
      getAssignableUsers: vi.fn(() => of([]))
    };

    await TestBed.configureTestingModule({
      imports: [ProjectSettingsComponent],
      providers: [
        provideRouter([
          {
            path: 'projects/:projectId/settings',
            component: ProjectSettingsComponent
          }
        ]),
        MessageService,
        ConfirmationService,
        { provide: ProjectsService, useValue: projectsService },
        { provide: IssuesService, useValue: issuesService },
        {
          provide: ToastService,
          useValue: { success: vi.fn(), error: vi.fn() }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectSettingsComponent);
    fixture.detectChanges();
  });

  it('disables profile editing for non-owner members', () => {
    expect(fixture.componentInstance.canManageProject()).toBe(false);
    expect(fixture.componentInstance.form.disabled).toBe(true);
    expect(issuesService.getAssignableUsers).not.toHaveBeenCalled();
  });

  it('does not submit profile changes for non-owner members', () => {
    fixture.componentInstance.onSubmit();

    expect(projectsService.updateProject).not.toHaveBeenCalled();
  });
});
