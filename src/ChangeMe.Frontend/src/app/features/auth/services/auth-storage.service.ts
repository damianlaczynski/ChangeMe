import { Injectable } from '@angular/core';
import { AuthResponse } from '../models/auth.model';

const AUTH_STORAGE_KEY = 'auth_session';

@Injectable({
  providedIn: 'root'
})
export class AuthStorageService {
  getSession(): AuthResponse | null {
    const rawValue =
      localStorage.getItem(AUTH_STORAGE_KEY) ??
      sessionStorage.getItem(AUTH_STORAGE_KEY);

    if (!rawValue) {
      return null;
    }

    try {
      const parsed = JSON.parse(rawValue) as AuthResponse;
      return {
        ...parsed,
        passwordChangeRequired: parsed.passwordChangeRequired === true,
        passwordChangeStrict: parsed.passwordChangeStrict === true,
        passwordExpiresAtUtc: parsed.passwordExpiresAtUtc ?? null
      };
    } catch {
      this.clearSession();
      return null;
    }
  }

  setSession(session: AuthResponse): void {
    this.clearSession();
    const storage = session.isPersistent ? localStorage : sessionStorage;
    storage.setItem(AUTH_STORAGE_KEY, JSON.stringify(session));
  }

  clearSession(): void {
    localStorage.removeItem(AUTH_STORAGE_KEY);
    sessionStorage.removeItem(AUTH_STORAGE_KEY);
  }
}
