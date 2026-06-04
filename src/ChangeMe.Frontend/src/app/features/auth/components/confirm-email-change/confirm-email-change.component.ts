import { Component, DestroyRef, inject, OnInit, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ToastService } from '@core/toast/services/toast.service';
import { AuthPageComponent } from '@features/auth/components/auth-page/auth-page.component';
import { AuthService } from '@features/auth/services/auth.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { BackButtonComponent } from '@shared/components/back-button/back-button.component';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';

@Component({
  selector: 'app-confirm-email-change',
  host: { class: 'flex min-h-0 flex-1 flex-col' },
  imports: [
    AuthPageComponent,
    BackButtonComponent,
    RouterLink,
    Button,
    Message,
    ProgressSpinner
  ],
  templateUrl: './confirm-email-change.component.html'
})
export class ConfirmEmailChangeComponent implements OnInit {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);
  private readonly toastService = inject(ToastService);

  private readonly wasSignedInAtStart = this.authService.isAuthenticated();
  private confirmStarted = false;

  readonly isVerifying = signal(false);
  readonly successMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly wrongAccount = signal(false);
  readonly signedOutAfterSuccess = signal(false);
  readonly canResendConfirmation = signal(false);
  readonly isResendingConfirmation = signal(false);
  readonly authMessages = AuthMessages;

  ngOnInit(): void {
    const token = this.route.snapshot.queryParamMap.get('token')?.trim();
    if (!token) {
      this.errorMessage.set(AuthMessages.invalidEmailChangeConfirmationLink);
      this.refreshResendEligibility();
      return;
    }

    if (this.confirmStarted) {
      return;
    }

    this.confirmStarted = true;
    this.isVerifying.set(true);
    this.authService
      .confirmEmailChange({ token })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response) => {
          this.isVerifying.set(false);
          if (response.succeeded) {
            this.handleSuccess(
              response.message ?? AuthMessages.confirmEmailChangeSuccess
            );
            return;
          }

          this.wrongAccount.set(response.wrongSignedInAccount);
          this.errorMessage.set(
            response.wrongSignedInAccount
              ? (response.message ?? AuthMessages.emailChangeConfirmationWrongAccount)
              : (response.message ?? AuthMessages.invalidEmailChangeConfirmationLink)
          );
          this.refreshResendEligibility();
        },
        error: (error) => {
          this.isVerifying.set(false);
          this.errorMessage.set(
            error instanceof Error
              ? error.message
              : AuthMessages.invalidEmailChangeConfirmationLink
          );
          this.refreshResendEligibility();
        }
      });
  }

  signOut(): void {
    this.authService.logout().subscribe({
      next: () => void this.router.navigate(['/login']),
      error: () => {
        this.authService.clearLocalSession();
        void this.router.navigate(['/login']);
      }
    });
  }

  goToLogin(): void {
    void this.router.navigate(['/login'], {
      queryParams: { emailChanged: '1' }
    });
  }

  resendConfirmation(): void {
    if (this.isResendingConfirmation() || !this.canResendConfirmation()) {
      return;
    }

    this.isResendingConfirmation.set(true);
    this.authService
      .resendEmailChangeConfirmation()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastService.success(AuthMessages.emailChangeResendSuccess);
        },
        error: (error: Error) => this.toastService.error(error.message),
        complete: () => this.isResendingConfirmation.set(false)
      });
  }

  private refreshResendEligibility(): void {
    if (this.wrongAccount() || !this.authService.isAuthenticated()) {
      this.canResendConfirmation.set(false);
      return;
    }

    this.authService
      .getMyAccount()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (account) => {
          this.canResendConfirmation.set(!!account.pendingEmailChange);
        },
        error: () => this.canResendConfirmation.set(false)
      });
  }

  private handleSuccess(message: string): void {
    this.successMessage.set(message);
    this.signedOutAfterSuccess.set(this.wasSignedInAtStart);
    this.canResendConfirmation.set(false);
    this.toastService.success('Email changed', message);

    this.authService.clearLocalSession();
  }
}
