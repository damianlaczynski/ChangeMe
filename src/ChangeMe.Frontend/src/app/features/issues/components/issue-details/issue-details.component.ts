import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueCommentDto,
  IssueDetailsDto,
  IssueHistoryEntryDto
} from '@features/issues/models/issue.model';
import { IssuesService } from '@features/issues/services/issues.service';
import {
  getDeleteIssueConfirmMessage,
  getIssueHistoryEventVisual,
  getIssuePriorityLabel,
  getIssuePrioritySeverity,
  getIssueStatusLabel,
  getIssueStatusSeverity,
  IssueCommentConstraints
} from '@features/issues/utils/issue.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { ConfirmationService, PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { Tag } from 'primeng/tag';
import { Textarea } from 'primeng/textarea';
import { Timeline } from 'primeng/timeline';
import { Tooltip } from 'primeng/tooltip';

type IssueDetailsTab = 'comments' | 'history';

type CommentForm = {
  content: FormControl<string>;
};

@Component({
  selector: 'app-issue-details',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    Textarea,
    Message,
    Tag,
    Panel,
    Timeline,
    PrimeTemplate,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    Tooltip,
    BackButtonComponent
  ],
  templateUrl: './issue-details.component.html'
})
export class IssueDetailsComponent {
  readonly id = input<string>();

