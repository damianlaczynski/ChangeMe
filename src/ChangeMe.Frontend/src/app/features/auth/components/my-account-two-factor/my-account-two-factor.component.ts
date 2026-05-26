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
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { ExternalStepUpReturnService } from '@features/auth/services/external-step-up-return.service';
import { TwoFactorSetupDialogService } from '@features/auth/services/two-factor-setup-dialog.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  clearPendingTwoFactorStepUp,
  PendingTwoFactorStepUpAction,
  readPendingTwoFactorStepUp,
  storePendingTwoFactorStepUp
} from '@features/auth/utils/pending-two-factor-step-up.storage';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { Dialog } from 'primeng/dialog';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-account-two-factor',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Panel,
    Button,
    Tag,
    Message,
    Dialog,
    Checkbox,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './my-account-two-factor.component.html'
})
export class MyAccountTwoFactorComponent implements OnInit {
  readonly account = input.required<MyAccountDto>();
  readonly twoFactorRequired = input(false);
  readonly stepUpValidityMinutes = input(15);
  readonly passkeysEnabled = input(false);
  readonly accountChanged = output<void>();

  private readonly authService = inject(AuthService);
  private readonly setupDialogService = inject(TwoFactorSetupDialogService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly externalStepUpReturn = inject(ExternalStepUpReturnService);

  private readonly resumeStepUpAfterExternalReturn = signal(false);

  readonly stepUpVisible = signal(false);
  readonly stepUpAction = signal<PendingTwoFactorStepUpAction | null>(null);
  readonly stepUpError = signal('');
  readonly isStepUpSubmitting = signal(false);
  readonly recoveryCodes = signal<string[]>([]);
  readonly recoveryCodesSaved = new FormControl(false, {
    nonNullable: true,
    validators: [Validators.requiredTrue]
  });

  readonly AuthMessages = AuthMessages;

  constructor() {
    this.externalStepUpReturn.resumeWhenExternalReauthFresh(this.destroyRef, {
      isResumePending: () => this.resumeStepUpAfterExternalReturn(),
      clearResumePending: () => this.resumeStepUpAfterExternalReturn.set(false),
      account: this.account,
      onReady: () => {
        this.stepUpError.set('');
        this.stepUpVisible.set(true);
      }
    });
  }

  ngOnInit(): void {
    this.externalStepUpReturn.watchQueryParamReturn(this.destroyRef, this.route, () => {
      const action = readPendingTwoFactorStepUp();
      if (!action) {
        return;
      }

      clearPendingTwoFactorStepUp();
      this.stepUpAction.set(action);
      this.resumeStepUpAfterExternalReturn.set(true);
      this.toastService.info(AuthMessages.twoFactorReenterAfterExternalStepUp);
    });
  }

  openEnableSetup(): void {
    this.setupDialogService.open();
  }

  confirmDisable(): void {
    this.confirmationService.confirm({
      header: 'Disable two-factor authentication?',
      message: 'Disable two-factor authentication on your account?',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Disable', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.openStepUp('disable')
    });
  }

  confirmRegenerate(): void {
    this.confirmationService.confirm({
      header: 'Regenerate recovery codes?',
      message: 'Regenerating recovery codes invalidates your existing codes. Continue?',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Regenerate', severity: 'warn' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.openStepUp('regenerate')
    });
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.stepUpAction.set(null);
    this.stepUpError.set('');
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    const action = this.stepUpAction();
    if (!action || this.isStepUpSubmitting()) {
      return;
    }

    const request = {
      currentPassword: result.currentPassword,
      verificationCode: result.verificationCode
    };

    this.isStepUpSubmitting.set(true);
    this.stepUpError.set('');

    if (action === 'disable') {
      this.authService
        .disableTwoFactor(request)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            this.isStepUpSubmitting.set(false);
            this.closeStepUp();
            this.toastService.success(AuthMessages.twoFactorDisabled);
            this.accountChanged.emit();
          },
          error: (error: unknown) => {
            this.stepUpError.set(
              error instanceof Error ? error.message : 'Unable to complete this action.'
            );
            this.isStepUpSubmitting.set(false);
          }
        });
      return;
    }

    this.authService
      .regenerateRecoveryCodes(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (codesResult) => {
          this.isStepUpSubmitting.set(false);
          this.closeStepUp();
          this.recoveryCodes.set(codesResult.recoveryCodes);
          this.recoveryCodesSaved.setValue(false);
        },
        error: (error: unknown) => {
          this.stepUpError.set(
            error instanceof Error ? error.message : 'Unable to complete this action.'
          );
          this.isStepUpSubmitting.set(false);
        }
      });
  }

  readonly prepareExternalRedirect = (): boolean => {
    const action = this.stepUpAction();
    if (!action) {
      return false;
    }

    storePendingTwoFactorStepUp(action);
    return true;
  };

  onExternalRedirectFailed(): void {
    clearPendingTwoFactorStepUp();
  }

  closeRecoveryCodes(): void {
    if (!this.recoveryCodesSaved.valid) {
      this.recoveryCodesSaved.markAsTouched();
      return;
    }

    this.recoveryCodes.set([]);
    this.recoveryCodesSaved.setValue(false);
    this.toastService.success(AuthMessages.recoveryCodesRegenerated);
    this.accountChanged.emit();
  }

  private openStepUp(action: PendingTwoFactorStepUpAction): void {
    this.stepUpAction.set(action);
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }
}
