import {
  HttpClient,
  HttpErrorResponse,
  HttpHandlerFn,
  HttpRequest
} from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '@environments/environment';
import { Result } from '@shared/api/models/api-response.model';
import { ApiService } from '@shared/api/services/api.service';
import { Observable, firstValueFrom, from, of, throwError } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import {
  AuthResponse,
  AuthSettings,
  BeginExternalSignInResponse,
  BeginTwoFactorSetupRequest,
  BeginTwoFactorSetupResponse,
  ChangePasswordRequest,
  CompleteExternalSignInRequest,
  ConfirmTwoFactorSetupRequest,
  DisableTwoFactorRequest,
  EmailVerificationAck,
  ExternalSignInResponse,
  LinkExternalAccountRequest,
  LoginRequest,
  LoginResponse,
  MyAccountDto,
  RegisterRequest,
  RegisterResponse,
  RequiredChangePasswordRequest,
  SetPasswordRequest,
  TwoFactorSetupCompletedResponse,
  TwoFactorStepUpRequest,
  UnlinkExternalAccountRequest,
  UpdateMyAccountRequest,
  UserSessionDto,
  UserSessionSearchParameters,
  VerifyTwoFactorRequest
} from '../models/auth.model';
import { AcceptInvitationRequest, InvitationPreview } from '../models/invitation.model';
import {
  PasswordResetAck,
  PasswordResetPreview,
  RequestPasswordResetRequest,
  ResetPasswordRequest
} from '../models/password-reset.model';
import { AuthConstraints } from '../utils/auth.utils';
import {
  clearExternalAccountFlow,
  readExternalAccountFlow,
  storeExternalAccountFlow
} from '../utils/external-account-flow.storage';
import { storeExternalLinkRequired } from '../utils/external-link.storage';
import { hasPendingSetPassword } from '../utils/pending-set-password.storage';
import {
  clearTwoFactorChallenge,
  readTwoFactorChallenge,
  storeTwoFactorChallenge
} from '../utils/two-factor-challenge.storage';
import { AuthStorageService } from './auth-storage.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiService = inject(ApiService);
  private readonly http = inject(HttpClient);
  private readonly authStorageService = inject(AuthStorageService);
  private readonly router = inject(Router);

  private readonly baseUrl = environment.apiUrl + '/';
  private readonly session = signal<AuthResponse | null>(
    this.authStorageService.getSession()
  );
  private renewalTimerId: ReturnType<typeof setTimeout> | null = null;
  private refreshInFlight: Promise<AuthResponse> | null = null;

  readonly currentSession = this.session.asReadonly();
  readonly isAuthenticated = computed(() => {
    const current = this.session();
    return current !== null && this.getAccessTokenLifetimeMs(current) > 0;
  });
  readonly passwordChangeRequired = computed(
    () => this.session()?.passwordChangeRequired === true
  );
  readonly requiresPasswordChangeScreen = computed(
    () =>
      this.session()?.passwordChangeRequired === true &&
      this.session()?.passwordChangeStrict === true
  );
  readonly twoFactorSetupRequired = computed(
    () => this.session()?.twoFactorSetupRequired === true
  );
  readonly requiresTwoFactorSetupScreen = computed(
    () =>
      this.session()?.twoFactorSetupRequired === true &&
      this.session()?.twoFactorSetupStrict === true
  );
  readonly token = computed(() => {
    const current = this.session();
    if (!current || this.getAccessTokenLifetimeMs(current) <= 0) {
      return null;
    }

    return current.token;
  });
  readonly permissions = computed(() => this.session()?.permissions ?? []);
  readonly currentUser = computed(() => {
    const session = this.session();
    if (!session) {
      return null;
    }

    return {
      id: session.userId,
      firstName: session.firstName,
      lastName: session.lastName,
      email: session.email
    };
  });

  constructor() {
    const storedSession = this.session();
    if (!storedSession) {
      return;
    }

    if (this.getAccessTokenLifetimeMs(storedSession) <= 0) {
      void this.refreshStoredSession().then((session) => {
        if (!session) {
          this.clearLocalSession();
        }
      });
      return;
    }

    this.scheduleRenewal(storedSession);
  }

  hasPermission(permissionCode: string): boolean {
    return this.permissions().includes(permissionCode);
  }

  login(request: LoginRequest) {
    return this.apiService
      .post<LoginResponse>('auth/login', request)
      .pipe(tap((response) => this.applyPasswordSignInResponse(response)));
  }

  beginExternalSignIn(providerKey: string) {
    return this.apiService.post<BeginExternalSignInResponse>(
      `auth/external/${encodeURIComponent(providerKey)}/begin`,
      {}
    );
  }

  completeExternalSignIn(request: CompleteExternalSignInRequest) {
    return this.apiService.post<ExternalSignInResponse>(
      'auth/external/complete',
      request
    );
  }

  linkExternalAccount(request: LinkExternalAccountRequest) {
    return this.apiService.post<ExternalSignInResponse>('auth/external/link', request);
  }

  beginExternalAccountLink(providerKey: string) {
    storeExternalAccountFlow('account-link');
    return this.beginExternalSignIn(providerKey);
  }

  beginExternalProviderStepUp(providerKey: string) {
    storeExternalAccountFlow('account-step-up');
    return this.apiService.post<BeginExternalSignInResponse>(
      `auth/external/${encodeURIComponent(providerKey)}/step-up/begin`,
      {}
    );
  }

  unlinkExternalAccount(providerKey: string, request: UnlinkExternalAccountRequest) {
    return this.apiService.post<boolean>(
      `auth/external/${encodeURIComponent(providerKey)}/unlink`,
      request
    );
  }

  setPassword(request: SetPasswordRequest) {
    return this.apiService.post<boolean>('auth/set-password', request);
  }

  continueAfterExternalSignIn(
    response: ExternalSignInResponse,
    returnUrl = '/issues'
  ): void {
    const accountFlow = readExternalAccountFlow();
    if (accountFlow) {
      clearExternalAccountFlow();
      if (response.accountLinkCompleted) {
        void this.router.navigate(['/account'], {
          queryParams: { externalLinked: '1' }
        });
        return;
      }
      if (response.externalStepUpCompleted) {
        void this.router.navigate(
          [hasPendingSetPassword() ? '/account/set-password' : '/account'],
          { queryParams: { externalStepUp: '1' } }
        );
        return;
      }

      const errorTarget = this.isAuthenticated() ? '/account' : '/login';
      void this.router.navigate([errorTarget], {
        queryParams: { externalSignInError: '1' }
      });
      return;
    }

    if (response.linkAccountRequired) {
      storeExternalLinkRequired(response.linkAccountRequired);
      void this.router.navigateByUrl('/link-external-account');
      return;
    }

    this.applyExternalSignInResponse(response);

    if (this.passwordChangeRequired()) {
      this.enablePasswordChangeScreen();
      void this.router.navigateByUrl('/required-password-change');
      return;
    }

    if (readTwoFactorChallenge()) {
      void this.router.navigateByUrl('/two-factor-verification');
      return;
    }

    if (this.twoFactorSetupRequired()) {
      this.enableTwoFactorSetupScreen();
      void this.router.navigateByUrl('/required-two-factor-setup');
      return;
    }

    void this.router.navigateByUrl(returnUrl);
  }

  private applyPasswordSignInResponse(response: LoginResponse): void {
    if (response.authSession) {
      this.setSession({
        ...response.authSession,
        passwordChangeStrict: false,
        twoFactorSetupStrict: response.authSession.twoFactorSetupRequired === true
      });
      return;
    }

    if (response.twoFactorChallenge) {
      storeTwoFactorChallenge({
        challengeId: response.twoFactorChallenge.challengeId
      });
    }
  }

  private applyExternalSignInResponse(response: ExternalSignInResponse): void {
    if (response.authSession) {
      this.setSession({
        ...response.authSession,
        passwordChangeStrict: false,
        twoFactorSetupStrict: response.authSession.twoFactorSetupRequired === true
      });
      return;
    }

    if (response.twoFactorChallenge) {
      storeTwoFactorChallenge({
        challengeId: response.twoFactorChallenge.challengeId
      });
    }
  }

  verifyTwoFactor(request: VerifyTwoFactorRequest) {
    return this.apiService.post<AuthResponse>('auth/two-factor/verify', request).pipe(
      tap((session) => {
        clearTwoFactorChallenge();
        this.setSession({
          ...session,
          passwordChangeStrict: false,
          twoFactorSetupStrict: session.twoFactorSetupRequired === true
        });
      })
    );
  }

  beginTwoFactorSetup(request: BeginTwoFactorSetupRequest) {
    return this.apiService.post<BeginTwoFactorSetupResponse>(
      'auth/two-factor/setup/begin',
      request
    );
  }

  confirmTwoFactorSetup(request: ConfirmTwoFactorSetupRequest) {
    return this.apiService.post<TwoFactorSetupCompletedResponse>(
      'auth/two-factor/setup/confirm',
      request
    );
  }

  disableTwoFactor(request: DisableTwoFactorRequest) {
    return this.apiService.post<boolean>('auth/two-factor/disable', request);
  }

  regenerateRecoveryCodes(request: TwoFactorStepUpRequest) {
    return this.apiService.post<TwoFactorSetupCompletedResponse>(
      'auth/two-factor/recovery-codes/regenerate',
      request
    );
  }

  getAuthSettings() {
    return this.apiService.get<AuthSettings>('auth/settings');
  }

  getInvitationPreview(token: string) {
    return this.apiService.get<InvitationPreview>('auth/invitation', { token });
  }

  acceptInvitation(request: AcceptInvitationRequest) {
    return this.apiService.post<boolean>('auth/accept-invitation', request);
  }

  requestPasswordReset(request: RequestPasswordResetRequest) {
    return this.apiService.post<PasswordResetAck>('auth/forgot-password', request);
  }

  getPasswordResetPreview(token: string) {
    return this.apiService.get<PasswordResetPreview>('auth/password-reset', { token });
  }

  resetPassword(request: ResetPasswordRequest) {
    return this.apiService.post<boolean>('auth/reset-password', request);
  }

  register(request: RegisterRequest) {
    return this.apiService.post<RegisterResponse>('auth/register', request).pipe(
      tap((response) => {
        if (response.authSession) {
          this.setSession({
            ...response.authSession,
            passwordChangeStrict: false,
            twoFactorSetupStrict: false
          });
        }
      })
    );
  }

  requestEmailVerification(email: string) {
    return this.apiService.post<EmailVerificationAck>(
      'auth/request-email-verification',
      {
        email
      }
    );
  }

  verifyEmail(token: string) {
    return this.apiService.post<boolean>('auth/verify-email', { token });
  }

  refreshSession() {
    return from(this.refreshSessionOnce());
  }

  logout() {
    return this.apiService.post<boolean>('auth/logout', {}).pipe(
      catchError(() => of(true)),
      finalize(() => this.clearLocalSession())
    );
  }

  logoutAll() {
    return this.apiService
      .post<boolean>('auth/logout-all', {})
      .pipe(finalize(() => this.clearLocalSession()));
  }

  getMySessions(params: UserSessionSearchParameters) {
    return this.apiService.getPaginated<UserSessionDto, UserSessionSearchParameters>(
      'auth/sessions',
      params
    );
  }

  revokeSession(sessionId: string) {
    return this.apiService.delete<boolean>(`auth/sessions/${sessionId}`);
  }

  getMyAccount() {
    return this.apiService.get<MyAccountDto>('auth/account');
  }

  updateMyAccount(request: UpdateMyAccountRequest) {
    return this.apiService.put<MyAccountDto>('auth/account', request);
  }

  syncProfileToSession(firstName: string, lastName: string): void {
    const current = this.session();
    if (!current) {
      return;
    }

    this.setSession({
      ...current,
      firstName,
      lastName
    });
  }

  changePassword(request: ChangePasswordRequest) {
    return this.apiService.post<boolean>('auth/change-password', request);
  }

  requiredChangePassword(request: RequiredChangePasswordRequest) {
    return this.apiService.post<boolean>('auth/required-change-password', request);
  }

  enablePasswordChangeScreen(): void {
    const current = this.session();
    if (!current) {
      return;
    }

    this.setSession({
      ...current,
      passwordChangeRequired: true,
      passwordChangeStrict: true
    });
  }

  markPasswordChangeRequired(): void {
    const current = this.session();
    if (!current || current.passwordChangeRequired) {
      return;
    }

    this.setSession({
      ...current,
      passwordChangeRequired: true,
      passwordChangeStrict: false
    });
  }

  clearPasswordChangeRequired(): void {
    const current = this.session();
    if (!current?.passwordChangeRequired && !current?.passwordChangeStrict) {
      return;
    }

    this.setSession({
      ...current,
      passwordChangeRequired: false,
      passwordChangeStrict: false
    });
  }

  enableTwoFactorSetupScreen(): void {
    const current = this.session();
    if (!current) {
      return;
    }

    this.setSession({
      ...current,
      twoFactorSetupRequired: true,
      twoFactorSetupStrict: true
    });
  }

  clearTwoFactorSetupRequired(): void {
    const current = this.session();
    if (!current?.twoFactorSetupRequired && !current?.twoFactorSetupStrict) {
      return;
    }

    this.setSession({
      ...current,
      twoFactorSetupRequired: false,
      twoFactorSetupStrict: false
    });
  }

  markTwoFactorSetupRequired(): void {
    const current = this.session();
    if (!current || current.twoFactorSetupRequired) {
      return;
    }

    this.setSession({
      ...current,
      twoFactorSetupRequired: true,
      twoFactorSetupStrict: false
    });
  }

  tryRefreshAndRetry(req: HttpRequest<unknown>, next: HttpHandlerFn) {
    if (req.headers.has('X-Skip-Auth-Refresh')) {
      return next(req);
    }

    return this.refreshSession().pipe(
      switchMap(() => {
        const token = this.token();
        if (!token) {
          return throwError(() => new Error('Session expired.'));
        }

        return next(
          req.clone({
            setHeaders: { Authorization: `Bearer ${token}` }
          })
        );
      }),
      catchError((error: unknown) => {
        this.clearLocalSession();
        void this.router.navigateByUrl('/login');
        return throwError(() => error);
      })
    );
  }

  private postAuth<T>(endpoint: string, body: unknown): Observable<T> {
    return this.http
      .post<Result<T>>(`${this.baseUrl}${endpoint}`, body, {
        headers: { 'X-Skip-Auth-Refresh': 'true' }
      })
      .pipe(
        map((response) => {
          if (response.isSuccess) {
            return response.value as T;
          }

          throw new Error(this.getErrorMessage(response));
        }),
        catchError((error: unknown) => throwError(() => this.toError(error)))
      );
  }

  private refreshSessionOnce(): Promise<AuthResponse> {
    const current = this.session();
    if (!current?.refreshToken) {
      return Promise.reject(new Error('Session expired.'));
    }

    if (this.refreshInFlight) {
      return this.refreshInFlight;
    }

    this.refreshInFlight = firstValueFrom(
      this.postAuth<AuthResponse>('auth/refresh', {
        refreshToken: current.refreshToken
      }).pipe(
        tap((session) =>
          this.setSession({
            ...session,
            passwordChangeStrict: current.passwordChangeStrict,
            twoFactorSetupStrict:
              session.twoFactorSetupRequired === true
                ? true
                : current.twoFactorSetupStrict
          })
        ),
        catchError((error: unknown) => {
          this.clearLocalSession();
          return throwError(() => error);
        })
      )
    )
      .catch((error: unknown) => {
        if (error instanceof Error) {
          throw error;
        }

        throw new Error('Session expired.');
      })
      .finally(() => {
        this.refreshInFlight = null;
      });

    return this.refreshInFlight;
  }

  private async refreshStoredSession(): Promise<AuthResponse | null> {
    try {
      return await this.refreshSessionOnce();
    } catch {
      return null;
    }
  }

  private setSession(session: AuthResponse): void {
    this.session.set(session);
    this.authStorageService.setSession(session);
    this.scheduleRenewal(session);
  }

  clearLocalSession(): void {
    this.clearRenewalTimer();
    this.refreshInFlight = null;
    this.session.set(null);
    this.authStorageService.clearSession();
  }

  private scheduleRenewal(session: AuthResponse): void {
    this.clearRenewalTimer();

    const delay = this.getRenewalDelayMs(session);
    if (delay === null) {
      return;
    }

    this.renewalTimerId = setTimeout(() => {
      void this.refreshSessionOnce().catch(() => {
        this.clearLocalSession();
        void this.router.navigateByUrl('/login');
      });
    }, delay);
  }

  private getRenewalDelayMs(session: AuthResponse): number | null {
    const lifetimeMs = this.getAccessTokenLifetimeMs(session);

    if (lifetimeMs <= AuthConstraints.MIN_RENEWAL_SCHEDULE_MS) {
      return null;
    }

    const leadTimeMs = Math.min(
      AuthConstraints.RENEWAL_LEAD_TIME_MS,
      Math.floor(lifetimeMs / 2)
    );

    return Math.max(AuthConstraints.MIN_RENEWAL_SCHEDULE_MS, lifetimeMs - leadTimeMs);
  }

  private getAccessTokenLifetimeMs(session: AuthResponse | null): number {
    if (!session) {
      return 0;
    }

    return new Date(session.expiresAtUtc).getTime() - Date.now();
  }

  private clearRenewalTimer(): void {
    if (this.renewalTimerId !== null) {
      clearTimeout(this.renewalTimerId);
      this.renewalTimerId = null;
    }
  }

  private getErrorMessage(result: Result<unknown>): string {
    if (result.errors?.length) {
      return result.errors.join(' ');
    }

    return 'Request failed.';
  }

  private toError(error: unknown): Error {
    if (error instanceof HttpErrorResponse) {
      const body = error.error as Partial<Result<unknown>> | null;
      if (body && typeof body.isSuccess === 'boolean' && !body.isSuccess) {
        return new Error(this.getErrorMessage(body as Result<unknown>));
      }
    }

    if (error instanceof Error) {
      return error;
    }

    return new Error('Request failed.');
  }
}
