import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import {
  LeaveDayPortion,
  LeaveRequestStatus
} from '@features/billing/models/leave-request.model';
import { LeaveTypeListItemDto } from '@features/billing/models/leave-type.model';
import { BillingSettingsService } from '@features/billing/services/billing-settings.service';
import { LeaveRequestsService } from '@features/billing/services/leave-requests.service';
import { LeaveTypesService } from '@features/billing/services/leave-types.service';
import {
  BillingMessages,
  LeaveRequestConstraints,
  leaveDayPortionOptions
} from '@features/billing/utils/billing.utils';
import { toIsoDateString } from '@features/time/utils/time.utils';
import { UsersService } from '@features/users/services/users.service';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-create-leave-request',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Panel,
    Button,
    Select,
    DatePicker,
    Textarea,
    Message
  ],
  templateUrl: './create-leave-request.component.html'
})
export class CreateLeaveRequestComponent {
  private readonly leaveRequestsService = inject(LeaveRequestsService);
  private readonly leaveTypesService = inject(LeaveTypesService);
  private readonly billingSettingsService = inject(BillingSettingsService);
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly leaveDayPortionOptions = leaveDayPortionOptions;
  readonly constraints = LeaveRequestConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly leaveTypes = signal<LeaveTypeListItemDto[]>([]);
  readonly users = signal<{ id: string; label: string }[]>([]);
  readonly allowHalfDayLeave = signal(true);

  readonly form = new FormGroup({
    userId: new FormControl<string | null>(null, Validators.required),
    leaveTypeId: new FormControl<string | null>(null, Validators.required),
    startDate: new FormControl<Date | null>(null, Validators.required),
    endDate: new FormControl<Date | null>(null, Validators.required),
    dayPortion: new FormControl<LeaveDayPortion | null>(LeaveDayPortion.FullDay),
    reason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(LeaveRequestConstraints.REASON_MAX_LENGTH)]
    })
  });

  get showDayPortion(): boolean {
    const start = this.form.controls.startDate.value;
    const end = this.form.controls.endDate.value;
    return (
      this.allowHalfDayLeave() &&
      !!start &&
      !!end &&
      toIsoDateString(start) === toIsoDateString(end)
    );
  }

  constructor() {
    this.loadData();

    this.form.controls.startDate.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((startDate) => {
        const endDate = this.form.controls.endDate.value;
        if (startDate && !endDate) {
          this.form.controls.endDate.setValue(startDate);
        }
      });
  }

  cancel(): void {
    void this.router.navigate(['/leave-requests']);
  }

  save(submit: boolean): void {
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (!raw.startDate || !raw.endDate || !raw.leaveTypeId || !raw.userId) {
      return;
    }

    if (raw.endDate < raw.startDate) {
      this.submitError.set('End date must be on or after start date.');
      return;
    }

    this.isSubmitting.set(true);
    this.leaveRequestsService
      .createLeaveRequest({
        userId: raw.userId,
        leaveTypeId: raw.leaveTypeId,
        startDate: toIsoDateString(raw.startDate),
        endDate: toIsoDateString(raw.endDate),
        dayPortion: this.showDayPortion ? raw.dayPortion : null,
        reason: raw.reason.trim() || null,
        submit
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          if (submit && details.status === LeaveRequestStatus.Approved) {
            this.toastService.success(BillingMessages.leaveRequestApproved);
          } else if (submit) {
            this.toastService.success(BillingMessages.leaveRequestSubmitted);
          } else {
            this.toastService.success(BillingMessages.leaveRequestSaved);
          }
          void this.router.navigate(['/leave-requests', details.id]);
          this.isSubmitting.set(false);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        }
      });
  }

  private loadData(): void {
    this.leaveTypesService
      .getLeaveTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (types) => this.leaveTypes.set(types.filter((type) => type.isActive))
      });

    this.billingSettingsService
      .getBillingSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => this.allowHalfDayLeave.set(settings.allowHalfDayLeave)
      });

    this.usersService
      .getUsers({
        pageNumber: 1,
        pageSize: 200,
        sortField: 'LastName',
        ascending: true
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) =>
          this.users.set(
            result.items
              .filter((user) => !user.deactivated)
              .map((user) => {
                const name = `${user.firstName} ${user.lastName}`.trim();
                return {
                  id: user.id,
                  label: name ? `${name} (${user.email})` : user.email
                };
              })
          )
      });
  }
}
