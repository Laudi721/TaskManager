import { Injectable, effect, inject, signal } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export interface ThemeOption {
  id: ThemeId;
  label: string;
  swatch: string;
}

export type ThemeId = 'azure' | 'green' | 'magenta' | 'orange' | 'violet';

const STORAGE_KEY = 'forge.theme';
const DEFAULT_THEME: ThemeId = 'azure';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly auth = inject(AuthService);

  readonly available: ThemeOption[] = [
    { id: 'azure',   label: 'Lazurowy',     swatch: '#0061A4' },
    { id: 'green',   label: 'Zielony',      swatch: '#4F6F52' },
    { id: 'magenta', label: 'Magenta',      swatch: '#8E4585' },
    { id: 'orange',  label: 'Pomarańczowy', swatch: '#9E4A20' },
    { id: 'violet',  label: 'Fioletowy',    swatch: '#6750A4' }
  ];

  private readonly _current = signal<ThemeId>(this.readFromStorage());
  readonly current = this._current.asReadonly();

  constructor() {
    // Apply the cached theme immediately to avoid a flash of the default palette.
    this.apply(this._current());

    // React to session changes: when a user logs in, override with their server-stored
    // preference; when they log out, fall back to default so the next person doesn't
    // inherit it.
    effect(() => {
      const prefs = this.auth.preferences();
      const loggedIn = this.auth.isLoggedIn();

      if (loggedIn) {
        const serverTheme = prefs?.themePreference;
        if (serverTheme && this.isKnown(serverTheme) && serverTheme !== this._current()) {
          this._current.set(serverTheme);
          localStorage.setItem(STORAGE_KEY, serverTheme);
          this.apply(serverTheme);
        }
      } else {
        if (this._current() !== DEFAULT_THEME) {
          this._current.set(DEFAULT_THEME);
          localStorage.removeItem(STORAGE_KEY);
          this.apply(DEFAULT_THEME);
        }
      }
    });
  }

  /**
   * Applies the theme locally (CSS class + signal + warm-cache w localStorage). NIE zapisuje
   * w backendzie — persystencja idzie przez UserSettingsService.save() z ekranu ustawień.
   * Pozwala to traktować ten serwis jako "live preview" w formularzu, a commit zrobić raz
   * przy kliknięciu "Zapisz".
   */
  set(themeId: ThemeId): void {
    if (!this.isKnown(themeId)) return;
    if (themeId === this._current()) return;

    this._current.set(themeId);
    localStorage.setItem(STORAGE_KEY, themeId);
    this.apply(themeId);
  }

  private apply(themeId: ThemeId): void {
    const body = document.body;
    for (const t of this.available) {
      body.classList.remove(`theme-${t.id}`);
    }
    // Default palette lives on <html> without a class — only override when not default.
    if (themeId !== DEFAULT_THEME) {
      body.classList.add(`theme-${themeId}`);
    }
  }

  private readFromStorage(): ThemeId {
    const stored = localStorage.getItem(STORAGE_KEY) as ThemeId | null;
    return stored && this.isKnown(stored) ? stored : DEFAULT_THEME;
  }

  private isKnown(themeId: string): themeId is ThemeId {
    return this.available.some(t => t.id === themeId);
  }
}
