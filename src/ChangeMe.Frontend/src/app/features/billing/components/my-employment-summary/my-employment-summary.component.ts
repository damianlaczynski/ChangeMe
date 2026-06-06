import { DatePipe, DecimalPipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@features/auth/services/auth.service';
import { MyEmploymentSummaryDto } from '@features/billing/models/employment.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import {
  BillingMessages,
  formatMonthlyHoursNorm,
  getContractTypeLabel
} from '@features/billing/utils/billing.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { Panel } from 'primeng/panel';

@Component({
  selector: 'app-my-employment-summary',
  imports: [DatePipe, DecimalPipe, Panel],
  templateUrl: './my-employment-summary.component.html'
})
export class MyEmploymentSummaryComponent implements OnInit {
  private readonly employmentService = inject(EmploymentService);
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly summary = signal<MyEmploymentSummaryDto | null>(null);
  readonly isLoading = signal(false);

  readonly canView = computed(() =>
    this.authService.hasPermission(PermissionCodes.billingViewOwn)
  );

  readonly BillingMessages = BillingMessages;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getContractTypeLabel = getContractTypeLabel;

  ngOnInit(): void {
    if (!this.canView()) {
      return;
    }

    this.isLoading.set(true);
    this.employmentService
      .getMyEmploymentSummary()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (summary) => {
          this.summary.set(summary);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
