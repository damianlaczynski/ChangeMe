import { Component, computed, inject } from '@angular/core';
import { PasswordExpirationNoticeService } from '@features/auth/services/password-expiration-notice.service';
import {
  AuthMessages,
  passwordExpiryWarningDetail
} from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-password-expiration-banner',
  imports: [Message, Button],
  templateUrl: './password-expiration-banner.component.html'
})
export class PasswordExpirationBannerComponent {
  private readonly noticeService = inject(PasswordExpirationNoticeService);

  readonly showBanner = this.noticeService.showChangePasswordAction;

  readonly message = computed(() => {
    if (this.noticeService.bannerMode() === 'expired') {
      return AuthMessages.passwordExpiredDetail;
    }

    const days = this.noticeService.bannerDaysRemaining();
    return days === null ? '' : passwordExpiryWarningDetail(days);
  });

  readonly severity = computed(() =>
    this.noticeService.bannerMode() === 'expired' ? 'error' : 'warn'
  );

  openChangePassword(): void {
    this.noticeService.openChangePasswordDialog();
  }
}
