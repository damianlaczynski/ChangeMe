import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { CreateSettlementPeriodDialogComponent } from '@features/billing/components/create-settlement-period-dialog/create-settlement-period-dialog.component';
import {
  SettlementPeriodDetailsDto,
  SettlementPeriodListItemDto,
  SettlementPeriodStatus,
  UserSettlementListItemDto
} from '@features/billing/models/settlement.model';
import { SettlementsService } from '@features/billing/services/settlements.service';
import {
  BillingMessages,
  formatMonthlyHoursNorm,
  getContractTypeLabel,
  getSettlementBalanceClass,
  getSettlementPeriodStatusSeverity
} from '@features/billing/utils/billing.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-settlements-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Card,
    Panel,
    Button,
    Select,
    TableModule,
    Message,
    Tag,
    ProgressSpinner,
    CreateSettlementPeriodDialogComponent
  ],
  templateUrl: './settlements-list.component.html'
})
export class SettlementsListComponent {
  private readonly settlementsService = inject(SettlementsService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly SettlementPeriodStatus = SettlementPeriodStatus;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getContractTypeLabel = getContractTypeLabel;
  readonly getSettlementBalanceClass = getSettlementBalanceClass;
  readonly getSettlementPeriodStatusSeverity = getSettlementPeriodStatusSeverity;

  readonly periods = signal<SettlementPeriodListItemDto[]>([]);
  readonly periodDetails = signal<SettlementPeriodDetailsDto | null>(null);
  readonly isLoadingPeriods = signal(true);
  readonly isLoadingDetails = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly createDialogVisible = signal(false);
  readonly isRecalculatingAll = signal(false);
  readonly isClosing = signal(false);
  readonly recalculatingRowId = signal<string | null>(null);

  readonly canManageSettlements = this.authService.hasPermission(
    PermissionCodes.billingManageSettlements
  );

  readonly selectedPeriodIdControl = new FormControl<string | null>(null);

  constructor() {
    this.loadPeriods();

    this.selectedPeriodIdControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((periodId) => {
        if (periodId) {
          this.loadPeriodDetails(periodId);
        } else {
          this.periodDetails.set(null);
        }
      });
  }

  openCreateDialog(): void {
    this.createDialogVisible.set(true);
  }

  onPeriodCreated(periodId: string): void {
    this.loadPeriods(periodId);
  }

  recalculateAll(): void {
    const details = this.periodDetails();
    if (!details?.canManage || details.status !== SettlementPeriodStatus.Open) {
      return;
    }

    this.isRecalculatingAll.set(true);
    this.settlementsService
      .recalculateAllSettlements(details.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.periodDetails.set(updated);
          this.toastService.success(BillingMessages.settlementsRecalculated);
          this.isRecalculatingAll.set(false);
        },
        error: (error: Error) => {
          this.toastService.error(error.message);
          this.isRecalculatingAll.set(false);
        }
      });
  }

  confirmClosePeriod(): void {
    const details = this.periodDetails();
    if (!details?.canManage || details.status !== SettlementPeriodStatus.Open) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Close period',
      message: BillingMessages.closeSettlementPeriodConfirm(details.label),
      ...createDestructiveConfirmationOptions('Close period'),
      accept: () => this.closePeriod()
    });
  }

  onRowClick(settlement: UserSettlementListItemDto): void {
    void this.router.navigate(['/user-settlements', settlement.id]);
  }

  recalculateRow(event: Event, settlement: UserSettlementListItemDto): void {
    event.stopPropagation();
    if (!settlement.canRecalculate) {
      return;
    }

    this.recalculatingRowId.set(settlement.id);
    this.settlementsService
      .recalculateUserSettlement(settlement.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.settlementRecalculated);
          const periodId = this.selectedPeriodIdControl.value;
          if (periodId) {
            this.loadPeriodDetails(periodId);
          }
          this.recalculatingRowId.set(null);
        },
        error: (error: Error) => {
          this.toastService.error(error.message);
          this.recalculatingRowId.set(null);
        }
      });
  }

  private closePeriod(): void {
    const details = this.periodDetails();
    if (!details) {
      return;
    }

    this.isClosing.set(true);
    this.settlementsService
      .closeSettlementPeriod(details.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.periodDetails.set(updated);
          this.periods.update((items) =>
            items.map((item) =>
              item.id === updated.id
                ? { ...item, status: updated.status, label: updated.label }
                : item
            )
          );
          this.toastService.success(BillingMessages.settlementPeriodClosed);
          this.isClosing.set(false);
        },
        error: (error: Error) => {
          this.toastService.error(error.message);
          this.isClosing.set(false);
        }
      });
  }

  private loadPeriods(selectPeriodId?: string): void {
    this.isLoadingPeriods.set(true);
    this.errorMessage.set(null);

    this.settlementsService
      .getSettlementPeriods()
      .pipe(
        catchError((error: Error) => {
          this.errorMessage.set(error.message);
          return of([] as SettlementPeriodListItemDto[]);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((periods) => {
        this.periods.set(periods);
        this.isLoadingPeriods.set(false);

        if (periods.length === 0) {
          this.selectedPeriodIdControl.setValue(null, { emitEvent: false });
          this.periodDetails.set(null);
          return;
        }

        const targetId =
          selectPeriodId && periods.some((period) => period.id === selectPeriodId)
            ? selectPeriodId
            : periods[0].id;
        this.selectedPeriodIdControl.setValue(targetId);
      });
  }

  private loadPeriodDetails(periodId: string): void {
    this.isLoadingDetails.set(true);
    this.errorMessage.set(null);

    this.settlementsService
      .getSettlementPeriodById(periodId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.periodDetails.set(details);
          this.isLoadingDetails.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoadingDetails.set(false);
        }
      });
  }
}
