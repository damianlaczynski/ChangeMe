import { Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  ProjectListItemDto,
  ProjectSearchParameters
} from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  formatDescription,
  getDeleteProjectConfirmMessage,
  getProjectRoleLabel,
  getProjectRoleSeverity,
  projectDeleteMenuItemDangerClasses,
  ProjectMessages
} from '@features/projects/utils/projects.utils';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { catchError, of, switchMap, tap } from 'rxjs';

type ProjectSortField = 'Name' | 'Issues';

@Component({
  selector: 'app-projects-list',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    TableModule,
    Message,
    Tag,
    Menu,
    Paginator,
    Tooltip
  ],
  templateUrl: './projects-list.component.html'
})
export class ProjectsListComponent {
  private readonly projectsService = inject(ProjectsService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly ProjectMessages = ProjectMessages;
  readonly formatDescription = formatDescription;
  readonly getProjectRoleLabel = getProjectRoleLabel;
  readonly getProjectRoleSeverity = getProjectRoleSeverity;

  readonly projects = signal<ProjectListItemDto[]>([]);
  readonly pagination = signal<PaginationResult<ProjectListItemDto> | null>(null);
  readonly query = signal<ProjectSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly projectActionItems = signal<MenuItem[]>([]);
  private readonly projectActionsMenu = viewChild.required<Menu>('projectActionsMenu');

  readonly filtersForm = new FormGroup({
    searchText: new FormControl('', { nonNullable: true })
  });

  constructor() {
    toObservable(this.query)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((params) =>
          this.projectsService.getProjects(params).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(createEmptyPaginationResult<ProjectListItemDto>(params));
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.projects.set(result.items);
        this.pagination.set(result);
        this.isLoading.set(false);
        this.hasLoaded.set(true);
      });
  }

  applyFilters(): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: 1,
      searchText: this.filtersForm.controls.searchText.value.trim() || undefined
    }));
  }

  onTableSort(event: { field?: string | null; order?: number | null }): void {
    if (!event.field || event.order == null || event.order === 0) {
      return;
    }

    const sortField = event.field as ProjectSortField;
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

  onPageChange(event: PaginatorState): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
  }

  openProjectActionsMenu(event: Event, project: ProjectListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/projects', project.id]
      }
    ];

    if (project.canManage && !project.isSystem) {
      items.push(
        {
          label: 'Edit project',
          icon: 'pi pi-pencil',
          routerLink: ['/projects', project.id, 'edit']
        },
        { separator: true },
        {
          label: 'Delete project',
          icon: 'pi pi-trash',
          ...projectDeleteMenuItemDangerClasses,
          command: () => this.confirmDeleteProject(project)
        }
      );
    }

    this.projectActionItems.set(items);
    this.projectActionsMenu().toggle(event);
  }

  confirmDeleteProject(project: ProjectListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete project',
      message: getDeleteProjectConfirmMessage(project.name),
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => {
        this.projectsService.deleteProject(project.id).subscribe({
          next: () => {
            this.toastService.success(ProjectMessages.projectDeleted);
            this.query.update((current) => ({ ...current }));
          },
          error: (error: Error) => {
            this.toastService.error(error.message);
          }
        });
      }
    });
  }

  refresh(): void {
    this.query.update((current) => ({ ...current, pageNumber: 1 }));
  }
}
