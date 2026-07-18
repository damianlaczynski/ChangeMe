import { Component, DestroyRef, inject, input, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import {
  buildPasswordPolicyValidators,
  defaultPasswordPolicySettings
} from '@features/auth/utils/password-policy.utils';
import { InvitationsService } from '@features/invitations/services/invitations.service';
import { InvitationConstraints } from '@features/invitations/utils/invitations.utils';
import { Button } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Password } from 'primeng/password';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-accept-invitation',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    ReactiveFormsModule,
    AuthPageComponent,
    Button,
    InputText,
    Message,
    Password,
    ProgressSpinner
  ],
  templateUrl: './accept-invitation.component.html'
})
export class AcceptInvitationComponent implements OnInit {
  readonly token = input<string>();

  private readonly invitationsService = inject(InvitationsService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly invitationConstraints = InvitationConstraints;
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup(
    {
      email: new FormControl({ value: '', disabled: true }, { nonNullable: true }),
      firstName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(InvitationConstraints.NAME_MAX_LENGTH)
        ]
      }),
      lastName: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.required,
          Validators.maxLength(InvitationConstraints.NAME_MAX_LENGTH)
        ]
      }),
      password: new FormControl('', {
        nonNullable: true,
        validators: buildPasswordPolicyValidators(defaultPasswordPolicySettings())
      }),
      confirmPassword: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required]
      })
    },
    { validators: [passwordMatchValidator] }
  );

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.authService
        .logout()
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.loadInvitation());
      return;
    }

    this.loadInvitation();
  }

  onSubmit(): void {
    const token = this.token();
    if (!token || this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();
    this.invitationsService
      .acceptInvitation(token, {
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        password: raw.password
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigate(['/login'], {
            queryParams: { accepted: 'true' }
          });
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  shouldShowError(control: AbstractControl): boolean {
    return control.touched && control.invalid;
  }

  private loadInvitation(): void {
    const token = this.token();
    if (!token) {
      this.loadError.set('This invitation link is not valid.');
      this.isLoading.set(false);
      return;
    }

    this.invitationsService
      .getInvitationByToken(token)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (invitation) => {
          this.form.controls.email.setValue(invitation.email);
          if (invitation.firstName) {
            this.form.controls.firstName.setValue(invitation.firstName);
          }
          if (invitation.lastName) {
            this.form.controls.lastName.setValue(invitation.lastName);
          }
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}

function passwordMatchValidator(control: AbstractControl): ValidationErrors | null {
  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  if (!password || !confirmPassword) {
    return null;
  }

  return password === confirmPassword ? null : { passwordMismatch: true };
}
