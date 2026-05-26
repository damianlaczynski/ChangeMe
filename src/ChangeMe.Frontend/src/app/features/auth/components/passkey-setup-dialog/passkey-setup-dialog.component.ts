import { Component, DestroyRef, effect, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ToastService } from '@core/toast/services/toast.service';
import { PasskeySettings } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { PasskeySetupDialogService } from '@features/auth/services/passkey-setup-dialog.service';
import { PasskeySetupNoticeService } from '@features/auth/services/passkey-setup-notice.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { isPasskeySupported } from '@features/auth/utils/passkey.utils';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { finalize, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-passkey-setup-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, InputText, Message],
  templateUrl: './passkey-setup-dialog.component.html'
})
export class PasskeySetupDialogComponent {
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(PasskeySetupDialogService);
  private readonly noticeService = inject(PasskeySetupNoticeService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dialogServiceRef = this.dialogService;
  readonly title = AuthMessages.passkeySetupRequiredTitle;
  readonly AuthMessages = AuthMessages;
  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

  readonly errorMessage = signal('');
  readonly isRegisteringCeremony = signal(false);
  readonly isSaving = signal(false);
  readonly nameDialogVisible = signal(false);
  readonly passkeysUnavailable = signal(false);
  readonly passkeySupported = signal(isPasskeySupported());

  private pendingCeremony: { ceremonyId: string; attestationResponse: unknown } | null =
    null;

  readonly nameForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    })
  });

  constructor() {
    effect(() => {
      if (this.dialogService.visible()) {
        this.loadSettings();
      }
    });
  }

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.dialogService.close();
      this.resetForm();
    }
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
          this.nameForm.reset({ name: 'Passkey 1' });
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
    if (this.isSaving() || !this.pendingCeremony || this.nameForm.invalid) {
      this.nameForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const name = this.nameForm.controls.name.value.trim();
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
          this.dialogService.notifyCompleted();
          this.dialogService.close();
          this.toastService.success(AuthMessages.passkeyAdded);
          this.resetForm();
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

  private resetForm(): void {
    this.nameForm.reset();
    this.nameDialogVisible.set(false);
    this.pendingCeremony = null;
    this.errorMessage.set('');
    this.isRegisteringCeremony.set(false);
    this.isSaving.set(false);
  }

  private loadSettings(): void {
    this.authService
      .getAuthSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => {
          const passkeys = settings.passkeys as PasskeySettings | undefined;
          this.passkeysUnavailable.set(
            passkeys?.passkeysAuthenticationEnabled !== true
          );
        },
        error: () => this.passkeysUnavailable.set(true)
      });
  }
}
