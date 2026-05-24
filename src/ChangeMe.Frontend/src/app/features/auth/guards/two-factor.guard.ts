import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { readTwoFactorChallenge } from '../utils/two-factor-challenge.storage';

export const twoFactorChallengeGuard: CanActivateFn = () => {
  const router = inject(Router);

  if (readTwoFactorChallenge()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

export const twoFactorSetupRequiredGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (authService.requiresTwoFactorSetupScreen()) {
    return true;
  }

  return router.createUrlTree(['/issues']);
};
