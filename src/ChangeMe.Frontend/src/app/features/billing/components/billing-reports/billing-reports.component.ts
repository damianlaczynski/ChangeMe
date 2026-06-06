import { DatePipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { LeaveTypeDialogComponent } from '@features/billing/components/leave-type-dialog/leave-type-dialog.component';
import {
  BillingLeaveReportGroupingMode,
  BillingLeaveReportResultDto,
  BillingSettlementReportGroupingMode,
  BillingSettlementReportResultDto,
  SettlementOperationLogListItemDto
} from '@features/billing/models/billing-report.model';
import {
  AvailabilityStatus,
  BillingSettingsDto,
  DayOfWeek
} from '@features/billing/models/billing-settings.model';
import { ContractType } from '@features/billing/models/employment.model';
import { LeaveRequestStatus } from '@features/billing/models/leave-request.model';
import { LeaveTypeListItemDto } from '@features/billing/models/leave-type.model';
import {
  SettlementPeriodListItemDto,
  UserSettlementListItemDto
} from '@features/billing/models/settlement.model';
import { BillingReportsService } from '@features/billing/services/billing-reports.service';
import { BillingSettingsService } from '@features/billing/services/billing-settings.service';
import { LeaveTypesService } from '@features/billing/services/leave-types.service';
import { SettlementsService } from '@features/billing/services/settlements.service';
import {
  billingLeaveGroupingModes,
  billingSettlementGroupingModes,
  exportLeaveReportCsv,
  exportSettlementReportCsv,
  getSettlementOperationLabel,
  pickDefaultSettlementPeriodId
} from '@features/billing/utils/billing-report.utils';
import {
  BillingMessages,
  BillingSettingsConstraints,
  availabilityStatusOptions,
  compareTimeStrings,
  contractTypeOptions,
  formatBooleanLabel,
  formatMonthlyHoursNorm,
  formatWorkdayDuration,
  getContractTypeLabel,
  getSettlementBalanceClass,
  getSettlementBalanceSeverity,
  isValidTimeString,
  leaveRequestStatusOptions,
  weekdayOptions
} from '@features/billing/utils/billing.utils';
import { UserListItemDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { Tag } from 'primeng/tag';
import { ToggleSwitch } from 'primeng/toggleswitch';
import { catchError, of, switchMap, tap } from 'rxjs';

type BillingReportsTab = 'reports' | 'leave' | 'audit-log' | 'settings';

@Component({
  selector: 'app-billing-reports',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Card,
    Button,
    Panel,
    TableModule,
    Message,
    Tag,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel,
    InputNumber,
    InputText,
    MultiSelect,
    Select,
    SelectButton,
    Paginator,
    ToggleSwitch,
    LeaveTypeDialogComponent
  ],
  templateUrl: './billing-reports.component.html'
})
export class BillingReportsComponent {
  private readonly billingReportsService = inject(BillingReportsService);
  private readonly settlementsService = inject(SettlementsService);
  private readonly billingSettingsService = inject(BillingSettingsService);
  private readonly leaveTypesService = inject(LeaveTypesService);
  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly weekdayOptions = weekdayOptions;
  readonly availabilityStatusOptions = availabilityStatusOptions;
  readonly formatBooleanLabel = formatBooleanLabel;
  readonly formatWorkdayDuration = formatWorkdayDuration;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getContractTypeLabel = getContractTypeLabel;
  readonly getSettlementBalanceSeverity = getSettlementBalanceSeverity;
  readonly getSettlementBalanceClass = getSettlementBalanceClass;
  readonly getSettlementOperationLabel = getSettlementOperationLabel;
  readonly BillingSettingsConstraints = BillingSettingsConstraints;
  readonly billingSettlementGroupingModes = billingSettlementGroupingModes;
  readonly billingLeaveGroupingModes = billingLeaveGroupingModes;
  readonly contractTypeOptions = contractTypeOptions;
  readonly leaveRequestStatusOptions = leaveRequestStatusOptions;
  readonly BillingSettlementReportGroupingMode = BillingSettlementReportGroupingMode;
  readonly BillingLeaveReportGroupingMode = BillingLeaveReportGroupingMode;

  readonly activeTab = signal<BillingReportsTab>('reports');
  readonly settlementPeriods = signal<SettlementPeriodListItemDto[]>([]);
  readonly periodUserOptions = signal<UserSettlementListItemDto[]>([]);
  readonly userOptions = signal<{ id: string; label: string }[]>([]);
  readonly settlementReport = signal<BillingSettlementReportResultDto | null>(null);
  readonly leaveReport = signal<BillingLeaveReportResultDto | null>(null);
  readonly auditLog =
    signal<PaginationResult<SettlementOperationLogListItemDto> | null>(null);
  readonly isLoadingSettlementReport = signal(false);
  readonly isLoadingLeaveReport = signal(false);
  readonly isLoadingAuditLog = signal(false);
  readonly settlementReportError = signal<string | null>(null);
  readonly leaveReportError = signal<string | null>(null);
  readonly auditLogError = signal<string | null>(null);
  readonly hasRunSettlementReport = signal(false);
  readonly hasRunLeaveReport = signal(false);
  readonly leaveTypes = signal<LeaveTypeListItemDto[]>([]);
  readonly isLoadingLeaveTypes = signal(false);
  readonly leaveTypesError = signal<string | null>(null);
  readonly settings = signal<BillingSettingsDto | null>(null);
  readonly isLoadingSettings = signal(false);
  readonly settingsError = signal<string | null>(null);
  readonly isSavingSettings = signal(false);
  readonly settingsSubmitError = signal<string | null>(null);
  readonly leaveTypeDialogVisible = signal(false);
  readonly editingLeaveType = signal<LeaveTypeListItemDto | null>(null);

  readonly canManageSettings = computed(() =>
    this.authService.hasPermission(PermissionCodes.billingManageSettlements)
  );

  readonly settlementFiltersForm = new FormGroup({
    settlementPeriodId: new FormControl<string | null>(null, Validators.required),
    userIds: new FormControl<string[]>([], { nonNullable: true }),
    contractTypes: new FormControl<ContractType[]>([], { nonNullable: true }),
    groupingMode: new FormControl(BillingSettlementReportGroupingMode.ByPerson, {
      nonNullable: true
    })
  });

  readonly leaveFiltersForm = new FormGroup({
    year: new FormControl(new Date().getFullYear(), {
      nonNullable: true,
      validators: [Validators.required]
    }),
    userIds: new FormControl<string[]>([], { nonNullable: true }),
    leaveTypeIds: new FormControl<string[]>([], { nonNullable: true }),
    statuses: new FormControl<LeaveRequestStatus[]>([LeaveRequestStatus.Approved], {
      nonNullable: true
    }),
    groupingMode: new FormControl(BillingLeaveReportGroupingMode.ByPerson, {
      nonNullable: true
    })
  });

  readonly auditFiltersForm = new FormGroup({
    settlementPeriodId: new FormControl<string | null>(null)
  });

  private readonly auditQuery = signal({
    pageNumber: 1,
    pageSize: 20,
    settlementPeriodId: undefined as string | undefined
  });

  readonly settingsForm = new FormGroup({
    defaultAnnualLeaveDays: new FormControl<number>(26, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(BillingSettingsConstraints.MIN_ANNUAL_LEAVE_DAYS),
        Validators.max(BillingSettingsConstraints.MAX_ANNUAL_LEAVE_DAYS)
      ]
    }),
    allowHalfDayLeave: new FormControl(true, { nonNullable: true }),
    defaultWorkdayStart: new FormControl('09:00', { nonNullable: true }),
    defaultWorkdayEnd: new FormControl('17:00', { nonNullable: true }),
    halfDaySplitTime: new FormControl('13:00', { nonNullable: true }),
    defaultWorkdays: new FormControl<DayOfWeek[]>(
      [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday
      ],
      { nonNullable: true, validators: [Validators.required] }
    ),
    defaultAvailabilityStatus: new FormControl<AvailabilityStatus>(
      AvailabilityStatus.OnSite,
      {
        nonNullable: true,
        validators: [Validators.required]
      }
    )
  });

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const tab = params.get('tab');
        if (
          tab === 'reports' ||
          tab === 'leave' ||
          tab === 'audit-log' ||
          tab === 'settings'
        ) {
          this.activeTab.set(tab);
          if (tab === 'settings') {
            this.loadSettingsTab();
          } else if (tab === 'audit-log') {
            this.refreshAuditLog();
          }
        }
      });

    this.loadSettlementPeriods();
    this.loadUserOptions();
    this.loadLeaveTypes();
    this.loadSettingsTab();

    this.settlementFiltersForm.controls.settlementPeriodId.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((periodId) => {
        if (periodId) {
          this.loadPeriodUsers(periodId);
        } else {
          this.periodUserOptions.set([]);
        }
      });

    toObservable(this.auditQuery)
      .pipe(
        tap(() => {
          this.isLoadingAuditLog.set(true);
          this.auditLogError.set(null);
        }),
        switchMap((params) =>
          this.billingReportsService.getSettlementOperationHistory(params).pipe(
            catchError((error: Error) => {
              this.auditLogError.set(error.message);
              return of(
                createEmptyPaginationResult<SettlementOperationLogListItemDto>(params)
              );
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.auditLog.set(result);
        this.isLoadingAuditLog.set(false);
      });
  }

  onTabChange(tab: string | number | undefined): void {
    if (tab === undefined) {
      return;
    }

    const value = String(tab) as BillingReportsTab;
    this.activeTab.set(value);
    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { tab: value },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });

    if (value === 'settings') {
      this.loadSettingsTab();
    } else if (value === 'audit-log') {
      this.refreshAuditLog();
    }
  }

  runSettlementReport(): void {
    if (this.settlementFiltersForm.invalid) {
      this.settlementFiltersForm.markAllAsTouched();
      return;
    }

    const raw = this.settlementFiltersForm.getRawValue();
    if (!raw.settlementPeriodId) {
      return;
    }

    this.isLoadingSettlementReport.set(true);
    this.settlementReportError.set(null);

    this.billingReportsService
      .getSettlementReport({
        settlementPeriodId: raw.settlementPeriodId,
        userIds: raw.userIds.length > 0 ? raw.userIds : undefined,
        contractTypes: raw.contractTypes.length > 0 ? raw.contractTypes : undefined,
        groupingMode: raw.groupingMode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (report) => {
          this.settlementReport.set(report);
          this.hasRunSettlementReport.set(true);
          this.isLoadingSettlementReport.set(false);
        },
        error: (error: Error) => {
          this.settlementReportError.set(error.message);
          this.isLoadingSettlementReport.set(false);
        }
      });
  }

  exportSettlementReport(): void {
    const report = this.settlementReport();
    if (!report) {
      return;
    }

    exportSettlementReportCsv(report);
    this.toastService.success(BillingMessages.reportExported);
  }

  runLeaveReport(): void {
    if (this.leaveFiltersForm.invalid) {
      this.leaveFiltersForm.markAllAsTouched();
      return;
    }

    const raw = this.leaveFiltersForm.getRawValue();
    this.isLoadingLeaveReport.set(true);
    this.leaveReportError.set(null);

    this.billingReportsService
      .getLeaveReport({
        year: raw.year,
        userIds: raw.userIds.length > 0 ? raw.userIds : undefined,
        leaveTypeIds: raw.leaveTypeIds.length > 0 ? raw.leaveTypeIds : undefined,
        statuses: raw.statuses.length > 0 ? raw.statuses : undefined,
        groupingMode: raw.groupingMode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (report) => {
          this.leaveReport.set(report);
          this.hasRunLeaveReport.set(true);
          this.isLoadingLeaveReport.set(false);
        },
        error: (error: Error) => {
          this.leaveReportError.set(error.message);
          this.isLoadingLeaveReport.set(false);
        }
      });
  }

  exportLeaveReport(): void {
    const report = this.leaveReport();
    if (!report) {
      return;
    }

    exportLeaveReportCsv(report);
    this.toastService.success(BillingMessages.reportExported);
  }

  applyAuditFilters(): void {
    const periodId =
      this.auditFiltersForm.controls.settlementPeriodId.value ?? undefined;
    this.auditQuery.update((current) => ({
      ...current,
      pageNumber: 1,
      settlementPeriodId: periodId
    }));
  }

  onAuditPageChange(event: PaginatorState): void {
    this.auditQuery.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
  }

  userDisplayLabel(user: UserListItemDto): string {
    const name = `${user.firstName} ${user.lastName}`.trim();
    return name || user.email;
  }

  periodUserLabel(user: UserSettlementListItemDto): string {
    return user.userDisplayName;
  }

  openCreateLeaveType(): void {
    this.editingLeaveType.set(null);
    this.leaveTypeDialogVisible.set(true);
  }

  openEditLeaveType(leaveType: LeaveTypeListItemDto): void {
    this.editingLeaveType.set(leaveType);
    this.leaveTypeDialogVisible.set(true);
  }

  confirmDeleteLeaveType(leaveType: LeaveTypeListItemDto): void {
    this.confirmationService.confirm({
      header: 'Delete leave type',
      message: BillingMessages.deleteLeaveTypeConfirm(leaveType.name),
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deleteLeaveType(leaveType.id)
    });
  }

  onLeaveTypeSaved(): void {
    this.loadLeaveTypes();
  }

  saveSettings(): void {
    this.settingsSubmitError.set(null);

    if (this.settingsForm.invalid) {
      this.settingsForm.markAllAsTouched();
      return;
    }

    const raw = this.settingsForm.getRawValue();
    if (!this.isOneDecimal(raw.defaultAnnualLeaveDays)) {
      this.settingsSubmitError.set(BillingMessages.annualLeaveDaysInvalid);
      return;
    }

    if (
      !isValidTimeString(raw.defaultWorkdayStart) ||
      !isValidTimeString(raw.defaultWorkdayEnd) ||
      !isValidTimeString(raw.halfDaySplitTime)
    ) {
      this.settingsSubmitError.set(BillingMessages.invalidTime);
      return;
    }

    if (compareTimeStrings(raw.defaultWorkdayStart, raw.defaultWorkdayEnd) <= 0) {
      this.settingsSubmitError.set(BillingMessages.workdayEndBeforeStart);
      return;
    }

    if (
      compareTimeStrings(raw.defaultWorkdayStart, raw.halfDaySplitTime) <= 0 ||
      compareTimeStrings(raw.halfDaySplitTime, raw.defaultWorkdayEnd) <= 0
    ) {
      this.settingsSubmitError.set(BillingMessages.halfDaySplitOutOfRange);
      return;
    }

    if (raw.defaultWorkdays.length === 0) {
      this.settingsSubmitError.set(BillingMessages.defaultWorkdaysRequired);
      return;
    }

    this.isSavingSettings.set(true);
    this.billingSettingsService
      .updateBillingSettings({
        defaultAnnualLeaveDays: raw.defaultAnnualLeaveDays,
        allowHalfDayLeave: raw.allowHalfDayLeave,
        defaultWorkdayStart: raw.defaultWorkdayStart.trim(),
        defaultWorkdayEnd: raw.defaultWorkdayEnd.trim(),
        halfDaySplitTime: raw.halfDaySplitTime.trim(),
        defaultWorkdays: raw.defaultWorkdays,
        defaultAvailabilityStatus: raw.defaultAvailabilityStatus
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (updated) => {
          this.settings.set(updated);
          this.toastService.success(BillingMessages.billingSettingsSaved);
          this.isSavingSettings.set(false);
        },
        error: (error: Error) => {
          this.settingsSubmitError.set(error.message);
          this.isSavingSettings.set(false);
        }
      });
  }

  private loadSettlementPeriods(): void {
    this.settlementsService
      .getSettlementPeriods()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (periods) => {
          this.settlementPeriods.set(periods);
          const defaultId = pickDefaultSettlementPeriodId(periods);
          if (
            defaultId &&
            !this.settlementFiltersForm.controls.settlementPeriodId.value
          ) {
            this.settlementFiltersForm.controls.settlementPeriodId.setValue(defaultId);
          }
        }
      });
  }

  private loadPeriodUsers(periodId: string): void {
    this.settlementsService
      .getSettlementPeriodById(periodId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (period) => this.periodUserOptions.set(period.userSettlements)
      });
  }

  private loadUserOptions(): void {
    this.usersService
      .getUsers({
        pageNumber: 1,
        pageSize: 200,
        sortField: 'LastName',
        ascending: true
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) =>
          this.userOptions.set(
            result.items
              .filter((user) => !user.deactivated)
              .map((user) => ({
                id: user.id,
                label: this.userDisplayLabel(user)
              }))
          )
      });
  }

  private refreshAuditLog(): void {
    this.auditQuery.update((current) => ({ ...current }));
  }

  private loadLeaveTypes(): void {
    this.isLoadingLeaveTypes.set(true);
    this.leaveTypesError.set(null);

    this.leaveTypesService
      .getLeaveTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (items) => {
          this.leaveTypes.set(items);
          this.isLoadingLeaveTypes.set(false);
        },
        error: (error: Error) => {
          this.leaveTypesError.set(error.message);
          this.isLoadingLeaveTypes.set(false);
        }
      });
  }

  private loadSettingsTab(): void {
    if (this.settings()) {
      return;
    }

    this.isLoadingSettings.set(true);
    this.settingsError.set(null);

    this.billingSettingsService
      .getBillingSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.settings.set(settings);
          this.settingsForm.reset({
            defaultAnnualLeaveDays: settings.defaultAnnualLeaveDays,
            allowHalfDayLeave: settings.allowHalfDayLeave,
            defaultWorkdayStart: settings.defaultWorkdayStart,
            defaultWorkdayEnd: settings.defaultWorkdayEnd,
            halfDaySplitTime: settings.halfDaySplitTime,
            defaultWorkdays: [...settings.defaultWorkdays],
            defaultAvailabilityStatus: settings.defaultAvailabilityStatus
          });
          if (!settings.canEdit) {
            this.settingsForm.disable();
          }
          this.isLoadingSettings.set(false);
        },
        error: (error: Error) => {
          this.settingsError.set(error.message);
          this.isLoadingSettings.set(false);
        }
      });
  }

  private deleteLeaveType(id: string): void {
    this.leaveTypesService
      .deleteLeaveType(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.leaveTypeDeleted);
          this.loadLeaveTypes();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private isOneDecimal(value: number): boolean {
    return Math.round(value * 10) / 10 === value;
  }
}
