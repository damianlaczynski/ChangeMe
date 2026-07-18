import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { InvitationsService } from '@features/invitations/services/invitations.service';
import {
  InvitationConstraints,
  InvitationMessages
} from '@features/invitations/utils/invitations.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { EffectivePermissionDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Panel } from 'primeng/panel';
import { catchError, debounceTime, of, startWith, switchMap } from 'rxjs';

@Component({
  selector: 'app-create-invitation',
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
  templateUrl: './create-invitation.component.html'
})
export class CreateInvitationComponent {
  private readonly invitationsService = inject(InvitationsService);
  private readonly usersService = inject(UsersService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly invitationConstraints = InvitationConstraints;
  readonly roleOptions = signal<{ id: string; name: string; isSystem: boolean }[]>([]);
  readonly effectivePermissions = signal<EffectivePermissionDto[]>([]);
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoadingRoles = signal(true);

  readonly canManageRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesManage)
  );

  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.email,
        Validators.maxLength(InvitationConstraints.EMAIL_MAX_LENGTH)
      ]
    }),
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(InvitationConstraints.NAME_MAX_LENGTH)]
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(InvitationConstraints.NAME_MAX_LENGTH)]
    }),
    roleIds: new FormControl<string[]>([], { nonNullable: true })
  });

  constructor() {
    if (this.canManageRoles()) {
      this.form.controls.roleIds.setValidators([Validators.required]);
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
    } else {
      this.isLoadingRoles.set(false);
    }
  }

  onSubmit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    this.submitError.set(null);

    const raw = this.form.getRawValue();
    const firstName = raw.firstName.trim();
    const lastName = raw.lastName.trim();

    this.invitationsService
      .createInvitation({
        email: raw.email.trim(),
        firstName: firstName || null,
        lastName: lastName || null,
        roleIds: this.canManageRoles() ? raw.roleIds : null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(InvitationMessages.sent);
          void this.router.navigate(['/invitations']);
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
}
