import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  effect,
  inject,
  input,
  OnInit,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { TwoFactorSetupDialogService } from '@features/auth/services/two-factor-setup-dialog.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildExternalReauthRequiredDetail,
  needsExternalReauth
} from '@features/auth/utils/external-step-up.utils';
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
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Password } from 'primeng/password';
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
    Password,
    InputText,
    Checkbox
  ],
  templateUrl: './my-account-two-factor.component.html'
})
export class MyAccountTwoFactorComponent implements OnInit {
  readonly account = input.required<MyAccountDto>();
  readonly twoFactorRequired = input(false);
  readonly stepUpValidityMinutes = input(15);
  readonly accountChanged = output<void>();

  private readonly authService = inject(AuthService);
  private readonly setupDialogService = inject(TwoFactorSetupDialogService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private externalStepUpHandled = false;
  private readonly resumeStepUpAfterExternalReturn = signal(false);

  readonly stepUpVisible = signal(false);
  readonly stepUpAction = signal<PendingTwoFactorStepUpAction | null>(null);
  readonly stepUpError = signal('');
  readonly isStepUpSubmitting = signal(false);
  readonly providerLoadingKey = signal<string | null>(null);
  readonly recoveryCodes = signal<string[]>([]);
  readonly recoveryCodesSaved = new FormControl(false, {
    nonNullable: true,
    validators: [Validators.requiredTrue]
  });

  readonly stepUpForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(64)]
    })
  });

  readonly AuthMessages = AuthMessages;

  readonly linkedProviders = () => this.account().externalLogins ?? [];
  readonly requiresPasswordStepUp = () => this.account().hasPasswordSet;
  readonly requiresExternalStepUp = () =>
    !this.account().hasPasswordSet && this.linkedProviders().length > 0;
  readonly needsExternalReauth = () => needsExternalReauth(this.account());
  readonly externalReauthDetail = () =>
    buildExternalReauthRequiredDetail(this.stepUpValidityMinutes());

  constructor() {
    effect(() => {
      if (!this.resumeStepUpAfterExternalReturn()) {
        return;
      }

      if (needsExternalReauth(this.account())) {
        return;
      }

      this.resumeStepUpAfterExternalReturn.set(false);
      this.stepUpForm.reset();
      this.stepUpVisible.set(true);
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        if (params.get('externalStepUp') !== '1' || this.externalStepUpHandled) {
          return;
        }

        this.externalStepUpHandled = true;
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { externalStepUp: null },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });

        const pending = readPendingTwoFactorStepUp();
        if (!pending) {
          return;
        }

        clearPendingTwoFactorStepUp();
        this.stepUpAction.set(pending.action);
        this.resumeStepUpAfterExternalReturn.set(true);
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
    this.stepUpForm.reset();
  }

  startExternalStepUp(providerKey: string): void {
    const action = this.stepUpAction();
    if (!action || this.providerLoadingKey()) {
      return;
    }

    if (this.providerLoadingKey()) {
      return;
    }

    if (
      !this.needsExternalReauth() &&
      this.stepUpForm.controls.verificationCode.invalid
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    storePendingTwoFactorStepUp({
      action,
      verificationCode: this.stepUpForm.controls.verificationCode.value.trim()
    });

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalProviderStepUp(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          clearPendingTwoFactorStepUp();
          this.providerLoadingKey.set(null);
          this.stepUpError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  submitStepUp(): void {
    const action = this.stepUpAction();
    if (!action || this.isStepUpSubmitting()) {
      if (!action) {
        return;
      }
      this.stepUpForm.markAllAsTouched();
      return;
    }

    if (
      this.requiresPasswordStepUp() &&
      !this.stepUpForm.controls.currentPassword.value.trim()
    ) {
      this.stepUpForm.controls.currentPassword.markAsTouched();
      return;
    }

    if (this.stepUpForm.controls.verificationCode.invalid) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    const request = {
      currentPassword: this.stepUpForm.controls.currentPassword.value || null,
      verificationCode: this.stepUpForm.controls.verificationCode.value.trim() || null
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
            this.providerLoadingKey.set(null);
            this.closeStepUp();
            this.toastService.success(AuthMessages.twoFactorDisabled);
            this.accountChanged.emit();
          },
          error: (error: unknown) => {
            this.stepUpError.set(
              error instanceof Error ? error.message : 'Unable to complete this action.'
            );
            this.isStepUpSubmitting.set(false);
            this.providerLoadingKey.set(null);
          }
        });
      return;
    }

    this.authService
      .regenerateRecoveryCodes(request)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.isStepUpSubmitting.set(false);
          this.providerLoadingKey.set(null);
          this.closeStepUp();
          this.recoveryCodes.set(result.recoveryCodes);
          this.recoveryCodesSaved.setValue(false);
        },
        error: (error: unknown) => {
          this.stepUpError.set(
            error instanceof Error ? error.message : 'Unable to complete this action.'
          );
          this.isStepUpSubmitting.set(false);
          this.providerLoadingKey.set(null);
        }
      });
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
    this.stepUpForm.reset();
    this.stepUpError.set('');
    this.providerLoadingKey.set(null);
    this.stepUpVisible.set(true);
  }
}
