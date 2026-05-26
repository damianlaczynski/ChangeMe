import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import {
  clearExternalAccountFlow,
  readExternalAccountFlow
} from '@features/auth/utils/external-account-flow.storage';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-external-sign-in-callback',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [AuthPageComponent, ProgressSpinner],
  template: `
    <app-auth-page
      [title]="callbackTitle"
      [subtitle]="authMessages.externalSignInCallbackLoading"
    >
      <div class="flex justify-center py-8">
        <p-progressSpinner />
      </div>
    </app-auth-page>
  `
})
export class ExternalSignInCallbackComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly authMessages = AuthMessages;
  readonly callbackTitle = readExternalAccountFlow()
    ? 'Completing verification'
    : 'Signing in';

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const providerError = params.get('error');
    const accountFlow = readExternalAccountFlow();

    if (providerError) {
      this.navigateExternalSignInError(accountFlow);
      return;
    }

    const code = params.get('code');
    const state = params.get('state');
    if (!code || !state) {
      this.navigateExternalSignInError(accountFlow);
      return;
    }

    this.authService
      .completeExternalSignIn({ code, state })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => this.authService.continueAfterExternalSignIn(response),
        error: (error) => {
          if (accountFlow) {
            clearExternalAccountFlow();
          }
          const message =
            error instanceof Error ? error.message : AuthMessages.externalSignInFailed;
          const errorTarget =
            accountFlow && this.authService.isAuthenticated() ? '/account' : '/login';
          void this.router.navigate([errorTarget], {
            queryParams: { externalSignInMessage: message }
          });
        }
      });
  }

  private navigateExternalSignInError(
    accountFlow: ReturnType<typeof readExternalAccountFlow>
  ): void {
    if (accountFlow) {
      clearExternalAccountFlow();
    }
    const errorTarget =
      accountFlow && this.authService.isAuthenticated() ? '/account' : '/login';
    void this.router.navigate([errorTarget], {
      queryParams: { externalSignInError: '1' }
    });
  }
}
