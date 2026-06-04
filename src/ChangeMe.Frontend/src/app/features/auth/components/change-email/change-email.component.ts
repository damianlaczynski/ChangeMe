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
import { MyAccountDto } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';

@Component({
  selector: 'app-change-email',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Panel,
    InputText,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './change-email.component.html'
})
export class ChangeEmailComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly account = signal<MyAccountDto | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly stepUpVisible = signal(false);
  readonly stepUpError = signal('');
  readonly passkeysEnabled = signal(false);
  readonly stepUpValidityMinutes = signal(15);
  readonly externalNotice = signal<string | null>(null);

  readonly form = new FormGroup({
    newEmail: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(AuthConstraints.EMAIL_MAX_LENGTH)
      ]
    })
  });

  ngOnInit(): void {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          if (!settings.selfServiceEmailChangeEnabled) {
            void this.router.navigate(['/account']);
            return;
          }

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
        next: (account) => {
          if (account.invitationPending || account.pendingEmailChange) {
            void this.router.navigate(['/account']);
            return;
          }

          this.account.set(account);
          if (account.externalLogins.length > 0) {
            this.externalNotice.set(
              `External sign-in methods stay linked. Notifications stay on your profile email (${account.email}). Provider addresses may differ.`
            );
          }
        },
        error: () => void this.router.navigate(['/account'])
      });
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    this.submitError.set(null);

    if (this.form.invalid) {
      return;
    }

    const profile = this.account();
    if (!profile) {
      return;
    }

    const newEmail = this.form.controls.newEmail.value.trim();
    if (newEmail.toLowerCase() === profile.email.toLowerCase()) {
      this.submitError.set('New email must differ from your current email.');
      return;
    }

    this.stepUpVisible.set(true);
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.stepUpError.set('');
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.stepUpError.set('');
    this.submitError.set(null);

    this.authService
      .requestEmailChange({
        newEmail: this.form.controls.newEmail.value.trim(),
        currentPassword: result.currentPassword,
        verificationCode: result.verificationCode
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.closeStepUp();
          this.toastService.success(
            'Check the new email address for a confirmation link.'
          );
          void this.router.navigate(['/account']);
        },
        error: (error) => {
          const message =
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed;
          if (this.stepUpVisible()) {
            this.stepUpError.set(message);
          } else {
            this.submitError.set(message);
          }
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }

  shouldShowError(control: FormControl): boolean {
    return control.invalid && (control.dirty || control.touched);
  }
}
