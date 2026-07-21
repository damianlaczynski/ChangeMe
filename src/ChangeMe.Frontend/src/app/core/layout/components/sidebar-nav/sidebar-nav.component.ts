import {
  Component,
  computed,
  DestroyRef,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router } from '@angular/router';
import {
  LayoutNavItem,
  LayoutNavSection
} from '@core/layout/models/layout-nav-item.model';
import { NavComponent, type NavNode } from '@laczynski/ui';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-sidebar-nav',
  host: {
    class: 'app-sidebar-nav-host'
  },
  imports: [NavComponent],
  template: `
    <ui-nav
      [items]="navItems()"
      appearance="transparent"
      variant="primary"
      size="medium"
    />
  `
})
export class SidebarNavComponent {
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly items = input.required<LayoutNavItem[]>();
  private readonly navigationTick = signal(0);

  readonly navigate = output<void>();

  readonly navItems = computed<NavNode[]>(() => {
    this.navigationTick();

    return this.buildGroupedNavItems(this.items());
  });

  constructor() {
    this.router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.navigationTick.update((value) => value + 1));
  }

  private buildGroupedNavItems(items: LayoutNavItem[]): NavNode[] {
    const nodes: NavNode[] = [];
    let currentSection: LayoutNavSection | null = null;

    for (const item of items) {
      if (item.section !== currentSection) {
        currentSection = item.section;
        nodes.push({
          id: `section-${currentSection}`,
          label: currentSection,
          isSectionHeader: true
        });
      }

      nodes.push({
        id: item.routerLink,
        label: item.label,
        icon: item.icon,
        selected: this.isSelected(item),
        onClick: () => this.navigateTo(item)
      });
    }

    return nodes;
  }

  private navigateTo(item: LayoutNavItem): void {
    void this.router.navigateByUrl(item.routerLink);
    this.navigate.emit();
  }

  private isSelected(item: LayoutNavItem, url = this.router.url): boolean {
    const path = url.split(/[?#]/)[0];
    return item.exact ? path === item.routerLink : path.startsWith(item.routerLink);
  }
}
