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
import { ActivatedRoute, Router } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import {
  ExternalProviderSettings,
  PasswordPolicySettings
} from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { clearExternalAccountFlow } from '@features/auth/utils/external-account-flow.storage';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-accept-invitation',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    Button,
    InputText,
    Password,
    Message,
    ProgressSpinner
  ],
  templateUrl: './accept-invitation.component.html'
})
export class AcceptInvitationComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly token = this.route.snapshot.queryParamMap.get('token') ?? '';
  readonly isLoadingPreview = signal(true);
  readonly isInvitationValid = signal(false);
  readonly previewEmail = signal<string | null>(null);
  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly invalidInvitationMessage = AuthMessages.invalidInvitationLink;
  readonly externalProvidersEnabled = signal(false);
  readonly externalProviders = signal<ExternalProviderSettings[]>([]);
  readonly externalProviderLoadingKey = signal<string | null>(null);

  readonly form = new FormGroup(
    {
      firstName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
        ]
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
        ]
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: buildPasswordPolicyValidators(defaultPasswordPolicySettings())
      }),
      confirmPassword: new FormControl('', {
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
        next: (settings) => {
          this.applyPasswordPolicy(settings.passwordPolicy);
          this.externalProvidersEnabled.set(settings.externalProvidersEnabled);
          this.externalProviders.set(settings.externalProviders);
        }
      });

    if (!this.token) {
      this.isLoadingPreview.set(false);
      this.isInvitationValid.set(false);
      return;
    }

    this.authService
      .getInvitationPreview(this.token)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (preview) => {
          this.isInvitationValid.set(preview.isValid);
          if (preview.isValid) {
            this.previewEmail.set(preview.email);
            this.form.patchValue({
              firstName: preview.firstName,
              lastName: preview.lastName
            });
          }
          this.isLoadingPreview.set(false);
        },
        error: () => {
          this.isInvitationValid.set(false);
          this.isLoadingPreview.set(false);
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
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          this.externalProviderLoadingKey.set(null);
          this.errorMessage.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  onSubmit(): void {
    if (!this.isInvitationValid() || this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    const { firstName, lastName, password } = this.form.getRawValue();

    this.authService
      .acceptInvitation({
        token: this.token,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        password
      })
      .subscribe({
        next: () => {
          void this.router.navigate(['/login'], {
            queryParams: { accountActivated: '1' }
          });
        },
        error: (error) => {
          const message = error instanceof Error ? error.message : 'Activation failed.';
          this.errorMessage.set(
            message.includes('invalid') || message.includes('expired')
              ? AuthMessages.invalidInvitationLink
              : message
          );
          this.isSubmitting.set(false);
        },
        complete: () => {
          this.isSubmitting.set(false);
        }
      });
  }

  private applyPasswordPolicy(policy: PasswordPolicySettings): void {
    this.form.controls.password.setValidators(buildPasswordPolicyValidators(policy));
    this.form.controls.password.updateValueAndValidity();
  }
}

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
}
