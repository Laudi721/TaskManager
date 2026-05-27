import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  login: string;
  password: string;
}

export interface UserPreferences {
  themePreference?: string | null;
}

export interface LoginResponse {
  success: boolean;
  message?: string;
  userId?: number;
  login?: string;
  name?: string;
  token?: string;
  expiresAtUtc?: string;
  preferences?: UserPreferences;
}

export interface CurrentUser {
  userId: number;
  login: string;
  name?: string;
}

interface StoredSession {
  user: CurrentUser;
  token: string;
  expiresAtUtc?: string;
  preferences: UserPreferences;
}

const STORAGE_KEY = 'tm.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/auth';

  private readonly _session = signal<StoredSession | null>(this.readFromStorage());

  readonly currentUser = computed<CurrentUser | null>(() => this._session()?.user ?? null);
  readonly isLoggedIn = computed(() => this._session() !== null);
  readonly preferences = computed<UserPreferences | null>(() => this._session()?.preferences ?? null);

  getToken(): string | null {
    const session = this._session();
    if (!session) {
      return null;
    }
    if (session.expiresAtUtc && new Date(session.expiresAtUtc).getTime() < Date.now()) {
      this.logout();
      return null;
    }
    return session.token;
  }

  login(payload: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, payload).pipe(
      tap(response => {
        if (response.success && response.token && response.userId != null && response.login) {
          const session: StoredSession = {
            user: {
              userId: response.userId,
              login: response.login,
              name: response.name
            },
            token: response.token,
            expiresAtUtc: response.expiresAtUtc,
            preferences: response.preferences ?? {}
          };
          this._session.set(session);
          localStorage.setItem(STORAGE_KEY, JSON.stringify(session));
        }
      })
    );
  }

  logout(): void {
    this._session.set(null);
    localStorage.removeItem(STORAGE_KEY);
  }

  /**
   * Updates the in-memory + persisted session with new preferences without re-issuing a token.
   * Used by feature services (e.g. ThemeService) after they successfully save a change.
   */
  updatePreferences(patch: Partial<UserPreferences>): void {
    const session = this._session();
    if (!session) return;
    const next: StoredSession = {
      ...session,
      preferences: { ...session.preferences, ...patch }
    };
    this._session.set(next);
    localStorage.setItem(STORAGE_KEY, JSON.stringify(next));
  }

  private readFromStorage(): StoredSession | null {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return null;
      }
      const session = JSON.parse(raw) as StoredSession;
      if (session.expiresAtUtc && new Date(session.expiresAtUtc).getTime() < Date.now()) {
        localStorage.removeItem(STORAGE_KEY);
        return null;
      }
      // Backward compat: pre-preferences sessions don't have the field.
      if (!session.preferences) {
        session.preferences = {};
      }
      return session;
    } catch {
      return null;
    }
  }
}
