import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { UsersService } from './users.service';
import { RolesService } from '../roles/roles.service';
import { OpenTabsService } from '../../open-tabs/open-tabs.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.scss'
})
export class UserFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly usersService = inject(UsersService);
  private readonly rolesService = inject(RolesService);
  private readonly openTabs = inject(OpenTabsService);

  private readonly formRoute = '/users/new';

  readonly availableRoles = computed(() => this.rolesService.roles() ?? []);
  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    login: ['', [Validators.required, Validators.minLength(3)]],
    name: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    roleIds: [[] as number[]]
  });

  ngOnInit(): void {
    if (this.rolesService.roles() === null) {
      this.rolesService.load().subscribe();
    }
  }

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.usersService.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.closeFormTab();
      },
      error: err => {
        this.submitting.set(false);
        this.error.set(err?.error?.message ?? `Nie udało się utworzyć użytkownika (${err.status ?? '?'}).`);
      }
    });
  }

  cancel(): void {
    this.closeFormTab();
  }

  private closeFormTab(): void {
    this.router.navigateByUrl('/users').then(() => {
      this.openTabs.close(this.formRoute);
    });
  }
}
