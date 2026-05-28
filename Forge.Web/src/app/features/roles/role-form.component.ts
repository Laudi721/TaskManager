import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RolesService } from './roles.service';
import { OpenTabsService } from '../../open-tabs/open-tabs.service';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ],
  templateUrl: './role-form.component.html',
  styleUrl: './role-form.component.scss'
})
export class RoleFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly rolesService = inject(RolesService);
  private readonly openTabs = inject(OpenTabsService);
  private readonly snack = inject(MatSnackBar);

  private readonly formRoute = '/roles/new';

  readonly submitting = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(2)]]
  });

  submit(): void {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.error.set(null);

    this.rolesService.create(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.snack.open('Rola utworzona.', 'OK', { duration: 2500 });
        this.closeFormTab();
      },
      error: err => {
        this.submitting.set(false);
        this.applyServerErrors(err);
      }
    });
  }

  cancel(): void {
    this.closeFormTab();
  }

  private closeFormTab(): void {
    this.router.navigateByUrl('/roles').then(() => {
      this.openTabs.close(this.formRoute);
    });
  }

  private applyServerErrors(err: any): void {
    const validationErrors = err?.error?.errors as Array<{ field: string; message: string }> | undefined;

    if (validationErrors?.length) {
      for (const e of validationErrors) {
        const control = this.form.get(this.toCamel(e.field));
        if (control) {
          control.setErrors({ server: e.message });
          control.markAsTouched();
        }
      }
      this.error.set(err?.error?.message ?? 'Walidacja nie powiodła się.');
      return;
    }

    this.error.set(err?.error?.message ?? `Nie udało się utworzyć roli (${err?.status ?? '?'}).`);
  }

  private toCamel(field: string): string {
    return field ? field.charAt(0).toLowerCase() + field.slice(1) : field;
  }
}
