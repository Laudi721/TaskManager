import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, tap } from 'rxjs';
import { RealtimeService } from '../../realtime/realtime.service';

export interface User {
  id: number;
  login: string;
  name: string;
  isArchive: boolean;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly realtime = inject(RealtimeService);
  private readonly apiUrl = '/api/users';

  private readonly _users = signal<User[] | null>(null);
  readonly users = this._users.asReadonly();

  // UserDto = { id, login, name, isArchive, roles[] } — roles[] (lista nazw) zależy też od Roles,
  // bo zmiana nazwy roli pociągnie zmianę etykiet w chip-ach.
  private readonly dependsOn = ['users', 'roles'];

  // Czy widok korzystający z tego cache jest aktualnie zamontowany.
  // Sterowane przez komponenty przez setActive() w ngOnInit/ngOnDestroy.
  private active = false;

  // Cache jest oznaczony jako nieaktualny, ale refetch jeszcze nie nastąpił
  // (bo brak aktywnego widoku). Następne setActive(true) wymusi pobranie.
  private stale = false;

  constructor() {
    this.realtime.on(this.dependsOn, () => this.handleChange());
    this.realtime.reconnected$.subscribe(() => this.handleChange());
  }

  setActive(value: boolean): void {
    this.active = value;
    if (value && this.stale && this._users() !== null) {
      this.refetch();
    }
  }

  load(force = false): Observable<User[]> {
    const cached = this._users();
    if (!force && cached && !this.stale) {
      return of(cached);
    }
    return this.http.get<User[]>(this.apiUrl).pipe(
      tap(data => {
        this._users.set(data);
        this.stale = false;
      })
    );
  }

  create(payload: UserCreatePayload): Observable<User> {
    return this.http.post<User>(this.apiUrl, payload).pipe(
      tap(() => this.handleChange())
    );
  }

  toggleArchive(id: number): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/archive`, {}).pipe(
      tap(() => this.handleChange())
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.handleChange())
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
      error: () => this._users.set(null)
    });
  }
}

export interface UserCreatePayload {
  login: string;
  name: string;
  password: string;
  roleIds: number[];
}
