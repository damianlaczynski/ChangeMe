import { Component, inject, input } from '@angular/core';
import { NavigationHistoryService } from '@core/navigation/services/navigation-history.service';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-back-button',
  imports: [Button],
  template: `
    <p-button
      [label]="label()"
      icon="pi pi-arrow-left"
      severity="secondary"
      [outlined]="true"
      size="small"
      (onClick)="onBack()"
    />
  `
})
export class BackButtonComponent {
  private readonly navigationHistory = inject(NavigationHistoryService);

  readonly label = input('Back');
  readonly fallbackUrl = input('/issues');

  onBack(): void {
    this.navigationHistory.goBack(this.fallbackUrl());
  }
}
