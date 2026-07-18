import { computed, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { IssueDto } from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import type { GridQuery } from '@query-grid/core';
import { GridResourceFactory, type GridResource } from '@query-grid/primeng';
import { ConfirmationService, MessageService } from 'primeng/api';
import { of } from 'rxjs';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { IssuesComponent } from './issues-list.component';

function createMockGrid(
  initialQuery: GridQuery = {
    skip: 0,
    take: 10,
    sort: [{ field: 'LastActivityAt', desc: true }]
  }
): GridResource<IssueDto> {
  const query = signal<GridQuery>(initialQuery);
  const totalCount = signal(0);

  return {
    query,
    items: signal<IssueDto[]>([]),
    totalCount,
    loading: signal(false),
    error: signal(null),
    page: computed(() => Math.floor((query().skip ?? 0) / (query().take ?? 10)) + 1),
    pageCount: computed(() =>
      query().take ? Math.ceil(totalCount() / (query().take ?? 10)) : 0
    ),
    setPage: vi.fn(),
    setTake: vi.fn(),
    setSort: vi.fn(),
    setFilter: vi.fn(),
    setSearch: vi.fn(),
    patchQuery: vi.fn((patch) => query.update((current) => ({ ...current, ...patch }))),
    resetQuery: vi.fn(() => query.set(initialQuery)),
    reload: vi.fn()
  };
}

describe('IssuesComponent', () => {
  let fixture: ComponentFixture<IssuesComponent>;
  let component: IssuesComponent;
  let mockGrid: GridResource<IssueDto>;
  let issuesService: {
    getAllIssues: ReturnType<typeof vi.fn>;
    getAssignableUsers: ReturnType<typeof vi.fn>;
  };

  let authServiceMock: {
    hasPermission: ReturnType<typeof vi.fn>;
    currentUser: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    mockGrid = createMockGrid();
    authServiceMock = {
      hasPermission: vi.fn(() => true),
      currentUser: vi.fn(() => ({ id: 'user-1' }))
    };
    issuesService = {
      getAllIssues: vi.fn(() =>
        of({
          items: [],
          totalCount: 0,
          skip: 0,
          take: 10,
          sort: [{ field: 'LastActivityAt', desc: true }]
        })
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
        { provide: IssuesService, useValue: issuesService },
        {
          provide: GridResourceFactory,
          useValue: { create: () => mockGrid }
        },
        {
          provide: AuthService,
          useValue: authServiceMock
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(IssuesComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('loads assignable users for the assigned-to column filter', () => {
    expect(component.assignedToFilter().options).toEqual([
      { label: 'Ada Lovelace', value: 'user-1' }
    ]);
  });

  it('reloads the grid on refresh', () => {
    component.refresh();
    expect(mockGrid.reload).toHaveBeenCalled();
  });

  it('surfaces grid errors as a message', () => {
    mockGrid.error.set(new Error('Boom'));
    expect(component.errorMessage()).toBe('Boom');
  });

  it('hides create action without Issues.Create permission', () => {
    authServiceMock.hasPermission.mockImplementation(
      (code: string) => code !== 'Issues.Create'
    );
    const localFixture = TestBed.createComponent(IssuesComponent);
    const localComponent = localFixture.componentInstance;
    localFixture.detectChanges();
    expect(localComponent.canCreateIssues()).toBe(false);
  });
});
