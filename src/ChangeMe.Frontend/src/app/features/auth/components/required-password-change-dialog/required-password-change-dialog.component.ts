import { Component, DestroyRef, inject, viewChild } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ToastService } from '@core/toast/services/toast.service';
import { RequiredPasswordChangeFormComponent } from '@features/auth/components/required-password-change-form/required-password-change-form.component';
import { AuthService } from '@features/auth/services/auth.service';
import { PasswordExpirationNoticeService } from '@features/auth/services/password-expiration-notice.service';
import { RequiredPasswordChangeDialogService } from '@features/auth/services/required-password-change-dialog.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { Dialog } from 'primeng/dialog';

@Component({
  selector: 'app-required-password-change-dialog',
  imports: [Dialog, RequiredPasswordChangeFormComponent],
  templateUrl: './required-password-change-dialog.component.html'
})
export class RequiredPasswordChangeDialogComponent {
  private readonly authService = inject(AuthService);
  private readonly dialogService = inject(RequiredPasswordChangeDialogService);
  private readonly noticeService = inject(PasswordExpirationNoticeService);
  private readonly toastService = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  readonly dialogServiceRef = this.dialogService;
  readonly title = AuthMessages.requiredPasswordChangeTitle;
  readonly subtitle = AuthMessages.passwordExpiredDetail;

  private readonly form = viewChild(RequiredPasswordChangeFormComponent);

  onVisibleChange(visible: boolean): void {
    if (!visible) {
      this.dialogService.close();
    }
  }

  onSubmit(newPassword: string): void {
    this.authService
      .requiredChangePassword({ newPassword })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.authService.clearPasswordChangeRequired();
          this.noticeService.clearNotices();
          this.form()?.resetAfterSuccess();
          this.dialogService.close();
          this.toastService.success(AuthMessages.passwordUpdated);
          this.authService
            .refreshSession()
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe();
        },
        error: (error: Error) => {
          this.form()?.reportError(error.message);
        }
      });
  }
}
