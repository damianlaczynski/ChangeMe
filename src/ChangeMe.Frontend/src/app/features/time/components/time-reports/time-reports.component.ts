import { DatePipe } from '@angular/common';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { ProjectOptionDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import {
  TimeEntryAuditLogEntryDto,
  TimeEntryAuditOperation,
  TimeReportGroupingMode,
  TimeReportResultDto,
  TimeReportRowDto
} from '@features/time/models/time.model';
import { TimeService } from '@features/time/services/time.service';
import {
  ProjectTimePermissionCodes,
  TimeConstraints,
  TimeDatePresetId,
  TimeMessages,
  auditOperationOptions,
  formatReportCsvFileName,
  getAuditOperationLabel,
  getAuditOperationSeverity,
  getCurrentMonthDateRange,
  getDateRangeForPreset,
  reportDateFilterPresets,
  timeReportGroupingModes,
  toIsoDateString,
  truncateText
} from '@features/time/utils/time.utils';
import { UserListItemDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { InputNumber } from 'primeng/inputnumber';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { SelectButton } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { Tab, TabList, TabPanel, TabPanels, Tabs } from 'primeng/tabs';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { finalize } from 'rxjs';

type TimeReportsTab = 'reports' | 'audit-log' | 'settings';

type ReportFilterForm = {
  dateFrom: FormControl<Date | null>;
  dateTo: FormControl<Date | null>;
  projectIds: FormControl<string[]>;
  userIds: FormControl<string[]>;
  groupingMode: FormControl<TimeReportGroupingMode>;
};

type AuditFilterForm = {
  dateFrom: FormControl<Date | null>;
  dateTo: FormControl<Date | null>;
  actingUserId: FormControl<string | null>;
  entryAuthorUserId: FormControl<string | null>;
  projectId: FormControl<string | null>;
  operations: FormControl<TimeEntryAuditOperation[]>;
};

@Component({
  selector: 'app-time-reports',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    Panel,
    DatePicker,
    MultiSelect,
    Select,
    SelectButton,
    InputNumber,
    TableModule,
    Message,
    Tag,
    Tooltip,
    ProgressSpinner,
    Tabs,
    TabList,
    Tab,
    TabPanels,
    TabPanel
  ],
  templateUrl: './time-reports.component.html'
})
export class TimeReportsComponent {
  private readonly timeService = inject(TimeService);
  private readonly projectsService = inject(ProjectsService);
  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly TimeMessages = TimeMessages;
  readonly reportDateFilterPresets = reportDateFilterPresets;
  readonly timeReportGroupingModes = timeReportGroupingModes;
  readonly auditOperationOptions = auditOperationOptions;
  readonly TimeEntryAuditOperation = TimeEntryAuditOperation;
  readonly getAuditOperationLabel = getAuditOperationLabel;
  readonly getAuditOperationSeverity = getAuditOperationSeverity;
  readonly TimeReportGroupingMode = TimeReportGroupingMode;
  readonly truncateDescription = (value: string) => truncateText(value, 60);

  readonly activeTab = signal<TimeReportsTab>('reports');
  readonly reportResult = signal<TimeReportResultDto | null>(null);
  readonly isRunningReport = signal(false);
  readonly isExporting = signal(false);
  readonly reportDateRangeError = signal<string | null>(null);
  readonly auditDateRangeError = signal<string | null>(null);
  readonly reportError = signal<string | null>(null);

  readonly projectOptions = signal<ProjectOptionDto[]>([]);
  readonly userOptions = signal<UserListItemDto[]>([]);
  readonly isLoadingProjects = signal(false);
  readonly isLoadingUsers = signal(false);

  readonly auditEntries = signal<TimeEntryAuditLogEntryDto[]>([]);
  readonly auditPagination = signal<PaginationResult<TimeEntryAuditLogEntryDto> | null>(
    null
  );
  readonly isLoadingAudit = signal(false);
  readonly isLoadingMoreAudit = signal(false);
  readonly auditError = signal<string | null>(null);
  readonly auditFiltersCollapsed = signal(false);
  readonly expandedAuditIds = signal<string[]>([]);

  readonly settingsBackdatingLimit = signal<number | null>(null);
  readonly settingsCanEdit = signal(false);
  readonly isLoadingSettings = signal(false);
  readonly isSavingSettings = signal(false);
  readonly settingsError = signal<string | null>(null);

  readonly expandedPersonUserId = signal<string | null>(null);
  readonly personEntries = signal<
    Array<{
      workDate: string;
      projectName: string;
      issueId?: string | null;
      issueTitle?: string | null;
      durationFormatted: string;
      description: string;
    }>
  >([]);
  readonly isLoadingPersonEntries = signal(false);
  readonly personEntriesHasNext = signal(false);
  private personEntriesPage = 1;

  readonly canManageSettings = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  readonly reportFilterForm = new FormGroup<ReportFilterForm>({
    dateFrom: new FormControl<Date | null>(getCurrentMonthDateRange().dateFrom, {
      nonNullable: true
    }),
    dateTo: new FormControl<Date | null>(getCurrentMonthDateRange().dateTo, {
      nonNullable: true
    }),
    projectIds: new FormControl<string[]>([], { nonNullable: true }),
    userIds: new FormControl<string[]>([], { nonNullable: true }),
    groupingMode: new FormControl(TimeReportGroupingMode.ByPerson, {
      nonNullable: true
    })
  });

  readonly auditFilterForm = new FormGroup<AuditFilterForm>({
    dateFrom: new FormControl<Date | null>(getCurrentMonthDateRange().dateFrom),
    dateTo: new FormControl<Date | null>(getCurrentMonthDateRange().dateTo),
    actingUserId: new FormControl<string | null>(null),
    entryAuthorUserId: new FormControl<string | null>(null),
    projectId: new FormControl<string | null>(null),
    operations: new FormControl<TimeEntryAuditOperation[]>([], { nonNullable: true })
  });

  readonly settingsForm = new FormGroup({
    backdatingLimitDays: new FormControl<number>(0, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(TimeConstraints.MIN_BACKDATING_LIMIT_DAYS),
        Validators.max(TimeConstraints.MAX_BACKDATING_LIMIT_DAYS)
      ]
    })
  });

  constructor() {
    this.loadProjectOptions();
    this.loadUserOptions();

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const tab = params.get('tab');
        this.activeTab.set(
          tab === 'audit-log'
            ? 'audit-log'
            : tab === 'settings'
              ? 'settings'
              : 'reports'
        );

        const projectIds = [
          ...params.getAll('projectIds'),
          ...(params.get('projectId') ? [params.get('projectId')!] : [])
        ];
        if (projectIds.length > 0) {
          this.reportFilterForm.patchValue({ projectIds });
        }
      });

    this.loadSettings();
  }

  onTabChange(tab: string | number | undefined): void {
    const value: TimeReportsTab =
      tab === 'audit-log' ? 'audit-log' : tab === 'settings' ? 'settings' : 'reports';
    if (this.activeTab() === value) {
      return;
    }

    this.activeTab.set(value);

    void this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        tab: value === 'reports' ? null : value
      },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });

    if (value === 'audit-log' && this.auditEntries().length === 0) {
      this.loadAuditLog(false);
    }
  }

  applyReportDatePreset(preset: TimeDatePresetId): void {
    const range = getDateRangeForPreset(preset);
    this.reportFilterForm.patchValue({
      dateFrom: range.dateFrom,
      dateTo: range.dateTo
    });
  }

  applyAuditDatePreset(preset: TimeDatePresetId): void {
    const range = getDateRangeForPreset(preset);
    this.auditFilterForm.patchValue({
      dateFrom: range.dateFrom,
      dateTo: range.dateTo
    });
  }

  runReport(): void {
    if (!this.validateReportDateRange()) {
      return;
    }

    this.isRunningReport.set(true);
    this.reportError.set(null);
    this.reportResult.set(null);
    this.expandedPersonUserId.set(null);

    const formValue = this.reportFilterForm.getRawValue();

    this.timeService
      .getTimeReports({
        dateFrom: toIsoDateString(formValue.dateFrom!),
        dateTo: toIsoDateString(formValue.dateTo!),
        projectIds: formValue.projectIds.length > 0 ? formValue.projectIds : undefined,
        userIds: formValue.userIds.length > 0 ? formValue.userIds : undefined,
        groupingMode: formValue.groupingMode
      })
      .pipe(
        finalize(() => this.isRunningReport.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => this.reportResult.set(result),
        error: (error: Error) => this.reportError.set(error.message)
      });
  }

  exportReport(): void {
    if (!this.reportResult() || this.isExporting()) {
      return;
    }

    const formValue = this.reportFilterForm.getRawValue();
    this.isExporting.set(true);

    this.timeService
      .exportTimeReports({
        dateFrom: toIsoDateString(formValue.dateFrom!),
        dateTo: toIsoDateString(formValue.dateTo!),
        projectIds: formValue.projectIds.length > 0 ? formValue.projectIds : undefined,
        userIds: formValue.userIds.length > 0 ? formValue.userIds : undefined,
        groupingMode: formValue.groupingMode
      })
      .pipe(
        finalize(() => this.isExporting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (blob) => {
          const url = URL.createObjectURL(blob);
          const anchor = document.createElement('a');
          anchor.href = url;
          anchor.download = formatReportCsvFileName();
          anchor.click();
          URL.revokeObjectURL(url);
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not export report')
      });
  }

  togglePersonRow(row: TimeReportRowDto): void {
    if (!row.userId) {
      return;
    }

    if (this.expandedPersonUserId() === row.userId) {
      this.expandedPersonUserId.set(null);
      this.personEntries.set([]);
      return;
    }

    this.expandedPersonUserId.set(row.userId);
    this.personEntriesPage = 1;
    this.loadPersonEntries(row.userId, false);
  }

  showMorePersonEntries(): void {
    const userId = this.expandedPersonUserId();
    if (!userId || !this.personEntriesHasNext() || this.isLoadingPersonEntries()) {
      return;
    }

    this.personEntriesPage += 1;
    this.loadPersonEntries(userId, true);
  }

  applyAuditFilters(): void {
    if (!this.validateAuditDateRange()) {
      return;
    }

    this.loadAuditLog(false);
  }

  clearAuditFilters(): void {
    const range = getCurrentMonthDateRange();
    this.auditFilterForm.reset({
      dateFrom: range.dateFrom,
      dateTo: range.dateTo,
      actingUserId: null,
      entryAuthorUserId: null,
      projectId: null,
      operations: []
    });
    this.auditDateRangeError.set(null);
    this.loadAuditLog(false);
  }

  showMoreAudit(): void {
    const pagination = this.auditPagination();
    if (!pagination?.hasNext || this.isLoadingMoreAudit()) {
      return;
    }

    this.loadAuditLog(true, pagination.currentPage + 1);
  }

  toggleAuditExpanded(entryId: string): void {
    this.expandedAuditIds.update((ids) =>
      ids.includes(entryId) ? ids.filter((id) => id !== entryId) : [...ids, entryId]
    );
  }

  isAuditExpanded(entryId: string): boolean {
    return this.expandedAuditIds().includes(entryId);
  }

  saveSettings(): void {
    this.settingsForm.markAllAsTouched();
    if (this.settingsForm.invalid || !this.canManageSettings()) {
      return;
    }

    this.isSavingSettings.set(true);
    this.settingsError.set(null);

    this.timeService
      .updateTimeSettings({
        backdatingLimitDays: this.settingsForm.controls.backdatingLimitDays.value
      })
      .pipe(
        finalize(() => this.isSavingSettings.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (settings) => {
          this.settingsBackdatingLimit.set(settings.backdatingLimitDays);
          this.settingsCanEdit.set(settings.canEdit);
          this.toastService.success(TimeMessages.timeSettingsSaved);
        },
        error: (error: Error) => this.settingsError.set(error.message)
      });
  }

  private loadPersonEntries(userId: string, append: boolean): void {
    const formValue = this.reportFilterForm.getRawValue();
    this.isLoadingPersonEntries.set(true);

    this.timeService
      .getReportPersonEntries({
        userId,
        pageNumber: this.personEntriesPage,
        pageSize: TimeConstraints.REPORT_PERSON_ENTRIES_PAGE_SIZE,
        sortField: 'WorkDate',
        ascending: false,
        dateFrom: toIsoDateString(formValue.dateFrom!),
        dateTo: toIsoDateString(formValue.dateTo!),
        projectIds: formValue.projectIds.length > 0 ? formValue.projectIds : undefined
      })
      .pipe(
        finalize(() => this.isLoadingPersonEntries.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          const mapped = result.items.map((entry) => ({
            workDate: entry.workDate,
            projectName: entry.projectName,
            issueId: entry.issueId,
            issueTitle: entry.issueTitle,
            durationFormatted: entry.durationFormatted,
            description: entry.description
          }));

          this.personEntries.update((current) =>
            append ? [...current, ...mapped] : mapped
          );
          this.personEntriesHasNext.set(result.hasNext);
        },
        error: (error: Error) => {
          this.personEntries.set([]);
          this.personEntriesHasNext.set(false);
          this.toastService.showApiError(error, 'Could not load person time details');
        }
      });
  }

  private loadAuditLog(append: boolean, pageNumber = 1): void {
    if (!this.validateAuditDateRange()) {
      return;
    }

    if (append) {
      this.isLoadingMoreAudit.set(true);
    } else {
      this.isLoadingAudit.set(true);
      this.auditError.set(null);
    }

    const formValue = this.auditFilterForm.getRawValue();

    this.timeService
      .getTimeAuditLog({
        pageNumber,
        pageSize: TimeConstraints.AUDIT_LOG_PAGE_SIZE,
        dateFrom: formValue.dateFrom ? toIsoDateString(formValue.dateFrom) : undefined,
        dateTo: formValue.dateTo ? toIsoDateString(formValue.dateTo) : undefined,
        actingUserId: formValue.actingUserId,
        entryAuthorUserId: formValue.entryAuthorUserId,
        projectId: formValue.projectId,
        operations: formValue.operations.length > 0 ? formValue.operations : undefined
      })
      .pipe(
        finalize(() => {
          this.isLoadingAudit.set(false);
          this.isLoadingMoreAudit.set(false);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (result) => {
          this.auditEntries.update((current) =>
            append ? [...current, ...result.items] : result.items
          );
          this.auditPagination.set(result);
        },
        error: (error: Error) => this.auditError.set(error.message)
      });
  }

  private loadSettings(): void {
    this.isLoadingSettings.set(true);

    this.timeService
      .getTimeSettings()
      .pipe(
        finalize(() => this.isLoadingSettings.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (settings) => {
          this.settingsBackdatingLimit.set(settings.backdatingLimitDays);
          this.settingsCanEdit.set(settings.canEdit);
          this.settingsForm.patchValue({
            backdatingLimitDays: settings.backdatingLimitDays
          });

          if (!settings.canEdit) {
            this.settingsForm.disable();
          } else {
            this.settingsForm.enable();
          }
        },
        error: (error: Error) => this.settingsError.set(error.message)
      });
  }

  private loadProjectOptions(): void {
    this.isLoadingProjects.set(true);

    this.projectsService
      .getManageableProjects(ProjectTimePermissionCodes.timeView)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (projects) => {
          this.projectOptions.set(projects);
          this.isLoadingProjects.set(false);
        },
        error: () => this.isLoadingProjects.set(false)
      });
  }

  private loadUserOptions(): void {
    this.isLoadingUsers.set(true);

    this.usersService
      .getUsers({
        pageNumber: 1,
        pageSize: 200,
        sortField: 'LastName',
        ascending: true
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.userOptions.set(result.items.filter((user) => !user.deactivated));
          this.isLoadingUsers.set(false);
        },
        error: () => this.isLoadingUsers.set(false)
      });
  }

  private validateReportDateRange(): boolean {
    const { dateFrom, dateTo } = this.reportFilterForm.getRawValue();
    if (dateFrom && dateTo && dateFrom > dateTo) {
      this.reportDateRangeError.set(TimeMessages.dateRangeInvalid);
      return false;
    }

    this.reportDateRangeError.set(null);
    return true;
  }

  private validateAuditDateRange(): boolean {
    const { dateFrom, dateTo } = this.auditFilterForm.getRawValue();
    if (dateFrom && dateTo && dateFrom > dateTo) {
      this.auditDateRangeError.set(TimeMessages.dateRangeInvalid);
      return false;
    }

    this.auditDateRangeError.set(null);
    return true;
  }
}
