import { DatePipe } from '@angular/common';
import { Component, computed, DestroyRef, inject, signal, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MySessionsComponent } from '@features/auth/components/my-sessions/my-sessions.component';
import { MyAccountDto } from '@features/auth/models/auth.model';
import { AuthService } from '@features/auth/services/auth.service';
import { PermissionCodes } from '@features/auth/utils/auth.utils';
import { EffectivePermissionsComponent } from '@features/users/components/effective-permissions/effective-permissions.component';
import { UserMessages } from '@features/users/utils/users.utils';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { Message } from 'primeng/message';
import { Panel } from 'primeng/panel';
import { ProgressSpinner } from 'primeng/progressspinner';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-my-account',
  imports: [
    DatePipe,
    RouterLink,
    Card,
    Button,
    Message,
    Tag,
    Panel,
    ProgressSpinner,
    EffectivePermissionsComponent,
    MySessionsComponent
  ],
  templateUrl: './my-account.component.html'
})
export class MyAccountComponent {
  private readonly authService = inject(AuthService);
  private readonly destroyRef = inject(DestroyRef);

  readonly account = signal<MyAccountDto | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly isLoading = signal(true);
  readonly UserMessages = UserMessages;

  private readonly sessionsComponent = viewChild(MySessionsComponent);

  readonly canViewSessions = computed(() =>
    this.authService.hasPermission(PermissionCodes.sessionsViewOwn)
  );
  readonly canManageSessions = computed(() =>
    this.authService.hasPermission(PermissionCodes.sessionsManageOwn)
  );
  readonly canViewRoles = computed(() =>
    this.authService.hasPermission(PermissionCodes.rolesView)
  );
  readonly isSigningOutEverywhere = computed(
    () => this.sessionsComponent()?.isSigningOutEverywhere() ?? false
  );

  readonly effectivePermissions = computed(
    () => this.account()?.effectivePermissions ?? []
  );

  constructor() {
    this.reload();
  }

  reload(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.account.set(account);
          this.isLoading.set(false);
        },
        error: (error: Error) => {
          this.errorMessage.set(error.message);
          this.isLoading.set(false);
        }
      });
  }

  signOutEverywhere(): void {
    this.sessionsComponent()?.confirmSignOutEverywhere();
  }
}
