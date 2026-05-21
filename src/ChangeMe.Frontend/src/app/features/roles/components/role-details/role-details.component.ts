import {
    Component,
    computed,
    DestroyRef,
    effect,
    inject,
    input,
    signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import {
    RoleAssignedUserDto,
    RoleAssignedUsersSearchParameters,
    RoleDetailsDto
} from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import {
    formatDescription,
    getDeleteRoleConfirmMessage,
    getRemoveUserFromRoleConfirmMessage,
    getUserStatusSeverity,
    groupEffectivePermissions,
    RoleMessages
} from '@features/roles/utils/roles.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-role-details',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    TableModule,
    Message,
    Tag,
    Paginator
  ],
  templateUrl: './role-details.component.html'
})
export class RoleDetailsComponent {
  readonly id = input.required<string>();

  private readonly rolesService = inject(RolesService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly RoleMessages = RoleMessages;
  readonly formatDescription = formatDescription;
  readonly getUserStatusSeverity = getUserStatusSeverity;

  readonly role = signal<RoleDetailsDto | null>(null);
  readonly assignedUsers = signal<RoleAssignedUserDto[]>([]);
  readonly assignedUsersPagination = signal<PaginationResult<RoleAssignedUserDto> | null>(
    null
  );
  readonly assignedUsersQuery = signal<RoleAssignedUsersSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'Name',
    ascending: true
  });
  readonly isLoading = signal(true);
  readonly isLoadingUsers = signal(true);
  readonly hasLoadedUsers = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly showSystemRoleEditBlockedMessage = signal(false);
  readonly usersSearchControl = new FormControl('', { nonNullable: true });

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  readonly groupedPermissions = computed(() => {
    const current = this.role();
    return current ? groupEffectivePermissions(current.permissions) : [];
  });

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const blocked = params.get('systemRoleEditBlocked') === '1';
        this.showSystemRoleEditBlockedMessage.set(blocked);

        if (blocked) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: { systemRoleEditBlocked: null },
            queryParamsHandling: 'merge',
            replaceUrl: true
          });
        }
      });

    effect(() => {
      this.id();
      this.loadRole();
    });

    this.usersSearchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => {
        this.assignedUsersQuery.update((current) => ({
          ...current,
          pageNumber: 1,
          searchText: this.usersSearchControl.value.trim() || undefined
        }));
        this.loadAssignedUsers();
      });
  }

  onAssignedUsersPageChange(event: PaginatorState): void {
    this.assignedUsersQuery.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
    this.loadAssignedUsers();
  }

  private loadRole(): void {
    this.isLoading.set(true);
    this.rolesService.getRoleById(this.id()).subscribe({
      next: (details) => {
        this.role.set(details);
        this.isLoading.set(false);
        this.loadAssignedUsers();
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.isLoading.set(false);
      }
    });
  }

  private loadAssignedUsers(): void {
    this.isLoadingUsers.set(true);

    this.rolesService.getRoleAssignedUsers(this.id(), this.assignedUsersQuery()).subscribe({
      next: (result) => {
        this.assignedUsers.set(result.items);
        this.assignedUsersPagination.set(result);
        this.isLoadingUsers.set(false);
        this.hasLoadedUsers.set(true);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.isLoadingUsers.set(false);
      }
    });
  }

  confirmDeleteRole(): void {
    const current = this.role();
    if (!current) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete role',
      message: getDeleteRoleConfirmMessage(current.name),
      accept: () => {
        this.rolesService.deleteRole(current.id).subscribe({
          next: () => {
            this.toastService.success(RoleMessages.roleDeleted);
            void this.router.navigateByUrl('/roles');
          },
          error: (error: Error) => this.toastService.error(error.message)
        });
      }
    });
  }

  confirmRemoveUser(user: RoleAssignedUserDto): void {
    const current = this.role();
    if (!current) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Remove from role',
      message: getRemoveUserFromRoleConfirmMessage(user.fullName, current.name),
      accept: () => {
        this.rolesService.removeUserFromRole(current.id, user.id).subscribe({
          next: () => {
            this.toastService.success(RoleMessages.userRemovedFromRole);
            this.loadRole();
          },
          error: (error: Error) => this.toastService.error(error.message)
        });
      }
    });
  }
}
