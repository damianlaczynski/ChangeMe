import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { PasswordPolicySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-required-password-change-form',
  imports: [ReactiveFormsModule, Button, Message, Panel, Password],
  templateUrl: './required-password-change-form.component.html'
})
export class RequiredPasswordChangeFormComponent {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly showLogout = input(false);
  readonly submitLabel = input('Change password');
  readonly submitted = output<string>();
  readonly logoutRequested = output<void>();

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
    this.submitted.emit(this.form.getRawValue().newPassword);
  }

  reportError(message: string): void {
    this.submitError.set(message);
    this.isSubmitting.set(false);
  }

  resetAfterSuccess(): void {
    this.submitError.set(null);
    this.isSubmitting.set(false);
    this.form.reset();
  }

  onLogout(): void {
    this.logoutRequested.emit();
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
