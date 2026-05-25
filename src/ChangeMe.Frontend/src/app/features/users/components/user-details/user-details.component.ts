import { DatePipe } from '@angular/common';
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
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  formatUserName,
  formatUserReference
} from '@core/user/utils/user-display.utils';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages, formatIpAddress } from '@features/auth/utils/auth.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { AdminUserSessionDto, UserDetailsDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import {
  getActivateConfirmMessage,
  getDeactivateConfirmMessage,
  getUserStatusLabel,
  getUserStatusSeverity,
  UserMessages
} from '@features/users/utils/users.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-user-details',
  imports: [
    DatePipe,
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Tag,
    TableModule,
    Paginator,
    Panel,
    ProgressSpinner,
    EffectivePermissionsComponent
  ],
  templateUrl: './user-details.component.html'
})
export class UserDetailsComponent {
  readonly id = input.required<string>();

  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly user = signal<UserDetailsDto | null>(null);
  readonly pageTitle = computed(() => {
    const profile = this.user();
    return profile ? formatUserReference(profile) : 'User details';
  });

  readonly formatUserName = formatUserName;

  readonly canManageInvitationActions = computed(() => {
    const profile = this.user();
    return (
      !!profile &&
      this.canManageUsers() &&
      profile.status === 'Invited' &&
      !!profile.pendingInvitation
    );
  });

