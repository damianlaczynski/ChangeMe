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
import {
  ContractType,
  EmploymentContractDetailsDto
} from '@features/billing/models/employment.model';
import { PositionListItemDto } from '@features/billing/models/position.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import { PositionsService } from '@features/billing/services/positions.service';
import {
  BillingMessages,
  EmploymentConstraints,
  combineDurationMinutes,
  contractTypeOptions,
  splitDurationMinutes
} from '@features/billing/utils/billing.utils';
import { parseIsoDateString, toIsoDateString } from '@features/time/utils/time.utils';
import { UserDetailsDto } from '@features/users/models/user.model';
import { UsersService } from '@features/users/services/users.service';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { DatePicker } from 'primeng/datepicker';
import { InputNumber } from 'primeng/inputnumber';
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
  selector: 'app-edit-employment-contract',
  imports: [
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Panel,
    Button,
    Select,
    DatePicker,
    InputNumber,
    Textarea,
    Message,
    ProgressSpinner
  ],
  templateUrl: './edit-employment-contract.component.html'
})
export class EditEmploymentContractComponent {
  readonly id = input.required<string>();
  readonly contractId = input.required<string>();

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
  readonly contract = signal<EmploymentContractDetailsDto | null>(null);
  readonly positions = signal<PositionListItemDto[]>([]);
  readonly userOptions = signal<{ id: string; label: string }[]>([]);

  readonly form = new FormGroup(
    {
      userId: new FormControl<string | null>(null, Validators.required),
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
      this.loadPage(this.id(), this.contractId());
    });
  }

  cancel(): void {
    void this.router.navigate(['/users', this.id()], {
      queryParams: { expandEmployment: '1' }
    });
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
      .updateEmploymentContract(this.id(), this.contractId(), {
        userId: raw.userId!,
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
          this.toastService.success(BillingMessages.contractSaved);
          void this.router.navigate(['/users', raw.userId!], {
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

  private loadPage(userId: string, contractId: string): void {
    this.isLoading.set(true);
    this.loadError.set(null);

    this.employmentService
      .getEmploymentContract(userId, contractId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (contract) => {
          if (!contract.canManage) {
            this.loadError.set('You do not have permission to edit this contract.');
            this.isLoading.set(false);
            return;
          }

          this.contract.set(contract);
          const duration = splitDurationMinutes(contract.monthlyHoursNormMinutes);
          this.form.reset({
            userId: contract.userId,
            positionId: contract.positionId,
            contractType: contract.contractType,
            startDate: parseIsoDateString(contract.startDate),
            endDate: contract.endDate ? parseIsoDateString(contract.endDate) : null,
            fte: contract.fte,
            normHours: duration.hours,
            normMinutes: duration.minutes,
            hourlyRate: contract.hourlyRate ?? null,
            monthlySalary: contract.monthlySalary ?? null,
            notes: contract.notes ?? ''
          });
          this.loadUser(userId, contract);
          this.loadUserOptions(contract.userId, contract.userDisplayName);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private loadUser(userId: string, contract: EmploymentContractDetailsDto): void {
    this.usersService
      .getUserById(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (user) => {
          this.user.set(user);
          this.loadPositions(contract.positionId, contract.positionName);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  private loadUserOptions(currentUserId: string, currentUserLabel: string): void {
    this.usersService
      .getUsers({
        pageNumber: 1,
        pageSize: 200,
        sortField: 'LastName',
        ascending: true,
        deactivated: [false]
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          const options = result.items.map((user) => ({
            id: user.id,
            label: `${user.firstName} ${user.lastName}`.trim() || user.email
          }));
          if (!options.some((option) => option.id === currentUserId)) {
            options.unshift({ id: currentUserId, label: currentUserLabel });
          }
          this.userOptions.set(options);
        },
        error: (error: Error) => {
          this.userOptions.set([{ id: currentUserId, label: currentUserLabel }]);
          this.loadError.set(error.message);
        }
      });
  }

  private loadPositions(currentPositionId: string, currentPositionName: string): void {
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
          const active = result.items.filter((p) => p.isActive);
          const hasCurrent = active.some((p) => p.id === currentPositionId);
          if (!hasCurrent) {
            active.unshift({
              id: currentPositionId,
              name: currentPositionName,
              isActive: false,
              contractCount: 0,
              canManage: false
            });
          }
          this.positions.set(active);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.loadError.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
