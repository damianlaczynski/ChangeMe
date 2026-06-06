import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed, toObservable } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService } from '@features/auth/services/auth.service';
import {
  LeaveRequestListItemDto,
  LeaveRequestSearchParameters,
  LeaveRequestStatus
} from '@features/billing/models/leave-request.model';
import { LeaveTypeListItemDto } from '@features/billing/models/leave-type.model';
import { LeaveRequestsService } from '@features/billing/services/leave-requests.service';
import { LeaveTypesService } from '@features/billing/services/leave-types.service';
import {
  BillingMessages,
  LeaveDatePresetId,
  LeaveRequestConstraints,
  getCurrentMonthDateRange,
  getLeaveDateRangeForPreset,
  getLeaveRequestStatusSeverity,
  leaveDateFilterPresets,
  leaveRequestStatusOptions
} from '@features/billing/utils/billing.utils';
import { toIsoDateString } from '@features/time/utils/time.utils';
import { UserListItemDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { SelectButton } from 'primeng/selectbutton';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { catchError, of, switchMap, tap } from 'rxjs';

@Component({
  selector: 'app-leave-requests-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    FormsModule,
    RouterLink,
    Card,
    Button,
    DatePicker,
    MultiSelect,
    SelectButton,
    TableModule,
    Message,
    Tag,
    Paginator
  ],
  templateUrl: './leave-requests-list.component.html'
})
export class LeaveRequestsListComponent {
  private readonly leaveRequestsService = inject(LeaveRequestsService);
  private readonly leaveTypesService = inject(LeaveTypesService);
  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly LeaveRequestConstraints = LeaveRequestConstraints;
  readonly leaveRequestStatusOptions = leaveRequestStatusOptions;
  readonly leaveDateFilterPresets = leaveDateFilterPresets;
  readonly getLeaveRequestStatusSeverity = getLeaveRequestStatusSeverity;

  readonly requests = signal<LeaveRequestListItemDto[]>([]);
  readonly pagination = signal<PaginationResult<LeaveRequestListItemDto> | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly leaveTypeOptions = signal<LeaveTypeListItemDto[]>([]);
  readonly userOptions = signal<{ id: string; label: string }[]>([]);
  readonly selectedDatePreset = signal<LeaveDatePresetId>('thisMonth');

  readonly canManageLeave = this.authService.hasPermission(
    PermissionCodes.billingManageLeave
  );
  readonly canFilterByUsers =
    this.authService.hasPermission(PermissionCodes.billingViewAny) ||
    this.authService.hasPermission(PermissionCodes.billingManageLeave);
  readonly canViewUsers = this.authService.hasPermission(PermissionCodes.usersView);

  private readonly defaultRange = getCurrentMonthDateRange();

  readonly filtersForm = new FormGroup({
    statuses: new FormControl<LeaveRequestStatus[]>([], { nonNullable: true }),
    leaveTypeIds: new FormControl<string[]>([], { nonNullable: true }),
    userIds: new FormControl<string[]>([], { nonNullable: true }),
    dateFrom: new FormControl<Date | null>(this.defaultRange.from),
    dateTo: new FormControl<Date | null>(this.defaultRange.to)
  });

  private readonly query = signal<LeaveRequestSearchParameters>({
    pageNumber: 1,
    pageSize: LeaveRequestConstraints.PAGE_SIZE,
    dateFrom: toIsoDateString(this.defaultRange.from),
    dateTo: toIsoDateString(this.defaultRange.to)
  });

  constructor() {
    this.loadFilterOptions();
    this.applyUserIdFromQuery(this.route.snapshot.queryParamMap.get('userId'));

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => this.applyUserIdFromQuery(params.get('userId')));

    toObservable(this.query)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((params) =>
          this.leaveRequestsService.getLeaveRequests(params).pipe(
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
        this.pagination.set(result);
        this.isLoading.set(false);
      });
  }

  applyFilters(): void {
    const raw = this.filtersForm.getRawValue();
    this.query.update((current) => ({
      ...current,
      pageNumber: 1,
      statuses: raw.statuses.length > 0 ? raw.statuses : undefined,
      leaveTypeIds: raw.leaveTypeIds.length > 0 ? raw.leaveTypeIds : undefined,
      userIds: raw.userIds.length > 0 ? raw.userIds : undefined,
      dateFrom: raw.dateFrom ? toIsoDateString(raw.dateFrom) : undefined,
      dateTo: raw.dateTo ? toIsoDateString(raw.dateTo) : undefined
    }));
  }

  applyDatePreset(presetId: LeaveDatePresetId): void {
    this.selectedDatePreset.set(presetId);
    const range = getLeaveDateRangeForPreset(presetId);
    this.filtersForm.patchValue({
      dateFrom: range.from,
      dateTo: range.to
    });
    this.applyFilters();
  }

  onPageChange(event: PaginatorState): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
  }

  onRowClick(request: LeaveRequestListItemDto): void {
    void this.router.navigate(['/leave-requests', request.id]);
  }

  userDisplayLabel(user: UserListItemDto): string {
    return `${user.firstName} ${user.lastName}`.trim() || user.email;
  }

  private applyUserIdFromQuery(userId: string | null): void {
    if (!userId || !this.canFilterByUsers) {
      return;
    }

    const range = getLeaveDateRangeForPreset('thisQuarter');
    this.selectedDatePreset.set('thisQuarter');
    this.filtersForm.patchValue({
      userIds: [userId],
      dateFrom: range.from,
      dateTo: range.to
    });
    this.applyFilters();
  }

  private loadFilterOptions(): void {
    this.leaveTypesService
      .getLeaveTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (types) =>
          this.leaveTypeOptions.set(types.filter((type) => type.isActive))
      });

    if (!this.canFilterByUsers) {
      return;
    }

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
}
