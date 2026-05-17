import { Injectable, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { NavigationStackEntry } from '@core/navigation/models/navigation-stack-entry.model';
import { filter } from 'rxjs/operators';

const STORAGE_KEY = 'changeme.navigation.stack';
const MAX_ENTRIES = 100;

@Injectable({
  providedIn: 'root'
})
export class NavigationHistoryService {
  private readonly router = inject(Router);

  private stack: NavigationStackEntry[] = [];
  private skipNextNavigation = false;

  constructor() {
    this.stack = this.loadStack();

    this.router.events
      .pipe(filter((event): event is NavigationEnd => event instanceof NavigationEnd))
      .subscribe((event) => this.onNavigationEnd(event.urlAfterRedirects));
  }

  goBack(fallbackUrl = '/issues'): void {
    const currentUrl = this.normalizeUrl(this.router.url);
    this.popThroughUrl(currentUrl);

    const target = this.stack[this.stack.length - 1];
    if (!target) {
      this.navigateTo(fallbackUrl, true);
      return;
    }

    this.navigateTo(target.url, true);
  }

  canGoBack(): boolean {
    const currentUrl = this.normalizeUrl(this.router.url);
    return (
      this.stack.findIndex((entry) => entry.url !== currentUrl) >= 0 &&
      this.stack.length > 1
    );
  }

  removeUrlsMatching(matcher: (url: string) => boolean): void {
    this.stack = this.stack.filter((entry) => !matcher(entry.url));
    this.persist();
  }

  removeIssue(issueId: string): void {
    const pattern = this.createIssueUrlPattern(issueId);
    this.removeUrlsMatching((url) => pattern.test(url));
  }

  navigateAfterIssueRemoval(issueId: string, fallbackUrl = '/issues'): void {
    this.removeIssue(issueId);
    this.goBack(fallbackUrl);
  }

  clear(): void {
    this.stack = [];
    this.skipNextNavigation = false;

    if (typeof sessionStorage !== 'undefined') {
      sessionStorage.removeItem(STORAGE_KEY);
    }
  }

  private onNavigationEnd(url: string): void {
    const normalizedUrl = this.normalizeUrl(url);

    if (this.skipNextNavigation) {
      this.skipNextNavigation = false;
      this.syncStackForUrl(normalizedUrl);
      return;
    }

    const top = this.stack[this.stack.length - 1];
    if (top?.url === normalizedUrl) {
      return;
    }

    const existingIndex = this.findEntryIndex(normalizedUrl);
    if (existingIndex >= 0) {
      this.stack = this.stack.slice(0, existingIndex + 1);
      this.stack[existingIndex] = { url: normalizedUrl };
      this.persist();
      return;
    }

    if (top && this.isSamePath(top.url, normalizedUrl)) {
      this.stack[this.stack.length - 1] = { url: normalizedUrl };
      this.persist();
      return;
    }

    this.pushEntry(normalizedUrl);
  }

  private syncStackForUrl(url: string): void {
    const existingIndex = this.findEntryIndex(url);
    if (existingIndex >= 0) {
      this.stack = this.stack.slice(0, existingIndex + 1);
      this.stack[existingIndex] = { url };
    } else if (this.stack.length === 0) {
      this.pushEntry(url);
    } else {
      this.stack[this.stack.length - 1] = { url };
    }

    this.persist();
  }

  private popThroughUrl(url: string): void {
    while (this.stack.length > 0 && this.stack[this.stack.length - 1].url === url) {
      this.stack.pop();
    }

    this.persist();
  }

  private pushEntry(url: string): void {
    this.stack.push({ url });
    if (this.stack.length > MAX_ENTRIES) {
      this.stack = this.stack.slice(-MAX_ENTRIES);
    }
    this.persist();
  }

  private navigateTo(url: string, skipNextNavigation: boolean): void {
    this.skipNextNavigation = skipNextNavigation;
    this.persist();
    void this.router.navigateByUrl(url);
  }

  private findEntryIndex(url: string): number {
    for (let index = this.stack.length - 1; index >= 0; index -= 1) {
      if (this.stack[index].url === url) {
        return index;
      }
    }

    return -1;
  }

  private normalizeUrl(url: string): string {
    if (!url) {
      return '/';
    }

    return url.startsWith('/') ? url : `/${url}`;
  }

  private isSamePath(leftUrl: string, rightUrl: string): boolean {
    return this.getPath(leftUrl) === this.getPath(rightUrl);
  }

  private getPath(url: string): string {
    return url.split(/[?#]/)[0];
  }

  private createIssueUrlPattern(issueId: string): RegExp {
    const escapedId = issueId.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
    return new RegExp(`/issues/${escapedId}(/|$|\\?|#)`);
  }

  private loadStack(): NavigationStackEntry[] {
    if (typeof sessionStorage === 'undefined') {
      return [];
    }

    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return [];
      }

      const parsed = JSON.parse(raw) as NavigationStackEntry[];
      if (!Array.isArray(parsed)) {
        return [];
      }

      return parsed
        .filter(
          (entry): entry is NavigationStackEntry => typeof entry?.url === 'string'
        )
        .map((entry) => ({ url: this.normalizeUrl(entry.url) }));
    } catch {
      return [];
    }
  }

  private persist(): void {
    if (typeof sessionStorage === 'undefined') {
      return;
    }

    sessionStorage.setItem(STORAGE_KEY, JSON.stringify(this.stack));
  }
}
