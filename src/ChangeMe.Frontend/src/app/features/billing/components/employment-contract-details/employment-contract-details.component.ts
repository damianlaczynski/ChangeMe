import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { EmploymentContractDetailsDto } from '@features/billing/models/employment.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import {
  BillingMessages,
  formatCompensationDisplay,
  formatMonthlyHoursNorm,
  getContractStatusSeverity,
  getContractTypeLabel
} from '@features/billing/utils/billing.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-employment-contract-details',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    BackButtonComponent,
    Card,
    Panel,
    Button,
    Message,
    Tag,
    ProgressSpinner
  ],
  templateUrl: './employment-contract-details.component.html'
})
export class EmploymentContractDetailsComponent {
  readonly id = input.required<string>();
  readonly contractId = input.required<string>();

  private readonly employmentService = inject(EmploymentService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly contract = signal<EmploymentContractDetailsDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  readonly BillingMessages = BillingMessages;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly formatCompensationDisplay = formatCompensationDisplay;
  readonly getContractTypeLabel = getContractTypeLabel;
  readonly getContractStatusSeverity = getContractStatusSeverity;

  constructor() {
    effect(() => {
      this.loadContract(this.id(), this.contractId());
    });
  }

  back(): void {
    void this.router.navigate(['/users', this.id()], {
      queryParams: { expandEmployment: '1' }
    });
  }

  private loadContract(userId: string, contractId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.employmentService
      .getEmploymentContract(userId, contractId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (contract) => {
          this.contract.set(contract);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
