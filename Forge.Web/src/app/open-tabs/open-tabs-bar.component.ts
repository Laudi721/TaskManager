import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { OpenTabsService } from './open-tabs.service';

@Component({
  selector: 'app-open-tabs-bar',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './open-tabs-bar.component.html',
  styleUrl: './open-tabs-bar.component.scss'
})
export class OpenTabsBarComponent {
  private readonly service = inject(OpenTabsService);

  readonly tabs = this.service.tabs;
  readonly activeRoute = this.service.activeRoute;

  close(event: Event, route: string): void {
    event.preventDefault();
    event.stopPropagation();
    this.service.close(route);
  }
}
