import { CommonModule, DatePipe } from '@angular/common';
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
import { ProjectMembershipHistoryEntryDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  getMembershipHistoryEventVisual,
  ProjectMessages,
  shouldShowMembershipBeforeAfter
} from '@features/projects/utils/projects.utils';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Timeline } from 'primeng/timeline';

@Component({
  selector: 'app-project-membership-history-tab',
  imports: [
    CommonModule,
    DatePipe,
    Button,
    Message,
    ProgressSpinner,
    Tag,
    Timeline,
    PrimeTemplate
  ],
  templateUrl: './project-membership-history-tab.component.html',
  host: { class: 'block' }
})
export class ProjectMembershipHistoryTabComponent {
  readonly projectId = input.required<string>();
  readonly refreshToken = input(0);

  private readonly projectsService = inject(ProjectsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly getMembershipHistoryEventVisual = getMembershipHistoryEventVisual;
  readonly shouldShowMembershipBeforeAfter = shouldShowMembershipBeforeAfter;
  readonly ProjectMessages = ProjectMessages;

  readonly historyEntries = signal<ProjectMembershipHistoryEntryDto[]>([]);
  readonly historyPagination =
    signal<PaginationResult<ProjectMembershipHistoryEntryDto> | null>(null);
  readonly historyQuery = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false
  });
  readonly loadError = signal<string | null>(null);
  readonly isLoadingHistory = signal(false);
  readonly isLoadingMoreHistory = signal(false);
  readonly hasLoadedHistory = signal(false);

  readonly canShowMoreHistory = computed(
    () => this.historyPagination()?.hasNext ?? false
  );

  private historyRequestId = 0;
  private lastLoadKey: string | null = null;

  constructor() {
    effect(() => {
      const projectId = this.projectId();
      const refreshToken = this.refreshToken();
      const loadKey = `${projectId}:${refreshToken}`;

      if (loadKey === this.lastLoadKey) {
        return;
      }

      this.lastLoadKey = loadKey;
      this.reloadHistoryFromStart(projectId);
    });
  }

  showMoreHistory(): void {
    const pagination = this.historyPagination();
    if (!pagination?.hasNext || this.isLoadingMoreHistory()) {
      return;
    }

    this.loadHistory(this.projectId(), {
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  private reloadHistoryFromStart(projectId: string): void {
    this.historyQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false
    });
    this.loadHistory(projectId);
  }

  private loadHistory(
    projectId: string,
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

    this.loadError.set(null);

    this.projectsService
      .getProjectMembershipHistory(projectId, query)
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
}
