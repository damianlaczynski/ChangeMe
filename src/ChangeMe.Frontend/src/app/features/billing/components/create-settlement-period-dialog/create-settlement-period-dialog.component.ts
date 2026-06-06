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
import { SettlementsService } from '@features/billing/services/settlements.service';
import {
  BillingMessages,
  SettlementConstraints,
  getSettlementYearRange,
  settlementMonthOptions
} from '@features/billing/utils/billing.utils';
import { Button } from 'primeng/button';
import { Dialog } from 'primeng/dialog';
import { InputNumber } from 'primeng/inputnumber';
import { Message } from 'primeng/message';
import { Select } from 'primeng/select';

@Component({
  selector: 'app-create-settlement-period-dialog',
  imports: [ReactiveFormsModule, Dialog, Button, InputNumber, Select, Message],
  templateUrl: './create-settlement-period-dialog.component.html'
})
export class CreateSettlementPeriodDialogComponent {
  readonly visible = input(false);
  readonly visibleChange = output<boolean>();
  readonly created = output<string>();

  private readonly settlementsService = inject(SettlementsService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly settlementMonthOptions = settlementMonthOptions;
  readonly yearRange = getSettlementYearRange();
  readonly submitError = signal<string | null>(null);
  readonly isSubmitting = signal(false);

  readonly form = new FormGroup({
    year: new FormControl(new Date().getFullYear(), {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(this.yearRange.min),
        Validators.max(this.yearRange.max)
      ]
    }),
    month: new FormControl(new Date().getMonth() + 1, {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.min(SettlementConstraints.MIN_MONTH),
        Validators.max(SettlementConstraints.MAX_MONTH)
      ]
    })
  });

  constructor() {
    effect(() => {
      if (!this.visible()) {
        return;
      }

      this.submitError.set(null);
      this.form.reset({
        year: new Date().getFullYear(),
        month: new Date().getMonth() + 1
      });
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

    this.settlementsService
      .createSettlementPeriod({ year: raw.year, month: raw.month })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (period) => {
          this.toastService.success(BillingMessages.settlementPeriodCreated);
          this.visibleChange.emit(false);
          this.created.emit(period.id);
        },
        error: (error: Error) => {
          this.submitError.set(error.message);
          this.isSubmitting.set(false);
        },
        complete: () => this.isSubmitting.set(false)
      });
  }
}
