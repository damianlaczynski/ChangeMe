import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  inject,
  input,
  OnInit,
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
import { ActivatedRoute, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { IdentityStepUpDialogComponent } from '@features/auth/components/identity-step-up-dialog/identity-step-up-dialog.component';
import { MyAccountDto, TwoFactorStepUpRequest } from '@features/auth/models/auth.model';
import { StepUpVerificationResult } from '@features/auth/models/step-up.model';
import { AuthService } from '@features/auth/services/auth.service';
import { ExternalStepUpReturnService } from '@features/auth/services/external-step-up-return.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import {
  getPasskeyCeremonyErrorMessage,
  isPasskeySupported
} from '@features/auth/utils/passkey.utils';
import {
  clearPendingPasskeyStepUp,
  PendingPasskeyStepUpAction,
  readPendingPasskeyStepUp,
  storePendingPasskeyStepUp
} from '@features/auth/utils/pending-passkey-step-up.storage';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Tag } from 'primeng/tag';

type PasskeyStepUpAction = PendingPasskeyStepUpAction;

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
    InputText,
    IdentityStepUpDialogComponent
  ],
  templateUrl: './my-account-passkeys.component.html'
})
export class MyAccountPasskeysComponent implements OnInit {
  readonly account = input.required<MyAccountDto>();
  readonly passkeysEnabled = input(false);
  readonly passkeysRequired = input(false);
  readonly externalProvidersEnabled = input(false);
  readonly maximumPasskeys = input(10);
  readonly stepUpValidityMinutes = input(15);
  readonly accountChanged = output<void>();

  readonly AuthMessages = AuthMessages;
  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly externalStepUpReturn = inject(ExternalStepUpReturnService);

  private readonly resumeStepUpAfterExternalReturn = signal(false);

  readonly passkeySupported = signal(isPasskeySupported());
  readonly addDialogVisible = signal(false);
  readonly renameDialogVisible = signal(false);
  readonly stepUpVisible = signal(false);
  readonly stepUpAction = signal<PasskeyStepUpAction | null>(null);
  readonly stepUpError = signal('');
  readonly isSaving = signal(false);
  readonly isRegisteringCeremony = signal(false);
  readonly errorMessage = signal('');

  readonly addForm = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
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

  private stepUpCredentials: TwoFactorStepUpRequest | null = null;
  private pendingCeremony: { ceremonyId: string; attestationResponse: unknown } | null =
    null;
  private renamingPasskeyId: string | null = null;
  private pendingRemovePasskeyId: string | null = null;

  readonly canAddPasskey = () => {
    const max = this.maximumPasskeys();
    return (
      this.passkeysEnabled() &&
      this.passkeySupported() &&
      this.account().passkeys.length < max
    );
  };

  readonly isAtMaxPasskeys = () =>
    this.passkeysEnabled() &&
    this.passkeySupported() &&
    this.account().passkeys.length >= this.maximumPasskeys();

  readonly skipPasskeyStepUp = () =>
    this.passkeysRequired() && this.account().passkeys.length === 0;

  constructor() {
    this.externalStepUpReturn.resumeWhenExternalReauthFresh(this.destroyRef, {
      isResumePending: () => this.resumeStepUpAfterExternalReturn(),
      clearResumePending: () => this.resumeStepUpAfterExternalReturn.set(false),
      account: this.account,
      onReady: () => {
        const action = this.stepUpAction();
        if (action) {
          this.completeStepUp(action);
        }
      }
    });
  }

  ngOnInit(): void {
    this.externalStepUpReturn.watchQueryParamReturn(this.destroyRef, this.route, () => {
      const pending = readPendingPasskeyStepUp();
      if (!pending) {
        return;
      }

      clearPendingPasskeyStepUp();
      this.stepUpAction.set(pending.action);
      if (pending.passkeyId) {
        this.renamingPasskeyId = pending.passkeyId;
        if (pending.passkeyName) {
          this.renameForm.controls.name.setValue(pending.passkeyName);
        }
      }
      if (pending.action === 'remove' && pending.passkeyId) {
        this.pendingRemovePasskeyId = pending.passkeyId;
      }
      this.stepUpCredentials = {
        currentPassword: null,
        verificationCode: pending.verificationCode || null
      };
      this.resumeStepUpAfterExternalReturn.set(true);
    });
  }

  openAddDialog(): void {
    if (this.skipPasskeyStepUp()) {
      void this.startPasskeyRegistrationCeremony();
      return;
    }

    this.openStepUp('add');
  }

  closeAddDialog(): void {
    this.addDialogVisible.set(false);
    this.pendingCeremony = null;
    this.stepUpCredentials = null;
  }

