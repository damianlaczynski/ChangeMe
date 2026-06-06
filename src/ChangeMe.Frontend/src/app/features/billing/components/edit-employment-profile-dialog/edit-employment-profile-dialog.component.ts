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
import { EmploymentProfileDto } from '@features/billing/models/employment.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import {
  BillingMessages,
  EmploymentConstraints
} from '@features/billing/utils/billing.utils';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-edit-employment-profile-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, InputText, Textarea, Message],
  templateUrl: './edit-employment-profile-dialog.component.html'
})
export class EditEmploymentProfileDialogComponent {
  readonly userId = input.required<string>();
  readonly profile = input.required<EmploymentProfileDto>();
  readonly visible = input(false);
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  private readonly employmentService = inject(EmploymentService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly constraints = EmploymentConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup({
    employeeId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(EmploymentConstraints.EMPLOYEE_ID_MAX_LENGTH)]
    }),
    nationalId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(EmploymentConstraints.NATIONAL_ID_MAX_LENGTH)]
    }),
    taxId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(EmploymentConstraints.TAX_ID_MAX_LENGTH)]
    }),
    bankAccount: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(EmploymentConstraints.BANK_ACCOUNT_MAX_LENGTH)]
    }),
    notes: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.maxLength(EmploymentConstraints.EMPLOYMENT_NOTES_MAX_LENGTH)
      ]
    })
  });

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      const profile = this.profile();
      this.form.reset({
        employeeId: profile.employeeId ?? '',
        nationalId: profile.nationalId ?? '',
        taxId: profile.taxId ?? '',
        bankAccount: profile.bankAccount ?? '',
        notes: profile.notes ?? ''
      });
      this.submitError.set(null);
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

    this.employmentService
      .upsertEmploymentProfile(this.userId(), {
        employeeId: raw.employeeId.trim() || null,
        nationalId: raw.nationalId.trim() || null,
        taxId: raw.taxId.trim() || null,
        bankAccount: raw.bankAccount.trim() || null,
        notes: raw.notes.trim() || null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.employmentProfileSaved);
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
