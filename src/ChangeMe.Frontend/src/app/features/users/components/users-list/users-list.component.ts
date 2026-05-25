import { DatePipe } from '@angular/common';
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
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import {
  UserListItemDto,
  UserSearchParameters
} from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import {
  emailVerifiedFilters,
  getActivateConfirmMessage,
  getDeactivateConfirmMessage,
  getEmailVerifiedBadgeLabel,
  getEmailVerifiedBadgeSeverity,
  getUserStatusLabel,
  getUserStatusSeverity,
  statusFilters,
  UserMembershipStatus,
  UserMessages
} from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { AppliedFiltersChipsComponent } from '@shared/components/applied-filters-chips/applied-filters-chips.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { createEmptyPaginationResult } from '@shared/data/utils/pagination.utils';
import { AppliedFilterChip } from '@shared/models/applied-filter-chip.model';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Paginator } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';
import { PaginatorState } from 'primeng/types/paginator';
import { catchError, of, switchMap, tap } from 'rxjs';

type UsersFilterForm = {
  searchText: FormControl<string>;
  status: FormControl<UserMembershipStatus[]>;
  emailVerified: FormControl<boolean[]>;
};

type UserSortField = 'Name' | 'CreatedAt' | 'LastSignIn';

@Component({
  selector: 'app-users-list',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    MultiSelect,
    TableModule,
    Paginator,
    Message,
    Tag,
    Panel,
    Menu,
    AppliedFiltersChipsComponent,
    Tooltip
  ],
  templateUrl: './users-list.component.html'
})
export class UsersListComponent {
  readonly formatUserName = formatUserName;
  readonly formatUserReference = formatUserReference;

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly statusFilters = statusFilters;
  readonly emailVerifiedFilters = emailVerifiedFilters;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly getEmailVerifiedBadgeLabel = getEmailVerifiedBadgeLabel;
  readonly getEmailVerifiedBadgeSeverity = getEmailVerifiedBadgeSeverity;
  readonly UserMessages = UserMessages;
  readonly emailVerificationEnabled = signal(false);
  readonly permissionCodes = PermissionCodes;

  readonly users = signal<UserListItemDto[]>([]);
  readonly pagination = signal<PaginationResult<UserListItemDto> | null>(null);
  readonly query = signal<UserSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly filtersCollapsed = signal(true);
  readonly userActionItems = signal<MenuItem[]>([]);
  private readonly userActionsMenu = viewChild.required<Menu>('userActionsMenu');

  readonly canManageUsers = computed(() =>
    this.authService.hasPermission(PermissionCodes.usersManage)
  );
  readonly canInviteUsers = computed(
    () =>
      this.authService.hasPermission(PermissionCodes.usersManage) &&
      this.authService.hasPermission(PermissionCodes.rolesManage)
  );
  readonly canDeactivateUsers = computed(() =>
    this.authService.hasPermission(PermissionCodes.usersDeactivate)
  );

  readonly filtersForm = new FormGroup<UsersFilterForm>({
    searchText: new FormControl('', { nonNullable: true }),
    status: new FormControl<UserMembershipStatus[]>([], { nonNullable: true }),
    emailVerified: new FormControl<boolean[]>([], { nonNullable: true })
  });

  readonly appliedFilterChips = computed(() => {
    const activeQuery = this.query();
    const chips: AppliedFilterChip[] = [];

    for (const status of activeQuery.status ?? []) {
      chips.push({
        id: `status-${status}`,
        label: `Status: ${getUserStatusLabel(status)}`
      });
    }

    if (this.emailVerificationEnabled()) {
      for (const emailVerified of activeQuery.emailVerified ?? []) {
        chips.push({
          id: `emailVerified-${emailVerified}`,
          label: `Email verified: ${getEmailVerifiedBadgeLabel(emailVerified)}`
        });
      }
    }

    return chips;
  });

  readonly tableColumnCount = computed(() => (this.emailVerificationEnabled() ? 8 : 7));

  readonly hasAppliedFilters = computed(() => this.appliedFilterChips().length > 0);

