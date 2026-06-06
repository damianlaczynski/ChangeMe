import { DatePipe, DecimalPipe } from '@angular/common';
import {
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '@features/auth/services/auth.service';
import { EditEmploymentProfileDialogComponent } from '@features/billing/components/edit-employment-profile-dialog/edit-employment-profile-dialog.component';
import {
  EmploymentProfileDto,
  UserEmploymentDto
} from '@features/billing/models/employment.model';
import { EmploymentService } from '@features/billing/services/employment.service';
import {
  BillingMessages,
  formatEmploymentField,
  formatMonthlyHoursNorm,
  getContractStatusSeverity,
  getContractTypeLabel
} from '@features/billing/utils/billing.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-user-employment-section',
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    Panel,
    Button,
    TableModule,
    Tag,
    Message,
    ProgressSpinner,
    EditEmploymentProfileDialogComponent
  ],
  templateUrl: './user-employment-section.component.html'
})
export class UserEmploymentSectionComponent {
  readonly userId = input.required<string>();

  private readonly employmentService = inject(EmploymentService);
  private readonly authService = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly employment = signal<UserEmploymentDto | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly editDialogVisible = signal(false);
  readonly panelCollapsed = signal(true);

  readonly canViewEmployment = computed(
    () =>
      this.authService.hasPermission(PermissionCodes.billingViewAny) ||
      this.authService.hasPermission(PermissionCodes.billingManageEmployment)
  );

  readonly canManageEmployment = computed(() =>
    this.authService.hasPermission(PermissionCodes.billingManageEmployment)
  );

  readonly canViewLeaveQuickLink = computed(
    () =>
      this.authService.hasPermission(PermissionCodes.billingViewAny) ||
      this.authService.hasPermission(PermissionCodes.billingManageLeave)
  );

  readonly canViewAvailabilityQuickLink = computed(() =>
    this.authService.hasPermission(PermissionCodes.billingViewAny)
  );

  readonly profile = computed<EmploymentProfileDto>(
    () =>
      this.employment()?.profile ?? {
        canManage: this.canManageEmployment()
      }
  );

  readonly BillingMessages = BillingMessages;
  readonly formatEmploymentField = formatEmploymentField;
  readonly formatMonthlyHoursNorm = formatMonthlyHoursNorm;
  readonly getContractTypeLabel = getContractTypeLabel;
  readonly getContractStatusSeverity = getContractStatusSeverity;

  constructor() {
    effect(() => {
      const userId = this.userId();
      if (userId && this.canViewEmployment()) {
        this.loadEmployment(userId);
      }
    });

    this.route.queryParamMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        if (params.get('expandEmployment') === '1') {
          this.panelCollapsed.set(false);
        }
      });
  }

  openEditDialog(): void {
    this.editDialogVisible.set(true);
  }

  onPanelCollapsedChange(collapsed: boolean | undefined): void {
    this.panelCollapsed.set(collapsed ?? true);
  }

  onProfileSaved(): void {
    this.loadEmployment(this.userId());
  }

  private loadEmployment(userId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.employmentService
      .getUserEmployment(userId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (employment) => {
          this.employment.set(employment);
          if (employment.isExpandedByDefault) {
            this.panelCollapsed.set(false);
          }
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }
}
