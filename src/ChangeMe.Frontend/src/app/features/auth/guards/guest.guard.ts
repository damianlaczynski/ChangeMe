import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    if (authService.passwordChangeRequired()) {
      return router.createUrlTree(['/required-password-change']);
    }

    if (authService.requiresTwoFactorSetupScreen()) {
      return router.createUrlTree(['/required-two-factor-setup']);
    }

    return router.createUrlTree(['/issues']);
  }

  authService.clearLocalSession();
  return true;
};
