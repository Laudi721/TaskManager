import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export interface LoginRequest {
  login: string;
  password: string;
}

export interface LoginResponse {
  success: boolean;
  message?: string;
  userId?: number;
  login?: string;
  name?: string;
  token?: string;
  expiresAtUtc?: string;
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
}

const STORAGE_KEY = 'tm.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/auth';

  private readonly _session = signal<StoredSession | null>(this.readFromStorage());

  readonly currentUser = computed<CurrentUser | null>(() => this._session()?.user ?? null);
  readonly isLoggedIn = computed(() => this._session() !== null);

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
            expiresAtUtc: response.expiresAtUtc
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
      return session;
    } catch {
      return null;
    }
  }
}
