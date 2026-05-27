import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatRadioModule } from '@angular/material/radio';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Subscription } from 'rxjs';
import { ThemeId, ThemeService } from '../../theme/theme.service';
import { UserSettingsService } from './user-settings.service';

@Component({
  selector: 'app-user-settings',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatRadioModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './user-settings.component.html',
  styleUrl: './user-settings.component.scss'
})
export class UserSettingsComponent implements OnInit, OnDestroy {
  private readonly fb = inject(FormBuilder);
  private readonly themeService = inject(ThemeService);
  private readonly userSettings = inject(UserSettingsService);
  private readonly snack = inject(MatSnackBar);

  readonly available = this.themeService.available;
  readonly saving = signal(false);
  readonly dirty = signal(false);

  readonly form = this.fb.nonNullable.group({
    themePreference: this.themeService.current() as ThemeId
  });

  // Snapshot of saved values when component mounted — used to revert preview if user
  // leaves without saving.
  private originalTheme!: ThemeId;
  private formSub?: Subscription;

  ngOnInit(): void {
    this.originalTheme = this.themeService.current();
    this.form.reset({ themePreference: this.originalTheme });

    this.formSub = this.form.controls.themePreference.valueChanges.subscribe(themeId => {
      // Live preview — applies CSS class so the user sees what they're picking. NIE zapisuje
      // do bazy; to robi przycisk "Zapisz".
      if (themeId && themeId !== this.themeService.current()) {
        this.themeService.set(themeId);
      }
      this.dirty.set(this.hasUnsavedChanges());
    });
  }

  ngOnDestroy(): void {
    this.formSub?.unsubscribe();
    // User opuścił ekran bez kliknięcia "Zapisz" — cofnij preview do ostatnio zapisanego motywu.
    if (this.themeService.current() !== this.originalTheme) {
      this.themeService.set(this.originalTheme);
    }
  }

  save(): void {
    if (this.saving() || !this.dirty()) return;

    this.saving.set(true);
    const payload = this.form.getRawValue();

    this.userSettings.save(payload).subscribe({
      next: () => {
        this.saving.set(false);
        this.originalTheme = payload.themePreference;
        this.dirty.set(false);
        this.snack.open('Zapisano ustawienia.', 'OK', { duration: 2500 });
      },
      error: err => {
        this.saving.set(false);
        const msg = err?.error?.message ?? `Nie udało się zapisać (${err?.status ?? '?'}).`;
        this.snack.open(msg, 'OK', { duration: 4000 });
      }
    });
  }

  cancel(): void {
    this.form.reset({ themePreference: this.originalTheme });
    this.themeService.set(this.originalTheme);
    this.dirty.set(false);
  }

  private hasUnsavedChanges(): boolean {
    const v = this.form.getRawValue();
    return v.themePreference !== this.originalTheme;
  }
}
