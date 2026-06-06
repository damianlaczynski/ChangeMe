import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { EditTimeEntryDialogComponent } from '@features/time/components/edit-time-entry-dialog/edit-time-entry-dialog.component';
import {
  MyTimeEntriesSearchParameters,
  TimeEntryListItemDto
} from '@features/time/models/time.model';
import { LogTimeDialogService } from '@features/time/services/log-time-dialog.service';
import { RunningTimerService } from '@features/time/services/running-timer.service';
import { TimeService } from '@features/time/services/time.service';
import {
  ProjectTimePermissionCodes,
  TimeConstraints,
  TimeDatePresetId,
  TimeMessages,
  dateFilterPresets,
  getCurrentMonthDateRange,
  getDateRangeForPreset,
  timeDeleteMenuItemDangerClasses,
  toIsoDateString,
  truncateText
} from '@features/time/utils/time.utils';
import { ProjectOptionDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tooltip } from 'primeng/tooltip';
import { finalize } from 'rxjs';

type MyTimeFilterForm = {
  dateFrom: FormControl<Date | null>;
  dateTo: FormControl<Date | null>;
  projectId: FormControl<string | null>;
};

type MyTimeSortField = 'WorkDate';

@Component({
  selector: 'app-my-time',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    Panel,
    Select,
    DatePicker,
    TableModule,
    Message,
    Menu,
    Tooltip,
    ProgressSpinner,
    EditTimeEntryDialogComponent
  ],
  templateUrl: './my-time.component.html'
})
export class MyTimeComponent {
  private readonly timeService = inject(TimeService);
  private readonly projectsService = inject(ProjectsService);
  private readonly authService = inject(AuthService);
  private readonly logTimeDialogService = inject(LogTimeDialogService);
  readonly runningTimerService = inject(RunningTimerService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly TimeMessages = TimeMessages;
  readonly dateFilterPresets = dateFilterPresets;
  readonly truncateDescription = (value: string) => truncateText(value, 60);

  readonly entries = signal<TimeEntryListItemDto[]>([]);
  readonly totalDurationFormatted = signal('0m');
  readonly isLoading = signal(true);
  readonly isLoadingMore = signal(false);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly hasNextPage = signal(false);
  readonly projectOptions = signal<ProjectOptionDto[]>([]);
  readonly isLoadingProjects = signal(false);
  readonly filtersCollapsed = signal(true);
  readonly dateRangeError = signal<string | null>(null);
  readonly entryActionItems = signal<MenuItem[]>([]);
  readonly editingEntry = signal<TimeEntryListItemDto | null>(null);
  readonly editDialogVisible = signal(false);
  readonly pendingDeleteIds = signal<string[]>([]);

  private readonly refreshToken = signal(0);
  private lastLoadKey: string | null = null;
  private lastHandledSaveToken = 0;
  private loadRequestId = 0;

  private readonly entryActionsMenu = viewChild.required<Menu>('entryActionsMenu');

  readonly canLogTime = computed(() =>
    this.authService.hasPermission(PermissionCodes.timeLogOwn)
  );

  readonly query = signal<MyTimeEntriesSearchParameters>({
    pageNumber: 1,
    pageSize: TimeConstraints.MY_TIME_PAGE_SIZE,
    sortField: 'WorkDate',
    ascending: false,
    ...this.getDefaultDateRangeParams()
  });

  readonly hasAppliedFilters = computed(() => {
    const query = this.query();
    const defaults = this.getDefaultDateRangeParams();
    return query.projectId != null;
  });

  readonly filtersForm = new FormGroup<MyTimeFilterForm>({
    dateFrom: new FormControl<Date | null>(getCurrentMonthDateRange().dateFrom),
    dateTo: new FormControl<Date | null>(getCurrentMonthDateRange().dateTo),
    projectId: new FormControl<string | null>(null)
  });

  constructor() {
    this.loadProjectOptions();

    effect(() => {
      const saved = this.logTimeDialogService.saved();
      if (saved === 0 || saved === this.lastHandledSaveToken) {
        return;
      }

      this.lastHandledSaveToken = saved;
      this.requestReloadFromStart();
    });

    effect(() => {
      const query = this.query();
      const refreshToken = this.refreshToken();
      const loadKey = `${refreshToken}:${this.serializeQuery(query)}`;

      if (loadKey === this.lastLoadKey) {
        return;
      }

      this.lastLoadKey = loadKey;
      this.loadEntries(query);
    });
  }

  onFiltersCollapsedChange(collapsed: boolean | undefined): void {
    this.filtersCollapsed.set(collapsed ?? true);
  }

  applyFilters(): void {
    if (!this.validateDateRange()) {
      return;
    }

    const formValue = this.filtersForm.getRawValue();

    this.query.set({
      ...this.query(),
      pageNumber: 1,
      dateFrom: formValue.dateFrom ? toIsoDateString(formValue.dateFrom) : undefined,
      dateTo: formValue.dateTo ? toIsoDateString(formValue.dateTo) : undefined,
      projectId: formValue.projectId
    });
  }

  clearFilters(): void {
    const range = getCurrentMonthDateRange();
    this.filtersForm.reset({
      dateFrom: range.dateFrom,
      dateTo: range.dateTo,
      projectId: null
    });
    this.dateRangeError.set(null);

    this.query.set({
      pageNumber: 1,
      pageSize: TimeConstraints.MY_TIME_PAGE_SIZE,
      sortField: 'WorkDate',
      ascending: false,
      ...this.getDefaultDateRangeParams()
    });
  }

  applyDatePreset(preset: TimeDatePresetId): void {
    const range = getDateRangeForPreset(preset);
    this.filtersForm.patchValue({
      dateFrom: range.dateFrom,
      dateTo: range.dateTo
    });
    this.applyFilters();
  }

  showMore(): void {
    if (!this.hasNextPage() || this.isLoadingMore()) {
      return;
    }

    this.isLoadingMore.set(true);
    this.query.update((current) => ({
      ...current,
      pageNumber: (current.pageNumber ?? 1) + 1
    }));
  }

  onTableSort(event: { field?: string | null; order?: number | null }): void {
    if (this.isLoading() || this.isLoadingMore()) {
      return;
    }

    if (!event.field || event.order == null || event.order === 0) {
      return;
    }

    const sortField = event.field as MyTimeSortField;
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

  openLogTimeDialog(): void {
    this.logTimeDialogService.open();
  }

  startTimer(): void {
    this.runningTimerService.startTimer({});
  }

  scrollToTimerControl(): void {
    document.getElementById('running-timer-control')?.scrollIntoView({
      behavior: 'smooth',
      block: 'nearest'
    });
    document.getElementById('running-timer-control')?.focus();
  }

  openEntryActionsMenu(event: Event, entry: TimeEntryListItemDto): void {
    const items: MenuItem[] = [];

    if (entry.canEdit) {
      items.push({
        label: 'Edit',
        icon: 'pi pi-pencil',
        command: () => this.openEditDialog(entry)
      });
    }

    if (entry.canDelete) {
      items.push({
        label: 'Delete',
        icon: 'pi pi-trash',
        ...timeDeleteMenuItemDangerClasses,
        disabled: this.pendingDeleteIds().includes(entry.id),
        command: () => this.confirmDeleteEntry(entry)
      });
    }

    this.entryActionItems.set(items);
    this.entryActionsMenu().toggle(event);
  }

  onEditSaved(): void {
    this.requestReloadFromStart();
  }

  private openEditDialog(entry: TimeEntryListItemDto): void {
    this.editingEntry.set(entry);
    this.editDialogVisible.set(true);
  }

  private confirmDeleteEntry(entry: TimeEntryListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete time entry',
      message: TimeMessages.deleteTimeEntryConfirm,
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deleteEntry(entry)
    });
  }

