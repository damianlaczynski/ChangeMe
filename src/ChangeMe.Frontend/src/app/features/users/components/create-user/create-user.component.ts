import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { UserConstraints, UserMessages } from '@features/users/utils/users.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Panel } from 'primeng/panel';
import { catchError, debounceTime, of, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-create-user',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    MultiSelect,
    Message,
    Panel,
    EffectivePermissionsComponent
  ],
  templateUrl: './create-user.component.html'
})
export class CreateUserComponent {
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly userConstraints = UserConstraints;
  readonly roleOptions = signal<{ id: string; name: string; isSystem: boolean }[]>([]);
  readonly effectivePermissions = signal<EffectivePermissionDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoadingRoles = signal(true);

  readonly form = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(UserConstraints.NAME_MAX_LENGTH)]
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(UserConstraints.NAME_MAX_LENGTH)]
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(UserConstraints.EMAIL_MAX_LENGTH)
      ]
    }),
    roleIds: new FormControl<string[]>([], {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  constructor() {
    this.usersService
      .getRolesForAssignment()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (roles) => {
          this.roleOptions.set(roles);
          this.isLoadingRoles.set(false);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isLoadingRoles.set(false);
        }
      });

    this.form.controls.roleIds.valueChanges
      .pipe(
        startWith(this.form.controls.roleIds.value),
        debounceTime(200),
        switchMap((roleIds) => {
          if (roleIds.length === 0) {
            this.effectivePermissions.set([]);
            return of([]);
          }

          return this.usersService
            .previewEffectivePermissions({ roleIds })
            .pipe(catchError(() => of([])));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((permissions) => this.effectivePermissions.set(permissions));
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();

    this.usersService
      .createUser({
        firstName: raw.firstName.trim(),
        lastName: raw.lastName.trim(),
        email: raw.email.trim(),
        roleIds: raw.roleIds
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.toastService.success(UserMessages.userCreated);
          void this.router.navigate(['/users', user.id]);
        },
        error: (error: Error) => {
          const message = error.message;
          this.submitError.set(
            message.includes('email') ? UserMessages.duplicateEmail : message
          );
          this.isSubmitting.set(false);
        }
      });
  }

  roleLabel(role: { name: string; isSystem: boolean }): string {
    return role.isSystem ? `${role.name} (System)` : role.name;
  }

  shouldShowError(control: FormControl<string | string[]>): boolean {
    return control.touched && control.invalid;
  }
}
