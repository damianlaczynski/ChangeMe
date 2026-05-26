import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class PasskeySetupDialogService {
  readonly visible = signal(false);
  readonly setupCompleted = signal(0);

  open(): void {
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }

  notifyCompleted(): void {
    this.setupCompleted.update((value) => value + 1);
  }
}
