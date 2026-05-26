import { Component, inject } from '@angular/core';
import { TwoFactorSetupNoticeService } from '@features/auth/services/two-factor-setup-notice.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-two-factor-setup-banner',
  imports: [Message, Button],
  templateUrl: './two-factor-setup-banner.component.html'
})
export class TwoFactorSetupBannerComponent {
  private readonly noticeService = inject(TwoFactorSetupNoticeService);

  readonly showBanner = this.noticeService.showSetupBanner;
  readonly message = AuthMessages.twoFactorSetupRequiredDetail;
  readonly actionLabel = AuthMessages.twoFactorSetupRequiredAction;

  openSetup(): void {
    this.noticeService.openSetupDialog();
  }
}
