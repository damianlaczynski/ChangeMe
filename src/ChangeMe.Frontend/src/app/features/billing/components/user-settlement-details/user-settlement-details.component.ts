import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { UserSettlementDetailsDto } from '@features/billing/models/settlement.model';
import { SettlementsService } from '@features/billing/services/settlements.service';
import {
  BillingMessages,
  formatMonthlyHoursNorm,
  getContractTypeLabel,
  getSettlementBalanceClass
} from '@features/billing/utils/billing.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';

@Component({
  selector: 'app-user-settlement-details',
  imports: [
    DatePipe,
    DecimalPipe,
    BackButtonComponent,
    Card,
    Panel,
    Message,
    ProgressSpinner,
    TableModule
  ],
  templateUrl: './user-settlement-details.component.html'
})
export class UserSettlementDetailsComponent {
  readonly id = input.required<string>();

  private readonly settlementsService = inject(SettlementsService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getContractTypeLabel = getContractTypeLabel;
  readonly getSettlementBalanceClass = getSettlementBalanceClass;

  readonly settlement = signal<UserSettlementDetailsDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly fromMyBilling = signal(false);

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.fromMyBilling.set(params.get('from') === 'my-billing');
      });

    effect(() => {
      this.loadSettlement(this.id());
    });
  }

  backRoute(): string[] {
    return this.fromMyBilling() ? ['/my-billing'] : ['/settlements'];
  }

  private loadSettlement(id: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.settlementsService
      .getUserSettlementById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (settlement) => {
          this.settlement.set(settlement);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
