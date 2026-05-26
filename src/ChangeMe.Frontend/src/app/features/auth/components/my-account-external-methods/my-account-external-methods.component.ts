import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  inject,
  input,
  OnInit,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { ExternalStepUpReturnService } from '@features/auth/services/external-step-up-return.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { needsExternalReauth } from '@features/auth/utils/external-step-up.utils';
import {
  clearPendingUnlinkProvider,
  readPendingUnlinkProvider,
  storePendingUnlinkProvider
} from '@features/auth/utils/pending-unlink.storage';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Panel } from 'primeng/panel';

@Component({
  selector: 'app-my-account-external-methods',
  imports: [DatePipe, Panel, Button, IdentityStepUpDialogComponent],
  templateUrl: './my-account-external-methods.component.html'
})
export class MyAccountExternalMethodsComponent implements OnInit {
  readonly account = input.required<MyAccountDto>();
  readonly stepUpValidityMinutes = input(15);
  readonly passkeysEnabled = input(false);
  readonly accountChanged = output<void>();

  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly externalStepUpReturn = inject(ExternalStepUpReturnService);

  private readonly resumeUnlinkAfterExternalReturn = signal(false);

  readonly stepUpVisible = signal(false);
  readonly pendingUnlinkProviderKey = signal<string | null>(null);
  readonly stepUpError = signal('');
  readonly isStepUpSubmitting = signal(false);
  readonly providerLoadingKey = signal<string | null>(null);

  readonly authMessages = AuthMessages;

  readonly linkedProviders = () => this.account().externalLogins ?? [];
  readonly linkableProviders = () => this.account().linkableProviders ?? [];

  constructor() {
    this.externalStepUpReturn.resumeWhenExternalReauthFresh(this.destroyRef, {
      isResumePending: () => this.resumeUnlinkAfterExternalReturn(),
      clearResumePending: () => this.resumeUnlinkAfterExternalReturn.set(false),
      account: this.account,
      onReady: () => this.openStepUp()
    });
  }

  ngOnInit(): void {
    this.externalStepUpReturn.watchQueryParamReturn(this.destroyRef, this.route, () => {
      const providerKey = readPendingUnlinkProvider();
      if (!providerKey) {
        return;
      }

      this.pendingUnlinkProviderKey.set(providerKey);

      if (!this.account().twoFactorEnabled) {
        clearPendingUnlinkProvider();
        this.unlinkAfterExternalStepUp(providerKey);
        return;
      }

      if (needsExternalReauth(this.account())) {
        this.resumeUnlinkAfterExternalReturn.set(true);
        return;
      }

      clearPendingUnlinkProvider();
      this.openStepUp();
    });
  }

  confirmUnlink(providerKey: string, displayName: string): void {
    if (!this.account().hasPasswordSet && this.linkedProviders().length <= 1) {
      this.toastService.error(AuthMessages.cannotRemoveOnlySignInMethod);
      return;
    }

    this.confirmationService.confirm({
      header: AuthMessages.unlinkExternalProviderTitle,
      message: AuthMessages.unlinkExternalProviderMessage(displayName),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Remove', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => {
        this.pendingUnlinkProviderKey.set(providerKey);
        this.openStepUp();
      }
    });
  }

  startLink(providerKey: string): void {
    if (this.providerLoadingKey()) {
      return;
    }

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalAccountLink(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          this.providerLoadingKey.set(null);
          this.toastService.error(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  openStepUp(): void {
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.pendingUnlinkProviderKey.set(null);
    this.stepUpError.set('');
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    const providerKey = this.pendingUnlinkProviderKey();
    if (!providerKey || this.isStepUpSubmitting()) {
      return;
    }

    this.isStepUpSubmitting.set(true);
    this.stepUpError.set('');

    this.authService
      .unlinkExternalAccount(providerKey, {
        currentPassword: result.currentPassword,
        verificationCode: result.verificationCode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          clearPendingUnlinkProvider();
          this.closeStepUp();
          this.toastService.success(AuthMessages.externalAccountUnlinked);
          this.accountChanged.emit();
        },
        error: (error) => {
          this.stepUpError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
          this.isStepUpSubmitting.set(false);
        },
        complete: () => {
          this.isStepUpSubmitting.set(false);
        }
      });
  }

  readonly prepareExternalRedirect = (): boolean => {
    const pendingUnlink = this.pendingUnlinkProviderKey();
    if (pendingUnlink) {
      storePendingUnlinkProvider(pendingUnlink);
    }
    return true;
  };

  private unlinkAfterExternalStepUp(providerKey: string): void {
    this.authService
      .unlinkExternalAccount(providerKey, {})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.externalAccountUnlinked);
          this.accountChanged.emit();
        },
        error: (error) => {
          this.toastService.error(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }
}
