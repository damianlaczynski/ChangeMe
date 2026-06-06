import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { RequestLeaveDialogComponent } from '@features/billing/components/request-leave-dialog/request-leave-dialog.component';
import { LeaveBalanceDto } from '@features/billing/models/billing-settings.model';
import {
  LeaveRequestDetailsDto,
  LeaveRequestListItemDto,
  LeaveRequestStatus,
  MyLeaveRequestSearchParameters
} from '@features/billing/models/leave-request.model';
import { BillingSettingsService } from '@features/billing/services/billing-settings.service';
import { EmploymentService } from '@features/billing/services/employment.service';
import { LeaveRequestsService } from '@features/billing/services/leave-requests.service';
import {
  BillingMessages,
  LeaveRequestConstraints,
  getLeaveRequestStatusSeverity
} from '@features/billing/utils/billing.utils';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { catchError, forkJoin, of, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-my-leave',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Card,
    Panel,
    Button,
    Message,
    Tag,
    TableModule,
    Menu,
    Paginator,
    ProgressSpinner,
    ToggleSwitch,
    RequestLeaveDialogComponent
  ],
  templateUrl: './my-leave.component.html'
})
export class MyLeaveComponent {
  private readonly leaveRequestsService = inject(LeaveRequestsService);
  private readonly billingSettingsService = inject(BillingSettingsService);
  private readonly employmentService = inject(EmploymentService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly LeaveRequestConstraints = LeaveRequestConstraints;
  readonly LeaveRequestStatus = LeaveRequestStatus;
  readonly getLeaveRequestStatusSeverity = getLeaveRequestStatusSeverity;

  readonly balance = signal<LeaveBalanceDto | null>(null);
  readonly hasActiveContract = signal(true);
  readonly isLoadingBalance = signal(true);
  readonly requests = signal<LeaveRequestListItemDto[]>([]);
  readonly isLoadingRequests = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly requestDialogVisible = signal(false);
  readonly editingRequest = signal<LeaveRequestDetailsDto | null>(null);
  readonly rowActionItems = signal<MenuItem[]>([]);
  readonly pageNumber = signal(1);
  readonly totalCount = signal(0);
  readonly showAllYearsControl = new FormControl(false, { nonNullable: true });

  private readonly rowActionsMenu = viewChild.required<Menu>('rowActionsMenu');

  private readonly query = signal<MyLeaveRequestSearchParameters>({
    pageNumber: 1,
    pageSize: LeaveRequestConstraints.PAGE_SIZE,
    showAllYears: false
  });

  constructor() {
    this.loadBalance();

    this.showAllYearsControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        this.query.update((current) => ({
          ...current,
          pageNumber: 1,
          showAllYears: value
        }));
      });

    toObservable(this.query)
      .pipe(
        tap(() => {
          this.isLoadingRequests.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((params) =>
          this.leaveRequestsService.getMyLeaveRequests(params).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(createEmptyPaginationResult<LeaveRequestListItemDto>(params));
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.requests.set(result.items);
        this.totalCount.set(result.totalCount);
        this.pageNumber.set(result.currentPage);
        this.isLoadingRequests.set(false);
      });
  }

  openRequestDialog(): void {
    this.editingRequest.set(null);
    this.requestDialogVisible.set(true);
  }

  onPageChange(event: PaginatorState): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
  }

  openRowActions(event: Event, request: LeaveRequestListItemDto): void {
    const items: MenuItem[] = [];

    if (request.status === LeaveRequestStatus.Draft) {
      items.push(
        {
          label: 'Edit',
          icon: 'pi pi-pencil',
          command: () => this.openEditDialog(request.id)
        },
        {
          label: 'Submit',
          icon: 'pi pi-check',
          command: () => this.submitRequest(request.id)
        },
        {
          label: 'Delete',
          icon: 'pi pi-trash',
          command: () => this.confirmDelete(request.id)
        }
      );
    } else if (request.status === LeaveRequestStatus.Submitted) {
      items.push(
        {
          label: 'View',
          icon: 'pi pi-eye',
          command: () => this.viewRequest(request.id)
        },
        {
          label: 'Cancel request',
          icon: 'pi pi-ban',
          command: () => this.confirmCancel(request.id)
        }
      );
    } else {
      items.push({
        label: 'View',
        icon: 'pi pi-eye',
        command: () => this.viewRequest(request.id)
      });
    }

    this.rowActionItems.set(items);
    this.rowActionsMenu().toggle(event);
  }

  onRowClick(request: LeaveRequestListItemDto): void {
    void this.router.navigate(['/leave-requests', request.id], {
      queryParams: { from: 'my-leave' }
    });
  }

  onSaved(): void {
    this.refreshRequests();
    this.loadBalance();
  }

  private loadBalance(): void {
    this.isLoadingBalance.set(true);
    const year = new Date().getFullYear();

    forkJoin({
      balance: this.billingSettingsService.getMyLeaveBalance(year),
      employment: this.employmentService.getMyEmploymentSummary()
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: ({ balance, employment }) => {
          this.hasActiveContract.set(employment !== null);
          this.balance.set(balance);
          this.isLoadingBalance.set(false);
        },
        error: () => {
          this.hasActiveContract.set(false);
          this.isLoadingBalance.set(false);
        }
      });
  }

  private refreshRequests(): void {
    this.query.update((current) => ({ ...current }));
  }

  private viewRequest(id: string): void {
    void this.router.navigate(['/leave-requests', id], {
      queryParams: { from: 'my-leave' }
    });
  }

  private openEditDialog(id: string): void {
    this.leaveRequestsService
      .getLeaveRequestById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.editingRequest.set(details);
          this.requestDialogVisible.set(true);
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private submitRequest(id: string): void {
    this.leaveRequestsService
      .submitLeaveRequest(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.toastService.success(
            details.status === LeaveRequestStatus.Approved
              ? BillingMessages.leaveRequestApproved
              : BillingMessages.leaveRequestSubmitted
          );
          this.onSaved();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private confirmCancel(id: string): void {
    this.confirmationService.confirm({
      header: 'Cancel leave request',
      message: BillingMessages.cancelLeaveRequestConfirm,
      ...createDestructiveConfirmationOptions('Cancel request'),
      accept: () => this.cancelRequest(id)
    });
  }

  private cancelRequest(id: string): void {
    this.leaveRequestsService
      .cancelLeaveRequest(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.leaveRequestCancelled);
          this.onSaved();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private confirmDelete(id: string): void {
    this.confirmationService.confirm({
      header: 'Delete leave request',
      message: BillingMessages.deleteDraftLeaveRequestConfirm,
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deleteRequest(id)
    });
  }

  private deleteRequest(id: string): void {
    this.leaveRequestsService
      .deleteLeaveRequest(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.leaveRequestDeleted);
          this.onSaved();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }
}
