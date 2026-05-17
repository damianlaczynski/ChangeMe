import { computed, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { ApiService } from '@shared/api/services/api.service';
import {
  NotificationDto,
  NotificationListDto,
  NotificationRealtimeMessage
} from '../models/notification.model';
import { NotificationsRealtimeConnectionService } from './notifications-realtime-connection.service';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  private readonly apiService = inject(ApiService);
  private readonly authService = inject(AuthService);
  private readonly realtimeConnectionService = inject(
    NotificationsRealtimeConnectionService
  );
  private readonly toastService = inject(ToastService);

  readonly notifications = signal<NotificationDto[]>([]);
  readonly unreadCount = signal(0);
  readonly isLoading = signal(false);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly unreadNotifications = computed(() =>
    this.notifications().filter((notification) => !notification.isRead)
  );
  readonly readNotifications = computed(() =>
    this.notifications().filter((notification) => notification.isRead)
  );

  constructor() {
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      if (!isAuthenticated) {
        this.notifications.set([]);
        this.unreadCount.set(0);
        this.hasLoaded.set(false);
        this.errorMessage.set(null);
        return;
      }

      this.loadNotifications();
    });

    effect(() => {
      const message = this.realtimeConnectionService.lastNotificationMessage();
      if (!message || !this.authService.isAuthenticated()) {
        return;
      }

      untracked(() => this.syncFromRealtime(message));
    });

    effect(() => {
      const reconnectCount = this.realtimeConnectionService.reconnectCount();
      if (reconnectCount === 0 || !this.authService.isAuthenticated()) {
        return;
      }

      this.loadNotifications();
    });
  }

  loadNotifications(): void {
    if (!this.authService.isAuthenticated()) {
      this.notifications.set([]);
      this.unreadCount.set(0);
      this.hasLoaded.set(false);
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.apiService.get<NotificationListDto>('notifications').subscribe({
      next: (result) => {
        this.notifications.set(result.items);
        this.unreadCount.set(result.unreadCount);
        this.hasLoaded.set(true);
        this.isLoading.set(false);
      },
      error: (error: Error) => {
        this.errorMessage.set(error.message);
        this.isLoading.set(false);
      }
    });
  }

  markAsRead(notificationId: string): void {
    this.apiService
      .put<NotificationDto>(`notifications/${notificationId}/read`, {})
      .subscribe({
        next: (notification) => {
          const wasUnread = this.notifications().some(
            (item) => item.id === notification.id && !item.isRead
          );
          this.notifications.update((items) =>
            items.map((item) => (item.id === notification.id ? notification : item))
          );
          if (wasUnread) {
            this.unreadCount.update((count) => Math.max(0, count - 1));
          }
        }
      });
  }

  markAllAsRead(): void {
    this.apiService.put<NotificationListDto>('notifications/read-all', {}).subscribe({
      next: (result) => {
        this.notifications.set(result.items);
        this.unreadCount.set(result.unreadCount);
      }
    });
  }

  syncFromRealtime(message: NotificationRealtimeMessage): void {
    const notification: NotificationDto = {
      id: message.notificationId,
      issueId: message.issueId,
      eventType: message.eventType,
      issueTitle: message.issueTitle,
      message: message.message,
      link: message.link,
      occurredAt: message.occurredAt,
      isRead: false,
      readAt: null
    };

    this.notifications.update((items) => {
      if (items.some((item) => item.id === notification.id)) {
        return items;
      }

      return [notification, ...items];
    });

    this.unreadCount.update((count) => count + 1);
    this.toastService.showIssueNotification(
      notification.issueTitle,
      notification.message
    );
  }
}
