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
import { MyAccountDto, PasswordPolicySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildExternalReauthRequiredDetail,
  needsExternalReauth
} from '@features/auth/utils/external-step-up.utils';
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
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
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
    Dialog,
    InputText
  ],
  templateUrl: './set-password.component.html'
})
export class SetPasswordComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  private externalStepUpHandled = false;

  readonly account = signal<MyAccountDto | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly stepUpVisible = signal(false);
  readonly stepUpError = signal('');
  readonly providerLoadingKey = signal<string | null>(null);
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

  readonly stepUpForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(64)]
    })
  });

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
          this.handleExternalStepUpReturn();
        },
        error: () => void this.router.navigateByUrl('/account')
      });

    this.route.queryParamMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (this.account()) {
        this.handleExternalStepUpReturn();
      }
    });
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
    this.stepUpForm.reset();
    clearPendingSetPassword();
  }

  submitWithStepUp(): void {
    const account = this.account();
    if (!account || this.isSubmitting()) {
      return;
    }

    if (
      account.twoFactorEnabled &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.stepUpError.set('');

    this.authService
      .setPassword({
        newPassword: this.form.controls.newPassword.value,
        currentPassword: account.hasPasswordSet
          ? this.stepUpForm.controls.currentPassword.value || null
          : null,
        verificationCode: this.stepUpForm.controls.verificationCode.value.trim() || null
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

  startExternalStepUp(providerKey: string): void {
    const account = this.account();
    if (!account || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (
      account.twoFactorEnabled &&
      !this.needsExternalReauth() &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    storePendingSetPassword({ newPassword: this.form.controls.newPassword.value });

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalProviderStepUp(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          clearPendingSetPassword();
          this.providerLoadingKey.set(null);
          this.stepUpError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  shouldShowError(control: AbstractControl): boolean {
    return control.touched && control.invalid;
  }

  requiresExternalStepUp(): boolean {
    const account = this.account();
    return !!account && !account.hasPasswordSet && account.externalLogins.length > 0;
  }

  needsExternalReauth(): boolean {
    const account = this.account();
    return !!account && needsExternalReauth(account);
  }

  externalReauthDetail(): string {
    return buildExternalReauthRequiredDetail(this.stepUpValidityMinutes());
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

  private handleExternalStepUpReturn(): void {
    const params = this.route.snapshot.queryParamMap;
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

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.restorePendingPassword();
          this.stepUpError.set('');
          this.stepUpVisible.set(true);
        }
      });
  }
}
