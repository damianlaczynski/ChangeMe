import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { ExternalAccountLinkRequired } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import {
  clearExternalLinkRequired,
  readExternalLinkRequired
} from '@features/auth/utils/external-link.storage';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-link-external-account',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [ReactiveFormsModule, AuthPageComponent, Button, Password, Message],
  templateUrl: './link-external-account.component.html'
})
export class LinkExternalAccountComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly link = signal<ExternalAccountLinkRequired | null>(
    readExternalLinkRequired()
  );
  readonly errorMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly authMessages = AuthMessages;

  readonly form = new FormGroup({
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(AuthConstraints.PASSWORD_MIN_LENGTH),
        Validators.maxLength(AuthConstraints.PASSWORD_MAX_LENGTH)
      ]
    })
  });

  constructor() {
    if (!this.link()) {
      void this.router.navigateByUrl('/login');
    }
  }

  cancel(): void {
    clearExternalLinkRequired();
    void this.router.navigateByUrl('/login');
  }

  onSubmit(): void {
    const link = this.link();
    if (!link || this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    this.authService
      .linkExternalAccount({
        state: link.state,
        password: this.form.controls.password.value
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          clearExternalLinkRequired();
          this.authService.continueAfterExternalSignIn(response);
        },
        error: (error) => {
          this.errorMessage.set(
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed
          );
          this.isSubmitting.set(false);
        },
        complete: () => {
          this.isSubmitting.set(false);
        }
      });
  }
}
