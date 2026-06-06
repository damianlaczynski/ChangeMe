import { DestroyRef, Injectable, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '@features/auth/services/auth.service';
import { RunningTimerDto } from '@features/time/models/time.model';
import { TimeService } from '@features/time/services/time.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { ConfirmationService } from 'primeng/api';
import { finalize, interval } from 'rxjs';
import { TimeMessages } from '../utils/time.utils';

@Injectable({
  providedIn: 'root'
})
export class RunningTimerService {
  private readonly timeService = inject(TimeService);
  private readonly authService = inject(AuthService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  readonly timer = signal<RunningTimerDto | null>(null);
  readonly isStarting = signal(false);
  readonly isDiscarding = signal(false);
  readonly started = signal(0);
  readonly discarded = signal(0);

  private lastSyncedAuthenticatedChrome: boolean | null = null;
  private refreshInProgress = false;
  private refreshRequestId = 0;

  readonly hasRunningTimer = computed(() => this.timer() !== null);
  readonly canUseTimer = computed(() =>
    this.authService.hasPermission(PermissionCodes.timeLogOwn)
  );

  constructor() {
    interval(60_000)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        if (this.hasRunningTimer() && this.canUseTimer()) {
          this.refreshTimer();
        }
      });
  }

  syncWithSession(showAuthenticatedChrome: boolean): void {
    if (showAuthenticatedChrome === this.lastSyncedAuthenticatedChrome) {
      return;
    }

    this.lastSyncedAuthenticatedChrome = showAuthenticatedChrome;

    if (!showAuthenticatedChrome || !this.canUseTimer()) {
      this.timer.set(null);
      return;
    }

    this.refreshTimer();
  }

  refreshTimer(): void {
    if (!this.canUseTimer() || this.refreshInProgress) {
      return;
    }

    this.refreshInProgress = true;
    const requestId = ++this.refreshRequestId;

    this.timeService
      .getRunningTimer()
      .pipe(
        finalize(() => {
          if (requestId === this.refreshRequestId) {
            this.refreshInProgress = false;
          }
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (state) => {
          if (requestId !== this.refreshRequestId) {
            return;
          }

          this.timer.set(state.timer ?? null);
        },
        error: () => {
          if (requestId !== this.refreshRequestId) {
            return;
          }

          this.timer.set(null);
        }
      });
  }

  startTimer(options: {
    projectId?: string | null;
    issueId?: string | null;
    replaceExisting?: boolean;
  }): void {
    if (!this.canUseTimer() || this.isStarting()) {
      return;
    }

    if (this.hasRunningTimer() && !options.replaceExisting) {
      this.confirmReplaceAndStart(options);
      return;
    }

    this.runStart(options.replaceExisting ?? false, options);
  }

  discardTimer(onComplete?: () => void): void {
    if (!this.hasRunningTimer() || this.isDiscarding()) {
      onComplete?.();
      return;
    }

    this.isDiscarding.set(true);

    this.timeService
      .discardRunningTimer()
      .pipe(
        finalize(() => this.isDiscarding.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: () => {
          this.timer.set(null);
          this.discarded.update((value) => value + 1);
          onComplete?.();
        },
        error: () => onComplete?.()
      });
  }

  confirmDiscardTimer(onAccept: () => void): void {
    this.confirmationService.confirm({
      header: 'Discard timer',
      message: TimeMessages.timerDiscardConfirm,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Discard', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.discardTimer(onAccept)
    });
  }

  private confirmReplaceAndStart(options: {
    projectId?: string | null;
    issueId?: string | null;
  }): void {
    this.confirmationService.confirm({
      header: 'Replace running timer',
      message: TimeMessages.timerReplaceConfirm,
      icon: 'pi pi-exclamation-triangle',
      acceptButtonProps: { label: 'Replace', severity: 'danger' },
      rejectButtonProps: { label: 'Cancel', severity: 'secondary', outlined: true },
      accept: () => this.runStart(true, options)
    });
  }

  private runStart(
    replaceExisting: boolean,
    options: { projectId?: string | null; issueId?: string | null }
  ): void {
    this.isStarting.set(true);

    this.timeService
      .startRunningTimer({
        projectId: options.projectId ?? null,
        issueId: options.issueId ?? null,
        replaceExisting
      })
      .pipe(
        finalize(() => this.isStarting.set(false)),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (timer) => {
          this.timer.set(timer);
          this.started.update((value) => value + 1);
        },
        error: (error: Error) => this.handleStartError(error, options, replaceExisting)
      });
  }

  private handleStartError(
    error: Error,
    options: { projectId?: string | null; issueId?: string | null },
    replaceExisting: boolean
  ): void {
    if (!replaceExisting && error.message.toLowerCase().includes('running timer')) {
      this.confirmReplaceAndStart(options);
    }
  }
}
