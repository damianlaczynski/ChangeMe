import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-verify-email',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    BackButtonComponent,
    Button,
    InputText,
    Message
  ],
  templateUrl: './verify-email.component.html'
})
export class VerifyEmailComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly infoMessage = signal('');
  readonly successMessage = signal('');
  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly isVerifyingToken = signal(false);
  readonly emailReadOnly = signal(false);
  readonly authConstraints = AuthConstraints;

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(AuthConstraints.EMAIL_MAX_LENGTH)
      ]
    })
  });

  ngOnInit(): void {
    const email = this.route.snapshot.queryParamMap.get('email')?.trim();
    if (email) {
      this.form.controls.email.setValue(email);
      this.emailReadOnly.set(true);
    }

    const accountCreated = this.route.snapshot.queryParamMap.get('accountCreated');
    if (accountCreated === '1') {
      this.infoMessage.set(AuthMessages.accountCreatedVerifyEmail);
    }

    const token = this.route.snapshot.queryParamMap.get('token')?.trim();
    if (token) {
      this.verifyToken(token);
    }
  }

  onResend(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');
    this.successMessage.set('');

    this.authService
      .requestEmailVerification(this.form.controls.email.value.trim())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (ack) => {
          this.successMessage.set(
            ack.message || AuthMessages.emailVerificationResendSuccess
          );
          this.isSubmitting.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  private verifyToken(token: string): void {
    this.isVerifyingToken.set(true);
    this.errorMessage.set('');

    this.authService
      .verifyEmail(token)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigate(['/login'], {
            queryParams: { emailVerified: '1' }
          });
        },
        error: (error: Error) => {
          this.errorMessage.set(
            error.message === AuthMessages.invalidEmailVerificationLink
              ? error.message
              : AuthMessages.invalidEmailVerificationLink
          );
          this.isVerifyingToken.set(false);
        }
      });
  }
}
