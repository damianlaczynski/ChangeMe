import { DatePipe } from '@angular/common';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { UserSessionDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import {
  AuthMessages,
  PermissionCodes,
  formatIpAddress,
  formatSessionType
} from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { PaginationResult } from '@shared/data/models/pagination-result.model';
import { ConfirmationService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Paginator, PaginatorState } from 'primeng/paginator';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-sessions',
  imports: [
    DatePipe,
    BackButtonComponent,
    Card,
    Button,
    Message,
    TableModule,
    Tag,
    Paginator
  ],
  templateUrl: './my-sessions.component.html'
})
export class MySessionsComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly sessions = signal<UserSessionDto[]>([]);
  readonly pagination = signal<PaginationResult<UserSessionDto> | null>(null);
  readonly query = signal({
    pageNumber: 1,
    pageSize: 10,
    sortField: 'LastActivityAt',
    ascending: false
  });
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly hasLoaded = signal(false);
  readonly pendingRevokeIds = signal<string[]>([]);
  readonly isSigningOutEverywhere = signal(false);
  readonly permissionCodes = PermissionCodes;
  readonly authMessages = AuthMessages;
  readonly formatSessionType = formatSessionType;
  readonly formatIpAddress = formatIpAddress;

  readonly canManageSessions = this.authService.hasPermission(
    PermissionCodes.sessionsManageOwn
  );

  constructor() {
    this.loadSessions();
  }

  loadSessions(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .getMySessions(this.query())
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result) => {
          this.sessions.set(result.items);
          this.pagination.set(result);
          this.isLoading.set(false);
          this.hasLoaded.set(true);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  onPageChange(event: PaginatorState): void {
    this.query.update((current) => ({
      ...current,
      pageNumber: (event.page ?? 0) + 1,
      pageSize: event.rows ?? current.pageSize
    }));
    this.loadSessions();
  }

  confirmRevokeSession(session: UserSessionDto): void {
    this.confirmationService.confirm({
      header: AuthMessages.revokeSessionTitle,
      message: AuthMessages.revokeSessionMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Revoke', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.revokeSession(session)
    });
  }

  confirmSignOutEverywhere(): void {
    this.confirmationService.confirm({
      header: AuthMessages.signOutEverywhereTitle,
      message: AuthMessages.signOutEverywhereMessage,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Sign out everywhere', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.signOutEverywhere()
    });
  }

  private revokeSession(session: UserSessionDto): void {
    if (this.pendingRevokeIds().includes(session.id)) {
      return;
    }

    this.pendingRevokeIds.update((ids) => [...ids, session.id]);

    this.authService
      .revokeSession(session.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.loadSessions();
          this.pendingRevokeIds.update((ids) => ids.filter((id) => id !== session.id));
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.pendingRevokeIds.update((ids) => ids.filter((id) => id !== session.id));
        }
      });
  }

  private signOutEverywhere(): void {
    if (this.isSigningOutEverywhere()) {
      return;
    }

    this.isSigningOutEverywhere.set(true);

    this.authService
      .logoutAll()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/login');
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isSigningOutEverywhere.set(false);
        }
      });
  }

  isRevokePending(sessionId: string): boolean {
    return this.pendingRevokeIds().includes(sessionId);
  }
}
