import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, computed, DestroyRef, inject } from '@angular/core';
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
import {
  isProjectWorkspaceUrl,
  ProjectWorkspaceService
} from '@features/projects/services/project-workspace.service';
import {
  getProjectStatusLabel,
  getProjectStatusSeverity
} from '@features/projects/utils/projects.utils';
import { PermissionCodes } from '@shared/authorization/permission-codes';
import { Button } from 'primeng/button';
import { Drawer } from 'primeng/drawer';
import { Tag } from 'primeng/tag';
import { filter, map } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  imports: [
    RouterOutlet,
    RouterLink,
    SidebarNavComponent,
    NotificationsBellComponent,
    PasswordExpirationBannerComponent,
    TwoFactorSetupBannerComponent,
    PasskeySetupBannerComponent,
    RequiredPasswordChangeDialogComponent,
    TwoFactorSetupDialogComponent,
    PasskeySetupDialogComponent,
    Button,
    Drawer,
    Tag
  ],
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly destroyRef = inject(DestroyRef);

  readonly layoutService = inject(LayoutService);
  readonly projectWorkspace = inject(ProjectWorkspaceService);

  readonly getProjectStatusLabel = getProjectStatusLabel;
  readonly getProjectStatusSeverity = getProjectStatusSeverity;

  readonly currentUser = this.authService.currentUser;
  readonly formatUserReference = formatUserReference;
  readonly isAuthenticated = this.authService.isAuthenticated;
  readonly requiresPasswordChangeScreen = this.authService.requiresPasswordChangeScreen;
  readonly requiresTwoFactorSetupScreen = this.authService.requiresTwoFactorSetupScreen;
  readonly requiresPasskeySetupScreen = this.authService.requiresPasskeySetupScreen;
  readonly isProjectWorkspace = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),
      map(() => isProjectWorkspaceUrl(this.router.url)),
      takeUntilDestroyed(this.destroyRef)
    ),
    { initialValue: isProjectWorkspaceUrl(this.router.url) }
  );

  readonly showGlobalAuthenticatedChrome = computed(
    () =>
      this.isAuthenticated() &&
      !this.requiresPasswordChangeScreen() &&
      !this.requiresTwoFactorSetupScreen() &&
      !this.requiresPasskeySetupScreen() &&
      !this.isProjectWorkspace()
  );

  readonly showProjectWorkspaceChrome = computed(
    () =>
      this.isAuthenticated() &&
      !this.requiresPasswordChangeScreen() &&
      !this.requiresTwoFactorSetupScreen() &&
      !this.requiresPasskeySetupScreen() &&
      this.isProjectWorkspace()
  );
  readonly isDesktop = toSignal(
    this.breakpointObserver
      .observe('(min-width: 768px)')
      .pipe(map((state) => state.matches)),
    { initialValue: false }
  );

  readonly authenticatedNavItems = computed<LayoutNavItem[]>(() => {
    const items: LayoutNavItem[] = [
      { label: 'Projects', icon: 'pi pi-folder', routerLink: '/projects', exact: true }
    ];

    if (this.authService.hasPermission(PermissionCodes.usersView)) {
      items.push({
        label: 'Users',
        icon: 'pi pi-users',
        routerLink: '/users',
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

    items.push({ label: 'My account', icon: 'pi pi-user', routerLink: '/account' });
    return items;
  });

  constructor() {
    inject(PasswordExpirationNoticeService);
    inject(TwoFactorSetupNoticeService);
    inject(PasskeySetupNoticeService);

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

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          void this.router.navigateByUrl('/login');
        },
        error: () => {
          void this.router.navigateByUrl('/login');
        }
      });
  }
}
