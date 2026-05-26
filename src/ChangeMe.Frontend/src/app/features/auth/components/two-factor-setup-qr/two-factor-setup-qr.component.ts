import { Component, effect, input, signal } from '@angular/core';
import { AuthMessages } from '@features/auth/utils/auth.utils';
import { Message } from 'primeng/message';
import { ProgressSpinner } from 'primeng/progressspinner';
import QRCode from 'qrcode';

@Component({
  selector: 'app-two-factor-setup-qr',
  imports: [ProgressSpinner, Message],
  templateUrl: './two-factor-setup-qr.component.html'
})
export class TwoFactorSetupQrComponent {
  readonly provisioningUri = input.required<string>();
  readonly sharedSecret = input<string | null>(null);
  readonly issuerName = input<string | null>(null);

  readonly qrDataUrl = signal<string | null>(null);
  readonly qrError = signal(false);
  readonly manualKeyVisible = signal(false);

  readonly AuthMessages = AuthMessages;

  constructor() {
    effect(() => {
      const uri = this.provisioningUri();
      void this.generateQr(uri);
    });
  }

  toggleManualKey(): void {
    this.manualKeyVisible.update((visible) => !visible);
  }

  private async generateQr(uri: string): Promise<void> {
    if (!uri.trim()) {
      this.qrDataUrl.set(null);
      this.qrError.set(false);
      return;
    }

    try {
      const dataUrl = await QRCode.toDataURL(uri, {
        errorCorrectionLevel: 'M',
        margin: 2,
        width: 220
      });
      this.qrDataUrl.set(dataUrl);
      this.qrError.set(false);
    } catch {
      this.qrDataUrl.set(null);
      this.qrError.set(true);
    }
  }
}
