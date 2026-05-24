import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, input, output, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { isPasskeySupported } from '@features/auth/utils/passkey.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-account-passkeys',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Panel,
    Button,
    Tag,
    Message,
    Dialog,
    InputText
  ],
  templateUrl: './my-account-passkeys.component.html'
})
export class MyAccountPasskeysComponent {
  readonly account = input.required<MyAccountDto>();
  readonly passkeysEnabled = input(false);
  readonly passkeysRequired = input(false);
  readonly maximumPasskeys = input(10);
  readonly accountChanged = output<void>();

  readonly AuthMessages = AuthMessages;
  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly passkeySupported = signal(isPasskeySupported());
  readonly addDialogVisible = signal(false);
  readonly renameDialogVisible = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal('');

  readonly addForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)]
    })
  });

  readonly renameForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    })
  });

  private renamingPasskeyId: string | null = null;

  readonly canAddPasskey = () => {
    const max = this.maximumPasskeys();
    return (
      this.passkeysEnabled() &&
      this.passkeySupported() &&
      this.account().passkeys.length < max
    );
  };

  openAddDialog(): void {
    this.errorMessage.set('');
    this.addForm.reset({ name: '' });
    this.addDialogVisible.set(true);
  }

  closeAddDialog(): void {
    this.addDialogVisible.set(false);
  }

  confirmAddPasskey(): void {
    if (this.isSaving()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const name = this.addForm.controls.name.value.trim();

    this.authService
      .registerPasskey(name)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyAdded);
          this.isSaving.set(false);
          this.addDialogVisible.set(false);
          this.accountChanged.emit();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to add passkey.'
          );
          this.isSaving.set(false);
        }
      });
  }

  openRenameDialog(passkeyId: string, currentName: string): void {
    this.renamingPasskeyId = passkeyId;
    this.errorMessage.set('');
    this.renameForm.controls.name.setValue(currentName);
    this.renameDialogVisible.set(true);
  }

  closeRenameDialog(): void {
    this.renameDialogVisible.set(false);
    this.renamingPasskeyId = null;
  }

  submitRename(): void {
    if (this.renameForm.invalid || !this.renamingPasskeyId || this.isSaving()) {
      this.renameForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');
    const name = this.renameForm.controls.name.value.trim();

    this.authService
      .renamePasskey(this.renamingPasskeyId, { name })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyRenamed);
          this.isSaving.set(false);
          this.closeRenameDialog();
          this.accountChanged.emit();
        },
        error: (error: unknown) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : 'Unable to rename passkey.'
          );
          this.isSaving.set(false);
        }
      });
  }

  confirmRemove(passkeyId: string, displayName: string): void {
    this.confirmationService.confirm({
      header: 'Remove passkey?',
      message: `Remove passkey "${displayName}" from your account?`,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Remove', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.removePasskey(passkeyId)
    });
  }

  private removePasskey(passkeyId: string): void {
    this.authService
      .removePasskey(passkeyId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyRemoved);
          this.accountChanged.emit();
        },
        error: (error: unknown) =>
          this.toastService.error(
            error instanceof Error ? error.message : 'Remove failed.'
          )
      });
  }
}
