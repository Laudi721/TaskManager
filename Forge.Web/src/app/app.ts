import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './theme/theme.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  // Eager instantiation so the saved theme is applied to body class before any view renders.
  private readonly theme = inject(ThemeService);

  protected readonly title = signal('TaskManager.Web');
}
