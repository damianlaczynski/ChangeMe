import { Injectable, signal } from '@angular/core';

export type LogTimeDialogContext = {
  projectId?: string;
  projectName?: string;
  issueId?: string | null;
  issueTitle?: string | null;
  readonlyProject?: boolean;
  readonlyIssue?: boolean;
  prefilledDurationMinutes?: number;
  hidePresets?: boolean;
};

@Injectable({
  providedIn: 'root'
})
export class LogTimeDialogService {
  readonly visible = signal(false);
  readonly context = signal<LogTimeDialogContext | null>(null);
  readonly saved = signal(0);
  readonly openGeneration = signal(0);

  open(context: LogTimeDialogContext | null = null): void {
    this.context.set(context);
    this.openGeneration.update((value) => value + 1);
    this.visible.set(true);
  }

  close(): void {
    this.visible.set(false);
    this.context.set(null);
  }

  notifySaved(): void {
    this.saved.update((value) => value + 1);
  }
}
