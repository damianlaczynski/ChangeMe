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
import { ProjectOperationHistoryEntryDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  getOperationHistoryEventVisual,
  ProjectMessages,
  shouldShowOperationBeforeAfter
} from '@features/projects/utils/projects.utils';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { PrimeTemplate } from 'primeng/api';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Timeline } from 'primeng/timeline';

@Component({
  selector: 'app-project-operation-history-tab',
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
  templateUrl: './project-operation-history-tab.component.html',
  host: { class: 'block' }
})
export class ProjectOperationHistoryTabComponent {
  readonly projectId = input.required<string>();

  private readonly projectsService = inject(ProjectsService);
  private readonly destroyRef = inject(DestroyRef);

  readonly getOperationHistoryEventVisual = getOperationHistoryEventVisual;
  readonly shouldShowOperationBeforeAfter = shouldShowOperationBeforeAfter;
  readonly ProjectMessages = ProjectMessages;

  readonly historyEntries = signal<ProjectOperationHistoryEntryDto[]>([]);
  readonly historyPagination =
    signal<PaginationResult<ProjectOperationHistoryEntryDto> | null>(null);
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

  private lastLoadedProjectId: string | null = null;
  private historyRequestId = 0;

  constructor() {
    effect(() => {
      const projectId = this.projectId();
      if (projectId === this.lastLoadedProjectId) {
        return;
      }

      this.lastLoadedProjectId = projectId;
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
      .getProjectOperationHistory(projectId, query)
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
