import {
  DestroyRef,
  Injectable,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthResponse } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { RequiredPasswordChangeDialogService } from '@features/auth/services/required-password-change-dialog.service';
import {
  AuthMessages,
  passwordExpiryWarningDetail
} from '@features/auth/utils/auth.utils';
import {
  clearWarningMarkers,
  getActiveWarningThreshold,
  getCalendarDaysUntilExpiry,
  hasShownWarning,
  markWarningShown,
  shouldUseSoftExpiryUx
} from '@features/auth/utils/password-expiration.utils';
import { filter } from 'rxjs/operators';

export type PasswordExpirationBannerMode = 'none' | 'warning' | 'expired';

@Injectable({
  providedIn: 'root'
})
export class PasswordExpirationNoticeService {
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly dialogService = inject(RequiredPasswordChangeDialogService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private expiredToastShown = false;

  readonly bannerMode = signal<PasswordExpirationBannerMode>('none');
  readonly bannerDaysRemaining = signal<number | null>(null);

  readonly showChangePasswordAction = computed(
    () => this.bannerMode() !== 'none' && shouldUseSoftExpiryUx(this.router.url)
  );

  constructor() {
    effect(() => {
      this.syncWithSession(this.authService.currentSession());
    });

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.syncWithSession(this.authService.currentSession()));
  }

  syncWithSession(session: AuthResponse | null): void {
    if (!session) {
      this.reset();
      return;
    }

    if (session.passwordChangeRequired) {
      this.showExpiredState();
      return;
    }

    this.expiredToastShown = false;
    this.updateWarningBanner(session);
    this.maybeShowWarningToast(session);
  }

  openChangePasswordDialog(): void {
    this.dialogService.open();
  }

  clearNotices(): void {
    clearWarningMarkers(this.authService.currentSession()?.userId);
    this.expiredToastShown = false;
    this.bannerMode.set('none');
    this.bannerDaysRemaining.set(null);
    this.toastService.clear();
  }

  private showExpiredState(): void {
    if (!shouldUseSoftExpiryUx(this.router.url)) {
      this.bannerMode.set('none');
      return;
    }

    this.bannerMode.set('expired');
    this.bannerDaysRemaining.set(null);

    if (this.expiredToastShown) {
      return;
    }

    this.expiredToastShown = true;
    this.toastService.show({
      severity: 'warn',
      summary: AuthMessages.passwordExpiredSummary,
      detail: AuthMessages.passwordExpiredDetail,
      sticky: true
    });
  }

  private updateWarningBanner(session: AuthResponse): void {
    if (!session.passwordExpiresAtUtc) {
      this.bannerMode.set('none');
      this.bannerDaysRemaining.set(null);
      return;
    }

    const daysUntil = getCalendarDaysUntilExpiry(session.passwordExpiresAtUtc);
    const threshold = getActiveWarningThreshold(daysUntil);

    if (threshold === null || !shouldUseSoftExpiryUx(this.router.url)) {
      this.bannerMode.set('none');
      this.bannerDaysRemaining.set(null);
      return;
    }

    this.bannerMode.set('warning');
    this.bannerDaysRemaining.set(daysUntil);
  }

  private maybeShowWarningToast(session: AuthResponse): void {
    if (!session.passwordExpiresAtUtc) {
      return;
    }

    const daysUntil = getCalendarDaysUntilExpiry(session.passwordExpiresAtUtc);
    const threshold = getActiveWarningThreshold(daysUntil);

    if (threshold === null || hasShownWarning(session.userId, threshold)) {
      return;
    }

    markWarningShown(session.userId, threshold);
    this.toastService.warn(
      AuthMessages.passwordExpiringSoon,
      passwordExpiryWarningDetail(daysUntil),
      10_000
    );
  }

  private reset(): void {
    this.expiredToastShown = false;
    this.bannerMode.set('none');
    this.bannerDaysRemaining.set(null);
    this.toastService.clear();
  }
}
