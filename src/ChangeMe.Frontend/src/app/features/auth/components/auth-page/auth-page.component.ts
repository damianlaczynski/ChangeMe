import { booleanAttribute, Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';
import { ButtonComponent } from '@laczynski/ui';

@Component({
  selector: 'app-auth-page',
  host: { class: 'app-auth-page' },
  imports: [ButtonComponent],
  template: `
    <div
      class="app-auth-page__card"
      [class.app-auth-page__card--wide]="wide()"
      [class.app-auth-page__card--narrow]="!wide()"
    >
      <header class="app-auth-page__header">
        <h1 class="app-auth-page__title">{{ title() }}</h1>
        @if (subtitle()) {
          <p class="app-auth-page__subtitle">{{ subtitle() }}</p>
        }
      </header>

      <ng-content />

      @if (footerPrompt() && footerLinkLabel() && footerRoute()) {
        <div class="app-auth-page__footer">
          <p class="app-auth-page__footer-text">
            <span>{{ footerPrompt() }}</span>
            <ui-button
              [text]="footerLinkLabel()"
              appearance="subtle"
              (click)="navigateToFooterRoute()"
            />
          </p>
        </div>
      }
    </div>
  `
})
export class AuthPageComponent {
  private readonly router = inject(Router);

  readonly title = input.required<string>();
  readonly subtitle = input('');
  readonly wide = input(false, { transform: booleanAttribute });
  readonly footerPrompt = input('');
  readonly footerLinkLabel = input('');
  readonly footerRoute = input<string | string[]>('');

  navigateToFooterRoute(): void {
    const route = this.footerRoute();
    if (!route) {
      return;
    }

    void this.router.navigate(Array.isArray(route) ? route : [route]);
  }
}
