import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { ManageRoleUsersFormDto } from '@features/roles/models/role.model';
import { RolesService } from '@features/roles/services/roles.service';
import { RoleMessages } from '@features/roles/utils/roles.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { MultiSelect } from 'primeng/multiselect';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-manage-role-users',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    InputText,
    MultiSelect,
    Message,
    Tag
  ],
  templateUrl: './manage-role-users.component.html'
})
export class ManageRoleUsersComponent {
  readonly id = input.required<string>();

  private readonly rolesService = inject(RolesService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  readonly RoleMessages = RoleMessages;
  readonly formData = signal<ManageRoleUsersFormDto | null>(null);
  readonly submitError = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly isSubmitting = signal(false);
  readonly userFilter = new FormControl('', { nonNullable: true });

  readonly form = new FormGroup({
    userIds: new FormControl<string[]>([], { nonNullable: true })
  });

  readonly filteredUserOptions = computed(() => {
    const data = this.formData();
    if (!data) {
      return [];
    }

    const filter = this.userFilter.value.trim().toLowerCase();
    if (!filter) {
      return data.availableUsers;
    }

    return data.availableUsers.filter(
      (user) =>
        user.fullName.toLowerCase().includes(filter) ||
        user.email.toLowerCase().includes(filter)
    );
  });

  readonly currentAssignments = computed(() => {
    const data = this.formData();
    if (!data) {
      return [];
    }

    const selected = new Set(this.form.controls.userIds.value);
    return data.availableUsers.filter((user) => selected.has(user.id));
  });

  constructor() {
    effect(() => {
      const roleId = this.id();
      this.isLoading.set(true);
      this.submitError.set(null);

      this.rolesService.getManageRoleUsersForm(roleId).subscribe({
        next: (form) => {
          this.formData.set(form);
          this.form.controls.userIds.setValue([...form.assignedUserIds]);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isLoading.set(false);
        }
      });
    });
  }

  onSubmit(): void {
    this.submitError.set(null);
    this.isSubmitting.set(true);
    this.rolesService
      .updateRoleUsers({
        roleId: this.id(),
        userIds: this.form.controls.userIds.value
      })
      .subscribe({
        next: () => {
          this.toastService.success(RoleMessages.roleAssignmentsUpdated);
          void this.router.navigate(['/roles', this.id()]);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}
