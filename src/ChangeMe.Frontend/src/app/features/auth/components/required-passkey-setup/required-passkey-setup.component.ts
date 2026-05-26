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
import { PasskeySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { PasskeySetupNoticeService } from '@features/auth/services/passkey-setup-notice.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { isPasskeySupported } from '@features/auth/utils/passkey.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { finalize, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-required-passkey-setup',
  imports: [ReactiveFormsModule, Card, Button, Dialog, InputText, Message],
  templateUrl: './required-passkey-setup.component.html'
})
export class RequiredPasskeySetupComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly noticeService = inject(PasskeySetupNoticeService);
  private readonly destroyRef = inject(DestroyRef);

  readonly title = AuthMessages.passkeySetupRequiredTitle;
  readonly subtitle = AuthMessages.passkeySetupRequiredSubtitle;
  readonly AuthMessages = AuthMessages;
  readonly errorMessage = signal('');
  readonly isRegisteringCeremony = signal(false);
  readonly isSaving = signal(false);
  readonly nameDialogVisible = signal(false);
  readonly passkeysUnavailable = signal(false);
  readonly passkeySupported = signal(isPasskeySupported());

  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

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
    if (!this.authService.requiresPasskeySetupScreen()) {
      void this.router.navigateByUrl('/issues');
      return;
    }

    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          const passkeys = settings.passkeys as PasskeySettings | undefined;
          const enabled = passkeys?.passkeysAuthenticationEnabled === true;
          this.passkeysUnavailable.set(!enabled);
        },
        error: () => this.passkeysUnavailable.set(true)
      });
  }

  startPasskeyRegistration(): void {
    if (
      this.isRegisteringCeremony() ||
      this.passkeysUnavailable() ||
      !this.passkeySupported()
    ) {
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
            error instanceof Error ? error.message : 'Unable to register passkey.'
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
          this.authService.clearPasskeySetupRequired();
          this.noticeService.clearNotices();
          this.toastService.success(AuthMessages.passkeyAdded);
          void this.router.navigateByUrl('/issues');
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to register passkey.'
          );
        }
      });
  }

  closeNameDialog(): void {
    this.nameDialogVisible.set(false);
    this.pendingCeremony = null;
    this.errorMessage.set('');
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
