import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  signal,
  untracked,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueAssignableUserDto,
  IssueDto,
  IssuePriority,
  IssueSearchParameters,
  IssueStatus
} from '@features/issues/models/issue.model';
import { IssueRealtimeService } from '@features/issues/services/issue-realtime.service';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteIssueConfirmMessage,
  getIssuePriorityLabel,
  getIssuePrioritySeverity,
  getIssueStatusLabel,
  getIssueStatusSeverity,
  issueDeleteMenuItemDangerClasses,
  issuePriorities,
  issueStatuses
} from '@features/issues/utils/issue.utils';
import { AppliedFiltersChipsComponent } from '@shared/components/applied-filters-chips/applied-filters-chips.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { AppliedFilterChip } from '@shared/models/applied-filter-chip.model';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Paginator } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { PaginatorState } from 'primeng/types/paginator';
import { catchError, of, switchMap, tap } from 'rxjs';

type IssuesFilterForm = {
  searchText: FormControl<string>;
  statuses: FormControl<IssueStatus[]>;
  priorities: FormControl<IssuePriority[]>;
  assignedToUserId: FormControl<string | null>;
  watchedByMe: FormControl<boolean>;
  createdByMe: FormControl<boolean>;
};

type IssueSortField = 'Id' | 'Title' | 'CreatedAt' | 'LastActivityAt';

