import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const externalSignInCallbackGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (!authService.isAuthenticated()) {
    authService.clearLocalSession();
  }

  return true;
};
