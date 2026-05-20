import { Component, input, output } from '@angular/core';
import { AppliedFilterChip } from '@shared/models/applied-filter-chip.model';
import { Chip } from 'primeng/chip';

@Component({
  selector: 'app-applied-filters-chips',
  imports: [Chip],
  template: `
    @if (chips().length > 0) {
      <div class="flex flex-wrap gap-2" [class]="containerClass()">
        @for (chip of chips(); track chip.id) {
          <p-chip
            [label]="chip.label"
            [removable]="removable()"
            [removeIcon]="'pi pi-times-circle'"
            (onRemove)="onChipRemove(chip, $event)"
          />
        }
      </div>
    }
  `
})
export class AppliedFiltersChipsComponent {
  readonly chips = input.required<AppliedFilterChip[]>();
  readonly removable = input(true);
  readonly containerClass = input('mb-4');
  readonly chipRemove = output<AppliedFilterChip>();

  onChipRemove(chip: AppliedFilterChip, event: MouseEvent): void {
    event.preventDefault();
    this.chipRemove.emit(chip);
  }
}
