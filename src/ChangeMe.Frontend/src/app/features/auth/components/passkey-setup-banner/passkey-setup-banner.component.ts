import { Component, inject } from '@angular/core';
import { PasskeySetupNoticeService } from '@features/auth/services/passkey-setup-notice.service';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { Button } from 'primeng/button';
import { Message } from 'primeng/message';

@Component({
  selector: 'app-passkey-setup-banner',
  imports: [Message, Button],
  templateUrl: './passkey-setup-banner.component.html'
})
export class PasskeySetupBannerComponent {
  private readonly noticeService = inject(PasskeySetupNoticeService);

  readonly showBanner = this.noticeService.showSetupBanner;
  readonly message = AuthMessages.passkeySetupRequiredDetail;
  readonly actionLabel = AuthMessages.passkeySetupRequiredAction;

  openSetup(): void {
    this.noticeService.openSetupDialog();
  }
}
