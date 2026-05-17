import { CommonModule } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
  signal,
  untracked
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NavigationHistoryService } from '@core/navigation/services/navigation-history.service';
import { ToastService } from '@core/toast/services/toast.service';
import {
  IssueDetailsDto,
  IssueHistoryEntryDto
} from '@features/issues/models/issue.model';
import { IssueRealtimeService } from '@features/issues/services/issue-realtime.service';
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
  private readonly issueRealtimeService = inject(IssueRealtimeService);
  private readonly navigationHistory = inject(NavigationHistoryService);
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
  readonly hasLoaded = signal(false);
  readonly loadError = signal<string | null>(null);
  readonly commentError = signal<string | null>(null);
  readonly isSubmitted = signal(false);
  readonly isSubmittingComment = signal(false);
  readonly isTogglingWatch = signal(false);
  readonly isDeleting = signal(false);
  readonly activeTab = signal<IssueDetailsTab>('comments');

  readonly commentForm = new FormGroup<CommentForm>({
    content: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(IssueCommentConstraints.CONTENT_MAX_LENGTH)
      ]
    })
  });

  readonly sortedHistoryEntries = computed(() =>
    [...(this.issue()?.historyEntries ?? [])].sort(this.sortByCreatedAtAscending)
  );
  readonly sortedComments = computed(() =>
    [...(this.issue()?.comments ?? [])].sort(this.sortByCreatedAtAscending)
  );

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

      this.loadIssue(id);
    });

    effect(() => {
      const messageVersion = this.issueRealtimeService.issueMessageVersion();
      const message = this.issueRealtimeService.lastIssueMessage();
      const issueId = this.id();

      if (
        messageVersion === 0 ||
        !message ||
        !issueId ||
        message.issueId !== issueId ||
        !this.hasLoaded()
      ) {
        return;
      }

      untracked(() => this.loadIssue(issueId, false));
    });

    effect(() => {
      const reconnectCount = this.issueRealtimeService.reconnectCount();
      const issueId = this.id();
      if (reconnectCount === 0 || !issueId || !this.hasLoaded()) {
        return;
      }

      untracked(() => this.loadIssue(issueId, false));
    });
  }

  onTabChange(tab: string | number | undefined): void {
    const value: IssueDetailsTab = tab === 'history' ? 'history' : 'comments';
    this.activeTab.set(value);

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: value === 'comments' ? null : value },
      queryParamsHandling: 'merge',
      replaceUrl: true
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
        next: (issue) => {
          this.issue.set(issue);
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

  private deleteIssue(issueId: string): void {
    this.isDeleting.set(true);
    this.loadError.set(null);

    this.issuesService
      .deleteIssue(issueId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success('Issue deleted');
          this.navigationHistory.navigateAfterIssueRemoval(issueId);
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
          this.hasLoaded.set(true);

          if (resetState) {
            this.commentError.set(null);
          }
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
          this.hasLoaded.set(true);
        }
      });
  }

  private formatWatchersCount(count: number): string {
    return count === 1 ? '1 watcher' : `${count} watchers`;
  }

  private sortByCreatedAtAscending(
    left: { createdAt: string },
    right: { createdAt: string }
  ): number {
    return new Date(left.createdAt).getTime() - new Date(right.createdAt).getTime();
  }
}