  constructor() {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) =>
          this.emailVerificationEnabled.set(settings.emailVerificationEnabled)
      });

    toObservable(this.query)
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        switchMap((params) =>
          this.usersService.getUsers(params).pipe(
            catchError((error: Error) => {
              this.errorMessage.set(error.message);
              return of(createEmptyPaginationResult<UserListItemDto>(params));
            })
          )
        ),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((result) => {
        this.users.set(result.items);
        this.pagination.set(result);
        this.isLoading.set(false);
        this.hasLoaded.set(true);
      });
  }

  applyFilters(): void {
    const { searchText, status, emailVerified } = this.filtersForm.getRawValue();
    this.query.update((current) => ({
      ...current,
      pageNumber: 1,
      searchText: searchText.trim() || undefined,
      status: status.length > 0 ? status : undefined,
      emailVerified:
        this.emailVerificationEnabled() && emailVerified.length > 0
          ? emailVerified
          : undefined
    }));
  }

  clearFilters(): void {
    this.filtersForm.reset({ searchText: '', status: [], emailVerified: [] });
    this.query.update((current) => ({
      ...current,
      pageNumber: 1,
      searchText: undefined,
      status: undefined,
      emailVerified: undefined
    }));
  }

  onFiltersCollapsedChange(collapsed: boolean | undefined): void {
    this.filtersCollapsed.set(collapsed ?? true);
  }

  onPageChange(event: PaginatorState): void {
    const pageSize = event.rows ?? this.query().pageSize ?? 10;
    const pageNumber = Math.floor((event.first ?? 0) / pageSize) + 1;
    const currentQuery = this.query();

    if (currentQuery.pageNumber === pageNumber && currentQuery.pageSize === pageSize) {
      return;
    }

    this.query.set({
      ...currentQuery,
      pageNumber,
      pageSize
    });
  }

  onTableSort(event: { field?: string | null; order?: number | null }): void {
    if (!event.field || event.order == null || event.order === 0) {
      return;
    }

    const sortField = event.field as UserSortField;
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

  openUserActionsMenu(event: Event, user: UserListItemDto): void {
    const items: MenuItem[] = [
      {
        label: 'Open details',
        icon: 'pi pi-eye',
        routerLink: ['/users', user.id]
      }
    ];

    if (this.canManageUsers()) {
      items.push({
        label: 'Edit',
        icon: 'pi pi-pencil',
        routerLink: ['/users', user.id, 'edit']
      });
    }

    if (this.canDeactivateUsers()) {
      if (!user.deactivated) {
        items.push({
          label: 'Deactivate',
          icon: 'pi pi-ban',
          command: () => this.confirmDeactivate(user)
        });
      } else {
        items.push({
          label: 'Activate',
          icon: 'pi pi-check',
          command: () => this.confirmActivate(user)
        });
      }
    }

    this.userActionItems.set(items);
    this.userActionsMenu().toggle(event);
  }

  confirmDeactivate(user: UserListItemDto): void {
    this.confirmationService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(user)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deactivateUser(user)
    });
  }

  confirmActivate(user: UserListItemDto): void {
    this.confirmationService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(user)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Activate', severity: 'success' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.activateUser(user)
    });
  }

  private deactivateUser(user: UserListItemDto): void {
    this.usersService
      .deactivateUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(UserMessages.userDeactivated);
          this.refreshList();
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private activateUser(user: UserListItemDto): void {
    this.usersService
      .activateUser(user.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(UserMessages.userActivated);
          this.refreshList();
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  refresh(): void {
    this.query.set({ ...this.query(), pageNumber: 1 });
  }

  private refreshList(): void {
    this.refresh();
  }

  removeAppliedFilter(chip: AppliedFilterChip): void {
    if (chip.id.startsWith('status-')) {
      const status = chip.id.slice('status-'.length) as UserMembershipStatus;
      const values = this.filtersForm.controls.status.value.filter(
        (item) => item !== status
      );
      this.filtersForm.controls.status.setValue(values);
      this.applyFilters();
      return;
    }

    if (chip.id.startsWith('emailVerified-')) {
      const emailVerified = chip.id.slice('emailVerified-'.length) === 'true';
      const values = this.filtersForm.controls.emailVerified.value.filter(
        (item) => item !== emailVerified
      );
      this.filtersForm.controls.emailVerified.setValue(values);
      this.applyFilters();
    }
  }
}
