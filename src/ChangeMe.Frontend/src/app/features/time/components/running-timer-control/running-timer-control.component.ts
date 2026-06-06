import { Component, computed, inject, viewChild } from '@angular/core';
import { ToastService } from '@core/toast/services/toast.service';
import { LogTimeDialogService } from '@features/time/services/log-time-dialog.service';
import { RunningTimerService } from '@features/time/services/running-timer.service';
import {
  TimeMessages,
  formatDuration,
  getRunningTimerTooltip
} from '@features/time/utils/time.utils';
import { Button } from 'primeng/button';
import { Popover } from 'primeng/popover';
import { Tooltip } from 'primeng/tooltip';

@Component({
  selector: 'app-running-timer-control',
  imports: [Button, Popover, Tooltip],
  templateUrl: './running-timer-control.component.html'
})
export class RunningTimerControlComponent {
  readonly runningTimerService = inject(RunningTimerService);
  readonly TimeMessages = TimeMessages;
  private readonly logTimeDialogService = inject(LogTimeDialogService);
  private readonly toastService = inject(ToastService);

  private readonly popover = viewChild.required<Popover>('popover');

  readonly timer = this.runningTimerService.timer;
  readonly elapsedLabel = computed(() => {
    const timer = this.timer();
    return timer ? formatDuration(timer.elapsedMinutes) : '';
  });
  readonly tooltip = computed(() => {
    const timer = this.timer();
    return timer ? getRunningTimerTooltip(timer) : TimeMessages.runningTimer;
  });

  togglePopover(event: Event): void {
    const popover = this.popover();

    if (popover.overlayVisible) {
      popover.hide();
      return;
    }

    this.runningTimerService.refreshTimer();
    popover.show(event);

    if (popover.container) {
      popover.align();
    }
  }

  stopAndLog(): void {
    const timer = this.timer();
    if (!timer) {
      return;
    }

    if (timer.elapsedMinutes < 1) {
      this.toastService.warn(TimeMessages.timerMinimumDuration);
      return;
    }

    this.popover().hide();
    this.logTimeDialogService.open({
      projectId: timer.projectId ?? undefined,
      projectName: timer.projectName ?? undefined,
      issueId: timer.issueId ?? null,
      issueTitle: timer.issueTitle ?? null,
      readonlyProject: Boolean(timer.projectId),
      readonlyIssue: Boolean(timer.issueId),
      prefilledDurationMinutes: timer.elapsedMinutes,
      hidePresets: true
    });

    this.runningTimerService.discardTimer();
  }

  discardTimer(): void {
    this.popover().hide();
    this.runningTimerService.confirmDiscardTimer(() => undefined);
  }
}
