import { DestroyRef, Injectable, computed, effect, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthResponse } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { PasskeySetupDialogService } from '@features/auth/services/passkey-setup-dialog.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { shouldUseSoftPasskeyUx } from '@features/auth/utils/passkey.utils';
import { shouldUseSoftTwoFactorUx } from '@features/auth/utils/two-factor.utils';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class PasskeySetupNoticeService {
  private readonly authService = inject(AuthService);
  private readonly toastService = inject(ToastService);
  private readonly dialogService = inject(PasskeySetupDialogService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private setupToastShown = false;

  readonly showSetupBanner = computed(
    () =>
      this.authService.passkeySetupRequired() &&
      !this.authService.requiresPasskeySetupScreen() &&
      shouldUseSoftPasskeyUx(this.router.url)
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

  openSetupDialog(): void {
    this.dialogService.open();
  }

  clearNotices(): void {
    this.setupToastShown = false;
    this.toastService.clear();
  }

  private syncWithSession(session: AuthResponse | null): void {
    if (!session) {
      this.reset();
      return;
    }

    if (session.passwordChangeRequired || session.twoFactorSetupRequired) {
      this.reset();
      return;
    }

    if (
      session.passkeySetupRequired &&
      !session.passkeySetupStrict &&
      shouldUseSoftPasskeyUx(this.router.url) &&
      shouldUseSoftTwoFactorUx(this.router.url)
    ) {
      this.maybeShowSetupToast();
      return;
    }

    this.setupToastShown = false;
  }

  private maybeShowSetupToast(): void {
    if (this.setupToastShown) {
      return;
    }

    this.setupToastShown = true;
    this.toastService.show({
      severity: 'warn',
      summary: AuthMessages.passkeySetupRequiredSummary,
      detail: `${AuthMessages.passkeySetupRequiredDetail} ${AuthMessages.passkeySetupRequiredAction}.`,
      sticky: true
    });
  }

  private reset(): void {
    this.setupToastShown = false;
  }
}
