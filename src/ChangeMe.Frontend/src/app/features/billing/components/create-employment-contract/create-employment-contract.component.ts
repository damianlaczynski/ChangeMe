import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { formatUserReference } from '@core/user/utils/user-display.utils';
import { ContractType } from '@features/billing/models/employment.model';
import { PositionListItemDto } from '@features/billing/models/position.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import { PositionsService } from '@features/billing/services/positions.service';
import {
  BillingMessages,
  EmploymentConstraints,
  combineDurationMinutes,
  contractTypeOptions
} from '@features/billing/utils/billing.utils';
import { toIsoDateString } from '@features/time/utils/time.utils';
import { UserDetailsDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { InputNumber } from 'primeng/inputnumber';
import { InputText } from 'primeng/inputtext';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';

function compensationValidator(control: AbstractControl): ValidationErrors | null {
  const hourlyRate = control.get('hourlyRate')?.value as number | null;
  const monthlySalary = control.get('monthlySalary')?.value as number | null;
  if (hourlyRate == null && monthlySalary == null) {
    return { compensationRequired: true };
  }
  return null;
}

@Component({
  selector: 'app-create-employment-contract',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Panel,
    Button,
    Select,
    DatePicker,
    InputNumber,
    InputText,
    Textarea,
    Message,
    ProgressSpinner
  ],
  templateUrl: './create-employment-contract.component.html'
})
export class CreateEmploymentContractComponent {
  readonly id = input.required<string>();

  private readonly employmentService = inject(EmploymentService);
  private readonly positionsService = inject(PositionsService);
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly constraints = EmploymentConstraints;
  readonly contractTypeOptions = contractTypeOptions;
  readonly BillingMessages = BillingMessages;
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);
  readonly isLoading = signal(true);
  readonly loadError = signal<string | null>(null);
  readonly user = signal<UserDetailsDto | null>(null);
  readonly positions = signal<PositionListItemDto[]>([]);

  readonly form = new FormGroup(
    {
      positionId: new FormControl<string | null>(null, Validators.required),
      contractType: new FormControl<ContractType | null>(null, Validators.required),
      startDate: new FormControl<Date | null>(null, Validators.required),
      endDate: new FormControl<Date | null>(null),
      fte: new FormControl<number | null>(1, [
        Validators.required,
        Validators.min(EmploymentConstraints.MIN_FTE),
        Validators.max(EmploymentConstraints.MAX_FTE)
      ]),
      normHours: new FormControl<number>(160, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(0), Validators.max(168)]
      }),
      normMinutes: new FormControl<number>(0, {
        nonNullable: true,
        validators: [Validators.required, Validators.min(0), Validators.max(59)]
      }),
      hourlyRate: new FormControl<number | null>(null, {
        validators: [Validators.min(EmploymentConstraints.MIN_COMPENSATION)]
      }),
      monthlySalary: new FormControl<number | null>(null, {
        validators: [Validators.min(EmploymentConstraints.MIN_COMPENSATION)]
      }),
      notes: new FormControl('', {
        nonNullable: true,
        validators: [
          Validators.maxLength(EmploymentConstraints.CONTRACT_NOTES_MAX_LENGTH)
        ]
      })
    },
    { validators: compensationValidator }
  );

  readonly userLabel = () => {
    const profile = this.user();
    return profile ? formatUserReference(profile) : '';
  };

  constructor() {
    effect(() => {
      this.loadPage(this.id());
    });
  }

  cancel(): void {
    void this.router.navigate(['/users', this.id()]);
  }

  onSubmit(): void {
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const monthlyHoursNormMinutes = combineDurationMinutes(
      raw.normHours,
      raw.normMinutes
    );
    if (
      monthlyHoursNormMinutes < EmploymentConstraints.MIN_MONTHLY_HOURS_NORM_MINUTES ||
      monthlyHoursNormMinutes > EmploymentConstraints.MAX_MONTHLY_HOURS_NORM_MINUTES
    ) {
      this.submitError.set('Monthly hours norm must be between 60 and 10080 minutes.');
      return;
    }

    if (raw.endDate && raw.startDate && raw.endDate < raw.startDate) {
      this.submitError.set('End date must be on or after start date.');
      return;
    }

    this.isSubmitting.set(true);
    this.employmentService
      .createEmploymentContract(this.id(), {
        positionId: raw.positionId!,
        contractType: raw.contractType!,
        startDate: toIsoDateString(raw.startDate!),
        endDate: raw.endDate ? toIsoDateString(raw.endDate) : null,
        fte: Number(raw.fte!.toFixed(2)),
        monthlyHoursNormMinutes,
        hourlyRate: raw.hourlyRate ?? null,
        monthlySalary: raw.monthlySalary ?? null,
        notes: raw.notes.trim() || null
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.contractCreated);
          void this.router.navigate(['/users', this.id()], {
            queryParams: { expandEmployment: '1' }
          });
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }

  private loadPage(userId: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.usersService
      .getUserById(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.loadPositions();
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private loadPositions(): void {
    this.positionsService
      .getPositions({
        pageNumber: 1,
        pageSize: 100,
        sortField: 'Name',
        ascending: true
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.positions.set(result.items.filter((p) => p.isActive));
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
