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
import {
  AvailabilityEntryDto,
  AvailabilityStatus,
  CreateAvailabilityEntryRequest,
  UpdateAvailabilityEntryRequest
} from '@features/billing/models/availability.model';
import { AvailabilityService } from '@features/billing/services/availability.service';
import {
  BillingMessages,
  availabilityStatusOptions
} from '@features/billing/utils/billing.utils';
import { parseIsoDateString, toIsoDateString } from '@features/time/utils/time.utils';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { DatePicker } from 'primeng/datepicker';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-edit-availability-dialog',
  imports: [
    ReactiveFormsModule,
    Dialog,
    Button,
    DatePicker,
    Checkbox,
    Select,
    InputText,
    Textarea,
    Message
  ],
  templateUrl: './edit-availability-dialog.component.html'
})
export class EditAvailabilityDialogComponent {
  private readonly availabilityService = inject(AvailabilityService);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly visible = input(false);
  readonly userId = input<string | null>(null);
  readonly manageForUser = input(false);
  readonly showUserPicker = input(false);
  readonly userOptions = input<{ label: string; value: string }[]>([]);
  readonly selectedDate = input<string | null>(null);
  readonly entry = input<AvailabilityEntryDto | null>(null);

  readonly hidden = output<void>();
  readonly saved = output<void>();

  readonly BillingMessages = BillingMessages;
  readonly availabilityStatusOptions = availabilityStatusOptions;
  readonly AvailabilityStatus = AvailabilityStatus;

  readonly isSubmitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly form = new FormGroup({
    userId: new FormControl<string | null>(null),
    startDate: new FormControl<Date | null>(null, Validators.required),
    endDate: new FormControl<Date | null>(null, Validators.required),
    allDay: new FormControl(true, { nonNullable: true }),
    startTime: new FormControl('09:00'),
    endTime: new FormControl('17:00'),
    status: new FormControl<AvailabilityStatus>(AvailabilityStatus.Available, {
      nonNullable: true
    }),
    notes: new FormControl('')
  });

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.submitError.set(null);
      const currentEntry = this.entry();
      const date = this.selectedDate();

      if (currentEntry) {
        this.form.patchValue({
          userId: currentEntry.userId,
          startDate: parseIsoDateString(currentEntry.startDate),
          endDate: parseIsoDateString(currentEntry.endDate),
          allDay: currentEntry.allDay,
          startTime: currentEntry.startTime ?? '09:00',
          endTime: currentEntry.endTime ?? '17:00',
          status: currentEntry.status,
          notes: currentEntry.notes
        });
      } else if (date) {
        const parsed = parseIsoDateString(date);
        this.form.reset({
          userId: this.showUserPicker() ? null : (this.userId() ?? null),
          startDate: parsed,
          endDate: parsed,
          allDay: true,
          startTime: '09:00',
          endTime: '17:00',
          status: AvailabilityStatus.Available,
          notes: ''
        });
      } else {
        const today = new Date();
        this.form.reset({
          userId: this.showUserPicker() ? null : (this.userId() ?? null),
          startDate: today,
          endDate: today,
          allDay: true,
          startTime: '09:00',
          endTime: '17:00',
          status: AvailabilityStatus.Available,
          notes: ''
        });
      }

      this.syncUserValidators();
    });

    this.form.controls.allDay.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.syncTimedValidators());
  }

  get isEditMode(): boolean {
    return this.entry() != null;
  }

  get dialogHeader(): string {
    return this.isEditMode ? 'Edit availability' : 'Add availability';
  }

  onHide(): void {
    this.hidden.emit();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload = this.buildPayload();
    this.isSubmitting.set(true);
    this.submitError.set(null);

    const request$ = this.isEditMode
      ? this.availabilityService.updateEntry(this.entry()!.id, payload)
      : this.shouldCreateForAnotherUser()
        ? this.availabilityService.createUserEntry({
            ...payload,
            userId: this.resolveTargetUserId()!
          })
        : this.availabilityService.createMyEntry(payload);

    request$.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: () => {
        this.toastService.success(BillingMessages.availabilitySaved);
        this.saved.emit();
      },
      error: (error) => {
        this.submitError.set(error?.error?.message ?? 'Unable to save availability.');
      }
    });
  }

  deleteEntry(): void {
    const currentEntry = this.entry();
    if (!currentEntry) {
      return;
    }

    this.confirmationService.confirm({
      message: BillingMessages.deleteAvailabilityConfirm,
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => {
        this.isSubmitting.set(true);
        this.availabilityService
          .deleteEntry(currentEntry.id)
          .pipe(finalize(() => this.isSubmitting.set(false)))
          .subscribe({
            next: () => {
              this.toastService.success(BillingMessages.availabilityDeleted);
              this.saved.emit();
            },
            error: (error) => {
              this.submitError.set(
                error?.error?.message ?? 'Unable to delete availability.'
              );
            }
          });
      }
    });
  }

  showTimedFields(): boolean {
    return !this.form.controls.allDay.value;
  }

  showUserField(): boolean {
    return this.showUserPicker() && !this.isEditMode;
  }

  private shouldCreateForAnotherUser(): boolean {
    if (this.isEditMode) {
      return false;
    }

    if (this.showUserPicker()) {
      return true;
    }

    return this.manageForUser();
  }

  private resolveTargetUserId(): string | null {
    if (this.showUserPicker()) {
      return this.form.controls.userId.value;
    }

    return this.userId();
  }

  private syncUserValidators(): void {
    if (this.showUserPicker() && !this.isEditMode) {
      this.form.controls.userId.setValidators(Validators.required);
    } else {
      this.form.controls.userId.clearValidators();
    }

    this.form.controls.userId.updateValueAndValidity({ emitEvent: false });
  }

  private buildPayload():
    | CreateAvailabilityEntryRequest
    | UpdateAvailabilityEntryRequest {
    const raw = this.form.getRawValue();
    const sameDay =
      raw.startDate &&
      raw.endDate &&
      toIsoDateString(raw.startDate) === toIsoDateString(raw.endDate);

    return {
      startDate: toIsoDateString(raw.startDate!),
      endDate: toIsoDateString(raw.endDate!),
      allDay: raw.allDay || !sameDay,
      startTime: raw.allDay || !sameDay ? null : raw.startTime,
      endTime: raw.allDay || !sameDay ? null : raw.endTime,
      status: raw.status,
      notes: raw.notes?.trim() || null
    };
  }

  private syncTimedValidators(): void {
    const allDay = this.form.controls.allDay.value;
    if (allDay) {
      this.form.controls.startTime.clearValidators();
      this.form.controls.endTime.clearValidators();
    } else {
      this.form.controls.startTime.setValidators(Validators.required);
      this.form.controls.endTime.setValidators(Validators.required);
    }

    this.form.controls.startTime.updateValueAndValidity({ emitEvent: false });
    this.form.controls.endTime.updateValueAndValidity({ emitEvent: false });
  }
}
