import { computed, Injectable, signal } from '@angular/core';
import { LayoutNavItem } from '@core/layout/models/layout-nav-item.model';
import { ProjectOverviewDto } from '@features/projects/models/project.model';

/** True for `/projects/:projectId` and nested routes; false for list and create. */
export function isProjectWorkspaceUrl(url: string): boolean {
  const path = (url.split('?')[0] ?? '').replace(/\/$/, '') || '/';
  const match = path.match(/^\/projects\/([^/]+)(?:\/.*)?$/);
  if (!match) {
    return false;
  }

  return match[1] !== 'create';
}

@Injectable({ providedIn: 'root' })
export class ProjectWorkspaceService {
  readonly projectId = signal('');
  readonly overview = signal<ProjectOverviewDto | null>(null);
  readonly isLoading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly isActive = computed(() => this.projectId().length > 0);

  readonly navItems = computed<LayoutNavItem[]>(() => {
    const id = this.projectId();
    if (!id) {
      return [];
    }

    return [
      {
        label: 'Overview',
        icon: 'pi pi-home',
        routerLink: `/projects/${id}`,
        exact: true
      },
      { label: 'Issues', icon: 'pi pi-list', routerLink: `/projects/${id}/issues` },
      { label: 'Settings', icon: 'pi pi-cog', routerLink: `/projects/${id}/settings` }
    ];
  });

  beginLoad(projectId: string): void {
    this.projectId.set(projectId);
    this.isLoading.set(true);
    this.loadError.set(null);
    this.overview.set(null);
  }

  setOverview(overview: ProjectOverviewDto): void {
    this.overview.set(overview);
    this.isLoading.set(false);
    this.loadError.set(null);
  }

  setLoadError(message: string): void {
    this.loadError.set(message);
    this.isLoading.set(false);
  }

  clear(): void {
    this.projectId.set('');
    this.overview.set(null);
    this.isLoading.set(false);
    this.loadError.set(null);
  }
}