  private readonly issuesService = inject(IssuesService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly issueCommentConstraints = IssueCommentConstraints;
  readonly getIssueStatusLabel = getIssueStatusLabel;
  readonly getIssueStatusSeverity = getIssueStatusSeverity;
  readonly getIssuePriorityLabel = getIssuePriorityLabel;
  readonly getIssuePrioritySeverity = getIssuePrioritySeverity;
  readonly getIssueHistoryEventVisual = getIssueHistoryEventVisual;

  readonly issue = signal<IssueDetailsDto | null>(null);
  readonly pageTitle = computed(() => this.issue()?.title ?? 'Issue Details');
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly commentError = signal<string | null>(null);
  readonly isSubmitted = signal(false);
  readonly isSubmittingComment = signal(false);
  readonly isTogglingWatch = signal(false);
  readonly isDeleting = signal(false);
  readonly activeTab = signal<IssueDetailsTab>('comments');
  readonly comments = signal<IssueCommentDto[]>([]);
  readonly commentsPagination = signal<PaginationResult<IssueCommentDto> | null>(null);
  readonly commentsQuery = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false
  });
  readonly historyEntries = signal<IssueHistoryEntryDto[]>([]);
  readonly historyPagination = signal<PaginationResult<IssueHistoryEntryDto> | null>(null);
  readonly historyQuery = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false
  });
  readonly isLoadingComments = signal(false);
  readonly isLoadingMoreComments = signal(false);
  readonly isLoadingHistory = signal(false);
  readonly isLoadingMoreHistory = signal(false);
  readonly hasLoadedComments = signal(false);
  readonly hasLoadedHistory = signal(false);

  readonly canShowMoreComments = computed(
    () => this.commentsPagination()?.hasNext ?? false
  );
  readonly canShowMoreHistory = computed(() => this.historyPagination()?.hasNext ?? false);

  readonly commentForm = new FormGroup<CommentForm>({
    content: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(IssueCommentConstraints.CONTENT_MAX_LENGTH)
      ]
    })
  });

  private lastLoadedIssueId: string | null = null;
  private lastTabLoadKey: string | null = null;
  private commentsRequestId = 0;
  private historyRequestId = 0;

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const tab = params.get('tab');
        this.activeTab.set(tab === 'history' ? 'history' : 'comments');
      });

    effect(() => {
      const id = this.id();
      if (!id) {
        this.isLoading.set(false);
        return;
      }

      if (id !== this.lastLoadedIssueId) {
        this.lastLoadedIssueId = id;
        this.lastTabLoadKey = null;
        this.loadIssue(id);
      }

      this.loadActiveTabData(id);
    });
  }

  onTabChange(tab: string | number | undefined): void {
    const value: IssueDetailsTab = tab === 'history' ? 'history' : 'comments';
    if (this.activeTab() === value) {
      return;
    }

    this.activeTab.set(value);
    this.lastTabLoadKey = null;

    const issueId = this.id();
    if (issueId) {
      this.loadActiveTabData(issueId);
    }

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: value === 'comments' ? null : value },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  showMoreComments(): void {
    const issueId = this.id();
    const pagination = this.commentsPagination();
    if (!issueId || !pagination?.hasNext || this.isLoadingMoreComments()) {
      return;
    }

    this.loadComments(issueId, {
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  showMoreHistory(): void {
    const issueId = this.id();
    const pagination = this.historyPagination();
    if (!issueId || !pagination?.hasNext || this.isLoadingMoreHistory()) {
      return;
    }

    this.loadHistory(issueId, {
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  shouldShowCommentError(): boolean {
    return (
      !!this.commentForm.controls.content.errors &&
      (this.commentForm.controls.content.touched || this.isSubmitted())
    );
  }

  addComment(): void {
    this.isSubmitted.set(true);
    this.commentError.set(null);

    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }

    const issueId = this.id();
    if (!issueId) {
      return;
    }

    this.isSubmittingComment.set(true);

    this.issuesService
      .addComment(issueId, {
        content: this.commentForm.controls.content.value.trim()
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.reloadCommentsFromStart(issueId);
          this.commentForm.reset({ content: '' });
          this.commentForm.markAsPristine();
          this.commentForm.markAsUntouched();
          this.isSubmitted.set(false);
          this.isSubmittingComment.set(false);
          this.toastService.success('Comment added');
        },
        error: (error: Error) => {
          this.commentError.set(error.message);
          this.isSubmittingComment.set(false);
        }
      });
  }

  getWatchTooltip(issue: IssueDetailsDto): string {
    const watchers = this.formatWatchersCount(issue.watchersCount);
    return issue.isWatchedByCurrentUser
      ? `Unwatch this issue (${watchers})`
      : `Watch this issue (${watchers})`;
  }

  confirmDeleteIssue(): void {
    const issue = this.issue();
    if (!issue || this.isDeleting()) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete issue',
      message: getDeleteIssueConfirmMessage(issue.title),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Delete', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deleteIssue(issue.id)
    });
  }

  toggleWatch(): void {
    const issue = this.issue();
    if (!issue || this.isTogglingWatch()) {
      return;
    }

    this.isTogglingWatch.set(true);

    const request = issue.isWatchedByCurrentUser
      ? this.issuesService.unwatchIssue(issue.id)
      : this.issuesService.watchIssue(issue.id);

    request.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (watchState) => {
        this.issue.update((currentIssue) =>
          currentIssue
            ? {
                ...currentIssue,
                isWatchedByCurrentUser: watchState.isWatchedByCurrentUser,
                watchersCount: watchState.watchersCount
              }
            : currentIssue
        );
        this.isTogglingWatch.set(false);
      },
      error: (error: Error) => {
        this.loadError.set(error.message);
        this.isTogglingWatch.set(false);
      }
    });
  }

  trackHistoryEntry(_index: number, entry: IssueHistoryEntryDto): string {
    return entry.id;
  }

  refresh(): void {
    const issueId = this.id();
    if (!issueId) {
      return;
    }
    this.loadIssue(issueId, false);
  }

  private deleteIssue(issueId: string): void {
    this.isDeleting.set(true);
    this.loadError.set(null);

    this.issuesService
      .deleteIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('Issue deleted');
          void this.router.navigate(['/issues']);
        },
        error: (error: Error) => {
          this.toastService.showApiError(error, 'Could not delete issue');
          this.isDeleting.set(false);
        }
      });
  }

  private loadIssue(issueId: string, resetState = true): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.issuesService
      .getIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (issue) => {
          this.issue.set(issue);
          this.isLoading.set(false);
          this.loadError.set(null);

          if (resetState) {
            this.commentError.set(null);
          }
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private loadActiveTabData(issueId: string): void {
    const tab = this.activeTab();
    const tabLoadKey = `${issueId}:${tab}`;
    if (tabLoadKey === this.lastTabLoadKey) {
      return;
    }

    this.lastTabLoadKey = tabLoadKey;

    if (tab === 'history') {
      this.reloadHistoryFromStart(issueId);
      return;
    }

    this.reloadCommentsFromStart(issueId);
  }

  private reloadCommentsFromStart(issueId: string): void {
    this.commentsQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false
    });
    this.loadComments(issueId);
  }

  private reloadHistoryFromStart(issueId: string): void {
    this.historyQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false
    });
    this.loadHistory(issueId);
  }

  private loadComments(
    issueId: string,
    options: { append?: boolean; pageNumber?: number } = {}
  ): void {
    const append = options.append ?? false;
    const pageNumber = options.pageNumber ?? this.commentsQuery().pageNumber;
    const query = { ...this.commentsQuery(), pageNumber };
    const requestId = ++this.commentsRequestId;

    if (append) {
      this.isLoadingMoreComments.set(true);
    } else {
      this.isLoadingComments.set(true);
    }

    this.issuesService
      .getIssueComments(issueId, query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.commentsRequestId) {
            return;
          }

          if (append) {
            this.comments.update((items) => [...items, ...result.items]);
          } else {
            this.comments.set(result.items);
          }

          this.commentsQuery.set({
            pageNumber: result.currentPage,
            pageSize: result.pageSize,
            sortField: 'CreatedAt',
            ascending: false
          });
          this.commentsPagination.set(result);
          this.isLoadingComments.set(false);
          this.isLoadingMoreComments.set(false);
          this.hasLoadedComments.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.commentsRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingComments.set(false);
          this.isLoadingMoreComments.set(false);
        }
      });
  }

  private loadHistory(
    issueId: string,
    options: { append?: boolean; pageNumber?: number } = {}
  ): void {
    const append = options.append ?? false;
    const pageNumber = options.pageNumber ?? this.historyQuery().pageNumber;
    const query = { ...this.historyQuery(), pageNumber };
    const requestId = ++this.historyRequestId;

    if (append) {
      this.isLoadingMoreHistory.set(true);
    } else {
      this.isLoadingHistory.set(true);
    }

    this.issuesService
      .getIssueHistory(issueId, query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          if (requestId !== this.historyRequestId) {
            return;
          }

          if (append) {
            this.historyEntries.update((items) => [...items, ...result.items]);
          } else {
            this.historyEntries.set(result.items);
          }

          this.historyQuery.set({
            pageNumber: result.currentPage,
            pageSize: result.pageSize,
            sortField: 'CreatedAt',
            ascending: false
          });
          this.historyPagination.set(result);
          this.isLoadingHistory.set(false);
          this.isLoadingMoreHistory.set(false);
          this.hasLoadedHistory.set(true);
        },
        error: (error: Error) => {
          if (requestId !== this.historyRequestId) {
            return;
          }

          this.loadError.set(error.message);
          this.isLoadingHistory.set(false);
          this.isLoadingMoreHistory.set(false);
        }
      });
  }

  private formatWatchersCount(count: number): string {
    return count === 1 ? '1 watcher' : `${count} watchers`;
  }
}
