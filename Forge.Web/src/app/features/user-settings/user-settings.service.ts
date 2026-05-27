import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { AuthService, UserPreferences } from '../../auth/auth.service';

/**
 * Single entry point for persisting per-user settings. Wszystkie wybory z ekranu
 * "Ustawienia użytkownika" (motyw + cokolwiek dojdzie później) lecą jednym PUT-em.
 *
 * Backend (UserSettingsController) przyjmuje UserPreferencesDto i nadpisuje to,
 * co zostało wysłane. Pole pominięte w payloadzie zostaje na backendzie jak było —
 * ale obecnie wysyłamy wszystkie pola, bo wszystkie są zarządzane z tego ekranu.
 */
@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly apiUrl = '/api/user-settings';

  save(preferences: UserPreferences): Observable<void> {
    return this.http.put<void>(this.apiUrl, preferences).pipe(
      // Synchronize the cached session so other parts of the app (and the next page reload)
      // see the new values without an additional fetch.
      tap(() => this.auth.updatePreferences(preferences))
    );
  }
}
