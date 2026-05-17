import { Injectable, signal } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

const SIDEBAR_COLLAPSED_KEY = 'sidebarCollapsed';

@Injectable({
  providedIn: 'root'
})
export class LayoutService {
  private readonly _sidebarCollapsed = signal(this.getInitialSidebarCollapsed());
  private readonly _mobileNavOpen = signal(false);
  private readonly _themeMode = signal<ThemeMode>(this.getInitialThemeMode());

  readonly $sidebarCollapsed = this._sidebarCollapsed.asReadonly();
  readonly $mobileNavOpen = this._mobileNavOpen.asReadonly();
  readonly $themeMode = this._themeMode.asReadonly();

  constructor() {
    this.applyTheme(this._themeMode());
  }

  toggleSidebarCollapsed(): void {
    this._sidebarCollapsed.update((collapsed) => !collapsed);
    localStorage.setItem(SIDEBAR_COLLAPSED_KEY, String(this._sidebarCollapsed()));
  }

  openMobileNav(): void {
    this._mobileNavOpen.set(true);
  }

  closeMobileNav(): void {
    this._mobileNavOpen.set(false);
  }

  toggleMobileNav(): void {
    this._mobileNavOpen.update((open) => !open);
  }

  toggleTheme(): void {
    const newTheme = this._themeMode() === 'light' ? 'dark' : 'light';
    this._themeMode.set(newTheme);
    localStorage.setItem('theme', newTheme);
    this.applyTheme(newTheme);
  }

  private getInitialSidebarCollapsed(): boolean {
    return localStorage.getItem(SIDEBAR_COLLAPSED_KEY) === 'true';
  }

  private getInitialThemeMode(): ThemeMode {
    const savedTheme = localStorage.getItem('theme');
    return savedTheme === 'dark' ? 'dark' : 'light';
  }

  private applyTheme(mode: ThemeMode): void {
    document.documentElement.classList.toggle('app-dark', mode === 'dark');
  }
}
