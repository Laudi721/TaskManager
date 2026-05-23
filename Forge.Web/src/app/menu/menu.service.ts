import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';

export type MenuItemType = 'Group' | 'View';

export interface MenuItem {
  menuName: string;
  type: MenuItemType;
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

  load(): Observable<MenuItem[]> {
    return this.http.get<MenuItem[]>(this.apiUrl).pipe(
      tap(items => this._items.set(items ?? []))
    );
  }
}
