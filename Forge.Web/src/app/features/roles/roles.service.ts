import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { RealtimeService } from '../../realtime/realtime.service';

export interface Role {
  id: number;
  name: string;
  usersCount: number;
  isArchive: boolean;
}

@Injectable({ providedIn: 'root' })
export class RolesService {
  private readonly http = inject(HttpClient);
  private readonly realtime = inject(RealtimeService);
  private readonly apiUrl = '/api/roles';

  private readonly _roles = signal<Role[] | null>(null);
  readonly roles = this._roles.asReadonly();

  // RoleDto = { id, name, usersCount } — usersCount jest projekcją po Users,
  // więc dowolna mutacja w 'users' też powoduje że ten cache jest stale.
  private readonly dependsOn = ['roles', 'users'];

  private active = false;
  private stale = false;

  constructor() {
    this.realtime.on(this.dependsOn, () => this.handleChange());
    this.realtime.reconnected$.subscribe(() => this.handleChange());
  }

  setActive(value: boolean): void {
    this.active = value;
    if (value && this.stale && this._roles() !== null) {
      this.refetch();
    }
  }

  load(force = false): Observable<Role[]> {
    const cached = this._roles();
    if (!force && cached && !this.stale) {
      return of(cached);
    }
    return this.http.get<Role[]>(this.apiUrl).pipe(
      tap(data => {
        this._roles.set(data);
        this.stale = false;
      })
    );
  }

  private handleChange(): void {
    this.stale = true;
    if (this.active) {
      this.refetch();
    }
  }

  private refetch(): void {
    this.load(true).subscribe({
      error: () => this._roles.set(null)
    });
  }

  create(payload: RoleCreatePayload): Observable<void> {
    return this.http.post<void>(this.apiUrl, payload).pipe(
      tap(() => this.handleChange())
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.handleChange())
    );
  }

  toggleArchive(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/archive/${id}`, {}).pipe(
      tap(() => this.handleChange())
    );
  }
}

export interface RoleCreatePayload {
  name: string;
}
