import { Component, effect, inject, input, output, signal } from '@angular/core';
import {
  FormArray,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { ToastService } from '@core/toast/services/toast.service';
import {
  AvailabilityStatus,
  WeeklyRecurringPatternDayDto,
  WeeklyRecurringPatternDto
} from '@features/billing/models/availability.model';
import { DayOfWeek } from '@features/billing/models/billing-settings.model';
import { AvailabilityService } from '@features/billing/services/availability.service';
import {
  BillingMessages,
  recurringAvailabilityStatusOptions,
  weekdayOptions
} from '@features/billing/utils/billing.utils';
import { Button } from 'primeng/button';
import { Checkbox } from 'primeng/checkbox';
import { Dialog } from 'primeng/dialog';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-edit-weekly-pattern-dialog',
  imports: [
    ReactiveFormsModule,
    Dialog,
    Button,
    Checkbox,
    InputText,
    Select,
    TableModule,
    Message
  ],
  templateUrl: './edit-weekly-pattern-dialog.component.html'
})
export class EditWeeklyPatternDialogComponent {
  private readonly availabilityService = inject(AvailabilityService);
  private readonly toastService = inject(ToastService);

  readonly visible = input(false);
  readonly userId = input<string | null>(null);
  readonly manageForUser = input(false);
  readonly pattern = input<WeeklyRecurringPatternDto | null>(null);

  readonly hidden = output<void>();
  readonly saved = output<void>();

  readonly BillingMessages = BillingMessages;
  readonly weekdayOptions = weekdayOptions;
  readonly recurringAvailabilityStatusOptions = recurringAvailabilityStatusOptions;

  readonly isSubmitting = signal(false);
  readonly submitError = signal<string | null>(null);

  readonly form = new FormGroup({
    days: new FormArray<
      FormGroup<{
        dayOfWeek: FormControl<DayOfWeek>;
        enabled: FormControl<boolean>;
        startTime: FormControl<string>;
        endTime: FormControl<string>;
        status: FormControl<AvailabilityStatus>;
      }>
    >([])
  });

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.submitError.set(null);
      this.buildForm(this.pattern());
    });
  }

  get dayRows(): FormArray {
    return this.form.controls.days;
  }

  onHide(): void {
    this.hidden.emit();
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const days = this.dayRows.controls.map((row) => {
      const value = row.getRawValue();
      return {
        dayOfWeek: value.dayOfWeek,
        enabled: value.enabled,
        startTime: value.enabled ? value.startTime : null,
        endTime: value.enabled ? value.endTime : null,
        status: value.enabled ? value.status : null
      } satisfies WeeklyRecurringPatternDayDto;
    });

    this.isSubmitting.set(true);
    const request$ = this.manageForUser()
      ? this.availabilityService.saveUserPattern(this.userId()!, days)
      : this.availabilityService.saveMyPattern(days);

    request$.pipe(finalize(() => this.isSubmitting.set(false))).subscribe({
      next: () => {
        this.toastService.success(BillingMessages.weeklyPatternSaved);
        this.saved.emit();
      },
      error: (error) => {
        this.submitError.set(error?.error?.message ?? 'Unable to save weekly pattern.');
      }
    });
  }

  private buildForm(pattern: WeeklyRecurringPatternDto | null): void {
    this.dayRows.clear();

    for (const weekday of weekdayOptions) {
      const row = pattern?.days.find((day) => day.dayOfWeek === weekday.value);
      const group = new FormGroup({
        dayOfWeek: new FormControl(weekday.value, { nonNullable: true }),
        enabled: new FormControl(row?.enabled ?? false, { nonNullable: true }),
        startTime: new FormControl(row?.startTime ?? '09:00'),
        endTime: new FormControl(row?.endTime ?? '17:00'),
        status: new FormControl(row?.status ?? AvailabilityStatus.OnSite, {
          nonNullable: true
        })
      });

      group.controls.enabled.valueChanges.subscribe((enabled) => {
        if (enabled) {
          group.controls.startTime.setValidators(Validators.required);
          group.controls.endTime.setValidators(Validators.required);
        } else {
          group.controls.startTime.clearValidators();
          group.controls.endTime.clearValidators();
        }

        group.controls.startTime.updateValueAndValidity({ emitEvent: false });
        group.controls.endTime.updateValueAndValidity({ emitEvent: false });
      });

      if (row?.enabled) {
        group.controls.startTime.setValidators(Validators.required);
        group.controls.endTime.setValidators(Validators.required);
      }

      this.dayRows.push(group);
    }
  }
}
