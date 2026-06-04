import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  OnInit,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountExternalMethodsComponent } from '@features/auth/components/my-account-external-methods/my-account-external-methods.component';
import { MyAccountPasskeysComponent } from '@features/auth/components/my-account-passkeys/my-account-passkeys.component';
import { MyAccountTwoFactorComponent } from '@features/auth/components/my-account-two-factor/my-account-two-factor.component';
import { MySessionsComponent } from '@features/auth/components/my-sessions/my-sessions.component';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { TwoFactorSetupDialogService } from '@features/auth/services/two-factor-setup-dialog.service';
import { AuthMessages, PermissionCodes } from '@features/auth/utils/auth.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { UserMessages } from '@features/users/utils/users.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-account',
  imports: [
    DatePipe,
    RouterLink,
    Card,
    Button,
    Message,
    Tag,
    Panel,
    ProgressSpinner,
    EffectivePermissionsComponent,
    MySessionsComponent,
    MyAccountTwoFactorComponent,
    MyAccountPasskeysComponent,
    MyAccountExternalMethodsComponent,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './my-account.component.html'
})
export class MyAccountComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly setupDialogService = inject(TwoFactorSetupDialogService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly confirmationService = inject(ConfirmationService);

  private handledSetupCompleted = 0;

  readonly account = signal<MyAccountDto | null>(null);
  readonly twoFactorAuthenticationEnabled = signal(false);
  readonly twoFactorAuthenticationRequired = signal(false);
  readonly passkeysAuthenticationEnabled = signal(false);
  readonly passkeysAuthenticationRequired = signal(false);
  readonly maximumPasskeysPerUser = signal(10);
  readonly externalProvidersEnabled = signal(false);
  readonly externalProviderLinkingEnabled = signal(false);
  readonly selfServiceEmailChangeEnabled = signal(false);
  readonly cancelEmailChangeStepUpVisible = signal(false);
  readonly cancelEmailChangeStepUpError = signal('');
  readonly isCancelEmailChangeSubmitting = signal(false);
  readonly isResendingEmailChange = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly UserMessages = UserMessages;

  private readonly sessionsComponent = viewChild(MySessionsComponent);

  readonly canViewSessions = computed(() =>
    this.authService.hasPermission(PermissionCodes.sessionsViewOwn)
  );
  readonly canManageSessions = computed(() =>
    this.authService.hasPermission(PermissionCodes.sessionsManageOwn)
  );
  readonly canViewRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesView)
  );
  readonly isSigningOutEverywhere = computed(
    () => this.sessionsComponent()?.isSigningOutEverywhere() ?? false
  );
  readonly canChangeEmail = computed(() => {
    const profile = this.account();
    return (
      this.selfServiceEmailChangeEnabled() &&
      profile !== null &&
      !profile.invitationPending &&
      !profile.pendingEmailChange
    );
  });
  readonly stepUpExternalSignInValidityMinutes = signal(15);

  readonly effectivePermissions = computed(
    () => this.account()?.effectivePermissions ?? []
  );

  constructor() {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.twoFactorAuthenticationEnabled.set(
            settings.twoFactorAuthenticationEnabled
          );
          this.twoFactorAuthenticationRequired.set(
            settings.twoFactorAuthenticationRequired
          );
          this.externalProvidersEnabled.set(settings.externalProvidersEnabled);
          this.externalProviderLinkingEnabled.set(
            settings.externalProviderLinkingEnabled
          );
          this.selfServiceEmailChangeEnabled.set(
            settings.selfServiceEmailChangeEnabled
          );
          this.stepUpExternalSignInValidityMinutes.set(
            settings.twoFactor?.stepUpExternalSignInValidityMinutes ?? 15
          );
          const passkeys = settings.passkeys;
          this.passkeysAuthenticationEnabled.set(
            passkeys?.passkeysAuthenticationEnabled === true
          );
          this.passkeysAuthenticationRequired.set(
            passkeys?.passkeysAuthenticationRequired === true
          );
          this.maximumPasskeysPerUser.set(passkeys?.maximumPasskeysPerUser ?? 10);
        }
      });

    effect(() => {
      const completed = this.setupDialogService.setupCompleted();
      if (completed <= this.handledSetupCompleted || !this.account()) {
        return;
      }

      this.handledSetupCompleted = completed;
      this.reload();
    });

    this.reload();
  }

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const queryCleanup: Record<string, null> = {};

        if (params.get('externalStepUp') === '1') {
          this.reload();
        }

        if (params.get('externalLinked') === '1') {
          this.toastService.success(AuthMessages.externalAccountLinked);
          queryCleanup['externalLinked'] = null;
        }
        if (params.get('externalSignInError') === '1') {
          this.toastService.error(AuthMessages.externalSignInFailed);
          queryCleanup['externalSignInError'] = null;
        }
        const message = params.get('externalSignInMessage');
        if (message) {
          this.toastService.error(message);
          queryCleanup['externalSignInMessage'] = null;
        }

        if (Object.keys(queryCleanup).length > 0) {
          void this.router.navigate([], {
            relativeTo: this.route,
            queryParams: queryCleanup,
            queryParamsHandling: 'merge',
            replaceUrl: true
          });
        }
      });
  }

  reload(): void {
    const hasAccount = this.account() !== null;
    if (!hasAccount) {
      this.isLoading.set(true);
    }
    this.errorMessage.set(null);

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  signOutEverywhere(): void {
    this.sessionsComponent()?.confirmSignOutEverywhere();
  }

  resendEmailChangeConfirmation(): void {
    if (this.isResendingEmailChange()) {
      return;
    }

    this.isResendingEmailChange.set(true);
    this.authService
      .resendEmailChangeConfirmation()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.emailChangeResendSuccess);
          this.reload();
        },
        error: (error: Error) => this.toastService.error(error.message),
        complete: () => this.isResendingEmailChange.set(false)
      });
  }

  confirmCancelEmailChange(newEmail: string): void {
    this.confirmationService.confirm({
      header: 'Cancel email change',
      message: `Cancel the pending email change to "${newEmail}"? Your current email will stay unchanged.`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Cancel email change', severity: 'danger' },
      rejectButtonProps: {
        label: 'Keep pending change',
        severity: 'secondary',
        outlined: true
      },
      accept: () => {
        this.cancelEmailChangeStepUpError.set('');
        this.cancelEmailChangeStepUpVisible.set(true);
      }
    });
  }

  closeCancelEmailChangeStepUp(): void {
    this.cancelEmailChangeStepUpVisible.set(false);
    this.cancelEmailChangeStepUpError.set('');
  }

  onCancelEmailChangeStepUpVerified(result: StepUpVerificationResult): void {
    if (this.isCancelEmailChangeSubmitting()) {
      return;
    }

    this.isCancelEmailChangeSubmitting.set(true);
    this.cancelEmailChangeStepUpError.set('');

    this.authService
      .cancelEmailChange({
        currentPassword: result.currentPassword,
        verificationCode: result.verificationCode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.closeCancelEmailChangeStepUp();
          this.toastService.success('Email change cancelled.');
          this.reload();
        },
        error: (error: Error) => {
          this.cancelEmailChangeStepUpError.set(error.message);
          this.isCancelEmailChangeSubmitting.set(false);
        },
        complete: () => this.isCancelEmailChangeSubmitting.set(false)
      });
  }
}
