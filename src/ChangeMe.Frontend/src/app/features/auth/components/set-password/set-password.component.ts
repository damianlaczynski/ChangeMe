import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountDto, PasswordPolicySettings } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { ExternalStepUpReturnService } from '@features/auth/services/external-step-up-return.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import {
  clearPendingSetPassword,
  readPendingSetPassword,
  storePendingSetPassword
} from '@features/auth/utils/pending-set-password.storage';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Password } from 'primeng/password';

function newPasswordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const group = control as FormGroup;
  const newPassword = group.controls['newPassword']?.value;
  const confirm = group.controls['confirmNewPassword']?.value;
  return newPassword === confirm ? null : { passwordMismatch: true };
}

@Component({
  selector: 'app-set-password',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Panel,
    Password,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './set-password.component.html'
})
export class SetPasswordComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly externalStepUpReturn = inject(ExternalStepUpReturnService);

  readonly account = signal<MyAccountDto | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly stepUpVisible = signal(false);
  readonly stepUpError = signal('');
  readonly passkeysEnabled = signal(false);
  readonly stepUpValidityMinutes = signal(15);
  readonly authMessages = AuthMessages;

  readonly form = new FormGroup(
    {
      newPassword: new FormControl('', {
        nonNullable: true,
        validators: buildPasswordPolicyValidators(defaultPasswordPolicySettings())
      }),
      confirmNewPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    { validators: [newPasswordMatchValidator] }
  );

  ngOnInit(): void {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.applyPasswordPolicy(settings.passwordPolicy);
          this.stepUpValidityMinutes.set(
            settings.twoFactor.stepUpExternalSignInValidityMinutes
          );
          this.passkeysEnabled.set(
            settings.passkeys?.passkeysAuthenticationEnabled ?? false
          );
        }
      });

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          if (account.hasPasswordSet) {
            void this.router.navigateByUrl('/account');
            return;
          }
          this.account.set(account);
          this.restorePendingPassword();
        },
        error: () => void this.router.navigateByUrl('/account')
      });

    this.externalStepUpReturn.watchReturnAndRefreshAccount(
      this.destroyRef,
      this.route,
      () => this.authService.getMyAccount(),
      (account) => {
        if (account.hasPasswordSet) {
          void this.router.navigateByUrl('/account');
          return;
        }

        this.account.set(account);
        this.restorePendingPassword();
        this.stepUpError.set('');
        this.stepUpVisible.set(true);
      }
    );
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.stepUpError.set('');
    clearPendingSetPassword();
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.stepUpError.set('');

    this.authService
      .setPassword({
        newPassword: this.form.controls.newPassword.value,
        currentPassword: result.currentPassword,
        verificationCode: result.verificationCode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          clearPendingSetPassword();
          this.closeStepUp();
          this.toastService.success(AuthMessages.passwordSet);
          void this.router.navigateByUrl('/account');
        },
        error: (error) => {
          this.stepUpError.set(
            error instanceof Error ? error.message : 'Unable to set password.'
          );
          this.isSubmitting.set(false);
        },
        complete: () => {
          this.isSubmitting.set(false);
        }
      });
  }

  readonly prepareExternalRedirect = (): boolean => {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return false;
    }

    storePendingSetPassword({ newPassword: this.form.controls.newPassword.value });
    return true;
  };

  onExternalRedirectFailed(): void {
    clearPendingSetPassword();
  }

  shouldShowError(control: AbstractControl): boolean {
    return control.touched && control.invalid;
  }

  private applyPasswordPolicy(policy: PasswordPolicySettings): void {
    this.form.controls.newPassword.setValidators(buildPasswordPolicyValidators(policy));
    this.form.controls.newPassword.updateValueAndValidity();
  }

  private restorePendingPassword(): void {
    const pending = readPendingSetPassword();
    if (!pending) {
      return;
    }

    this.form.controls.newPassword.setValue(pending.newPassword);
    this.form.controls.confirmNewPassword.setValue(pending.newPassword);
  }
}
