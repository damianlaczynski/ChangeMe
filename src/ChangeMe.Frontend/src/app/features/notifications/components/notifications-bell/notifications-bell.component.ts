import { Component, inject, viewChild } from '@angular/core';
import { NotificationsPanelComponent } from '@features/notifications/components/notifications-panel/notifications-panel.component';
import { NotificationsService } from '@features/notifications/services/notifications.service';
import { Button } from 'primeng/button';
import { Popover } from 'primeng/popover';

@Component({
  selector: 'app-notifications-bell',
  imports: [Button, Popover, NotificationsPanelComponent],
  template: `
    <p-button
      icon="pi pi-bell"
      [rounded]="true"
      [text]="true"
      severity="secondary"
      ariaLabel="Notifications"
      [badge]="unreadCount() > 0 ? unreadCount().toString() : undefined"
      badgeSeverity="danger"
      (onClick)="togglePanel($event)"
    />
    <p-popover #popover [dismissable]="true">
      <ng-template #content>
        <app-notifications-panel (closed)="hidePanel()" />
      </ng-template>
    </p-popover>
  `
})
export class NotificationsBellComponent {
  private readonly notificationsService = inject(NotificationsService);

  private readonly popover = viewChild.required<Popover>('popover');

  readonly unreadCount = this.notificationsService.unreadCount;

  togglePanel(event: Event): void {
    const popover = this.popover();

    if (popover.overlayVisible) {
      popover.hide();
      return;
    }

    popover.show(event);

    if (popover.container) {
      popover.align();
    }

    if (!this.notificationsService.hasLoaded()) {
      this.notificationsService.loadNotifications();
    }
  }

  hidePanel(): void {
    this.popover().hide();
  }
}
