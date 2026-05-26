import { DatePipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  effect,
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
import { MyAccountDto, TwoFactorStepUpRequest } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildExternalReauthRequiredDetail,
  needsExternalReauth
} from '@features/auth/utils/external-step-up.utils';
import {
  canOfferPasskeyStepUp,
  requiresVerificationCodeStepUp
} from '@features/auth/utils/passkey-step-up.utils';
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
import { Password } from 'primeng/password';
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
    Password
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

  private externalStepUpHandled = false;
  private readonly resumeStepUpAfterExternalReturn = signal(false);

  readonly passkeySupported = signal(isPasskeySupported());
  readonly addDialogVisible = signal(false);
  readonly renameDialogVisible = signal(false);
  readonly stepUpVisible = signal(false);
  readonly stepUpAction = signal<PasskeyStepUpAction | null>(null);
  readonly stepUpError = signal('');
  readonly isSaving = signal(false);
  readonly isRegisteringCeremony = signal(false);
  readonly isStepUpSubmitting = signal(false);
  readonly isPasskeyStepUpSubmitting = signal(false);
  readonly providerLoadingKey = signal<string | null>(null);
  readonly errorMessage = signal('');

  readonly stepUpForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(64)]
    })
  });

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

  readonly linkedProviders = () => this.account().externalLogins ?? [];
  readonly requiresPasswordStepUp = () => this.account().hasPasswordSet;
  readonly requiresVerificationCode = () =>
    requiresVerificationCodeStepUp(this.account());
  readonly needsExternalReauth = () => needsExternalReauth(this.account());
  readonly canOfferPasskeyStepUp = () =>
    canOfferPasskeyStepUp(this.account(), this.passkeysEnabled());
  readonly externalReauthDetail = () =>
    buildExternalReauthRequiredDetail(this.stepUpValidityMinutes());

  constructor() {
    effect(() => {
      if (!this.resumeStepUpAfterExternalReturn()) {
        return;
      }

      if (needsExternalReauth(this.account())) {
        return;
      }

      this.resumeStepUpAfterExternalReturn.set(false);
      const action = this.stepUpAction();
      if (action) {
        this.completeStepUp(action);
      }
    });
  }

  ngOnInit(): void {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        if (params.get('externalStepUp') !== '1' || this.externalStepUpHandled) {
          return;
        }

        this.externalStepUpHandled = true;
        void this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { externalStepUp: null },
          queryParamsHandling: 'merge',
          replaceUrl: true
        });

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
    this.stepUpForm.reset();
    this.pendingRemovePasskeyId = null;
    this.providerLoadingKey.set(null);
  }

  startExternalStepUp(providerKey: string): void {
    const action = this.stepUpAction();
    if (!action || this.providerLoadingKey()) {
      return;
    }

    if (
      this.requiresVerificationCode() &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    storePendingPasskeyStepUp({
      action,
      verificationCode: this.stepUpForm.controls.verificationCode.value.trim(),
      passkeyId: this.renamingPasskeyId ?? this.pendingRemovePasskeyId ?? undefined,
      passkeyName: this.renameForm.controls.name.value.trim() || undefined
    });

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalProviderStepUp(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          clearPendingPasskeyStepUp();
          this.providerLoadingKey.set(null);
          this.stepUpError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  submitStepUp(): void {
    const action = this.stepUpAction();
    if (!action || this.isStepUpSubmitting()) {
      return;
    }

    if (
      this.requiresPasswordStepUp() &&
      !this.stepUpForm.controls.currentPassword.value.trim()
    ) {
      this.stepUpForm.controls.currentPassword.markAsTouched();
      return;
    }

    if (
      this.requiresVerificationCode() &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    this.stepUpCredentials = {
      currentPassword: this.requiresPasswordStepUp()
        ? this.stepUpForm.controls.currentPassword.value.trim()
        : null,
      verificationCode: this.requiresVerificationCode()
        ? this.stepUpForm.controls.verificationCode.value.trim()
        : null
    };

    this.isStepUpSubmitting.set(true);
    this.stepUpError.set('');
    this.completeStepUp(action);
    this.isStepUpSubmitting.set(false);
  }

  verifyWithPasskeyStepUp(): void {
    const action = this.stepUpAction();
    if (!action || this.isPasskeyStepUpSubmitting()) {
      return;
    }

    if (
      this.requiresVerificationCode() &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    this.isPasskeyStepUpSubmitting.set(true);
    this.stepUpError.set('');

    this.authService
      .verifyWithPasskey()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isPasskeyStepUpSubmitting.set(false);
          this.toastService.success(AuthMessages.passkeyStepUpCompleted);
          this.stepUpCredentials = {
            currentPassword: null,
            verificationCode: this.requiresVerificationCode()
              ? this.stepUpForm.controls.verificationCode.value.trim()
              : null
          };
          this.completeStepUp(action);
        },
        error: (error: unknown) => {
          this.isPasskeyStepUpSubmitting.set(false);
          const message = getPasskeyCeremonyErrorMessage(
            error,
            AuthMessages.passkeySignInFailed
          );
          if (message) {
            this.stepUpError.set(message);
          }
        },
        complete: () => {
          this.isPasskeyStepUpSubmitting.set(false);
        }
      });
  }

  private openStepUp(action: PasskeyStepUpAction): void {
    this.stepUpAction.set(action);
    this.stepUpForm.reset();
    this.stepUpError.set('');
    this.stepUpVisible.set(true);
  }

  private completeStepUp(action: PasskeyStepUpAction): void {
    this.stepUpVisible.set(false);
    this.stepUpAction.set(null);
    this.stepUpError.set('');
    this.stepUpForm.reset();

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
