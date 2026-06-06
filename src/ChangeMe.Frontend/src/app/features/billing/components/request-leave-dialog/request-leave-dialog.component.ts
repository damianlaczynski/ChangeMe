import {
  Component,
  DestroyRef,
  computed,
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
  LeaveDayPortion,
  LeaveRequestDetailsDto,
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
import { parseIsoDateString, toIsoDateString } from '@features/time/utils/time.utils';
import { Button } from 'primeng/button';
import { DatePicker } from 'primeng/datepicker';
import { Dialog } from 'primeng/dialog';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-request-leave-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, Select, DatePicker, Textarea, Message],
  templateUrl: './request-leave-dialog.component.html'
})
export class RequestLeaveDialogComponent {
  readonly request = input<LeaveRequestDetailsDto | null>(null);
  readonly visible = input(false);
  readonly isAdmin = input(false);
  readonly userId = input<string | null>(null);
  readonly visibleChange = output<boolean>();
  readonly saved = output<void>();

  private readonly leaveRequestsService = inject(LeaveRequestsService);
  private readonly leaveTypesService = inject(LeaveTypesService);
  private readonly billingSettingsService = inject(BillingSettingsService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly leaveDayPortionOptions = leaveDayPortionOptions;
  readonly constraints = LeaveRequestConstraints;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly leaveTypes = signal<LeaveTypeListItemDto[]>([]);
  readonly allowHalfDayLeave = signal(true);

  readonly form = new FormGroup({
    leaveTypeId: new FormControl<string | null>(null, Validators.required),
    startDate: new FormControl<Date | null>(null, Validators.required),
    endDate: new FormControl<Date | null>(null, Validators.required),
    dayPortion: new FormControl<LeaveDayPortion | null>(LeaveDayPortion.FullDay),
    reason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.maxLength(LeaveRequestConstraints.REASON_MAX_LENGTH)]
    })
  });

  readonly showDayPortion = computed(() => {
    const start = this.form.controls.startDate.value;
    const end = this.form.controls.endDate.value;
    return (
      this.allowHalfDayLeave() &&
      !!start &&
      !!end &&
      toIsoDateString(start) === toIsoDateString(end)
    );
  });

  readonly isEditMode = () => this.request() !== null;

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.loadData();
      const existing = this.request();
      this.submitError.set(null);
      if (existing) {
        this.form.reset({
          leaveTypeId: existing.leaveTypeId,
          startDate: parseIsoDateString(existing.startDate),
          endDate: parseIsoDateString(existing.endDate),
          dayPortion: existing.dayPortion ?? LeaveDayPortion.FullDay,
          reason: existing.reason ?? ''
        });
      } else {
        this.form.reset({
          leaveTypeId: null,
          startDate: null,
          endDate: null,
          dayPortion: LeaveDayPortion.FullDay,
          reason: ''
        });
      }
    });

    this.form.controls.startDate.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((startDate) => {
        const endDate = this.form.controls.endDate.value;
        if (startDate && !endDate) {
          this.form.controls.endDate.setValue(startDate);
        }
      });
  }

  onHide(): void {
    this.visibleChange.emit(false);
  }

  save(submit: boolean): void {
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    if (!raw.startDate || !raw.endDate || !raw.leaveTypeId) {
      return;
    }

    if (raw.endDate < raw.startDate) {
      this.submitError.set('End date must be on or after start date.');
      return;
    }

    this.isSubmitting.set(true);
    const payload = {
      leaveTypeId: raw.leaveTypeId,
      startDate: toIsoDateString(raw.startDate),
      endDate: toIsoDateString(raw.endDate),
      dayPortion: this.showDayPortion() ? raw.dayPortion : null,
      reason: raw.reason.trim() || null
    };

    const existing = this.request();
    if (existing) {
      this.leaveRequestsService
        .updateLeaveRequest(existing.id, payload)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: () => {
            if (!submit) {
              this.finishSuccess(false);
              return;
            }

            this.leaveRequestsService
              .submitLeaveRequest(existing.id)
              .pipe(takeUntilDestroyed(this.destroyRef))
              .subscribe({
                next: (details) => this.finishSuccess(true, details),
                error: (error: Error) => this.onError(error)
              });
          },
          error: (error: Error) => this.onError(error)
        });
      return;
    }

    const create$ = this.isAdmin()
      ? this.leaveRequestsService.createLeaveRequest({
          ...payload,
          userId: this.userId(),
          submit
        })
      : this.leaveRequestsService.createMyLeaveRequest({ ...payload, submit });

    create$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: (details) => this.finishSuccess(submit, details),
      error: (error: Error) => this.onError(error)
    });
  }

  private finishSuccess(submitted: boolean, details?: LeaveRequestDetailsDto): void {
    if (submitted && details?.status === LeaveRequestStatus.Approved) {
      this.toastService.success(BillingMessages.leaveRequestApproved);
    } else if (submitted) {
      this.toastService.success(BillingMessages.leaveRequestSubmitted);
    } else {
      this.toastService.success(BillingMessages.leaveRequestSaved);
    }
    this.visibleChange.emit(false);
    this.saved.emit();
    this.isSubmitting.set(false);
  }

  private onError(error: Error): void {
    this.submitError.set(error.message);
    this.isSubmitting.set(false);
  }

  private loadData(): void {
    this.leaveTypesService
      .getLeaveTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (types) => this.leaveTypes.set(types.filter((t) => t.isActive))
      });

    this.billingSettingsService
      .getBillingSettings()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settings) => this.allowHalfDayLeave.set(settings.allowHalfDayLeave)
      });
  }
}
