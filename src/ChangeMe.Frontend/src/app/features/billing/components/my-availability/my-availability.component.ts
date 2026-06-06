import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { DayAvailabilityPanelComponent } from '@features/billing/components/day-availability-panel/day-availability-panel.component';
import { EditAvailabilityDialogComponent } from '@features/billing/components/edit-availability-dialog/edit-availability-dialog.component';
import { EditWeeklyPatternDialogComponent } from '@features/billing/components/edit-weekly-pattern-dialog/edit-weekly-pattern-dialog.component';
import {
  AvailabilityCalendarView,
  AvailabilityEntryDto,
  AvailabilityEntrySource,
  WeeklyRecurringPatternDto
} from '@features/billing/models/availability.model';
import { BillingSettingsDto } from '@features/billing/models/billing-settings.model';
import { AvailabilityService } from '@features/billing/services/availability.service';
import { BillingSettingsService } from '@features/billing/services/billing-settings.service';
import {
  formatAvailabilityTimeRange,
  getAllDayEntriesForDay,
  getAvailabilityStatusLabel,
  getAvailabilityStatusSeverity,
  getCalendarRange,
  getEntriesForUserDay,
  getEntryBlockClass,
  getEntryBlockStyle,
  getMonthGridDates,
  getTimedEntriesForDay,
  getWeekDates,
  getWeekGridBounds,
  isInMonth,
  isTodayDate,
  isWeekendDate,
  resolveEffectiveEntry,
  shiftMonth,
  shiftWeek,
  truncateAvailabilityNotes,
  WEEK_GRID_SLOT_HEIGHT_PX
} from '@features/billing/utils/availability-calendar.utils';
import { BillingMessages, weekdayOptions } from '@features/billing/utils/billing.utils';
import { parseIsoDateString } from '@features/time/utils/time.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { SelectButton } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { catchError, finalize, forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-my-availability',
  imports: [
    DatePipe,
    FormsModule,
    Panel,
    Button,
    Message,
    Tag,
    SelectButton,
    TableModule,
    ProgressSpinner,
    DayAvailabilityPanelComponent,
    EditAvailabilityDialogComponent,
    EditWeeklyPatternDialogComponent
  ],
  templateUrl: './my-availability.component.html'
})
export class MyAvailabilityComponent {
  private readonly availabilityService = inject(AvailabilityService);
  private readonly billingSettingsService = inject(BillingSettingsService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);

  readonly BillingMessages = BillingMessages;
  readonly weekdayOptions = weekdayOptions;
  readonly viewOptions = [
    { label: 'Month', value: 'month' as AvailabilityCalendarView },
    { label: 'Week', value: 'week' as AvailabilityCalendarView }
  ];

  readonly view = signal<AvailabilityCalendarView>('month');
  readonly anchorDate = signal(new Date());
  readonly entries = signal<AvailabilityEntryDto[]>([]);
  readonly pattern = signal<WeeklyRecurringPatternDto | null>(null);
  readonly settings = signal<BillingSettingsDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly panelVisible = signal(false);
  readonly panelDate = signal<string | null>(null);
  readonly panelEntries = signal<AvailabilityEntryDto[]>([]);

  readonly availabilityDialogVisible = signal(false);
  readonly patternDialogVisible = signal(false);
  readonly editingEntry = signal<AvailabilityEntryDto | null>(null);
  readonly initialized = signal(false);

  readonly AvailabilityEntrySource = AvailabilityEntrySource;

  readonly canManage = this.authService.hasPermission(
    PermissionCodes.billingManageOwnAvailability
  );

  readonly userId = computed(() => this.authService.currentUser()?.id ?? '');
  readonly monthDates = computed(() => {
    const anchor = this.anchorDate();
    return getMonthGridDates(anchor.getFullYear(), anchor.getMonth());
  });
  readonly weekDates = computed(() => getWeekDates(this.anchorDate()));
  readonly calendarRange = computed(() =>
    getCalendarRange(this.view(), this.anchorDate())
  );
  readonly periodLabel = computed(() => {
    const anchor = this.anchorDate();
    if (this.view() === 'week') {
      const dates = this.weekDates();
      return `${dates[0]} – ${dates[dates.length - 1]}`;
    }

    return anchor.toLocaleString(undefined, { month: 'long', year: 'numeric' });
  });
  readonly organizationDefaultsLine = computed(() => {
    const currentSettings = this.settings();
    if (!currentSettings) {
      return '';
    }

    const workdays = currentSettings.defaultWorkdays
      .map((day) => weekdayOptions.find((option) => option.value === day)?.label ?? day)
      .join(', ');

    return `Organization defaults: ${workdays} ${currentSettings.defaultWorkdayStart}–${currentSettings.defaultWorkdayEnd}, ${currentSettings.defaultAvailabilityStatus}.`;
  });
  readonly weekGrid = computed(() => getWeekGridBounds(this.settings()));
  readonly weekGridHeight = computed(
    () => this.weekGrid().slots.length * WEEK_GRID_SLOT_HEIGHT_PX
  );

