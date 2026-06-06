import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { PositionDetailsDto } from '@features/billing/models/position.model';
import { PositionsService } from '@features/billing/services/positions.service';
import {
  BillingMessages,
  getPositionActiveLabel,
  getPositionActiveSeverity
} from '@features/billing/utils/billing.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-position-details',
  imports: [
    RouterLink,
    BackButtonComponent,
    Card,
    Button,
    Message,
    Tag,
    ProgressSpinner
  ],
  templateUrl: './position-details.component.html'
})
export class PositionDetailsComponent {
  readonly id = input.required<string>();

  private readonly positionsService = inject(PositionsService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly getPositionActiveLabel = getPositionActiveLabel;
  readonly getPositionActiveSeverity = getPositionActiveSeverity;

  readonly position = signal<PositionDetailsDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    effect(() => {
      const positionId = this.id();
      this.loadPosition(positionId);
    });
  }

  confirmDelete(): void {
    const current = this.position();
    if (!current?.canDelete) {
      return;
    }

    this.confirmationService.confirm({
      header: 'Delete position',
      message: BillingMessages.deletePositionConfirm(current.name),
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deletePosition(current.id)
    });
  }

  private deletePosition(id: string): void {
    this.positionsService
      .deletePosition(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.positionDeleted);
          void this.router.navigate(['/billing/positions']);
        },
        error: (error: Error) =>
          this.toastService.showApiError(error, 'Could not delete position')
      });
  }

  private loadPosition(id: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.positionsService
      .getPositionById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (position) => {
          this.position.set(position);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
