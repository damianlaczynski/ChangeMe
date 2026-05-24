import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login'], {
      queryParams: {
        returnUrl: state.url
      }
    });
  }

  if (authService.requiresPasswordChangeScreen()) {
    return router.createUrlTree(['/required-password-change']);
  }

  if (authService.requiresTwoFactorSetupScreen()) {
    return router.createUrlTree(['/required-two-factor-setup']);
  }

  if (authService.requiresPasskeySetupScreen()) {
    return router.createUrlTree(['/required-passkey-setup']);
  }

  return true;
};
