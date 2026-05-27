import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RolesService } from './roles.service';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [
    MatTableModule,
    MatProgressSpinnerModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.scss'
})
export class RolesComponent implements OnInit, OnDestroy {
  private readonly rolesService = inject(RolesService);

  readonly roles = computed(() => this.rolesService.roles() ?? []);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly columns = ['id', 'name', 'usersCount'];

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
    // TODO: po utworzeniu wywołać this.rolesService.invalidate() + this.fetch(true)
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
}
