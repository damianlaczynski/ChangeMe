import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { Button } from 'primeng/button';

@Component({
  selector: 'app-back-button',
  imports: [Button],
  template: `
    <p-button
      [label]="label()"
      size="small"
      icon="pi pi-arrow-left"
      severity="secondary"
      [outlined]="true"
      (onClick)="onBack()"
    />
  `
})
export class BackButtonComponent {
  private readonly router = inject(Router);

  readonly label = input.required<string>();
  readonly route = input.required<readonly string[]>();

  onBack(): void {
    void this.router.navigate(this.route());
  }
}
