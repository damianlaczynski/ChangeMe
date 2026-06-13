import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueDto,
  IssuePriority,
  IssueSearchParameters,
  IssueStatus
} from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IssuesComponent } from './issues-list.component';

describe('IssuesComponent', () => {
  let fixture: ComponentFixture<IssuesComponent>;
  let component: IssuesComponent;
  let issuesService: {
    getAllIssues: ReturnType<typeof vi.fn>;
    getAssignableUsers: ReturnType<typeof vi.fn>;
  };

  const defaultQuery: IssueSearchParameters = {
    pageNumber: 1,
    pageSize: 10,
    sortField: 'LastActivityAt',
    ascending: false
  };

  beforeEach(async () => {
    issuesService = {
      getAllIssues: vi.fn(() =>
        of(createEmptyPaginationResult<IssueDto>(defaultQuery))
      ),
      getAssignableUsers: vi.fn(() =>
        of([{ id: 'user-1', displayLabel: 'Ada Lovelace' }])
      )
    };

    await TestBed.configureTestingModule({
      imports: [IssuesComponent],
      providers: [
        provideRouter([]),
        MessageService,
        ConfirmationService,
        ToastService,
        { provide: IssuesService, useValue: issuesService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(IssuesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('maps filter form values into the list query', () => {
    component.projectId.set('project-1');
    component.filtersForm.patchValue({
      searchText: '  regression  ',
      statuses: [IssueStatus.NEW, IssueStatus.IN_PROGRESS],
      priorities: [IssuePriority.HIGH],
      assignedToUserId: 'user-1',
      watchedByMe: true,
      createdByMe: true
    });

    component.applyFilters();

    expect(component.query()).toEqual({
      ...defaultQuery,
      projectId: 'project-1',
      pageNumber: 1,
      searchText: 'regression',
      statuses: [IssueStatus.NEW, IssueStatus.IN_PROGRESS],
      priorities: [IssuePriority.HIGH],
      assignedToUserId: 'user-1',
      watchedByMe: true,
      createdByMe: true
    });
  });

  it('omits empty filter values from the list query', () => {
    component.filtersForm.patchValue({
      searchText: '   ',
      statuses: [],
      priorities: [],
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });

    component.applyFilters();

    expect(component.query()).toEqual({
      ...defaultQuery,
      pageNumber: 1,
      searchText: undefined,
      statuses: undefined,
      priorities: undefined,
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });
  });

  it('builds applied filter chips from the active query', () => {
    component.filtersForm.patchValue({
      searchText: 'auth',
      statuses: [IssueStatus.NEW],
      priorities: [IssuePriority.CRITICAL],
      assignedToUserId: 'user-1',
      watchedByMe: true,
      createdByMe: true
    });
    component.applyFilters();

    expect(component.appliedFilterChips()).toEqual([
      { id: 'search', label: 'Search: auth' },
      { id: `status-${IssueStatus.NEW}`, label: 'Status: New' },
      { id: `priority-${IssuePriority.CRITICAL}`, label: 'Priority: Critical' },
      { id: 'assignee', label: 'Assignee: Ada Lovelace' },
      { id: 'watched-by-me', label: 'Watched by me' },
      { id: 'my-issues', label: 'My issues' }
    ]);
  });

  it('removes a single filter chip from the form and query', () => {
    component.filtersForm.patchValue({
      searchText: 'auth',
      statuses: [IssueStatus.NEW, IssueStatus.CLOSED]
    });
    component.applyFilters();

    component.removeAppliedFilter({
      id: `status-${IssueStatus.NEW}`,
      label: 'Status: New'
    });

    expect(component.filtersForm.controls.statuses.value).toEqual([IssueStatus.CLOSED]);
    expect(component.query().statuses).toEqual([IssueStatus.CLOSED]);
  });

  it('clears all filters and resets pagination to page one', () => {
    component.filtersForm.patchValue({
      searchText: 'auth',
      statuses: [IssueStatus.NEW],
      watchedByMe: true
    });
    component.applyFilters();
    component.query.set({ ...component.query(), pageNumber: 3 });

    component.clearFilters();

    expect(component.filtersForm.getRawValue()).toEqual({
      searchText: '',
      statuses: [],
      priorities: [],
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });
    expect(component.query()).toEqual({
      ...defaultQuery,
      pageNumber: 1,
      searchText: undefined,
      statuses: undefined,
      priorities: undefined,
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });
  });

  it('ignores invalid paginator page changes', () => {
    const pagination: PaginationResult<IssueDto> = {
      items: [],
      currentPage: 1,
      pageSize: 10,
      totalCount: 12,
      totalPages: 2,
      hasPrevious: false,
      hasNext: true
    };

    component.pagination.set(pagination);
    component.query.set({ ...defaultQuery, pageNumber: 1 });

    component.onPageChange({ page: 5 });

    expect(component.query().pageNumber).toBe(1);
  });
});
