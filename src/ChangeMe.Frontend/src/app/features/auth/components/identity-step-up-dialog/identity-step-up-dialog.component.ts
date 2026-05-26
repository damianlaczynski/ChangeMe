import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  input,
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
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildExternalReauthRequiredDetail,
  needsExternalReauth
} from '@features/auth/utils/external-step-up.utils';
import { getPasskeyCeremonyErrorMessage } from '@features/auth/utils/passkey.utils';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-identity-step-up-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, Message, Password, InputText],
  templateUrl: './identity-step-up-dialog.component.html'
})
export class IdentityStepUpDialogComponent {
  readonly account = input.required<MyAccountDto>();
  readonly visible = input.required<boolean>();
  readonly passkeysEnabled = input(false);
  readonly stepUpValidityMinutes = input(15);
  readonly submitLabel = input('Continue');
  readonly submitting = input(false);
  readonly errorMessage = input('');
  readonly fieldIdPrefix = input('identityStepUp');
  readonly beforeExternalRedirect = input<
    (context: { verificationCode: string }) => boolean
  >(() => true);

  readonly verified = output<StepUpVerificationResult>();
  readonly cancelled = output<void>();
  readonly visibleChange = output<boolean>();
  readonly externalRedirectFailed = output<void>();

  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly authMessages = AuthMessages;

  readonly providerLoadingKey = signal<string | null>(null);
  readonly isPasskeySubmitting = signal(false);
  private readonly localError = signal('');
  private readonly passkeyVerified = signal(false);

  readonly form = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(64)]
    })
  });

  readonly linkedProviders = computed(() => this.account().externalLogins ?? []);
  readonly requiresPassword = computed(() => this.account().hasPasswordSet);
  readonly requiresVerificationCode = computed(() => this.account().twoFactorEnabled);
  readonly needsExternalReauth = computed(() => needsExternalReauth(this.account()));
  readonly canOfferPasskey = computed(
    () => this.passkeysEnabled() && this.account().passkeys.length > 0
  );
  readonly externalReauthDetail = computed(() =>
    buildExternalReauthRequiredDetail(this.stepUpValidityMinutes())
  );
  readonly displayError = computed(() => this.errorMessage() || this.localError());

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.account();
      this.resetForm();
    });

    effect(() => {
      if (this.requiresVerificationCode()) {
        this.form.controls.verificationCode.setValidators([
          Validators.required,
          Validators.maxLength(64)
        ]);
      } else {
        this.form.controls.verificationCode.setValidators([Validators.maxLength(64)]);
      }

      this.form.controls.verificationCode.updateValueAndValidity({ emitEvent: false });
    });
  }

  fieldId(suffix: string): string {
    return `${this.fieldIdPrefix()}${suffix}`;
  }

  onVisibleChange(nextVisible: boolean): void {
    if (!nextVisible) {
      this.cancel();
    }
  }

  cancel(): void {
    this.resetForm();
    this.visibleChange.emit(false);
    this.cancelled.emit();
  }

  submit(): void {
    if (this.submitting() || this.needsExternalReauth()) {
      return;
    }

    if (!this.validateCredentials()) {
      return;
    }

    this.localError.set('');
    this.verified.emit(this.buildVerificationResult(false));
  }

  verifyWithPasskey(): void {
    if (this.isPasskeySubmitting()) {
      return;
    }

    if (!this.validateVerificationCode()) {
      return;
    }

    this.isPasskeySubmitting.set(true);
    this.localError.set('');

    this.authService
      .verifyWithPasskey()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isPasskeySubmitting.set(false);
          this.toastService.success(AuthMessages.passkeyStepUpCompleted);
          this.passkeyVerified.set(true);
          this.localError.set('');
          this.verified.emit(this.buildVerificationResult(true));
        },
        error: (error: unknown) => {
          this.isPasskeySubmitting.set(false);
          const message = getPasskeyCeremonyErrorMessage(
            error,
            AuthMessages.passkeySignInFailed
          );
          if (message) {
            this.localError.set(message);
          }
        }
      });
  }

  startExternalStepUp(providerKey: string): void {
    if (this.providerLoadingKey()) {
      return;
    }

    if (!this.validateVerificationCode()) {
      return;
    }

    const context = {
      verificationCode: this.form.controls.verificationCode.value.trim()
    };
    if (!this.beforeExternalRedirect()(context)) {
      return;
    }

    this.providerLoadingKey.set(providerKey);
    this.localError.set('');

    this.authService
      .beginExternalProviderStepUp(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          this.providerLoadingKey.set(null);
          this.externalRedirectFailed.emit();
          this.localError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  private validateCredentials(): boolean {
    if (
      this.requiresPassword() &&
      !this.passkeyVerified() &&
      !this.form.controls.currentPassword.value.trim()
    ) {
      this.form.controls.currentPassword.markAsTouched();
      return false;
    }

    return this.validateVerificationCode();
  }

  private validateVerificationCode(): boolean {
    if (
      this.requiresVerificationCode() &&
      this.form.controls.verificationCode.invalid
    ) {
      this.form.controls.verificationCode.markAsTouched();
      return false;
    }

    return true;
  }

  private buildVerificationResult(passkeyVerified: boolean): StepUpVerificationResult {
    return {
      currentPassword:
        passkeyVerified || !this.requiresPassword()
          ? null
          : this.form.controls.currentPassword.value.trim() || null,
      verificationCode: this.requiresVerificationCode()
        ? this.form.controls.verificationCode.value.trim() || null
        : null,
      passkeyVerified
    };
  }

  private resetForm(): void {
    this.form.reset();
    this.localError.set('');
    this.providerLoadingKey.set(null);
    this.isPasskeySubmitting.set(false);
    this.passkeyVerified.set(false);
  }
}
