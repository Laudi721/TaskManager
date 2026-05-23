import { Component, OnInit, inject, signal } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Role, RolesService } from './roles.service';

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
export class RolesComponent implements OnInit {
  private readonly rolesService = inject(RolesService);

  readonly roles = signal<Role[]>([]);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);
  readonly columns = ['id', 'name', 'usersCount'];

  ngOnInit(): void {
    this.loading.set(true);
    this.rolesService.getAll().subscribe({
      next: data => {
        this.roles.set(data);
        this.loading.set(false);
      },
      error: err => {
        this.error.set(`Nie udało się pobrać ról (${err.status ?? '?'}).`);
        this.loading.set(false);
      }
    });
  }

  addRole(): void {
    // TODO: otwarcie dialogu / formularza tworzenia roli
  }
}
