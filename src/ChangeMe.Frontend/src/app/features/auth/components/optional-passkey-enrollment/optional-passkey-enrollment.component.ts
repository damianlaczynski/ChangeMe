import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountDto, TwoFactorStepUpRequest } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import {
  getPasskeyCeremonyErrorMessage,
  isPasskeySupported
} from '@features/auth/utils/passkey.utils';
import { clearPendingPasskeyEnrollmentOffer } from '@features/auth/utils/pending-passkey-enrollment.storage';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { finalize, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-optional-passkey-enrollment',
  imports: [
    ReactiveFormsModule,
    Card,
    Button,
    Dialog,
    InputText,
    Message,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './optional-passkey-enrollment.component.html'
})
export class OptionalPasskeyEnrollmentComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly AuthMessages = AuthMessages;
  readonly title = AuthMessages.optionalPasskeyEnrollmentTitle;
  readonly subtitle = AuthMessages.optionalPasskeyEnrollmentSubtitle;
  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

  readonly account = signal<MyAccountDto | null>(null);
  readonly passkeysEnabled = signal(false);
  readonly stepUpValidityMinutes = signal(15);
  readonly errorMessage = signal('');
  readonly stepUpVisible = signal(false);
  readonly stepUpError = signal('');
  readonly isRegisteringCeremony = signal(false);
  readonly isSaving = signal(false);
  readonly nameDialogVisible = signal(false);
  readonly passkeySupported = signal(isPasskeySupported());

  private stepUpCredentials: TwoFactorStepUpRequest | null = null;
  private pendingCeremony: { ceremonyId: string; attestationResponse: unknown } | null =
    null;

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    })
  });

  ngOnInit(): void {
    if (!this.authService.isAuthenticated() || !this.passkeySupported()) {
      this.finishWithoutEnrollment();
      return;
    }

    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          this.passkeysEnabled.set(
            settings.passkeys?.passkeysAuthenticationEnabled === true
          );
          this.stepUpValidityMinutes.set(
            settings.twoFactor.stepUpExternalSignInValidityMinutes
          );
        }
      });

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (profile) => this.account.set(profile),
        error: () => this.finishWithoutEnrollment()
      });
  }

  startPasskeyRegistration(): void {
    if (this.isRegisteringCeremony() || !this.passkeySupported() || !this.account()) {
      return;
    }

    this.errorMessage.set('');
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.stepUpError.set('');
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    this.stepUpCredentials = {
      currentPassword: result.currentPassword,
      verificationCode: result.verificationCode
    };
    this.stepUpVisible.set(false);
    this.stepUpError.set('');
    this.runPasskeyRegistrationCeremony();
  }

  savePasskeyName(): void {
    if (this.isSaving() || !this.pendingCeremony || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const name = this.form.controls.name.value.trim();
    const ceremony = this.pendingCeremony;

    this.authService
      .completePasskeyRegistrationAfterCeremony(
        ceremony,
        name,
        this.stepUpCredentials ?? undefined
      )
      .pipe(
        switchMap(() => this.authService.refreshSession()),
        finalize(() => this.isSaving.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyAdded);
          this.finishWithoutEnrollment();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to add passkey.'
          );
        }
      });
  }

  closeNameDialog(): void {
    this.nameDialogVisible.set(false);
    this.pendingCeremony = null;
    this.stepUpCredentials = null;
    this.errorMessage.set('');
  }

  skip(): void {
    this.finishWithoutEnrollment();
  }

  private runPasskeyRegistrationCeremony(): void {
    if (this.isRegisteringCeremony()) {
      return;
    }

    this.isRegisteringCeremony.set(true);
    this.errorMessage.set('');

    this.authService
      .performPasskeyRegistrationCeremony(this.stepUpCredentials ?? undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ceremony) => {
          this.isRegisteringCeremony.set(false);
          this.pendingCeremony = ceremony;
          this.form.reset({ name: 'Passkey 1' });
          this.nameDialogVisible.set(true);
        },
        error: (error: unknown) => {
          this.isRegisteringCeremony.set(false);
          this.stepUpCredentials = null;
          const message = getPasskeyCeremonyErrorMessage(
            error,
            'Unable to add passkey.'
          );
          if (message) {
            this.errorMessage.set(message);
          }
        },
        complete: () => {
          this.isRegisteringCeremony.set(false);
        }
      });
  }

  private finishWithoutEnrollment(): void {
    clearPendingPasskeyEnrollmentOffer();
    void this.router.navigateByUrl('/issues');
  }
}
