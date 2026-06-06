import { Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import {
  PositionListItemDto,
  PositionSearchParameters
} from '@features/billing/models/position.model';
import { PositionsService } from '@features/billing/services/positions.service';
import {
  BillingMessages,
  getPositionActiveLabel,
  getPositionActiveSeverity
} from '@features/billing/utils/billing.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
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
import { catchError, of, switchMap, tap } from 'rxjs';

type PositionSortField = 'Name' | 'Department' | 'Contracts' | 'Active';

@Component({
  selector: 'app-positions-list',
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
  templateUrl: './positions-list.component.html'
})
export class PositionsListComponent {
  private readonly positionsService = inject(PositionsService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly getPositionActiveLabel = getPositionActiveLabel;
  readonly getPositionActiveSeverity = getPositionActiveSeverity;

  readonly positions = signal<PositionListItemDto[]>([]);
  readonly pagination = signal<PaginationResult<PositionListItemDto> | null>(null);
  readonly query = signal<PositionSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly positionActionItems = signal<MenuItem[]>([]);
  private readonly positionActionsMenu =
    viewChild.required<Menu>('positionActionsMenu');

  readonly canManagePositions = this.authService.hasPermission(
    PermissionCodes.billingManageEmployment
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
          this.positionsService.getPositions(params).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(createEmptyPaginationResult<PositionListItemDto>());
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.positions.set(result.items);
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

  refresh(): void {
    this.query.update((current) => ({ ...current }));
  }

  onPageChange(event: PaginatorState): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
  }

  onTableSort(event: { field?: string | null; order?: number | null }): void {
    if (!event.field || event.order == null || event.order === 0) {
      return;
    }

    const field = event.field as PositionSortField;
    const ascending = event.order === 1;
    const currentQuery = this.query();

    if (currentQuery.sortField === field && currentQuery.ascending === ascending) {
      return;
    }

    this.query.set({
      ...currentQuery,
      pageNumber: 1,
      sortField: field,
      ascending
    });
  }

  openPositionActionsMenu(event: Event, position: PositionListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'View',
        icon: 'pi pi-eye',
        routerLink: ['/billing/positions', position.id]
      }
    ];

    if (position.canManage) {
      items.push({
        label: 'Edit',
        icon: 'pi pi-pencil',
        routerLink: ['/billing/positions', position.id, 'edit']
      });
    }

    if (position.canManage && position.contractCount === 0) {
      items.push({
        label: 'Delete',
        icon: 'pi pi-trash',
        command: () => this.confirmDelete(position)
      });
    }

    this.positionActionItems.set(items);
    this.positionActionsMenu().toggle(event);
  }

  private confirmDelete(position: PositionListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete position',
      message: BillingMessages.deletePositionConfirm(position.name),
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deletePosition(position.id)
    });
  }

  private deletePosition(id: string): void {
    this.positionsService
      .deletePosition(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.positionDeleted);
          this.refresh();
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not delete position')
      });
  }
}
