import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { EditTimeEntryDialogComponent } from '@features/time/components/edit-time-entry-dialog/edit-time-entry-dialog.component';
import {
  IssueTimeEntryListItemDto,
  TimeEntryListItemDto
} from '@features/time/models/time.model';
import { LogTimeDialogService } from '@features/time/services/log-time-dialog.service';
import { RunningTimerService } from '@features/time/services/running-timer.service';
import { TimeService } from '@features/time/services/time.service';
import {
  TimeConstraints,
  TimeMessages,
  timeDeleteMenuItemDangerClasses,
  truncateText
} from '@features/time/utils/time.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-issue-time-tab',
  imports: [
    DatePipe,
    Card,
    Button,
    TableModule,
    Message,
    Tag,
    Menu,
    Tooltip,
    ProgressSpinner,
    EditTimeEntryDialogComponent
  ],
  templateUrl: './issue-time-tab.component.html',
  host: { class: 'block' }
})
export class IssueTimeTabComponent implements OnInit {
  readonly issueId = input.required<string>();
  readonly projectId = input.required<string>();
  readonly projectName = input<string | null>(null);
  readonly issueTitle = input.required<string>();

  readonly summaryLoaded = output<{
    totalDurationMinutes: number;
    totalDurationFormatted: string;
    hasAccess: boolean;
  }>();

