import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, of, switchMap } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import {
  canOfferOptionalPasskeyEnrollment,
  isPasskeySupported
} from '../utils/passkey.utils';
import { hasPendingPasskeyEnrollmentOffer } from '../utils/pending-passkey-enrollment.storage';

export const passkeySetupRequiredGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (authService.requiresPasskeySetupScreen()) {
    return true;
  }

  return router.createUrlTree(['/issues']);
};

export const optionalPasskeyEnrollmentGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    return router.createUrlTree(['/login']);
  }

  if (
    authService.requiresPasswordChangeScreen() ||
    authService.requiresTwoFactorSetupScreen() ||
    authService.requiresPasskeySetupScreen()
  ) {
    return router.createUrlTree(['/issues']);
  }

  if (!hasPendingPasskeyEnrollmentOffer() || !isPasskeySupported()) {
    return router.createUrlTree(['/issues']);
  }

  return authService.getAuthSettings().pipe(
    switchMap((settings) => {
      const passkeysEnabled = settings.passkeys?.passkeysAuthenticationEnabled === true;
      const enrollmentPromptEnabled =
        settings.passkeys?.offerPasskeyEnrollmentPrompt === true;
      if (!passkeysEnabled || !enrollmentPromptEnabled) {
        return of(router.createUrlTree(['/issues']));
      }

      return authService.getMyAccount().pipe(
        map((account) =>
          canOfferOptionalPasskeyEnrollment(
            passkeysEnabled,
            enrollmentPromptEnabled,
            account.passkeys.length
          )
            ? true
            : router.createUrlTree(['/issues'])
        ),
        catchError(() => of(router.createUrlTree(['/issues'])))
      );
    }),
    catchError(() => of(router.createUrlTree(['/issues'])))
  );
};
