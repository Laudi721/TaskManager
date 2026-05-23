import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../../auth/auth.service';
import { User, UsersService } from './users.service';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatSnackBarModule
  ],
  templateUrl: './users.component.html',
  styleUrl: './users.component.scss'
})
export class UsersComponent implements OnInit, OnDestroy {
  private readonly usersService = inject(UsersService);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly snack = inject(MatSnackBar);

  readonly users = computed(() => this.usersService.users() ?? []);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly columns = ['id', 'login', 'name', 'roles', 'isArchive', 'actions'];
  private readonly currentUserId = computed(() => this.auth.currentUser()?.userId ?? null);

  ngOnInit(): void {
    this.usersService.setActive(true);
    if (this.usersService.users() === null) {
      this.fetch(false);
    }
  }

  ngOnDestroy(): void {
    this.usersService.setActive(false);
  }

  refresh(): void {
    this.fetch(true);
  }

  addUser(): void {
    this.router.navigateByUrl('/users/new');
  }

  isSelf(user: User): boolean {
    return user.id === this.currentUserId();
  }

  toggleArchive(user: User): void {
    if (this.isSelf(user)) return;
    this.usersService.toggleArchive(user.id).subscribe({
      next: () => this.snack.open(
        user.isArchive ? 'Użytkownik przywrócony.' : 'Użytkownik zarchiwizowany.',
        'OK',
        { duration: 2500 }
      ),
      error: err => this.showError(err, 'Nie udało się zmienić statusu')
    });
  }

  deleteUser(user: User): void {
    if (this.isSelf(user)) return;
    const confirmed = window.confirm(`Czy na pewno usunąć użytkownika "${user.login}"? Operacja jest nieodwracalna.`);
    if (!confirmed) return;

    this.usersService.delete(user.id).subscribe({
      next: () => this.snack.open('Użytkownik usunięty.', 'OK', { duration: 2500 }),
      error: err => this.showError(err, 'Nie udało się usunąć')
    });
  }

  private fetch(force: boolean): void {
    this.loading.set(true);
    this.error.set(null);
    this.usersService.load(force).subscribe({
      next: () => this.loading.set(false),
      error: err => {
        this.error.set(`Nie udało się pobrać użytkowników (${err.status ?? '?'}).`);
        this.loading.set(false);
      }
    });
  }

  private showError(err: any, fallback: string): void {
    const msg = err?.error?.message ?? `${fallback} (${err?.status ?? '?'}).`;
    this.snack.open(msg, 'OK', { duration: 4000 });
  }
}
