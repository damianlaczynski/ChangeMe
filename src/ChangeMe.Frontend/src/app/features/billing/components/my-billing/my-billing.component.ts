import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { MySettlementListItemDto } from '@features/billing/models/settlement.model';
import { SettlementsService } from '@features/billing/services/settlements.service';
import {
  BillingMessages,
  formatMonthlyHoursNorm,
  getSettlementBalanceClass
} from '@features/billing/utils/billing.utils';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-my-billing',
  imports: [Card, Panel, Message, ProgressSpinner, TableModule],
  templateUrl: './my-billing.component.html'
})
export class MyBillingComponent {
  private readonly settlementsService = inject(SettlementsService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getSettlementBalanceClass = getSettlementBalanceClass;

  readonly settlements = signal<MySettlementListItemDto[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.loadSettlements();
  }

  onRowClick(settlement: MySettlementListItemDto): void {
    void this.router.navigate(['/user-settlements', settlement.id], {
      queryParams: { from: 'my-billing' }
    });
  }

  private loadSettlements(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.settlementsService
      .getMySettlements()
      .pipe(
        catchError((error: Error) => {
          this.errorMessage.set(error.message);
          return of([] as MySettlementListItemDto[]);
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((settlements) => {
        this.settlements.set(settlements);
        this.isLoading.set(false);
      });
  }
}
