import { Component, input, output } from '@angular/core';
import {
  AvailabilityEntryDto,
  AvailabilityEntrySource
} from '@features/billing/models/availability.model';
import {
  formatAvailabilityTimeRange,
  getAvailabilityStatusLabel,
  getAvailabilityStatusSeverity
} from '@features/billing/utils/availability-calendar.utils';
import { BillingMessages } from '@features/billing/utils/billing.utils';
import { Button } from 'primeng/button';
import { Drawer } from 'primeng/drawer';
import { Tag } from 'primeng/tag';

@Component({
  selector: 'app-day-availability-panel',
  imports: [Drawer, Button, Tag],
  templateUrl: './day-availability-panel.component.html'
})
export class DayAvailabilityPanelComponent {
  readonly visible = input(false);
  readonly title = input('');
  readonly entries = input<AvailabilityEntryDto[]>([]);
  readonly canManage = input(false);
  readonly canManagePattern = input(false);

  readonly hidden = output<void>();
  readonly addException = output<void>();
  readonly editEntry = output<AvailabilityEntryDto>();
  readonly editPattern = output<void>();

  readonly BillingMessages = BillingMessages;
  readonly AvailabilityEntrySource = AvailabilityEntrySource;
  readonly getAvailabilityStatusLabel = getAvailabilityStatusLabel;
  readonly getAvailabilityStatusSeverity = getAvailabilityStatusSeverity;
  readonly formatAvailabilityTimeRange = formatAvailabilityTimeRange;

  onHide(): void {
    this.hidden.emit();
  }

  sourceLabel(source: AvailabilityEntrySource): string {
    switch (source) {
      case AvailabilityEntrySource.Leave:
        return 'Leave';
      case AvailabilityEntrySource.Manual:
        return 'Manual';
      case AvailabilityEntrySource.Recurring:
        return 'Recurring';
      default:
        return source;
    }
  }
}
