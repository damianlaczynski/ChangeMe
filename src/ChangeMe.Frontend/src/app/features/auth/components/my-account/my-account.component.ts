import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import {
  AuthConstraints,
  AuthMessages,
  PermissionCodes
} from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-account',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    RouterLink,
    Card,
    Button,
    InputText,
    Message,
    Tag,
    EffectivePermissionsComponent
  ],
  templateUrl: './my-account.component.html'
})
export class MyAccountComponent {
  readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly account = signal<MyAccountDto | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isSaving = signal(false);
  readonly authConstraints = AuthConstraints;
  readonly permissionCodes = PermissionCodes;

  readonly effectivePermissions = computed(
    () => this.account()?.effectivePermissions ?? []
  );

  readonly form = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(AuthConstraints.NAME_MAX_LENGTH)
      ]
    })
  });

  constructor() {
    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.form.patchValue({
            firstName: account.firstName,
            lastName: account.lastName
          });
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSaving()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);

    this.authService
      .updateMyAccount(this.form.getRawValue())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.isSaving.set(false);
          this.toastService.success(AuthMessages.profileUpdated);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isSaving.set(false);
        }
      });
  }

  shouldShowError(control: FormControl<string>): boolean {
    return control.touched && control.invalid;
  }
}
