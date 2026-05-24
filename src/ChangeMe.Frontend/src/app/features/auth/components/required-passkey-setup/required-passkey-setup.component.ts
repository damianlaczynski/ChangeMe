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
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { isPasskeySupported } from '@features/auth/utils/passkey.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { finalize, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-required-passkey-setup',
  imports: [ReactiveFormsModule, Card, Button, InputText, Message],
  templateUrl: './required-passkey-setup.component.html'
})
export class RequiredPasskeySetupComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly title = AuthMessages.passkeySetupRequiredTitle;
  readonly subtitle = AuthMessages.passkeySetupRequiredSubtitle;
  readonly AuthMessages = AuthMessages;
  readonly errorMessage = signal('');
  readonly isRegistering = signal(false);
  readonly passkeysUnavailable = signal(false);
  readonly passkeySupported = signal(isPasskeySupported());

  readonly nameMaxLength = AuthConstraints.NAME_MAX_LENGTH;

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)]
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

  registerPasskey(): void {
    if (
      this.isRegistering() ||
      this.passkeysUnavailable() ||
      !this.passkeySupported()
    ) {
      return;
    }

    this.isRegistering.set(true);
    this.errorMessage.set('');

    const name = this.form.controls.name.value.trim();

    this.authService
      .registerPasskey(name)
      .pipe(
        switchMap(() => this.authService.refreshSession()),
        finalize(() => this.isRegistering.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
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
