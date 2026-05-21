import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import {
  RoleListItemDto,
  RoleSearchParameters
} from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
  formatDescription,
  getDeleteRoleConfirmMessage,
  RoleMessages
} from '@features/roles/utils/roles.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { catchError, of, switchMap, tap } from 'rxjs';

type RoleSortField = 'Name' | 'Users' | 'Permissions';

@Component({
  selector: 'app-roles-list',
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
    Paginator
  ],
  templateUrl: './roles-list.component.html'
})
export class RolesListComponent {
  private readonly rolesService = inject(RolesService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly permissionCodes = PermissionCodes;

  readonly roles = signal<RoleListItemDto[]>([]);
  readonly pagination = signal<PaginationResult<RoleListItemDto> | null>(null);
  readonly query = signal<RoleSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly roleActionItems = signal<MenuItem[]>([]);
  private readonly roleActionsMenu = viewChild.required<Menu>('roleActionsMenu');

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

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
          this.rolesService.getRoles(params).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(this.createEmptyPaginationResult(params));
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.roles.set(result.items);
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

    const sortField = event.field as RoleSortField;
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

  openRoleActionsMenu(event: Event, role: RoleListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/roles', role.id]
      }
    ];

    if (this.canManageRoles() && !role.isSystem) {
      items.push(
        {
          label: 'Edit role',
          icon: 'pi pi-pencil',
          routerLink: ['/roles', role.id, 'edit']
        },
        {
          label: 'Delete role',
          icon: 'pi pi-trash',
          command: () => this.confirmDeleteRole(role)
        }
      );
    }

    this.roleActionItems.set(items);
    this.roleActionsMenu().toggle(event);
  }

  confirmDeleteRole(role: RoleListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(role.name),
      accept: () => {
        this.rolesService.deleteRole(role.id).subscribe({
          next: () => {
            this.toastService.success(RoleMessages.roleDeleted);
            this.query.update((current) => ({ ...current }));
          },
          error: (error: Error) => {
            this.toastService.error(error.message);
          }
        });
      }
    });
  }

  private createEmptyPaginationResult(
    params: RoleSearchParameters
  ): PaginationResult<RoleListItemDto> {
    return {
      items: [],
      totalCount: 0,
      currentPage: params.pageNumber,
      pageSize: params.pageSize,
      totalPages: 0,
      hasPrevious: false,
      hasNext: false
    };
  }
}
