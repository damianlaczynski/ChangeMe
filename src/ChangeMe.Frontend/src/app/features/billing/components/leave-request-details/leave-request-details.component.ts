import { DatePipe } from '@angular/common';
import { Component, DestroyRef, effect, inject, input, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { RequestLeaveDialogComponent } from '@features/billing/components/request-leave-dialog/request-leave-dialog.component';
import {
  LeaveRequestDetailsDto,
  LeaveRequestStatus
} from '@features/billing/models/leave-request.model';
import { LeaveRequestsService } from '@features/billing/services/leave-requests.service';
import {
  BillingMessages,
  LeaveRequestConstraints,
  getLeaveDayPortionLabel,
  getLeaveRequestStatusSeverity
} from '@features/billing/utils/billing.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { createDestructiveConfirmationOptions } from '@shared/ui/utils/confirmation-dialog.utils';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Dialog } from 'primeng/dialog';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';
import { Textarea } from 'primeng/textarea';

@Component({
  selector: 'app-leave-request-details',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    BackButtonComponent,
    Card,
    Panel,
    Button,
    Message,
    Tag,
    ProgressSpinner,
    Dialog,
    Textarea,
    RequestLeaveDialogComponent
  ],
  templateUrl: './leave-request-details.component.html'
})
export class LeaveRequestDetailsComponent {
  readonly id = input.required<string>();

  private readonly leaveRequestsService = inject(LeaveRequestsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly BillingMessages = BillingMessages;
  readonly getLeaveRequestStatusSeverity = getLeaveRequestStatusSeverity;
  readonly getLeaveDayPortionLabel = getLeaveDayPortionLabel;

  readonly request = signal<LeaveRequestDetailsDto | null>(null);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly editDialogVisible = signal(false);
  readonly rejectDialogVisible = signal(false);
  readonly rejectError = signal<string | null>(null);
  readonly isRejecting = signal(false);
  readonly fromMyLeave = signal(false);

  readonly rejectReasonControl = new FormControl('', {
    nonNullable: true,
    validators: [
      Validators.required,
      Validators.maxLength(LeaveRequestConstraints.REJECT_REASON_MAX_LENGTH)
    ]
  });

  constructor() {
    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.fromMyLeave.set(params.get('from') === 'my-leave');
      });

    effect(() => {
      this.loadRequest(this.id());
    });
  }

  backRoute(): string[] {
    return this.fromMyLeave() ? ['/my-leave'] : ['/leave-requests'];
  }

  openEdit(): void {
    this.editDialogVisible.set(true);
  }

  submitRequest(): void {
    const current = this.request();
    if (!current?.canSubmit) {
      return;
    }

    this.leaveRequestsService
      .submitLeaveRequest(current.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.request.set(details);
          this.toastService.success(
            details.status === LeaveRequestStatus.Approved
              ? BillingMessages.leaveRequestApproved
              : BillingMessages.leaveRequestSubmitted
          );
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  confirmCancel(): void {
    this.confirmationService.confirm({
      header: 'Cancel leave request',
      message: BillingMessages.cancelLeaveRequestConfirm,
      ...createDestructiveConfirmationOptions('Cancel request'),
      accept: () => this.cancelRequest()
    });
  }

  confirmDelete(): void {
    this.confirmationService.confirm({
      header: 'Delete leave request',
      message: BillingMessages.deleteDraftLeaveRequestConfirm,
      ...createDestructiveConfirmationOptions('Delete'),
      accept: () => this.deleteRequest()
    });
  }

  approveRequest(): void {
    const current = this.request();
    if (!current?.canApprove) {
      return;
    }

    this.leaveRequestsService
      .approveLeaveRequest(current.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.request.set(details);
          this.toastService.success(BillingMessages.leaveRequestApproved);
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  openRejectDialog(): void {
    this.rejectReasonControl.reset('');
    this.rejectError.set(null);
    this.rejectDialogVisible.set(true);
  }

  rejectRequest(): void {
    const current = this.request();
    if (!current?.canReject) {
      return;
    }

    this.rejectReasonControl.markAsTouched();
    if (this.rejectReasonControl.invalid) {
      this.rejectError.set('Reject reason is required.');
      return;
    }

    const reason = this.rejectReasonControl.value.trim();
    this.isRejecting.set(true);
    this.leaveRequestsService
      .rejectLeaveRequest(current.id, { rejectReason: reason })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.request.set(details);
          this.rejectDialogVisible.set(false);
          this.toastService.success(BillingMessages.leaveRequestRejected);
          this.isRejecting.set(false);
        },
        error: (error: Error) => {
          this.rejectError.set(error.message);
          this.isRejecting.set(false);
        }
      });
  }

  onSaved(): void {
    this.loadRequest(this.id());
  }

  private cancelRequest(): void {
    const current = this.request();
    if (!current?.canCancel) {
      return;
    }

    this.leaveRequestsService
      .cancelLeaveRequest(current.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (details) => {
          this.request.set(details);
          this.toastService.success(BillingMessages.leaveRequestCancelled);
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private deleteRequest(): void {
    const current = this.request();
    if (!current?.canDelete) {
      return;
    }

    this.leaveRequestsService
      .deleteLeaveRequest(current.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(BillingMessages.leaveRequestDeleted);
          void this.router.navigate(this.backRoute());
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private loadRequest(id: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.leaveRequestsService
      .getLeaveRequestById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (request) => {
          this.request.set(request);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
