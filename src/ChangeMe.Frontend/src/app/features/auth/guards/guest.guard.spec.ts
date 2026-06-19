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
import { guestGuard } from './guest.guard';

describe('guestGuard', () => {
  let router: Router;
  let authService: {
    isAuthenticated: ReturnType<typeof vi.fn>;
    passwordChangeRequired: ReturnType<typeof vi.fn>;
    requiresTwoFactorSetupScreen: ReturnType<typeof vi.fn>;
    requiresPasskeySetupScreen: ReturnType<typeof vi.fn>;
    clearLocalSession: ReturnType<typeof vi.fn>;
  };

  const route = {} as ActivatedRouteSnapshot;
  const state = { url: '/login' } as RouterStateSnapshot;

  function runGuard(): GuardResult {
    return TestBed.runInInjectionContext(() => guestGuard(route, state)) as GuardResult;
  }

  function expectUrl(result: GuardResult, expectedPath: string): void {
    expect(result).toBeInstanceOf(UrlTree);
    expect(router.serializeUrl(result as UrlTree)).toBe(expectedPath);
  }

  beforeEach(() => {
    authService = {
      isAuthenticated: vi.fn(() => false),
      passwordChangeRequired: vi.fn(() => false),
      requiresTwoFactorSetupScreen: vi.fn(() => false),
      requiresPasskeySetupScreen: vi.fn(() => false),
      clearLocalSession: vi.fn()
    };

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: authService }]
    });

    router = TestBed.inject(Router);
  });

  it('clears stale local session for guests', () => {
    expect(runGuard()).toBe(true);
    expect(authService.clearLocalSession).toHaveBeenCalledOnce();
  });

  it('redirects authenticated users to issues', () => {
    authService.isAuthenticated.mockReturnValue(true);

    expectUrl(runGuard(), '/issues');
    expect(authService.clearLocalSession).not.toHaveBeenCalled();
  });

  it('redirects authenticated users with required password change', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.passwordChangeRequired.mockReturnValue(true);

    expectUrl(runGuard(), '/required-password-change');
  });

  it('redirects authenticated users with required two-factor setup', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.requiresTwoFactorSetupScreen.mockReturnValue(true);

    expectUrl(runGuard(), '/required-two-factor-setup');
  });

  it('redirects authenticated users with required passkey setup', () => {
    authService.isAuthenticated.mockReturnValue(true);
    authService.requiresPasskeySetupScreen.mockReturnValue(true);

    expectUrl(runGuard(), '/required-passkey-setup');
  });
});
