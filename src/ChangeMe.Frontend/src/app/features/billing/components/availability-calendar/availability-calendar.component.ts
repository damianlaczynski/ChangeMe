import { DatePipe } from '@angular/common';
import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '@features/auth/services/auth.service';
import { DayAvailabilityPanelComponent } from '@features/billing/components/day-availability-panel/day-availability-panel.component';
import { EditAvailabilityDialogComponent } from '@features/billing/components/edit-availability-dialog/edit-availability-dialog.component';
import { EditWeeklyPatternDialogComponent } from '@features/billing/components/edit-weekly-pattern-dialog/edit-weekly-pattern-dialog.component';
import {
  AvailabilityCalendarDensity,
  AvailabilityCalendarResultDto,
  AvailabilityCalendarUserDto,
  AvailabilityCalendarView,
  AvailabilityEntryDto,
  WeeklyRecurringPatternDto
} from '@features/billing/models/availability.model';
import { AvailabilityService } from '@features/billing/services/availability.service';
import {
  countAvailableDays,
  countDayEntries,
  countTeamDaySummary,
  formatAvailabilityTimeRange,
  getAvailabilityEntryTooltip,
  getAvailabilityStatusLabel,
  getAvailabilityStatusSeverity,
  getCalendarRange,
  getEntriesForUserDay,
  getMonthGridDates,
  getWeekDates,
  isEffectiveInStatus,
  isInMonth,
  isTodayDate,
  isWeekendDate,
  resolveEffectiveEntry,
  shiftMonth,
  shiftWeek
} from '@features/billing/utils/availability-calendar.utils';
import {
  BillingMessages,
  availabilityStatusFilterOptions,
  weekdayOptions
} from '@features/billing/utils/billing.utils';
import { ProjectListItemDto } from '@features/projects/models/project.model';
import { ProjectsService } from '@features/projects/services/projects.service';
import { UserListItemDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { ProgressSpinner } from 'primeng/progressspinner';
import { SelectButton } from 'primeng/selectbutton';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { finalize } from 'rxjs';

const VIEW_STORAGE_KEY = 'billing.availability-calendar.view';
const DENSITY_STORAGE_KEY = 'billing.availability-calendar.density';

@Component({
  selector: 'app-availability-calendar',
  imports: [
    DatePipe,
    FormsModule,
    Button,
    Message,
    Tag,
    Tooltip,
    MultiSelect,
    SelectButton,
    ProgressSpinner,
    DayAvailabilityPanelComponent,
    EditAvailabilityDialogComponent,
    EditWeeklyPatternDialogComponent
  ],
  templateUrl: './availability-calendar.component.html'
})
export class AvailabilityCalendarComponent {
  private readonly availabilityService = inject(AvailabilityService);
  private readonly usersService = inject(UsersService);
  private readonly projectsService = inject(ProjectsService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly BillingMessages = BillingMessages;
  readonly weekdayOptions = weekdayOptions;
  readonly statusFilterOptions = availabilityStatusFilterOptions;
  readonly viewOptions = [
    { label: 'Month', value: 'month' as AvailabilityCalendarView },
    { label: 'Week', value: 'week' as AvailabilityCalendarView }
  ];
  readonly densityOptions = [
    { label: 'Compact', value: 'compact' as AvailabilityCalendarDensity },
    { label: 'Standard', value: 'standard' as AvailabilityCalendarDensity }
  ];

  readonly view = signal<AvailabilityCalendarView>(
    (localStorage.getItem(VIEW_STORAGE_KEY) as AvailabilityCalendarView) || 'month'
  );
  readonly density = signal<AvailabilityCalendarDensity>(
    (localStorage.getItem(DENSITY_STORAGE_KEY) as AvailabilityCalendarDensity) ||
      'standard'
  );
  readonly anchorDate = signal(new Date());
  readonly selectedUserIds = signal<string[]>([]);
  readonly selectedProjectIds = signal<string[]>([]);
  readonly selectedStatuses = signal<string[]>([]);
  readonly users = signal<UserListItemDto[]>([]);
  readonly projects = signal<ProjectListItemDto[]>([]);
  readonly calendar = signal<AvailabilityCalendarResultDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly panelVisible = signal(false);
  readonly panelUser = signal<AvailabilityCalendarUserDto | null>(null);
  readonly panelDate = signal<string | null>(null);
  readonly panelEntries = signal<AvailabilityEntryDto[]>([]);
  readonly panelPattern = signal<WeeklyRecurringPatternDto | null>(null);

  readonly availabilityDialogVisible = signal(false);
  readonly availabilityDialogMode = signal<'header' | 'panel'>('panel');
  readonly patternDialogVisible = signal(false);
  readonly editingEntry = signal<AvailabilityEntryDto | null>(null);

  readonly canManageAny = this.authService.hasPermission(
    PermissionCodes.billingManageAvailability
  );
  readonly canManageOwn = this.authService.hasPermission(
    PermissionCodes.billingManageOwnAvailability
  );
  readonly currentUserId = this.authService.currentUser()?.id ?? '';

  readonly monthDates = computed(() => {
    const anchor = this.anchorDate();
    return getMonthGridDates(anchor.getFullYear(), anchor.getMonth());
  });
  readonly weekDates = computed(() => getWeekDates(this.anchorDate()));
  readonly calendarUsers = computed(() => this.calendar()?.users ?? []);
  readonly entries = computed(() => this.calendar()?.entries ?? []);
  readonly calendarRange = computed(() =>
    getCalendarRange(this.view(), this.anchorDate())
  );
  readonly userPickerOptions = computed(() =>
    this.users().map((user) => ({
      label: `${user.firstName} ${user.lastName}`.trim() || user.email,
      value: user.id
    }))
  );
  readonly availabilityDialogShowUserPicker = computed(
    () => this.availabilityDialogMode() === 'header' && this.canManageAny
  );
  readonly availabilityDialogManageForUser = computed(() => {
    if (this.availabilityDialogMode() === 'header') {
      return false;
    }

    const userId = this.panelUser()?.id;
    return this.canManageAny && userId !== this.currentUserId;
  });
  readonly availabilityDialogUserId = computed(() =>
    this.availabilityDialogMode() === 'header' ? null : (this.panelUser()?.id ?? null)
  );
  readonly availabilityDialogSelectedDate = computed(() =>
    this.availabilityDialogMode() === 'header' ? null : this.panelDate()
  );
  readonly periodLabel = computed(() => {
    const anchor = this.anchorDate();
    if (this.view() === 'week') {
      const dates = this.weekDates();
      return `${dates[0]} – ${dates[dates.length - 1]}`;
    }

    return anchor.toLocaleString(undefined, { month: 'long', year: 'numeric' });
  });

  readonly getAvailabilityStatusLabel = getAvailabilityStatusLabel;
  readonly getAvailabilityStatusSeverity = getAvailabilityStatusSeverity;
  readonly formatAvailabilityTimeRange = formatAvailabilityTimeRange;
  readonly getAvailabilityEntryTooltip = getAvailabilityEntryTooltip;
  readonly isTodayDate = isTodayDate;
  readonly isWeekendDate = isWeekendDate;
  readonly isInMonth = isInMonth;
  readonly countDayEntries = countDayEntries;
  readonly countAvailableDays = countAvailableDays;
  readonly countTeamDaySummary = countTeamDaySummary;

  constructor() {
    const userId = this.route.snapshot.queryParamMap.get('userId');
    if (userId) {
      this.selectedUserIds.set([userId]);
    }

    this.loadFilters();
    effect(() => {
      this.view();
      this.anchorDate();
      this.selectedUserIds();
      this.selectedProjectIds();
      this.selectedStatuses();
      this.loadCalendar();
    });
  }

  loadFilters(): void {
    this.usersService
      .getUsers({ pageNumber: 1, pageSize: 200, deactivated: [false] })
      .subscribe((result) => this.users.set(result.items));

    this.projectsService
      .getProjects({ pageNumber: 1, pageSize: 200 })
      .subscribe((result) => this.projects.set(result.items));
  }

  loadCalendar(): void {
    const range = this.calendarRange();
    this.isLoading.set(true);
    this.availabilityService
      .getTeamCalendar({
        from: range.from,
        to: range.to,
        userIds: this.selectedUserIds(),
        projectIds: this.selectedProjectIds(),
        statuses: this.selectedStatuses()
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (result) => this.calendar.set(result),
        error: () => this.errorMessage.set('Unable to load availability calendar.')
      });
  }

  setView(view: AvailabilityCalendarView): void {
    this.view.set(view);
    localStorage.setItem(VIEW_STORAGE_KEY, view);
  }

  setDensity(density: AvailabilityCalendarDensity): void {
    this.density.set(density);
    localStorage.setItem(DENSITY_STORAGE_KEY, density);
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

  effectiveEntry(userId: string, date: string): AvailabilityEntryDto | null {
    const entry = resolveEffectiveEntry(this.entries(), userId, date);
    if (!entry) {
      return null;
    }

    return isEffectiveInStatus(entry, this.selectedStatuses()) ? entry : entry;
  }

  openCell(user: AvailabilityCalendarUserDto, date: string): void {
    this.panelUser.set(user);
    this.panelDate.set(date);
    this.panelEntries.set(getEntriesForUserDay(this.entries(), user.id, date));
    this.panelVisible.set(true);
    this.availabilityService.getUserPattern(user.id).subscribe({
      next: (pattern) => this.panelPattern.set(pattern),
      error: () => this.panelPattern.set(null)
    });
  }

  canManageUser(userId: string): boolean {
    return this.canManageAny || (this.canManageOwn && userId === this.currentUserId);
  }

  panelTitle(): string {
    const user = this.panelUser();
    const date = this.panelDate();
    if (!user || !date) {
      return '';
    }

    const formatted = new Date(`${date}T00:00:00`).toLocaleDateString(undefined, {
      weekday: 'long',
      month: 'long',
      day: 'numeric',
      year: 'numeric'
    });

    return `${user.fullName} — ${formatted}`;
  }

  openAddException(): void {
    this.availabilityDialogMode.set('panel');
    this.editingEntry.set(null);
    this.availabilityDialogVisible.set(true);
  }

  openHeaderAddAvailability(): void {
    this.availabilityDialogMode.set('header');
    this.editingEntry.set(null);
    this.availabilityDialogVisible.set(true);
  }

  openEditEntry(entry: AvailabilityEntryDto): void {
    this.availabilityDialogMode.set('panel');
    this.editingEntry.set(entry);
    this.availabilityDialogVisible.set(true);
  }

  openPatternDialog(): void {
    this.patternDialogVisible.set(true);
  }

  onAvailabilitySaved(): void {
    this.availabilityDialogVisible.set(false);
    this.loadCalendar();
    const user = this.panelUser();
    const date = this.panelDate();
    if (user && date) {
      this.availabilityService.getUserDay(user.id, date).subscribe((result) => {
        this.panelEntries.set(result.entries);
      });
    }
  }

  onPatternSaved(): void {
    this.patternDialogVisible.set(false);
    this.loadCalendar();
    const user = this.panelUser();
    if (user) {
      this.availabilityService.getUserPattern(user.id).subscribe((pattern) => {
        this.panelPattern.set(pattern);
      });
    }
  }
}
