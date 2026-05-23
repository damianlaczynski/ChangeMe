import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

export const registerGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    if (authService.passwordChangeRequired()) {
      return router.createUrlTree(['/required-password-change']);
    }

    return router.createUrlTree(['/issues']);
  }

  return authService.getAuthSettings().pipe(
    map((settings) => {
      if (!settings.publicRegistrationEnabled) {
        return router.createUrlTree(['/login'], {
          queryParams: { registrationDisabled: '1' }
        });
      }

      return true;
    })
  );
};
