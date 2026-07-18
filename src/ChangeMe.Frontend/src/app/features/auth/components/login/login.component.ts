import { Component, inject, OnInit, signal } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthConstraints, AuthMessages } from '@features/auth/utils/auth.utils';
import { InvitationMessages } from '@features/invitations/utils/invitations.utils';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';

@Component({
  selector: 'app-login',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    Button,
    InputText,
    Message,
    Password
  ],
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);

  readonly errorMessage = signal('');
  readonly successMessage = signal('');
  readonly isSubmitting = signal(false);
  readonly authConstraints = AuthConstraints;

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(AuthConstraints.EMAIL_MAX_LENGTH)
      ]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(AuthConstraints.PASSWORD_MIN_LENGTH),
        Validators.maxLength(AuthConstraints.PASSWORD_MAX_LENGTH)
      ]
    })
  });

  ngOnInit(): void {
    if (this.route.snapshot.queryParamMap.get('accepted') === 'true') {
      this.successMessage.set(InvitationMessages.accountCreated);
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set('');

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        const returnUrl =
          this.route.snapshot.queryParamMap.get('returnUrl') ?? '/issues';
        this.authService.continueAfterLogin(returnUrl);
      },
      error: (error) => {
        const message =
          error instanceof Error ? error.message : AuthMessages.invalidCredentials;
        this.errorMessage.set(
          message === AuthMessages.deactivatedAccount
            ? message
            : AuthMessages.invalidCredentials
        );
        this.isSubmitting.set(false);
      },
      complete: () => {
        this.isSubmitting.set(false);
      }
    });
  }
}
