import {
  Component,
  DestroyRef,
  effect,
  inject,
  input,
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
import { ToastService } from '@core/toast/services/toast.service';
import { LeaveTypeListItemDto } from '@features/billing/models/leave-type.model';
import { LeaveTypesService } from '@features/billing/services/leave-types.service';
import {
  BillingMessages,
  LeaveTypeConstraints
} from '@features/billing/utils/billing.utils';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { ToggleSwitch } from 'primeng/toggleswitch';

@Component({
  selector: 'app-leave-type-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, InputText, Message, ToggleSwitch],
  templateUrl: './leave-type-dialog.component.html'
})
export class LeaveTypeDialogComponent {
  readonly leaveType = input<LeaveTypeListItemDto | null>(null);
  readonly visible = input(false);
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  private readonly leaveTypesService = inject(LeaveTypesService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly constraints = LeaveTypeConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(LeaveTypeConstraints.NAME_MIN_LENGTH),
        Validators.maxLength(LeaveTypeConstraints.NAME_MAX_LENGTH)
      ]
    }),
    code: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.minLength(LeaveTypeConstraints.CODE_MIN_LENGTH),
        Validators.maxLength(LeaveTypeConstraints.CODE_MAX_LENGTH)
      ]
    }),
    countsAsPaid: new FormControl(true, { nonNullable: true }),
    usesAllowance: new FormControl(false, { nonNullable: true }),
    requiresApproval: new FormControl(true, { nonNullable: true }),
    isActive: new FormControl(true, { nonNullable: true })
  });

  readonly isEditMode = () => this.leaveType() !== null;

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      const existing = this.leaveType();
      this.submitError.set(null);
      if (existing) {
        this.form.reset({
          name: existing.name,
          code: existing.code,
          countsAsPaid: existing.countsAsPaid,
          usesAllowance: existing.usesAllowance,
          requiresApproval: existing.requiresApproval,
          isActive: existing.isActive
        });
      } else {
        this.form.reset({
          name: '',
          code: '',
          countsAsPaid: true,
          usesAllowance: false,
          requiresApproval: true,
          isActive: true
        });
      }
    });
  }

  onHide(): void {
    this.visibleChange.emit(false);
  }

  save(): void {
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting.set(true);
    const raw = this.form.getRawValue();
    const request = {
      name: raw.name.trim(),
      code: raw.code.trim().toUpperCase(),
      countsAsPaid: raw.countsAsPaid,
      usesAllowance: raw.usesAllowance,
      requiresApproval: raw.requiresApproval,
      isActive: raw.isActive
    };

    const existing = this.leaveType();
    const save$ = existing
      ? this.leaveTypesService.updateLeaveType(existing.id, request)
      : this.leaveTypesService.createLeaveType(request);

    save$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toastService.success(
          existing ? BillingMessages.leaveTypeSaved : BillingMessages.leaveTypeCreated
        );
        this.visibleChange.emit(false);
        this.saved.emit();
      },
      error: (error: Error) => {
        this.submitError.set(error.message);
        this.isSubmitting.set(false);
      },
      complete: () => this.isSubmitting.set(false)
    });
  }
}