  confirmAddPasskey(): void {
    if (this.isSaving() || !this.pendingCeremony) {
      this.addForm.markAllAsTouched();
      return;
    }

    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');

    const name = this.addForm.controls.name.value.trim();
    const ceremony = this.pendingCeremony;

    this.authService
      .completePasskeyRegistrationAfterCeremony(
        ceremony,
        name,
        this.stepUpCredentials ?? undefined
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyAdded);
          this.isSaving.set(false);
          this.closeAddDialog();
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
    this.renameForm.controls.name.setValue(currentName);
    this.openStepUp('rename');
  }

  closeRenameDialog(): void {
    this.renameDialogVisible.set(false);
    this.renamingPasskeyId = null;
    this.stepUpCredentials = null;
  }

  submitRename(): void {
    if (this.renameForm.invalid || !this.renamingPasskeyId || this.isSaving()) {
      this.renameForm.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set('');
    const name = this.renameForm.controls.name.value.trim();
    const stepUp = this.stepUpCredentials ?? {};

    this.authService
      .renamePasskey(this.renamingPasskeyId, {
        name,
        currentPassword: stepUp.currentPassword ?? null,
        verificationCode: stepUp.verificationCode ?? null
      })
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
      accept: () => {
        this.pendingRemovePasskeyId = passkeyId;
        this.openStepUp('remove');
      }
    });
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.stepUpAction.set(null);
    this.stepUpError.set('');
    this.pendingRemovePasskeyId = null;
  }

  onStepUpVerified(result: StepUpVerificationResult): void {
    const action = this.stepUpAction();
    if (!action) {
      return;
    }

    this.stepUpCredentials = {
      currentPassword: result.currentPassword,
      verificationCode: result.verificationCode
    };
    this.completeStepUp(action);
  }

  readonly prepareExternalRedirect = (context: {
    verificationCode: string;
  }): boolean => {
    const action = this.stepUpAction();
    if (!action) {
      return false;
    }

    storePendingPasskeyStepUp({
      action,
      verificationCode: context.verificationCode,
      passkeyId: this.renamingPasskeyId ?? this.pendingRemovePasskeyId ?? undefined,
      passkeyName: this.renameForm.controls.name.value.trim() || undefined
    });
    return true;
  };

  onExternalRedirectFailed(): void {
    clearPendingPasskeyStepUp();
  }

  private openStepUp(action: PasskeyStepUpAction): void {
    this.stepUpAction.set(action);
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }

  private completeStepUp(action: PasskeyStepUpAction): void {
    this.stepUpVisible.set(false);
    this.stepUpAction.set(null);
    this.stepUpError.set('');

    switch (action) {
      case 'add':
        void this.startPasskeyRegistrationCeremony();
        break;
      case 'rename':
        this.renameDialogVisible.set(true);
        this.errorMessage.set('');
        break;
      case 'remove':
        this.executeRemove();
        break;
    }
  }

  private defaultPasskeyName(): string {
    return `Passkey ${this.account().passkeys.length + 1}`;
  }

  private startPasskeyRegistrationCeremony(): void {
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
          this.showNamePasskeyDialog();
        },
        error: (error: unknown) => {
          this.isRegisteringCeremony.set(false);
          const message = getPasskeyCeremonyErrorMessage(
            error,
            'Unable to add passkey.'
          );
          if (message) {
            this.toastService.error(message);
          }
        },
        complete: () => {
          this.isRegisteringCeremony.set(false);
        }
      });
  }

  private showNamePasskeyDialog(): void {
    this.errorMessage.set('');
    this.addForm.reset({ name: this.defaultPasskeyName() });
    this.addDialogVisible.set(true);
  }

  private executeRemove(): void {
    const passkeyId = this.pendingRemovePasskeyId;
    if (!passkeyId) {
      return;
    }

    this.authService
      .removePasskey(passkeyId, this.stepUpCredentials ?? undefined)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.passkeyRemoved);
          this.stepUpCredentials = null;
          this.pendingRemovePasskeyId = null;
          this.accountChanged.emit();
        },
        error: (error: unknown) => {
          this.stepUpCredentials = null;
          this.pendingRemovePasskeyId = null;
          this.handleRemoveError(error);
        }
      });
  }

  private handleRemoveError(error: unknown): void {
    const message =
      error instanceof Error ? error.message : AuthMessages.passkeySignInFailed;

    if (message === AuthMessages.passkeyRemoveOnlySignInMethod) {
      const detail = this.externalProvidersEnabled()
        ? ' Set a password or link an external sign-in method from My account.'
        : '';

      this.confirmationService.confirm({
        header: 'Cannot remove passkey',
        message: `${AuthMessages.passkeyRemoveOnlySignInMethod}${detail}`,
        icon: 'pi pi-exclamation-triangle',
        acceptButtonProps: { label: 'Set password', severity: 'primary' },
        rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
        accept: () => {
          void this.router.navigate(['/account/set-password']);
        }
      });
      return;
    }

    if (message === AuthMessages.passkeyRemoveRequiredPasskey) {
      this.toastService.error(message);
      return;
    }

    this.toastService.error(message);
  }
}