  readonly canSendInvitation = computed(() => {
    const profile = this.user();
    return (
      !!profile &&
      this.canManageUsers() &&
      profile.status === 'InvitationCanceled' &&
      !profile.deactivated
    );
  });
  readonly sessions = signal<AdminUserSessionDto[]>([]);
  readonly sessionsPagination = signal<PaginationResult<AdminUserSessionDto> | null>(
    null
  );
  readonly sessionsQuery = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'LastActivityAt',
    ascending: false
  });
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isLoadingSessions = signal(false);
  readonly hasLoadedSessions = signal(false);
  readonly pendingRevokeSessionIds = signal<string[]>([]);
  readonly passwordExpirationEnabled = signal(false);
  readonly emailVerificationEnabled = signal(false);
  readonly twoFactorAuthenticationEnabled = signal(false);
  readonly passkeysAuthenticationEnabled = signal(false);
  readonly externalProvidersEnabled = signal(false);

  readonly UserMessages = UserMessages;
  readonly formatIpAddress = formatIpAddress;
  readonly getUserStatusLabel = getUserStatusLabel;
  readonly getUserStatusSeverity = getUserStatusSeverity;
  readonly canManageUsers = () =>
    this.authService.hasPermission(PermissionCodes.usersManage);
  readonly canDeactivateUsers = () =>
    this.authService.hasPermission(PermissionCodes.usersDeactivate);
  readonly canViewSessions = () =>
    this.authService.hasPermission(PermissionCodes.sessionsViewAny);
  readonly canManageSessions = () =>
    this.authService.hasPermission(PermissionCodes.sessionsManageAny);

  readonly canResetTwoFactor = computed(() => {
    const profile = this.user();
    return (
      !!profile &&
      this.twoFactorAuthenticationEnabled() &&
      profile.twoFactorEnabled &&
      this.canManageUsers() &&
      !profile.deactivated
    );
  });

  readonly canResetPasskeys = computed(() => {
    const profile = this.user();
    return (
      !!profile &&
      this.passkeysAuthenticationEnabled() &&
      profile.passkeys &&
      profile.passkeys.length > 0 &&
      this.canManageUsers() &&
      !profile.deactivated
    );
  });

  constructor() {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.passwordExpirationEnabled.set(settings.passwordExpirationEnabled);
          this.emailVerificationEnabled.set(settings.emailVerificationEnabled);
          this.twoFactorAuthenticationEnabled.set(
            settings.twoFactorAuthenticationEnabled
          );
          this.passkeysAuthenticationEnabled.set(
            settings.passkeys?.passkeysAuthenticationEnabled === true
          );
          this.externalProvidersEnabled.set(settings.externalProvidersEnabled);
        }
      });

    effect(() => {
      this.id();
      this.loadUser();
    });
  }

  confirmDeactivate(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Deactivate user',
      message: getDeactivateConfirmMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Deactivate', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.deactivateUser()
    });
  }

  confirmActivate(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Activate user',
      message: getActivateConfirmMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Activate', severity: 'success' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.activateUser()
    });
  }

  confirmUnlinkExternal(providerKey: string, displayName: string): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    if (!profile.hasPasswordSet && profile.externalLogins.length <= 1) {
      this.toastService.error(AuthMessages.cannotRemoveOnlySignInMethod);
      return;
    }

    this.confirmationService.confirm({
      header: AuthMessages.unlinkExternalProviderTitle,
      message: AuthMessages.unlinkExternalProviderMessage(displayName),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Remove', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.unlinkExternal(providerKey)
    });
  }

  confirmResetTwoFactor(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.resetTwoFactorTitle,
      message: UserMessages.resetTwoFactorMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Reset', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.resetTwoFactor()
    });
  }

  confirmResetPasskeys(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.resetPasskeysTitle,
      message: UserMessages.resetPasskeysMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Reset', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.resetPasskeys()
    });
  }

  confirmSendPasswordReset(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.sendPasswordResetTitle,
      message: UserMessages.sendPasswordResetMessage(profile.email),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Send', severity: 'warn' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.sendPasswordReset()
    });
  }

  confirmConfirmEmail(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.confirmEmailTitle,
      message: UserMessages.confirmEmailMessage(formatUserReference(profile)),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Confirm', severity: 'warn' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.confirmUserEmail()
    });
  }

  confirmResendInvitation(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.resendInvitationTitle,
      message: UserMessages.resendInvitationMessage(profile.email),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Resend', severity: 'warn' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.resendInvitation()
    });
  }

  confirmCancelInvitation(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.cancelInvitationTitle,
      message: UserMessages.cancelInvitationMessage(profile.email),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Cancel invitation', severity: 'danger' },
      rejectButtonProps: { label: 'Keep', severity: 'secondary', outlined: true },
      accept: () => this.cancelInvitation()
    });
  }

  confirmSendInvitation(): void {
    const profile = this.user();
    if (!profile) {
      return;
    }

    this.confirmationService.confirm({
      header: UserMessages.sendInvitationTitle,
      message: UserMessages.sendInvitationMessage(profile.email),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Send', severity: 'warn' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.sendInvitation()
    });
  }

  confirmRevokeAllSessions(): void {
    this.confirmationService.confirm({
      header: UserMessages.revokeAllSessionsTitle,
      message: UserMessages.revokeAllSessionsMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Revoke all', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.revokeAllSessions()
    });
  }

  confirmRevokeSession(session: AdminUserSessionDto): void {
    this.confirmationService.confirm({
      header: UserMessages.revokeSessionTitle,
      message: UserMessages.revokeSessionMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Revoke', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.revokeSession(session)
    });
  }

  isRevokePending(sessionId: string): boolean {
    return this.pendingRevokeSessionIds().includes(sessionId);
  }

  refresh(): void {
    this.loadUser();
  }

  private loadUser(): void {
    const userId = this.id();
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.usersService
      .getUserById(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.isLoading.set(false);

          if (this.canViewSessions() && !user.deactivated) {
            this.loadSessions(userId);
          } else {
            this.sessions.set([]);
          }
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  onSessionsPageChange(event: PaginatorState): void {
    this.sessionsQuery.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
    this.loadSessions(this.id());
  }

  private loadSessions(userId: string): void {
    this.isLoadingSessions.set(true);

    this.usersService
      .getUserSessions(userId, this.sessionsQuery())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.sessions.set(result.items);
          this.sessionsPagination.set(result);
          this.isLoadingSessions.set(false);
          this.hasLoadedSessions.set(true);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoadingSessions.set(false);
        }
      });
  }

  private unlinkExternal(providerKey: string): void {
    this.usersService
      .unlinkExternalLogin(this.id(), providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(AuthMessages.externalAccountUnlinked);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private resetTwoFactor(): void {
    this.usersService
      .resetTwoFactor(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.twoFactorReset);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private resetPasskeys(): void {
    this.usersService
      .resetPasskeys(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.passkeysReset);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private sendPasswordReset(): void {
    this.usersService
      .sendPasswordReset(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => this.toastService.success(UserMessages.passwordResetSent),
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private confirmUserEmail(): void {
    this.usersService
      .confirmUserEmail(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.emailMarkedAsVerified);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private resendInvitation(): void {
    this.usersService
      .resendInvitation(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.invitationResent);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private cancelInvitation(): void {
    this.usersService
      .cancelInvitation(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.invitationCanceled);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private sendInvitation(): void {
    this.usersService
      .sendInvitation(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.invitationSent);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private deactivateUser(): void {
    this.usersService
      .deactivateUser(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.sessions.set([]);
          this.toastService.success(UserMessages.userDeactivated);
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private activateUser(): void {
    this.usersService
      .activateUser(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.toastService.success(UserMessages.userActivated);
          if (this.canViewSessions()) {
            this.loadSessions(user.id);
          }
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private revokeAllSessions(): void {
    this.usersService
      .revokeAllUserSessions(this.id())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sessions.set([]);
          this.toastService.success('All sessions revoked.');
        },
        error: (error: Error) => this.errorMessage.set(error.message)
      });
  }

  private revokeSession(session: AdminUserSessionDto): void {
    this.pendingRevokeSessionIds.update((ids) => [...ids, session.id]);

    this.usersService
      .revokeUserSession(this.id(), session.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.sessions.update((items) =>
            items.filter((item) => item.id !== session.id)
          );
          this.pendingRevokeSessionIds.update((ids) =>
            ids.filter((id) => id !== session.id)
          );
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.pendingRevokeSessionIds.update((ids) =>
            ids.filter((id) => id !== session.id)
          );
        }
      });
  }
}
