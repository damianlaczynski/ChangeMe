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
import { ExternalProviderSettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { clearExternalAccountFlow } from '@features/auth/utils/external-account-flow.storage';
import { readTwoFactorChallenge } from '@features/auth/utils/two-factor-challenge.storage';
import { Button } from 'primeng/button';
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
  readonly infoMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly showEmailVerificationResend = signal(false);
  readonly publicRegistrationEnabled = signal(true);
  readonly externalProvidersEnabled = signal(false);
  readonly externalProviders = signal<ExternalProviderSettings[]>([]);
  readonly externalProviderLoadingKey = signal<string | null>(null);

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
    })
  });

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const messages = this.readLoginQueryMessages(params);
        this.infoMessage.set(messages.info);
        this.errorMessage.set(messages.error);
      });

    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.publicRegistrationEnabled.set(settings.publicRegistrationEnabled);
          this.externalProvidersEnabled.set(settings.externalProvidersEnabled);
          this.externalProviders.set(settings.externalProviders);
        }
      });
  }

  beginExternalSignIn(provider: ExternalProviderSettings): void {
    if (this.externalProviderLoadingKey()) {
      return;
    }

    this.errorMessage.set('');
    this.externalProviderLoadingKey.set(provider.providerKey);
    clearExternalAccountFlow();

    this.authService
      .beginExternalSignIn(provider.providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          window.location.assign(response.authorizationUrl);
        },
        error: (error) => {
          this.externalProviderLoadingKey.set(null);
          this.errorMessage.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
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
          this.authService.enablePasswordChangeScreen();
          void this.router.navigateByUrl('/required-password-change');
          return;
        }

        const challenge = readTwoFactorChallenge();
        if (challenge) {
          void this.router.navigateByUrl('/two-factor-verification');
          return;
        }

        if (this.authService.twoFactorSetupRequired()) {
          this.authService.enableTwoFactorSetupScreen();
          void this.router.navigateByUrl('/required-two-factor-setup');
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

  private readLoginQueryMessages(params: { get(name: string): string | null }): {
    info: string;
    error: string;
  } {
    const externalMessage = params.get('externalSignInMessage');
    if (externalMessage) {
      return { info: '', error: externalMessage };
    }
    if (params.get('externalSignInError') === '1') {
      return { info: '', error: AuthMessages.externalSignInFailed };
    }

    const returnUrl = params.get('returnUrl');
    if (returnUrl) {
      const queryIndex = returnUrl.indexOf('?');
      if (queryIndex >= 0) {
        const returnParams = new URLSearchParams(returnUrl.slice(queryIndex + 1));
        const returnMessage = returnParams.get('externalSignInMessage');
        if (returnMessage) {
          return { info: '', error: returnMessage };
        }
        if (returnParams.get('externalSignInError') === '1') {
          return { info: '', error: AuthMessages.externalSignInFailed };
        }
      }
    }

    if (params.get('accountActivated') === '1') {
      return { info: AuthMessages.accountActivatedLogin, error: '' };
    }
    if (params.get('passwordReset') === '1') {
      return { info: AuthMessages.passwordResetLogin, error: '' };
    }
    if (params.get('passwordChanged') === '1') {
      return { info: AuthMessages.passwordChangedLogin, error: '' };
    }
    if (params.get('emailVerified') === '1') {
      return { info: AuthMessages.emailVerifiedLogin, error: '' };
    }
    if (params.get('registrationDisabled') === '1') {
      return { info: AuthMessages.registrationDisabled, error: '' };
    }

    return { info: '', error: '' };
  }
}
