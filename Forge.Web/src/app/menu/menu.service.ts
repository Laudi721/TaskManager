import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, of, shareReplay, tap } from 'rxjs';

export type MenuItemType = 'Group' | 'View';

export interface MenuItem {
  menuName: string;
  type: MenuItemType;
  icon?: string;
  controller?: string;
  action?: string;
  route?: string;
  children: MenuItem[];
}

@Injectable({ providedIn: 'root' })
export class MenuService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/menu';

  private readonly _items = signal<MenuItem[]>([]);
  readonly items = this._items.asReadonly();

  private loaded = false;
  private inFlight$: Observable<MenuItem[]> | null = null;

  load(force = false): Observable<MenuItem[]> {
    if (!force && this.loaded) {
      return of(this._items());
    }
    if (this.inFlight$) {
      return this.inFlight$;
    }
    this.inFlight$ = this.http.get<MenuItem[]>(this.apiUrl).pipe(
      tap(data => {
        this._items.set(data ?? []);
        this.loaded = true;
        this.inFlight$ = null;
      }),
      shareReplay(1)
    );
    return this.inFlight$;
  }

  invalidate(): void {
    this.loaded = false;
    this.inFlight$ = null;
  }
}
