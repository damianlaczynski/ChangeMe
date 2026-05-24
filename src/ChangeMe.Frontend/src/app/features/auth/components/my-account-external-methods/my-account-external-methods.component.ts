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
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  buildExternalReauthRequiredDetail,
  needsExternalReauth
} from '@features/auth/utils/external-step-up.utils';
import {
  clearPendingUnlinkProvider,
  readPendingUnlinkProvider,
  storePendingUnlinkProvider
} from '@features/auth/utils/pending-unlink.storage';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-my-account-external-methods',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    Panel,
    Button,
    Message,
    Dialog,
    Password,
    InputText
  ],
  templateUrl: './my-account-external-methods.component.html'
})
export class MyAccountExternalMethodsComponent implements OnInit {
  readonly account = input.required<MyAccountDto>();
  readonly stepUpValidityMinutes = input(15);
  readonly accountChanged = output<void>();

  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  private externalStepUpHandled = false;
  private readonly resumeUnlinkAfterExternalReturn = signal<string | null>(null);

  readonly stepUpVisible = signal(false);
  readonly pendingUnlinkProviderKey = signal<string | null>(null);
  readonly stepUpError = signal('');
  readonly isStepUpSubmitting = signal(false);
  readonly providerLoadingKey = signal<string | null>(null);

  readonly stepUpForm = new FormGroup({
    currentPassword: new FormControl('', { nonNullable: true }),
    verificationCode: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(64)]
    })
  });

  readonly authMessages = AuthMessages;

  readonly linkedProviders = () => this.account().externalLogins ?? [];
  readonly linkableProviders = () => this.account().linkableProviders ?? [];
  readonly requiresVerificationCode = () => this.account().twoFactorEnabled;
  readonly requiresPasswordStepUp = () => this.account().hasPasswordSet;
  readonly requiresExternalStepUp = () =>
    !this.account().hasPasswordSet && this.linkedProviders().length > 0;
  readonly needsExternalReauth = () => needsExternalReauth(this.account());
  readonly externalReauthDetail = () =>
    buildExternalReauthRequiredDetail(this.stepUpValidityMinutes());

  constructor() {
    effect(() => {
      const providerKey = this.resumeUnlinkAfterExternalReturn();
      if (!providerKey || needsExternalReauth(this.account())) {
        return;
      }

      this.resumeUnlinkAfterExternalReturn.set(null);
      this.openStepUp();
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

        const providerKey = readPendingUnlinkProvider();
        if (!providerKey) {
          return;
        }

        this.pendingUnlinkProviderKey.set(providerKey);

        if (!this.account().twoFactorEnabled) {
          clearPendingUnlinkProvider();
          this.unlinkAfterExternalStepUp(providerKey);
          return;
        }

        if (needsExternalReauth(this.account())) {
          this.resumeUnlinkAfterExternalReturn.set(providerKey);
          return;
        }

        clearPendingUnlinkProvider();
        this.openStepUp();
      });
  }

  confirmUnlink(providerKey: string, displayName: string): void {
    if (!this.account().hasPasswordSet && this.linkedProviders().length <= 1) {
      this.toastService.error(AuthMessages.cannotRemoveOnlySignInMethod);
      return;
    }

    this.confirmationService.confirm({
      header: AuthMessages.unlinkExternalProviderTitle,
      message: AuthMessages.unlinkExternalProviderMessage(displayName),
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Remove', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => {
        this.pendingUnlinkProviderKey.set(providerKey);
        this.openStepUp();
      }
    });
  }

  startLink(providerKey: string): void {
    if (this.providerLoadingKey()) {
      return;
    }

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalAccountLink(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          this.providerLoadingKey.set(null);
          this.toastService.error(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  startExternalStepUp(providerKey: string): void {
    if (this.providerLoadingKey()) {
      return;
    }

    const pendingUnlink = this.pendingUnlinkProviderKey();
    if (pendingUnlink) {
      storePendingUnlinkProvider(pendingUnlink);
    }

    this.providerLoadingKey.set(providerKey);
    this.authService
      .beginExternalProviderStepUp(providerKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => window.location.assign(response.authorizationUrl),
        error: (error) => {
          this.providerLoadingKey.set(null);
          this.toastService.error(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  openStepUp(): void {
    this.stepUpError.set('');
    this.stepUpForm.reset();
    this.stepUpVisible.set(true);
  }

  closeStepUp(): void {
    this.stepUpVisible.set(false);
    this.pendingUnlinkProviderKey.set(null);
    this.stepUpError.set('');
    this.stepUpForm.reset();
  }

  private unlinkAfterExternalStepUp(providerKey: string): void {
    this.authService
      .unlinkExternalAccount(providerKey, {})
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.externalAccountUnlinked);
          this.accountChanged.emit();
        },
        error: (error) => {
          this.toastService.error(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
        }
      });
  }

  submitStepUp(): void {
    const providerKey = this.pendingUnlinkProviderKey();
    if (!providerKey || this.isStepUpSubmitting()) {
      return;
    }

    if (
      this.requiresPasswordStepUp() &&
      this.stepUpForm.controls.currentPassword.invalid
    ) {
      this.stepUpForm.markAllAsTouched();
      return;
    }

    if (
      this.requiresVerificationCode() &&
      !this.stepUpForm.controls.verificationCode.value.trim()
    ) {
      this.stepUpForm.controls.verificationCode.markAsTouched();
      return;
    }

    this.isStepUpSubmitting.set(true);
    this.stepUpError.set('');

    this.authService
      .unlinkExternalAccount(providerKey, {
        currentPassword: this.stepUpForm.controls.currentPassword.value || null,
        verificationCode: this.stepUpForm.controls.verificationCode.value.trim() || null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          clearPendingUnlinkProvider();
          this.closeStepUp();
          this.toastService.success(AuthMessages.externalAccountUnlinked);
          this.accountChanged.emit();
        },
        error: (error) => {
          this.stepUpError.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
          this.isStepUpSubmitting.set(false);
        },
        complete: () => {
          this.isStepUpSubmitting.set(false);
        }
      });
  }
}
