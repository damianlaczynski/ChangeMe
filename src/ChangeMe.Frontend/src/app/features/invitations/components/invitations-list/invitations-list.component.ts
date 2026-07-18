import { DatePipe } from '@angular/common';
import {
  Component,
  computed,
  DestroyRef,
  inject,
  signal,
  viewChild
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { InvitationListItemDto } from '@features/invitations/models/invitation.model';
import { InvitationsService } from '@features/invitations/services/invitations.service';
import {
  getInvitationStatusLabel,
  getInvitationStatusSeverity,
  getRevokeConfirmMessage,
  InvitationMessages,
  invitationStatusFilters
} from '@features/invitations/utils/invitations.utils';
import {
  GridResourceFactory,
  PrimeDataGridComponent,
  QgColumnDirective,
  QgEmptyDirective,
  type GridResource
} from '@query-grid/primeng';
import { getGridListEmptyMessage } from '@shared/data/utils/grid.utils';
import { ConfirmationService, MenuItem } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Menu } from 'primeng/menu';
import { Message } from 'primeng/message';
import { Tag } from 'primeng/tag';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-invitations-list',
  imports: [
    DatePipe,
    RouterLink,
    Card,
    Button,
    Message,
    Tag,
    Menu,
    Tooltip,
    PrimeDataGridComponent,
    QgColumnDirective,
    QgEmptyDirective
  ],
  templateUrl: './invitations-list.component.html'
})
export class InvitationsListComponent {
  private readonly invitationsService = inject(InvitationsService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly gridFactory = inject(GridResourceFactory);
  private readonly destroyRef = inject(DestroyRef);

  readonly getInvitationStatusLabel = getInvitationStatusLabel;
  readonly getInvitationStatusSeverity = getInvitationStatusSeverity;
  readonly invitationStatusFilters = invitationStatusFilters;

  readonly invitationActionItems = signal<MenuItem[]>([]);
  private readonly invitationActionsMenu = viewChild.required<Menu>(
    'invitationActionsMenu'
  );

  readonly grid: GridResource<InvitationListItemDto>;

  readonly errorMessage = computed(() => {
    const error = this.grid.error();
    return error instanceof Error ? error.message : error ? String(error) : null;
  });

  readonly emptyMessage = computed(() => getGridListEmptyMessage(this.grid.query()));

  constructor() {
    this.grid = this.gridFactory.create<InvitationListItemDto>({
      destroyRef: this.destroyRef,
      load: (query) => this.invitationsService.getInvitations(query),
      defaultSort: [{ field: 'CreatedAt', desc: true }],
      defaultTake: 10,
      persistState: { key: 'changeme.invitations-list', storage: 'session' }
    });
  }

  refresh(): void {
    this.grid.reload();
  }

  openInvitationActionsMenu(event: Event, invitation: InvitationListItemDto): void {
    const items: MenuItem[] = [];

    if (invitation.status === 'PENDING') {
      items.push({
        label: 'Resend',
        icon: 'pi pi-send',
        command: () => this.resendInvitation(invitation)
      });
      items.push({
        label: 'Revoke',
        icon: 'pi pi-ban',
        command: () => this.confirmRevoke(invitation)
      });
    }

    this.invitationActionItems.set(items);
    this.invitationActionsMenu().toggle(event);
  }

  private resendInvitation(invitation: InvitationListItemDto): void {
    this.invitationsService
      .resendInvitation(invitation.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(InvitationMessages.resent);
          this.refresh();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }

  private confirmRevoke(invitation: InvitationListItemDto): void {
    this.confirmationService.confirm({
      header: 'Revoke invitation',
      message: getRevokeConfirmMessage(invitation.email),
      acceptLabel: 'Revoke',
      rejectLabel: 'Cancel',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => this.revokeInvitation(invitation)
    });
  }

  private revokeInvitation(invitation: InvitationListItemDto): void {
    this.invitationsService
      .revokeInvitation(invitation.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(InvitationMessages.revoked);
          this.refresh();
        },
        error: (error: Error) => this.toastService.error(error.message)
      });
  }
}
