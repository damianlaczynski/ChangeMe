import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Result } from '@shared/api/models/api-response.model';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';
import { isPasskeySetupRequiredApiError } from '../utils/passkey.utils';
import { isPasswordExpiredApiError } from '../utils/password-expiration.utils';
import { isTwoFactorSetupRequiredApiError } from '../utils/two-factor.utils';

const AUTH_ENDPOINTS_WITHOUT_REFRESH = [
  '/auth/login',
  '/auth/register',
  '/auth/refresh',
  '/auth/external/complete',
  '/auth/external/link',
  '/auth/two-factor/verify'
] as const;

function shouldAttemptTokenRefresh(requestUrl: string): boolean {
  if (requestUrl.includes('/auth/email-change/confirm')) {
    return false;
  }

  return !AUTH_ENDPOINTS_WITHOUT_REFRESH.some((segment) =>
    requestUrl.includes(segment)
  );
}

function getApiErrorMessage(error: HttpErrorResponse): string | null {
  const body = error.error as Partial<Result<unknown>> | null;
  if (!body?.errors?.length) {
    return null;
  }

  return body.errors.filter((message) => message.trim().length > 0).join(' ');
}

export const authTokenInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.token();

  const authorizedRequest = token
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      })
    : req;

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      const status =
        error && typeof error === 'object' && 'status' in error
          ? (error as { status: number }).status
          : undefined;

      if (error instanceof HttpErrorResponse && status === 403) {
        const apiMessage = getApiErrorMessage(error);
        if (apiMessage && isPasswordExpiredApiError(new Error(apiMessage))) {
          authService.markPasswordChangeRequired();
        } else if (
          apiMessage &&
          isTwoFactorSetupRequiredApiError(new Error(apiMessage))
        ) {
          authService.markTwoFactorSetupRequired();
        } else if (
          apiMessage &&
          isPasskeySetupRequiredApiError(new Error(apiMessage))
        ) {
          authService.markPasskeySetupRequired();
        }
      }

      if (status !== 401 || !shouldAttemptTokenRefresh(req.url)) {
        return throwError(() => error);
      }

      return authService.tryRefreshAndRetry(req, next);
    })
  );
};