@Component({
  selector: 'app-issues',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    MultiSelect,
    Select,
    Checkbox,
    TableModule,
    Paginator,
    Message,
    Tag,
    Panel,
    Tooltip,
    Menu,
    AppliedFiltersChipsComponent
  ],
  templateUrl: './issues-list.component.html'
})
export class IssuesComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly issueRealtimeService = inject(IssueRealtimeService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly issuePriorities = issuePriorities;
  readonly issueStatuses = issueStatuses;
  readonly getIssueStatusLabel = getIssueStatusLabel;
  readonly getIssueStatusSeverity = getIssueStatusSeverity;
  readonly getIssuePriorityLabel = getIssuePriorityLabel;
  readonly getIssuePrioritySeverity = getIssuePrioritySeverity;

  readonly issues = signal<IssueDto[]>([]);
  readonly pagination = signal<PaginationResult<IssueDto> | null>(null);
  readonly query = signal<IssueSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'LastActivityAt',
    ascending: false
  });
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly assignableUsers = signal<IssueAssignableUserDto[]>([]);
  readonly isLoadingAssignableUsers = signal(false);
  readonly pendingWatchIssueIds = signal<string[]>([]);
  readonly pendingDeleteIssueIds = signal<string[]>([]);
  readonly filtersCollapsed = signal(true);
  readonly issueActionItems = signal<MenuItem[]>([]);
  private readonly issueActionsMenu = viewChild.required<Menu>('issueActionsMenu');

  readonly appliedFilterChips = computed(() => {
    const query = this.query();
    const chips: AppliedFilterChip[] = [];

    const search = query.searchText?.trim();
    if (search) {
      chips.push({ id: 'search', label: `Search: "${search}"` });
    }

    for (const status of query.statuses ?? []) {
      chips.push({
        id: `status-${status}`,
        label: `Status: ${getIssueStatusLabel(status)}`
      });
    }

    for (const priority of query.priorities ?? []) {
      chips.push({
        id: `priority-${priority}`,
        label: `Priority: ${getIssuePriorityLabel(priority)}`
      });
    }

    if (query.assignedToUserId) {
      const assignee = this.assignableUsers().find(
        (user) => user.id === query.assignedToUserId
      );
      chips.push({
        id: 'assignee',
        label: `Assignee: ${assignee?.fullName ?? 'Unknown'}`
      });
    }

    if (query.watchedByMe) {
      chips.push({ id: 'watched-by-me', label: 'Watched by me' });
    }

    if (query.createdByMe) {
      chips.push({ id: 'my-issues', label: 'My issues' });
    }

    return chips;
  });

  readonly hasAppliedFilters = computed(() => this.appliedFilterChips().length > 0);

  readonly filtersForm = new FormGroup<IssuesFilterForm>({
    searchText: new FormControl('', { nonNullable: true }),
    statuses: new FormControl<IssueStatus[]>([], { nonNullable: true }),
    priorities: new FormControl<IssuePriority[]>([], { nonNullable: true }),
    assignedToUserId: new FormControl<string | null>(null),
    watchedByMe: new FormControl(false, { nonNullable: true }),
    createdByMe: new FormControl(false, { nonNullable: true })
  });

  constructor() {
    this.loadAssignableUsers();

    toObservable(this.query)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((query) =>
          this.issuesService.getAllIssues(query).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(this.createEmptyPaginationResult(query));
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.issues.set(result.items);
        this.pagination.set(result);
        this.isLoading.set(false);
        this.hasLoaded.set(true);
      });

    effect(() => {
      const messageVersion = this.issueRealtimeService.issueMessageVersion();
      if (messageVersion === 0 || !this.hasLoaded()) {
        return;
      }

      untracked(() => this.refreshCurrentPage());
    });

    effect(() => {
      const reconnectCount = this.issueRealtimeService.reconnectCount();
      if (reconnectCount === 0 || !this.hasLoaded()) {
        return;
      }

      untracked(() => this.refreshCurrentPage());
    });
  }

  onFiltersCollapsedChange(collapsed: boolean | undefined): void {
    this.filtersCollapsed.set(collapsed ?? true);
  }

  applyFilters(): void {
    const formValue = this.filtersForm.getRawValue();

    this.query.set({
      ...this.query(),
      pageNumber: 1,
      searchText: formValue.searchText.trim() || undefined,
      statuses: formValue.statuses.length > 0 ? formValue.statuses : undefined,
      priorities: formValue.priorities.length > 0 ? formValue.priorities : undefined,
      assignedToUserId: formValue.assignedToUserId,
      watchedByMe: formValue.watchedByMe,
      createdByMe: formValue.createdByMe
    });
  }

  removeAppliedFilter(chip: AppliedFilterChip): void {
    const formValue = this.filtersForm.getRawValue();
    const nextQuery = { ...this.query(), pageNumber: 1 };

    if (chip.id === 'search') {
      this.filtersForm.patchValue({ searchText: '' });
      this.query.set({ ...nextQuery, searchText: undefined });
      return;
    }

    if (chip.id.startsWith('status-')) {
      const status = chip.id.slice('status-'.length) as IssueStatus;
      const statuses = (nextQuery.statuses ?? []).filter((value) => value !== status);
      this.filtersForm.patchValue({ statuses });
      this.query.set({
        ...nextQuery,
        statuses: statuses.length > 0 ? statuses : undefined
      });
      return;
    }

    if (chip.id.startsWith('priority-')) {
      const priority = chip.id.slice('priority-'.length) as IssuePriority;
      const priorities = (nextQuery.priorities ?? []).filter(
        (value) => value !== priority
      );
      this.filtersForm.patchValue({ priorities });
      this.query.set({
        ...nextQuery,
        priorities: priorities.length > 0 ? priorities : undefined
      });
      return;
    }

    if (chip.id === 'assignee') {
      this.filtersForm.patchValue({ assignedToUserId: null });
      this.query.set({ ...nextQuery, assignedToUserId: null });
      return;
    }

    if (chip.id === 'watched-by-me') {
      this.filtersForm.patchValue({ watchedByMe: false });
      this.query.set({ ...nextQuery, watchedByMe: false });
      return;
    }

    if (chip.id === 'my-issues') {
      this.filtersForm.patchValue({ createdByMe: false });
      this.query.set({ ...nextQuery, createdByMe: false });
    }
  }

  clearFilters(): void {
    this.filtersForm.reset({
      searchText: '',
      statuses: [],
      priorities: [],
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });

    this.query.set({
      ...this.query(),
      pageNumber: 1,
      searchText: undefined,
      statuses: undefined,
      priorities: undefined,
      assignedToUserId: null,
      watchedByMe: false,
      createdByMe: false
    });
  }

  onPageChange(event: PaginatorState): void {
    const pageNumber = event.page ?? 0;
    const currentPagination = this.pagination();
    if (
      !currentPagination ||
      pageNumber < 1 ||
      pageNumber > currentPagination.totalPages
    ) {
      return;
    }

    this.query.set({
      ...this.query(),
      pageNumber
    });
  }

  onTableSort(event: { field?: string | null; order?: number | null }): void {
    if (!event.field || event.order == null || event.order === 0) {
      return;
    }

    const sortField = event.field as IssueSortField;
    const ascending = event.order === 1;
    const currentQuery = this.query();

    if (currentQuery.sortField === sortField && currentQuery.ascending === ascending) {
      return;
    }

    this.query.set({
      ...currentQuery,
      pageNumber: 1,
      sortField,
      ascending
    });
  }

  trackIssue(_index: number, issue: IssueDto): string {
    return issue.id;
  }

  openIssueActionsMenu(event: Event, issue: IssueDto): void {
    this.issueActionItems.set([
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/issues', issue.id]
      },
      { separator: true },
      {
        label: 'Edit issue',
        icon: 'pi pi-pencil',
        routerLink: ['/issues', issue.id, 'edit']
      },
      {
        label: 'Delete issue',
        icon: 'pi pi-trash',
        ...issueDeleteMenuItemDangerClasses,
        disabled: this.isDeletePending(issue.id),
        command: () => this.confirmDeleteIssue(issue)
      }
    ]);
    this.issueActionsMenu().toggle(event);
  }

  confirmDeleteIssue(issue: IssueDto): void {
    this.confirmationService.confirm({
      header: 'Delete issue',
      message: getDeleteIssueConfirmMessage(issue.title),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deleteIssue(issue)
    });
  }

  isDeletePending(issueId: string): boolean {
    return this.pendingDeleteIssueIds().includes(issueId);
  }

  private deleteIssue(issue: IssueDto): void {
    if (this.isDeletePending(issue.id)) {
      return;
    }

    this.setDeletePending(issue.id, true);
    this.errorMessage.set(null);

    this.issuesService
      .deleteIssue(issue.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.refreshCurrentPage();
          this.setDeletePending(issue.id, false);
          this.toastService.success('Issue deleted', issue.title);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete issue');
          this.setDeletePending(issue.id, false);
        }
      });
  }

  getWatchTooltip(issue: IssueDto): string {
    const watchers = this.formatWatchersCount(issue.watchersCount);
    return issue.isWatchedByCurrentUser
      ? `Unwatch this issue (${watchers})`
      : `Watch this issue (${watchers})`;
  }

  toggleWatch(issue: IssueDto): void {
    if (this.isWatchPending(issue.id)) {
      return;
    }

    this.setWatchPending(issue.id, true);

    const request = issue.isWatchedByCurrentUser
      ? this.issuesService.unwatchIssue(issue.id)
      : this.issuesService.watchIssue(issue.id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (watchState) => {
        this.issues.update((items) =>
          items.map((item) =>
            item.id === watchState.issueId
              ? {
                  ...item,
                  isWatchedByCurrentUser: watchState.isWatchedByCurrentUser,
                  watchersCount: watchState.watchersCount
                }
              : item
          )
        );
        this.setWatchPending(issue.id, false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.setWatchPending(issue.id, false);
      }
    });
  }

  isWatchPending(issueId: string): boolean {
    return this.pendingWatchIssueIds().includes(issueId);
  }

  refresh(): void {
    this.query.set({
      ...this.query(),
      pageNumber: 1
    });
  }

  private loadAssignableUsers(): void {
    this.isLoadingAssignableUsers.set(true);

    this.issuesService
      .getAssignableUsers()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (users) => {
          this.assignableUsers.set(users);
          this.isLoadingAssignableUsers.set(false);
        },
        error: () => {
          this.isLoadingAssignableUsers.set(false);
        }
      });
  }

  private refreshCurrentPage(): void {
    this.query.set({
      ...this.query()
    });
  }

  private setWatchPending(issueId: string, isPending: boolean): void {
    this.pendingWatchIssueIds.update((issueIds) =>
      isPending ? [...issueIds, issueId] : issueIds.filter((id) => id !== issueId)
    );
  }

  private setDeletePending(issueId: string, isPending: boolean): void {
    this.pendingDeleteIssueIds.update((issueIds) =>
      isPending ? [...issueIds, issueId] : issueIds.filter((id) => id !== issueId)
    );
  }

  private formatWatchersCount(count: number): string {
    return count === 1 ? '1 watcher' : `${count} watchers`;
  }

  private createEmptyPaginationResult(
    query: IssueSearchParameters
  ): PaginationResult<IssueDto> {
    return {
      items: [],
      totalCount: 0,
      currentPage: query.pageNumber,
      pageSize: query.pageSize,
      totalPages: 0,
      hasPrevious: false,
      hasNext: false
    };
  }
}
