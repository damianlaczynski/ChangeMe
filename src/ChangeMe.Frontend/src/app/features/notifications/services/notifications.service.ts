import { computed, effect, inject, Injectable, signal, untracked } from '@angular/core';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthService } from '@features/auth/services/auth.service';
import { ApiService } from '@shared/api/services/api.service';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import {
  NotificationDto,
  NotificationListDto,
  NotificationRealtimeMessage,
  NotificationSearchParameters
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

  readonly unreadNotifications = signal<NotificationDto[]>([]);
  readonly readNotifications = signal<NotificationDto[]>([]);
  readonly unreadPagination = signal<PaginationResult<NotificationDto> | null>(null);
  readonly readPagination = signal<PaginationResult<NotificationDto> | null>(null);
  readonly unreadQuery = signal<NotificationSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false,
    isRead: false
  });
  readonly readQuery = signal<NotificationSearchParameters>({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'CreatedAt',
    ascending: false,
    isRead: true
  });
  readonly unreadCount = signal(0);
  readonly isLoading = signal(false);
  readonly isLoadingMoreUnread = signal(false);
  readonly isLoadingMoreRead = signal(false);
  readonly hasLoaded = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly canShowMoreUnread = computed(() => this.unreadPagination()?.hasNext ?? false);
  readonly canShowMoreRead = computed(() => this.readPagination()?.hasNext ?? false);

  readonly notifications = computed(() => [
    ...this.unreadNotifications(),
    ...this.readNotifications()
  ]);

  private unreadRequestId = 0;
  private readRequestId = 0;
  private hasInitializedForAuth = false;
  private lastHandledReconnectCount = 0;
  private lastProcessedNotificationVersion = 0;

  constructor() {
    effect(() => {
      const isAuthenticated = this.authService.isAuthenticated();
      if (!isAuthenticated) {
        this.resetAuthState();
        return;
      }

      if (this.hasInitializedForAuth) {
        return;
      }

      this.hasInitializedForAuth = true;
      untracked(() => this.reloadUnreadFromStart());
    });

    effect(() => {
      const version = this.realtimeConnectionService.notificationMessageVersion();
      if (
        version === 0 ||
        version === this.lastProcessedNotificationVersion ||
        !this.authService.isAuthenticated()
      ) {
        return;
      }

      const message = this.realtimeConnectionService.lastNotificationMessage();
      if (!message) {
        return;
      }

      this.lastProcessedNotificationVersion = version;
      untracked(() => this.syncFromRealtime(message));
    });

    effect(() => {
      const reconnectCount = this.realtimeConnectionService.reconnectCount();
      if (
        reconnectCount === 0 ||
        reconnectCount <= this.lastHandledReconnectCount ||
        !this.authService.isAuthenticated()
      ) {
        return;
      }

      this.lastHandledReconnectCount = reconnectCount;
      untracked(() => this.reloadUnreadFromStart());
    });
  }

  private resetAuthState(): void {
    this.hasInitializedForAuth = false;
    this.lastHandledReconnectCount = 0;
    this.lastProcessedNotificationVersion = 0;
    this.unreadNotifications.set([]);
    this.readNotifications.set([]);
    this.unreadCount.set(0);
    this.hasLoaded.set(false);
    this.errorMessage.set(null);
  }

  loadNotifications(): void {
    this.reloadUnreadFromStart();
  }

  reloadUnreadFromStart(): void {
    this.unreadQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false,
      isRead: false
    });
    this.loadUnreadNotifications();
  }

  reloadReadFromStart(): void {
    this.readQuery.set({
      pageNumber: 1,
      pageSize: 10,
      sortField: 'CreatedAt',
      ascending: false,
      isRead: true
    });
    this.loadReadNotifications();
  }

  showMoreUnread(): void {
    const pagination = this.unreadPagination();
    if (!pagination?.hasNext || this.isLoadingMoreUnread()) {
      return;
    }

    this.loadUnreadNotifications({
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  showMoreRead(): void {
    const pagination = this.readPagination();
    if (!pagination?.hasNext || this.isLoadingMoreRead()) {
      return;
    }

    this.loadReadNotifications({
      append: true,
      pageNumber: pagination.currentPage + 1
    });
  }

  loadUnreadNotifications(
    options: { append?: boolean; pageNumber?: number } = {}
  ): void {
    const append = options.append ?? false;
    const pageNumber = options.pageNumber ?? this.unreadQuery().pageNumber;
    const query = { ...this.unreadQuery(), pageNumber };
    const requestId = ++this.unreadRequestId;

    this.loadNotificationsPage(query, requestId, append, (result) => {
      if (requestId !== this.unreadRequestId) {
        return;
      }

      if (append) {
        const current = this.unreadNotifications();
        const merged = this.appendUniqueNotifications(current, result.page.items);
        const addedCount = merged.length - current.length;
        this.unreadNotifications.set(merged);
        this.unreadPagination.set(
          addedCount === 0 && result.page.hasNext
            ? { ...result.page, hasNext: false }
            : result.page
        );
      } else {
        this.unreadNotifications.set(result.page.items);
        this.unreadPagination.set(result.page);
      }

      this.unreadQuery.set({
        pageNumber: result.page.currentPage,
        pageSize: result.page.pageSize,
        sortField: 'CreatedAt',
        ascending: false,
        isRead: false
      });
      this.unreadCount.set(result.unreadCount);
      this.isLoadingMoreUnread.set(false);
    }, true);
  }

  loadReadNotifications(options: { append?: boolean; pageNumber?: number } = {}): void {
    const append = options.append ?? false;
    const pageNumber = options.pageNumber ?? this.readQuery().pageNumber;
    const query = { ...this.readQuery(), pageNumber };
    const requestId = ++this.readRequestId;

    this.loadNotificationsPage(query, requestId, append, (result) => {
      if (requestId !== this.readRequestId) {
        return;
      }

      if (append) {
        const current = this.readNotifications();
        const merged = this.appendUniqueNotifications(current, result.page.items);
        const addedCount = merged.length - current.length;
        this.readNotifications.set(merged);
        this.readPagination.set(
          addedCount === 0 && result.page.hasNext
            ? { ...result.page, hasNext: false }
            : result.page
        );
      } else {
        this.readNotifications.set(result.page.items);
        this.readPagination.set(result.page);
      }

      this.readQuery.set({
        pageNumber: result.page.currentPage,
        pageSize: result.page.pageSize,
        sortField: 'CreatedAt',
        ascending: false,
        isRead: true
      });
      this.unreadCount.set(result.unreadCount);
      this.isLoadingMoreRead.set(false);
    }, false);
  }

  markAsRead(notificationId: string): void {
    this.apiService
      .put<NotificationDto>(`notifications/${notificationId}/read`, {})
      .subscribe({
        next: (notification) => {
          const wasUnread = this.unreadNotifications().some(
            (item) => item.id === notification.id && !item.isRead
          );
          this.unreadNotifications.update((items) =>
            items.filter((item) => item.id !== notification.id)
          );
          this.readNotifications.update((items) => {
            if (items.some((item) => item.id === notification.id)) {
              return items.map((item) => (item.id === notification.id ? notification : item));
            }

            return [notification, ...items];
          });
          if (wasUnread) {
            this.unreadCount.update((count) => Math.max(0, count - 1));
          }
        }
      });
  }

  markAllAsRead(): void {
    this.apiService.put<boolean>('notifications/read-all', {}).subscribe({
      next: () => {
        this.unreadNotifications.set([]);
        this.unreadPagination.set(null);
        this.unreadCount.set(0);
        this.unreadQuery.set({
          pageNumber: 1,
          pageSize: 10,
          sortField: 'CreatedAt',
          ascending: false,
          isRead: false
        });
        this.reloadReadFromStart();
      }
    });
  }

  private appendUniqueNotifications(
    existing: NotificationDto[],
    incoming: NotificationDto[]
  ): NotificationDto[] {
    const existingIds = new Set(existing.map((item) => item.id));
    const uniqueIncoming = incoming.filter((item) => !existingIds.has(item.id));
    return [...existing, ...uniqueIncoming];
  }

  syncFromRealtime(message: NotificationRealtimeMessage): void {
    const notification: NotificationDto = {
      id: message.notificationId,
      issueId: message.issueId,
      eventType: message.eventType,
      issueTitle: message.issueTitle,
      message: message.message,
      link: message.link,
      createdAt: message.createdAt,
      isRead: false,
      readAt: null
    };

    this.unreadNotifications.update((items) => {
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

  private loadNotificationsPage(
    query: NotificationSearchParameters,
    requestId: number,
    append: boolean,
    onSuccess: (result: NotificationListDto) => void,
    isUnreadList: boolean
  ): void {
    if (!this.authService.isAuthenticated()) {
      this.unreadNotifications.set([]);
      this.readNotifications.set([]);
      this.unreadCount.set(0);
      this.hasLoaded.set(false);
      return;
    }

    if (append) {
      if (isUnreadList) {
        this.isLoadingMoreUnread.set(true);
      } else {
        this.isLoadingMoreRead.set(true);
      }
    } else {
      this.isLoading.set(true);
    }

    this.errorMessage.set(null);

    this.apiService.get<NotificationListDto>('notifications', query).subscribe({
      next: (result) => {
        const isStale =
          (isUnreadList && requestId !== this.unreadRequestId) ||
          (!isUnreadList && requestId !== this.readRequestId);
        if (isStale) {
          this.clearLoadingFlags(append, isUnreadList);
          return;
        }

        onSuccess(result);
        this.hasLoaded.set(true);
        this.isLoading.set(false);
      },
      error: (error: Error) => {
        const isStale =
          (isUnreadList && requestId !== this.unreadRequestId) ||
          (!isUnreadList && requestId !== this.readRequestId);
        if (isStale) {
          this.clearLoadingFlags(append, isUnreadList);
          return;
        }

        this.errorMessage.set(error.message);
        this.isLoading.set(false);
        this.isLoadingMoreUnread.set(false);
        this.isLoadingMoreRead.set(false);
      }
    });
  }

  private clearLoadingFlags(append: boolean, isUnreadList: boolean): void {
    if (!append) {
      return;
    }

    if (isUnreadList) {
      this.isLoadingMoreUnread.set(false);
    } else {
      this.isLoadingMoreRead.set(false);
    }
  }
}
