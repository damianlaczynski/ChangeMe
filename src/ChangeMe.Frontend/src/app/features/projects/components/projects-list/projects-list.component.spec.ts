import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AuthService } from '@features/auth/services/auth.service';
import {
  ProjectListItemDto,
  ProjectMemberRole,
  ProjectStatus,
  ProjectVisibility
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { ProjectsListComponent } from './projects-list.component';

describe('ProjectsListComponent', () => {
  let fixture: ComponentFixture<ProjectsListComponent>;
  let component: ProjectsListComponent;
  let authService: { hasPermission: ReturnType<typeof vi.fn> };

  const baseProject: ProjectListItemDto = {
    id: 'project-1',
    name: 'Platform',
    key: 'PLAT',
    description: 'Workspace',
    status: ProjectStatus.ACTIVE,
    visibility: ProjectVisibility.INTERNAL,
    color: '#3B82F6',
    issueCount: 0,
    memberCount: 1
  };

  beforeEach(async () => {
    authService = {
      hasPermission: vi.fn(
        (permission: string) => permission === PermissionCodes.projectsManage
      )
    };

    await TestBed.configureTestingModule({
      imports: [ProjectsListComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        {
          provide: ProjectsService,
          useValue: {
            getProjects: vi.fn(() =>
              of(createEmptyPaginationResult<ProjectListItemDto>())
            ),
            deleteProject: vi.fn(() => of(true))
          }
        },
        { provide: AuthService, useValue: authService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProjectsListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('maps search text into the list query', () => {
    component.filtersForm.controls.searchText.setValue('  portal  ');

    component.applyFilters();

    expect(component.query().searchText).toBe('portal');
    expect(component.query().pageNumber).toBe(1);
  });

  it('shows create project action when the user has Projects.Manage', () => {
    expect(component.canCreateProjects()).toBe(true);
  });

  it('hides create project action when the user lacks Projects.Manage', async () => {
    authService.hasPermission.mockReturnValue(false);

    const restrictedFixture = TestBed.createComponent(ProjectsListComponent);
    restrictedFixture.detectChanges();

    expect(restrictedFixture.componentInstance.canCreateProjects()).toBe(false);
  });

  it('includes stewardship actions only when the user is project owner', () => {
    component.openProjectActionsMenu(new Event('click'), {
      ...baseProject,
      currentUserRole: ProjectMemberRole.OWNER
    });

    expect(component.projectActionItems().map((item) => item.label)).toEqual([
      'Open workspace',
      'Browse issues',
      'Project settings',
      'Delete project'
    ]);
  });

  it('hides stewardship actions for project members without owner role', () => {
    component.openProjectActionsMenu(new Event('click'), {
      ...baseProject,
      currentUserRole: ProjectMemberRole.MEMBER
    });

    expect(component.projectActionItems().map((item) => item.label)).toEqual([
      'Open workspace',
      'Browse issues'
    ]);
  });
});
