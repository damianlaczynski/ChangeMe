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
import { TwoFactorSetupQrComponent } from '@features/auth/components/two-factor-setup-qr/two-factor-setup-qr.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Checkbox } from 'primeng/checkbox';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-required-two-factor-setup',
  imports: [
    ReactiveFormsModule,
    Card,
    Button,
    InputText,
    Password,
    Message,
    Checkbox,
    TwoFactorSetupQrComponent,
    ProgressSpinner
  ],
  templateUrl: './required-two-factor-setup.component.html'
})
export class RequiredTwoFactorSetupComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly title = AuthMessages.twoFactorSetupRequiredTitle;
  readonly subtitle = AuthMessages.twoFactorSetupRequiredSubtitle;
  readonly errorMessage = signal('');
  readonly requiresPassword = signal(true);
  readonly isBeginning = signal(false);
  readonly isConfirming = signal(false);
  readonly setupPreview = signal<{
    sharedSecret: string;
    provisioningUri: string;
    issuerName: string;
  } | null>(null);
  readonly recoveryCodes = signal<string[]>([]);
  readonly recoveryCodesSaved = new FormControl(false, {
    nonNullable: true,
    validators: [Validators.requiredTrue]
  });

  readonly setupForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(64)]
    })
  });

  ngOnInit(): void {
    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.requiresPassword.set(account.hasPasswordSet);
          if (!account.hasPasswordSet) {
            this.beginSetup();
          }
        },
        error: () => this.requiresPassword.set(true)
      });
  }

  beginSetup(): void {
    if (this.isBeginning()) {
      return;
    }

    if (
      this.requiresPassword() &&
      !this.setupForm.controls.currentPassword.value.trim()
    ) {
      this.setupForm.controls.currentPassword.markAsTouched();
      this.errorMessage.set('Current password is required.');
      return;
    }

    this.isBeginning.set(true);
    this.errorMessage.set('');

    this.authService
      .beginTwoFactorSetup({
        currentPassword: this.requiresPassword()
          ? this.setupForm.controls.currentPassword.value || null
          : null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (preview) => {
          this.setupPreview.set(preview);
          this.isBeginning.set(false);
        },
        error: (error) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to start two-factor setup.'
          );
          this.isBeginning.set(false);
        }
      });
  }

  confirmSetup(): void {
    if (this.setupForm.invalid || this.isConfirming() || !this.setupPreview()) {
      this.setupForm.markAllAsTouched();
      return;
    }

    this.isConfirming.set(true);
    this.errorMessage.set('');

    this.authService
      .confirmTwoFactorSetup({
        verificationCode: this.setupForm.controls.verificationCode.value.trim()
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.recoveryCodes.set(result.recoveryCodes);
          this.isConfirming.set(false);
        },
        error: (error) => {
          this.errorMessage.set(
            error instanceof Error
              ? error.message
              : AuthMessages.invalidVerificationCode
          );
          this.isConfirming.set(false);
        }
      });
  }

  finishSetup(): void {
    if (!this.recoveryCodesSaved.valid) {
      this.recoveryCodesSaved.markAsTouched();
      return;
    }

    this.authService.clearTwoFactorSetupRequired();
    this.toastService.success(AuthMessages.twoFactorEnabled);
    void this.router.navigateByUrl('/issues');
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/login');
        },
        error: () => {
          void this.router.navigateByUrl('/login');
        }
      });
  }
}