  private readonly timeService = inject(TimeService);
  private readonly authService = inject(AuthService);
  private readonly logTimeDialogService = inject(LogTimeDialogService);
  readonly runningTimerService = inject(RunningTimerService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly TimeMessages = TimeMessages;
  readonly truncateDescription = (value: string) => truncateText(value, 80);

  readonly entries = signal<IssueTimeEntryListItemDto[]>([]);
  readonly totalDurationFormatted = signal('0m');
  readonly totalDurationMinutes = signal(0);
  readonly contributorNames = signal<string[]>([]);
  readonly isLoading = signal(false);
  readonly isLoadingMore = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly hasLoaded = signal(false);
  readonly hasAccess = signal(true);
  readonly pageNumber = signal(1);
  readonly hasNextPage = signal(false);
  readonly entryActionItems = signal<MenuItem[]>([]);
  readonly editingEntry = signal<TimeEntryListItemDto | null>(null);
  readonly editDialogVisible = signal(false);
  readonly pendingDeleteIds = signal<string[]>([]);
  readonly loggableProjectIds = signal<Set<string>>(new Set());

  private lastLoadedIssueId: string | null = null;
  private lastHandledSaveToken = 0;
  private loadRequestId = 0;

  private readonly entryActionsMenu = viewChild.required<Menu>('entryActionsMenu');

  readonly canLogTime = computed(
    () =>
      this.authService.hasPermission(PermissionCodes.timeLogOwn) &&
      this.loggableProjectIds().has(this.projectId())
  );

  ngOnInit(): void {
    this.loadLoggableProjects();
  }

  readonly visibleContributors = computed(() =>
    [...this.contributorNames()].sort((a, b) => a.localeCompare(b)).slice(0, 5)
  );
  readonly hiddenContributorCount = computed(() =>
    Math.max(0, this.contributorNames().length - 5)
  );
  readonly hiddenContributorNames = computed(() =>
    [...this.contributorNames()]
      .sort((a, b) => a.localeCompare(b))
      .slice(5)
      .join(', ')
  );

  constructor() {
    effect(() => {
      const issueId = this.issueId();
      if (issueId === this.lastLoadedIssueId) {
        return;
      }

      this.lastLoadedIssueId = issueId;
      this.reloadEntries();
    });

    effect(() => {
      const saved = this.logTimeDialogService.saved();
      if (saved === 0 || saved === this.lastHandledSaveToken) {
        return;
      }

      this.lastHandledSaveToken = saved;
      this.reloadEntries();
    });
  }

  openLogTimeDialog(): void {
    this.logTimeDialogService.open({
      projectId: this.projectId(),
      projectName: this.projectName() ?? undefined,
      issueId: this.issueId(),
      issueTitle: this.issueTitle(),
      readonlyProject: true,
      readonlyIssue: true
    });
  }

  startTimer(): void {
    this.runningTimerService.startTimer({
      projectId: this.projectId(),
      issueId: this.issueId()
    });
  }

  showMore(): void {
    if (!this.hasNextPage() || this.isLoadingMore()) {
      return;
    }

    this.pageNumber.update((value) => value + 1);
    this.loadEntries(true);
  }

  openEntryActionsMenu(event: Event, entry: IssueTimeEntryListItemDto): void {
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
    this.reloadEntries();
  }

  private openEditDialog(entry: IssueTimeEntryListItemDto): void {
    this.editingEntry.set({
      id: entry.id,
      projectId: this.projectId(),
      projectName: this.projectName() ?? '',
      issueId: this.issueId(),
      issueTitle: this.issueTitle(),
      workDate: entry.workDate,
      durationMinutes: entry.durationMinutes,
      durationFormatted: entry.durationFormatted,
      description: entry.description,
      createdAt: entry.createdAt,
      canViewProject: true,
      canEdit: entry.canEdit,
      canDelete: entry.canDelete
    });
    this.editDialogVisible.set(true);
  }

  private confirmDeleteEntry(entry: IssueTimeEntryListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete time entry',
      message: TimeMessages.deleteTimeEntryConfirm,
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deleteEntry(entry)
    });
  }

  private deleteEntry(entry: IssueTimeEntryListItemDto): void {
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
          this.reloadEntries();
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not delete entry')
      });
  }

  private reloadEntries(): void {
    this.pageNumber.set(1);
    this.entries.set([]);
    this.loadEntries(false);
  }

  private loadLoggableProjects(): void {
    if (!this.authService.hasPermission(PermissionCodes.timeLogOwn)) {
      return;
    }

    this.timeService
      .getLoggableProjects()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (projects) =>
          this.loggableProjectIds.set(new Set(projects.map((project) => project.id))),
        error: () => this.loggableProjectIds.set(new Set())
      });
  }

  private loadEntries(append: boolean): void {
    const requestId = ++this.loadRequestId;

    if (append) {
      this.isLoadingMore.set(true);
    } else {
      this.isLoading.set(true);
      this.loadError.set(null);
    }

    this.timeService
      .getIssueTimeEntries(this.issueId(), {
        pageNumber: this.pageNumber(),
        pageSize: TimeConstraints.ISSUE_TIME_PAGE_SIZE,
        sortField: 'WorkDate',
        ascending: false
      })
      .pipe(
        finalize(() => {
          if (requestId !== this.loadRequestId) {
            return;
          }

          this.isLoading.set(false);
          this.isLoadingMore.set(false);
          this.hasLoaded.set(true);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          if (requestId !== this.loadRequestId) {
            return;
          }

          this.hasAccess.set(true);
          this.totalDurationMinutes.set(result.totalDurationMinutes);
          this.totalDurationFormatted.set(result.totalDurationFormatted);
          this.contributorNames.set(result.contributorNames);
          this.hasNextPage.set(result.entries.hasNext);
          this.entries.update((current) =>
            append ? [...current, ...result.entries.items] : result.entries.items
          );
          this.summaryLoaded.emit({
            totalDurationMinutes: result.totalDurationMinutes,
            totalDurationFormatted: result.totalDurationFormatted,
            hasAccess: true
          });
        },
        error: (error: Error) => {
          if (requestId !== this.loadRequestId) {
            return;
          }

          if (error.message.toLowerCase().includes('permission')) {
            this.hasAccess.set(false);
            this.summaryLoaded.emit({
              totalDurationMinutes: 0,
              totalDurationFormatted: '0m',
              hasAccess: false
            });
            return;
          }

          this.loadError.set(error.message);
        }
      });
  }
}
