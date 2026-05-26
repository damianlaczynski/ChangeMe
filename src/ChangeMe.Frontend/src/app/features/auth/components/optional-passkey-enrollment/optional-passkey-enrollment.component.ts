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
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { isPasskeySupported } from '@features/auth/utils/passkey.utils';
import { clearPendingPasskeyEnrollmentOffer } from '@features/auth/utils/pending-passkey-enrollment.storage';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { finalize, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-optional-passkey-enrollment',
  imports: [ReactiveFormsModule, Card, Button, Dialog, InputText, Message],
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

  readonly errorMessage = signal('');
  readonly isRegisteringCeremony = signal(false);
  readonly isSaving = signal(false);
  readonly nameDialogVisible = signal(false);
  readonly passkeySupported = signal(isPasskeySupported());

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
    }
  }

  startPasskeyRegistration(): void {
    if (this.isRegisteringCeremony() || !this.passkeySupported()) {
      return;
    }

    this.isRegisteringCeremony.set(true);
    this.errorMessage.set('');

    this.authService
      .performPasskeyRegistrationCeremony()
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
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to add passkey.'
          );
        }
      });
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
      .completePasskeyRegistrationAfterCeremony(ceremony, name)
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
    this.errorMessage.set('');
  }

  skip(): void {
    this.finishWithoutEnrollment();
  }

  private finishWithoutEnrollment(): void {
    clearPendingPasskeyEnrollmentOffer();
    void this.router.navigateByUrl('/issues');
  }
}