  readonly getAvailabilityStatusLabel = getAvailabilityStatusLabel;
  readonly getAvailabilityStatusSeverity = getAvailabilityStatusSeverity;
  readonly formatAvailabilityTimeRange = formatAvailabilityTimeRange;
  readonly truncateAvailabilityNotes = truncateAvailabilityNotes;
  readonly getEntryBlockStyle = getEntryBlockStyle;
  readonly getEntryBlockClass = getEntryBlockClass;
  readonly isTodayDate = isTodayDate;
  readonly isWeekendDate = isWeekendDate;
  readonly isInMonth = isInMonth;

  constructor() {
    const dateParam = this.route.snapshot.queryParamMap.get('date');
    if (dateParam) {
      try {
        this.anchorDate.set(parseIsoDateString(dateParam));
      } catch {
        // Ignore invalid deep links and keep the default month.
      }
    }

    this.loadData();

    effect(() => {
      if (!this.initialized()) {
        return;
      }

      this.view();
      this.anchorDate();
      this.loadCalendar();
    });
  }

  loadData(): void {
    this.isLoading.set(true);
    forkJoin({
      settings: this.billingSettingsService
        .getBillingSettings()
        .pipe(catchError(() => of(null))),
      pattern: this.availabilityService.getMyPattern()
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ settings, pattern }) => {
          this.settings.set(settings);
          this.pattern.set(pattern);
          this.initialized.set(true);
        },
        error: () => this.errorMessage.set('Unable to load availability.')
      });
  }

  loadCalendar(): void {
    const range = this.calendarRange();
    this.isLoading.set(true);
    this.availabilityService
      .getMyCalendar(range.from, range.to)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => this.entries.set(result.entries),
        error: () => this.errorMessage.set('Unable to load calendar.')
      });
  }

  setView(view: AvailabilityCalendarView): void {
    this.view.set(view);
  }

  goToday(): void {
    this.anchorDate.set(new Date());
  }

  goPrevious(): void {
    const current = this.anchorDate();
    this.anchorDate.set(
      this.view() === 'month' ? shiftMonth(current, -1) : shiftWeek(current, -1)
    );
  }

  goNext(): void {
    const current = this.anchorDate();
    this.anchorDate.set(
      this.view() === 'month' ? shiftMonth(current, 1) : shiftWeek(current, 1)
    );
  }

  openDay(date: string): void {
    const userId = this.userId();
    this.panelDate.set(date);
    this.panelEntries.set(getEntriesForUserDay(this.entries(), userId, date));
    this.panelVisible.set(true);
  }

  effectiveEntry(date: string): AvailabilityEntryDto | null {
    return resolveEffectiveEntry(this.entries(), this.userId(), date);
  }

  allDayEntries(date: string): AvailabilityEntryDto[] {
    return getAllDayEntriesForDay(this.entries(), this.userId(), date);
  }

  timedEntries(date: string): AvailabilityEntryDto[] {
    return getTimedEntriesForDay(this.entries(), this.userId(), date);
  }

  entryBlockStyle(entry: AvailabilityEntryDto): { top: string; height: string } {
    const grid = this.weekGrid();
    return getEntryBlockStyle(entry, grid.startMinutes, grid.endMinutes);
  }

  panelTitle(): string {
    const date = this.panelDate();
    if (!date) {
      return '';
    }

    return new Date(`${date}T00:00:00`).toLocaleDateString(undefined, {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });
  }

  openAddException(date?: string | null): void {
    this.editingEntry.set(null);
    if (date) {
      this.panelDate.set(date);
    }
    this.availabilityDialogVisible.set(true);
  }

  openEditEntry(entry: AvailabilityEntryDto): void {
    this.editingEntry.set(entry);
    this.availabilityDialogVisible.set(true);
  }

  openPatternDialog(): void {
    this.patternDialogVisible.set(true);
  }

  resetPattern(): void {
    this.confirmationService.confirm({
      message: BillingMessages.resetWeeklyPatternConfirm,
      ...createDestructiveConfirmationOptions('Reset'),
      accept: () => {
        this.availabilityService.resetMyPattern().subscribe({
          next: (pattern) => {
            this.pattern.set(pattern);
            this.toastService.success(BillingMessages.weeklyPatternReset);
            this.loadCalendar();
          },
          error: () => this.toastService.error('Unable to reset weekly pattern.')
        });
      }
    });
  }

  onAvailabilitySaved(): void {
    this.availabilityDialogVisible.set(false);
    this.loadCalendar();
    const date = this.panelDate();
    if (date) {
      this.availabilityService.getMyDay(date).subscribe((result) => {
        this.panelEntries.set(result.entries);
      });
    }
  }

  onPatternSaved(): void {
    this.patternDialogVisible.set(false);
    this.availabilityService
      .getMyPattern()
      .subscribe((pattern) => this.pattern.set(pattern));
    this.loadCalendar();
  }

  patternHours(day: WeeklyRecurringPatternDto['days'][number]): string {
    if (!day.enabled || !day.startTime || !day.endTime) {
      return '—';
    }

    return `${day.startTime}–${day.endTime}`;
  }
}
