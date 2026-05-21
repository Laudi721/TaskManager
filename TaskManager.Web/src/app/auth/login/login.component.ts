import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { AuthService, LoginResponse } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatIconModule
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly form = this.fb.nonNullable.group({
    login: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  readonly loading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly hidePassword = signal(true);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: (response: LoginResponse) => {
        this.loading.set(false);
        if (response.success) {
          this.successMessage.set(`Zalogowano jako ${response.name ?? response.login}`);
          this.router.navigateByUrl('/home');
        } else {
          this.errorMessage.set(response.message ?? 'Logowanie nie powiodło się.');
        }
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        const serverMsg = (err.error && err.error.message) as string | undefined;
        if (err.status === 0) {
          this.errorMessage.set('Brak połączenia z serwerem.');
        } else if (err.status === 401) {
          this.errorMessage.set(serverMsg ?? 'Nieprawidłowy login lub hasło.');
        } else {
          this.errorMessage.set(serverMsg ?? `Błąd serwera (${err.status}).`);
        }
      }
    });
  }

  togglePassword(): void {
    this.hidePassword.update(v => !v);
  }
}
