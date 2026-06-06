import { BreakpointObserver } from '@angular/cdk/layout';
import {
  Component,
  computed,
  DestroyRef,
  effect,
  inject,
  untracked
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { SidebarNavComponent } from '@core/layout/components/sidebar-nav/sidebar-nav.component';
import { LayoutNavItem } from '@core/layout/models/layout-nav-item.model';
import { LayoutService } from '@core/layout/services/layout.service';
import { formatUserReference } from '@core/user/utils/user-display.utils';
import { PasskeySetupBannerComponent } from '@features/auth/components/passkey-setup-banner/passkey-setup-banner.component';
import { PasskeySetupDialogComponent } from '@features/auth/components/passkey-setup-dialog/passkey-setup-dialog.component';
import { PasswordExpirationBannerComponent } from '@features/auth/components/password-expiration-banner/password-expiration-banner.component';
import { RequiredPasswordChangeDialogComponent } from '@features/auth/components/required-password-change-dialog/required-password-change-dialog.component';
import { TwoFactorSetupBannerComponent } from '@features/auth/components/two-factor-setup-banner/two-factor-setup-banner.component';
import { TwoFactorSetupDialogComponent } from '@features/auth/components/two-factor-setup-dialog/two-factor-setup-dialog.component';
import { AuthService } from '@features/auth/services/auth.service';
import { PasskeySetupNoticeService } from '@features/auth/services/passkey-setup-notice.service';
import { PasswordExpirationNoticeService } from '@features/auth/services/password-expiration-notice.service';
import { TwoFactorSetupNoticeService } from '@features/auth/services/two-factor-setup-notice.service';
import { NotificationsBellComponent } from '@features/notifications/components/notifications-bell/notifications-bell.component';
import { LogTimeDialogComponent } from '@features/time/components/log-time-dialog/log-time-dialog.component';
import { RunningTimerControlComponent } from '@features/time/components/running-timer-control/running-timer-control.component';
import { LogTimeDialogService } from '@features/time/services/log-time-dialog.service';
import { RunningTimerService } from '@features/time/services/running-timer.service';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { Button } from 'primeng/button';
import { Drawer } from 'primeng/drawer';
import { filter, map } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    SidebarNavComponent,
    NotificationsBellComponent,
    LogTimeDialogComponent,
    RunningTimerControlComponent,
    PasswordExpirationBannerComponent,
    TwoFactorSetupBannerComponent,
    PasskeySetupBannerComponent,
    RequiredPasswordChangeDialogComponent,
    TwoFactorSetupDialogComponent,
    PasskeySetupDialogComponent,
    Button,
    Drawer
  ],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly logTimeDialogService = inject(LogTimeDialogService);
  private readonly runningTimerService = inject(RunningTimerService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly layoutService = inject(LayoutService);

  readonly currentUser = this.authService.currentUser;
  readonly formatUserReference = formatUserReference;
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly requiresPasswordChangeScreen = this.authService.requiresPasswordChangeScreen;
  readonly requiresTwoFactorSetupScreen = this.authService.requiresTwoFactorSetupScreen;
  readonly requiresPasskeySetupScreen = this.authService.requiresPasskeySetupScreen;
  readonly showAuthenticatedChrome = computed(
    () =>
      this.isAuthenticated() &&
      !this.requiresPasswordChangeScreen() &&
      !this.requiresTwoFactorSetupScreen() &&
      !this.requiresPasskeySetupScreen()
  );
  readonly isDesktop = toSignal(
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .pipe(map((state) => state.matches)),
    { initialValue: false }
  );
  readonly isSmallScreen = toSignal(
    this.breakpointObserver
      .observe('(min-width: 640px)')
      .pipe(map((state) => state.matches)),
    { initialValue: false }
  );
  readonly canLogTime = computed(() =>
    this.authService.hasPermission(PermissionCodes.timeLogOwn)
  );

  readonly authenticatedNavItems = computed<LayoutNavItem[]>(() => {
    const items: LayoutNavItem[] = [
      { label: 'Issues', icon: 'pi pi-list', routerLink: '/issues', exact: true },
      { label: 'Create issue', icon: 'pi pi-plus', routerLink: '/issues/create' },
      { label: 'Projects', icon: 'pi pi-folder', routerLink: '/projects', exact: true }
    ];

    if (this.authService.hasPermission(PermissionCodes.timeViewOwn)) {
      items.push({
        label: 'My time',
        icon: 'pi pi-clock',
        routerLink: '/my-time',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.billingViewOwn)) {
      items.push(
        {
          label: 'My leave',
          icon: 'pi pi-sun',
          routerLink: '/my-leave',
          exact: true
        },
        {
          label: 'My availability',
          icon: 'pi pi-calendar-plus',
          routerLink: '/my-availability',
          exact: true
        },
        {
          label: 'My billing',
          icon: 'pi pi-wallet',
          routerLink: '/my-billing',
          exact: true
        }
      );
    }

    if (this.authService.hasPermission(PermissionCodes.usersView)) {
      items.push({
        label: 'Users',
        icon: 'pi pi-users',
        routerLink: '/users',
        exact: true
      });
    }

    if (
      this.authService.hasPermission(PermissionCodes.billingViewAny) ||
      this.authService.hasPermission(PermissionCodes.billingManageLeave) ||
      this.authService.hasPermission(PermissionCodes.billingApproveLeave)
    ) {
      items.push({
        label: 'Leave requests',
        icon: 'pi pi-calendar',
        routerLink: '/leave-requests',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.rolesView)) {
      items.push({
        label: 'Roles',
        icon: 'pi pi-shield',
        routerLink: '/roles',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.billingViewAny)) {
      items.push({
        label: 'Availability calendar',
        icon: 'pi pi-users',
        routerLink: '/availability-calendar',
        exact: true
      });
    }

    if (
      this.authService.hasPermission(PermissionCodes.billingManageEmployment) ||
      this.authService.hasPermission(PermissionCodes.billingViewAny)
    ) {
      items.push({
        label: 'Positions',
        icon: 'pi pi-briefcase',
        routerLink: '/billing/positions',
        exact: true
      });
    }

    if (
      this.authService.hasPermission(PermissionCodes.billingManageSettlements) ||
      this.authService.hasPermission(PermissionCodes.billingViewReports)
    ) {
      items.push({
        label: 'Settlements',
        icon: 'pi pi-calculator',
        routerLink: '/settlements',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.timeViewReports)) {
      items.push({
        label: 'Time reports',
        icon: 'pi pi-chart-bar',
        routerLink: '/time-reports',
        exact: true
      });
    }

    if (this.authService.hasPermission(PermissionCodes.billingViewReports)) {
      items.push({
        label: 'Billing reports',
        icon: 'pi pi-chart-line',
        routerLink: '/billing-reports',
        exact: true
      });
    }

    items.push({ label: 'My account', icon: 'pi pi-user', routerLink: '/account' });
    return items;
  });

  constructor() {
    inject(PasswordExpirationNoticeService);
    inject(TwoFactorSetupNoticeService);
    inject(PasskeySetupNoticeService);

    effect(() => {
      const showChrome = this.showAuthenticatedChrome();
      untracked(() => this.runningTimerService.syncWithSession(showChrome));
    });

    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.layoutService.closeMobileNav());

    this.breakpointObserver
      .observe('(min-width: 768px)')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((state) => {
        if (state.matches) {
          this.layoutService.closeMobileNav();
        }
      });
  }

  onMenuToggle(): void {
    if (this.isDesktop()) {
      this.layoutService.toggleSidebarCollapsed();
      return;
    }

    this.layoutService.toggleMobileNav();
  }

  onMobileNavVisibleChange(visible: boolean): void {
    if (visible) {
      this.layoutService.openMobileNav();
      return;
    }

    this.layoutService.closeMobileNav();
  }

  openLogTimeDialog(): void {
    this.logTimeDialogService.open();
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.runningTimerService.timer.set(null);
          void this.router.navigateByUrl('/login');
        },
        error: () => {
          this.runningTimerService.timer.set(null);
          void this.router.navigateByUrl('/login');
        }
      });
  }
}