  private deleteEntry(entry: TimeEntryListItemDto): void {
    if (this.pendingDeleteIds().includes(entry.id)) {
      return;
    }

    this.pendingDeleteIds.update((ids) => [...ids, entry.id]);

    this.timeService
      .deleteTimeEntry(entry.id)
      .pipe(
        finalize(() =>
          this.pendingDeleteIds.update((ids) => ids.filter((id) => id !== entry.id))
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.toastService.success(TimeMessages.timeEntryDeleted);
          this.requestReloadFromStart();
        },
        error: (error: Error) => this.toastService.showApiError(error, 'Could not delete entry')
      });
  }

  private requestReloadFromStart(): void {
    if ((this.query().pageNumber ?? 1) > 1) {
      this.query.update((current) => ({
        ...current,
        pageNumber: 1
      }));
      return;
    }

    this.refreshToken.update((value) => value + 1);
  }

  private loadEntries(query: MyTimeEntriesSearchParameters): void {
    const append = (query.pageNumber ?? 1) > 1;
    const requestId = ++this.loadRequestId;

    if (append) {
      this.isLoadingMore.set(true);
    } else {
      this.isLoading.set(true);
      this.errorMessage.set(null);
    }

    this.timeService
      .getMyTimeEntries(query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.loadRequestId) {
            return;
          }

          if (append) {
            this.entries.update((items) => [...items, ...result.entries.items]);
          } else {
            this.entries.set(result.entries.items);
          }

          this.totalDurationFormatted.set(result.totalDurationFormatted);
          this.hasNextPage.set(result.entries.hasNext);
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
          this.hasLoaded.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.loadRequestId) {
            return;
          }

          this.errorMessage.set(error.message);
          this.isLoading.set(false);
          this.isLoadingMore.set(false);
          this.hasLoaded.set(true);
        }
      });
  }

  private serializeQuery(query: MyTimeEntriesSearchParameters): string {
    return JSON.stringify({
      pageNumber: query.pageNumber,
      pageSize: query.pageSize,
      sortField: query.sortField,
      ascending: query.ascending,
      dateFrom: query.dateFrom,
      dateTo: query.dateTo,
      projectId: query.projectId
    });
  }

  private loadProjectOptions(): void {
    this.isLoadingProjects.set(true);

    this.projectsService
      .getManageableProjects(ProjectTimePermissionCodes.timeView)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (projects) => {
          this.projectOptions.set(projects);
          this.isLoadingProjects.set(false);
        },
        error: () => this.isLoadingProjects.set(false)
      });
  }

  private validateDateRange(): boolean {
    const { dateFrom, dateTo } = this.filtersForm.getRawValue();

    if (dateFrom && dateTo && dateFrom > dateTo) {
      this.dateRangeError.set(TimeMessages.dateRangeInvalid);
      return false;
    }

    this.dateRangeError.set(null);
    return true;
  }

  private getDefaultDateRangeParams(): Pick<MyTimeEntriesSearchParameters, 'dateFrom' | 'dateTo'> {
    const range = getCurrentMonthDateRange();
    return {
      dateFrom: toIsoDateString(range.dateFrom),
      dateTo: toIsoDateString(range.dateTo)
    };
  }
}
