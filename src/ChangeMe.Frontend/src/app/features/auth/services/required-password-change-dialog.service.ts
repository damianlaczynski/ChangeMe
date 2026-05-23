import { Injectable, signal } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class RequiredPasswordChangeDialogService {
  readonly visible = signal(false);

  open(): void {
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
  }
}
