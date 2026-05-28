import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Role, RolesService } from './roles.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule,
    MatSnackBarModule
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit, OnDestroy {
  private readonly rolesService = inject(RolesService);
  private readonly router = inject(Router);
  private readonly snack = inject(MatSnackBar);

  readonly roles = computed(() => this.rolesService.roles() ?? []);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly columns = ['id', 'name', 'usersCount', 'isArchive', 'actions'];

  ngOnInit(): void {
    this.rolesService.setActive(true);
    if (this.rolesService.roles() === null) {
      this.fetch(false);
    }
  }

  ngOnDestroy(): void {
    this.rolesService.setActive(false);
  }

  refresh(): void {
    this.fetch(true);
  }

  addRole(): void {
    this.router.navigateByUrl('/roles/new');
  }

  toggleArchive(role: Role): void {
    this.rolesService.toggleArchive(role.id).subscribe({
      next: () => this.snack.open(
        role.isArchive ? 'Rola przywrócona.' : 'Rola zarchiwizowana.',
        'OK',
        { duration: 2500 }
      ),
      error: err => this.showError(err, 'Nie udało się zmienić statusu roli')
    });
  }

  deleteRole(role: Role): void {
    const confirmed = window.confirm(`Czy na pewno usunąć rolę "${role.name}"? Operacja jest nieodwracalna.`);

    if (!confirmed) return;

    this.rolesService.delete(role.id).subscribe({
      next: () => this.snack.open('Rola usunięta.', 'OK', { duration: 2500 }),
      error: err => this.showError(err, 'Nie udało się usunąć roli')
    });
  }

  private fetch(force: boolean): void {
    this.loading.set(true);
    this.error.set(null);
    this.rolesService.load(force).subscribe({
      next: () => this.loading.set(false),
      error: err => {
        this.error.set(`Nie udało się pobrać ról (${err.status ?? '?'}).`);
        this.loading.set(false);
      }
    });
  }

  private showError(err: any, fallback: string): void {
    const msg = err?.error?.message ?? `${fallback} (${err?.status ?? '?'}).`;
    this.snack.open(msg, 'OK', { duration: 4000 });
  }
}
