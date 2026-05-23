import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-login',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    RouterLink,
    Button,
    Checkbox,
    InputText,
    Password,
    Message
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly errorMessage = signal('');
  readonly infoMessage = signal(this.readLoginInfoMessage());
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly showEmailVerificationResend = signal(false);
  readonly publicRegistrationEnabled = signal(true);

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(AuthConstraints.EMAIL_MAX_LENGTH)
      ]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(AuthConstraints.PASSWORD_MIN_LENGTH),
        Validators.maxLength(AuthConstraints.PASSWORD_MAX_LENGTH)
      ]
    }),
    rememberMe: new FormControl(false, { nonNullable: true })
  });

  constructor() {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) =>
          this.publicRegistrationEnabled.set(settings.publicRegistrationEnabled)
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.showEmailVerificationResend.set(false);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        if (this.authService.passwordChangeRequired()) {
          void this.router.navigateByUrl('/required-password-change');
          return;
        }

        const returnUrl =
          this.route.snapshot.queryParamMap.get('returnUrl') ?? '/issues';
        void this.router.navigateByUrl(returnUrl);
      },
      error: (error) => {
        const message =
          error instanceof Error ? error.message : AuthMessages.invalidCredentials;
        if (message === AuthMessages.deactivatedAccount) {
          this.errorMessage.set(message);
        } else if (message === AuthMessages.emailNotVerified) {
          this.errorMessage.set(message);
          this.showEmailVerificationResend.set(true);
        } else {
          this.errorMessage.set(
            message === 'Please sign in to continue.' ||
              message === AuthMessages.invalidCredentials
              ? AuthMessages.invalidCredentials
              : message
          );
        }
        this.isSubmitting.set(false);
      },
      complete: () => {
        this.isSubmitting.set(false);
      }
    });
  }

  private readLoginInfoMessage(): string {
    const params = this.route.snapshot.queryParamMap;
    if (params.get('accountActivated') === '1') {
      return AuthMessages.accountActivatedLogin;
    }
    if (params.get('passwordReset') === '1') {
      return AuthMessages.passwordResetLogin;
    }
    if (params.get('passwordChanged') === '1') {
      return AuthMessages.passwordChangedLogin;
    }
    if (params.get('emailVerified') === '1') {
      return AuthMessages.emailVerifiedLogin;
    }
    if (params.get('registrationDisabled') === '1') {
      return AuthMessages.registrationDisabled;
    }
    return '';
  }
}
