import { Injectable, inject, signal, effect } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';
import { MenuItem, MenuService } from '../menu/menu.service';

export interface OpenTab {
  route: string;
  label: string;
}

const IGNORED_ROUTES = new Set(['/', '/home', '/login']);

@Injectable({ providedIn: 'root' })
export class OpenTabsService {
  private readonly router = inject(Router);
  private readonly menuService = inject(MenuService);

  private readonly _tabs = signal<OpenTab[]>([]);
  readonly tabs = this._tabs.asReadonly();

  private readonly _activeRoute = signal<string | null>(null);
  readonly activeRoute = this._activeRoute.asReadonly();

  constructor() {
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe(e => this.onNavigated(e.urlAfterRedirects));

    effect(() => {
      const items = this.menuService.items();
      if (items.length === 0) return;
      this._tabs.update(tabs => tabs.map(tab => {
        if (tab.label !== tab.route) return tab;
        const label = this.findLabel(tab.route, items);
        return label ? { ...tab, label } : tab;
      }));
    });
  }

  clearAll(): void {
    this._tabs.set([]);
    this._activeRoute.set(null);
  }

  close(route: string): void {
    const tabs = this._tabs();
    const idx = tabs.findIndex(t => t.route === route);
    if (idx < 0) return;

    const remaining = tabs.filter(t => t.route !== route);
    this._tabs.set(remaining);

    if (this._activeRoute() === route) {
      const next = remaining[idx] ?? remaining[idx - 1];
      this.router.navigateByUrl(next ? next.route : '/home');
    }
  }

  private onNavigated(url: string): void {
    const route = url.split('?')[0].split('#')[0];

    if (IGNORED_ROUTES.has(route)) {
      this._activeRoute.set(null);
      return;
    }

    if (!this._tabs().some(t => t.route === route)) {
      const label = this.findLabel(route, this.menuService.items()) ?? route;
      this._tabs.update(tabs => [...tabs, { route, label }]);
    }

    this._activeRoute.set(route);
  }

  private findLabel(route: string, items: MenuItem[]): string | null {
    const stack: MenuItem[] = [...items];
    while (stack.length > 0) {
      const item = stack.shift()!;
      if (item.route === route) {
        return item.menuName;
      }
      if (item.children?.length) {
        stack.push(...item.children);
      }
    }
    return null;
  }
}
