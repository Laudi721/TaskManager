import { Component, Input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MenuItem } from './menu.service';

@Component({
  selector: 'app-menu-item',
  standalone: true,
  imports: [
    MatExpansionModule,
    MatIconModule,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './menu-item.component.html',
  styleUrl: './menu-item.component.scss'
})
export class MenuItemComponent {
  @Input({ required: true }) item!: MenuItem;
}
