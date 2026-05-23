import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { PasswordPolicySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-required-password-change',
  imports: [ReactiveFormsModule, Card, Button, Message, Panel, Password],
  templateUrl: './required-password-change.component.html'
})
export class RequiredPasswordChangeComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly title = AuthMessages.requiredPasswordChangeTitle;
  readonly subtitle = AuthMessages.requiredPasswordChangeSubtitle;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

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

  constructor() {
    this.applyPasswordPolicy(defaultPasswordPolicySettings());

    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => this.applyPasswordPolicy(settings.passwordPolicy)
      });
  }

  onSubmit(): void {
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const { newPassword } = this.form.getRawValue();

    this.authService
      .requiredChangePassword({ newPassword })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.authService.clearPasswordChangeRequired();
          this.toastService.success(AuthMessages.passwordUpdated);
          void this.router.navigateByUrl('/issues');
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/login');
        },
        error: () => {
          void this.router.navigateByUrl('/login');
        }
      });
  }

  shouldShowError(control: FormControl<string>): boolean {
    return control.touched && control.invalid;
  }

  private applyPasswordPolicy(policy: PasswordPolicySettings): void {
    this.form.controls.newPassword.setValidators(buildPasswordPolicyValidators(policy));
    this.form.controls.newPassword.updateValueAndValidity();
  }
}

function newPasswordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmNewPassword = control.get('confirmNewPassword')?.value;

  if (!newPassword || !confirmNewPassword) {
    return null;
  }

  return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
}
