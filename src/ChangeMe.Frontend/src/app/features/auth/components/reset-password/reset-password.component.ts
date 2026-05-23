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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { PasswordPolicySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-reset-password',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    BackButtonComponent,
    RouterLink,
    Button,
    Password,
    Message,
    ProgressSpinner
  ],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  readonly isLoadingPreview = signal(true);
  readonly isTokenValid = signal(false);
  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly invalidLinkMessage = AuthMessages.invalidPasswordResetLink;

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
    { validators: [passwordMatchValidator] }
  );

  constructor() {
    this.applyPasswordPolicy(defaultPasswordPolicySettings());

    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => this.applyPasswordPolicy(settings.passwordPolicy)
      });

    if (!this.token) {
      this.isLoadingPreview.set(false);
      this.isTokenValid.set(false);
      return;
    }

    this.authService
      .getPasswordResetPreview(this.token)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (preview) => {
          this.isTokenValid.set(preview.isValid);
          this.isLoadingPreview.set(false);
        },
        error: () => {
          this.isTokenValid.set(false);
          this.isLoadingPreview.set(false);
        }
      });
  }

  onSubmit(): void {
    if (!this.isTokenValid() || this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const { newPassword } = this.form.getRawValue();

    this.authService
      .resetPassword({ token: this.token, newPassword })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigate(['/login'], {
            queryParams: { passwordReset: '1' }
          });
        },
        error: (error: Error) => {
          const message =
            error instanceof Error ? error.message : 'Password reset failed.';
          this.errorMessage.set(
            message.includes('invalid') || message.includes('expired')
              ? AuthMessages.invalidPasswordResetLink
              : message
          );
          this.isSubmitting.set(false);
        }
      });
  }

  private applyPasswordPolicy(policy: PasswordPolicySettings): void {
    this.form.controls.newPassword.setValidators(buildPasswordPolicyValidators(policy));
    this.form.controls.newPassword.updateValueAndValidity();
  }
}

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const newPassword = control.get('newPassword')?.value;
  const confirmNewPassword = control.get('confirmNewPassword')?.value;

  if (!newPassword || !confirmNewPassword) {
    return null;
  }

  return newPassword === confirmNewPassword ? null : { passwordMismatch: true };
}
