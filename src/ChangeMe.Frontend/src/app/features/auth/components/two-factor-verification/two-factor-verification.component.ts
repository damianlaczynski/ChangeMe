import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  clearTwoFactorChallenge,
  readTwoFactorChallenge
} from '@features/auth/utils/two-factor-challenge.storage';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-two-factor-verification',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    RouterLink,
    Button,
    InputText,
    Message
  ],
  templateUrl: './two-factor-verification.component.html'
})
export class TwoFactorVerificationComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authMessages = AuthMessages;

  readonly form = new FormGroup({
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(64)]
    })
  });

  constructor() {
    if (!readTwoFactorChallenge()) {
      void this.router.navigateByUrl('/login');
    }
  }

  clearChallenge(): void {
    clearTwoFactorChallenge();
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const challenge = readTwoFactorChallenge();
    if (!challenge) {
      void this.router.navigateByUrl('/login');
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    this.authService
      .verifyTwoFactor({
        challengeId: challenge.challengeId,
        verificationCode: this.form.controls.verificationCode.value.trim()
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          if (this.authService.passwordChangeRequired()) {
            this.authService.enablePasswordChangeScreen();
            void this.router.navigateByUrl('/required-password-change');
            return;
          }

          if (this.authService.twoFactorSetupRequired()) {
            this.authService.enableTwoFactorSetupScreen();
            void this.router.navigateByUrl('/required-two-factor-setup');
            return;
          }

          void this.router.navigateByUrl('/issues');
        },
        error: (error) => {
          const message =
            error instanceof Error
              ? error.message
              : AuthMessages.invalidVerificationCode;
          if (
            message === AuthMessages.signInTimedOut ||
            message === AuthMessages.tooManyAttempts
          ) {
            clearTwoFactorChallenge();
            void this.router.navigate(['/login'], {
              queryParams: {
                twoFactorError:
                  message === AuthMessages.tooManyAttempts ? 'attempts' : 'timeout'
              }
            });
            return;
          }

          this.errorMessage.set(
            message === AuthMessages.invalidVerificationCode
              ? AuthMessages.invalidVerificationCode
              : message
          );
          this.isSubmitting.set(false);
        },
        complete: () => {
          this.isSubmitting.set(false);
        }
      });
  }
}
