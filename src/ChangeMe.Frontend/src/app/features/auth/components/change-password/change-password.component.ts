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
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-change-password',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Password
  ],
  templateUrl: './change-password.component.html'
})
export class ChangePasswordComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly changePasswordNotice = AuthMessages.changePasswordNotice;

  readonly form = new FormGroup(
    {
      currentPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      }),
      newPassword: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.minLength(AuthConstraints.PASSWORD_MIN_LENGTH),
          Validators.maxLength(AuthConstraints.PASSWORD_MAX_LENGTH)
        ]
      }),
      confirmNewPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    { validators: [newPasswordMatchValidator] }
  );

  onSubmit(): void {
    this.submitError.set(null);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.confirmationService.confirm({
      header: AuthMessages.changePasswordTitle,
      message: AuthMessages.changePasswordMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Confirm', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.savePassword()
    });
  }

  private savePassword(): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const { currentPassword, newPassword } = this.form.getRawValue();

    this.authService
      .changePassword({ currentPassword, newPassword })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.authService.clearLocalSession();
          void this.router.navigate(['/login'], {
            queryParams: { passwordChanged: '1' }
          });
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  shouldShowError(control: FormControl<string>): boolean {
    return control.touched && control.invalid;
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
