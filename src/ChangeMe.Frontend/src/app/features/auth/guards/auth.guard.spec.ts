import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  GuardResult,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter
} from '@angular/router';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthService } from '../services/auth.service';
import { authGuard } from './auth.guard';

describe('authGuard', () => {
  let router: Router;
  let authService: {
    isAuthenticated: ReturnType<typeof vi.fn>;
    requiresPasswordChangeScreen: ReturnType<typeof vi.fn>;
    requiresTwoFactorSetupScreen: ReturnType<typeof vi.fn>;
    requiresPasskeySetupScreen: ReturnType<typeof vi.fn>;
  };

  const route = {} as ActivatedRouteSnapshot;

  function runGuard(url: string): GuardResult {
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() => authGuard(route, state)) as GuardResult;
  }

  function expectUrl(result: GuardResult, expectedPath: string): void {
    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(expectedPath);
  }

  beforeEach(() => {
    authService = {
      isAuthenticated: vi.fn(() => false),
      requiresPasswordChangeScreen: vi.fn(() => false),
      requiresTwoFactorSetupScreen: vi.fn(() => false),
      requiresPasskeySetupScreen: vi.fn(() => false)
    };

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }]
    });

    router = TestBed.inject(Router);
  });

  it('redirects unauthenticated users to login with returnUrl', () => {
    expectUrl(runGuard('/issues'), '/login?returnUrl=%2Fissues');
  });

  it('redirects to required password change before app routes', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.requiresPasswordChangeScreen.mockReturnValue(true);

    expectUrl(runGuard('/issues'), '/required-password-change');
  });

  it('redirects to required two-factor setup before app routes', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.requiresTwoFactorSetupScreen.mockReturnValue(true);

    expectUrl(runGuard('/issues'), '/required-two-factor-setup');
  });

  it('redirects to required passkey setup before app routes', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.requiresPasskeySetupScreen.mockReturnValue(true);

    expectUrl(runGuard('/issues'), '/required-passkey-setup');
  });

  it('allows access when the session is fully established', () => {
    authService.isAuthenticated.mockReturnValue(true);

    expect(runGuard('/issues')).toBe(true);
  });
});
