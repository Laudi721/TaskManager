import { Component, OnInit, inject } from '@angular/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MenuItemComponent } from './menu-item.component';
import { MenuService } from './menu.service';

@Component({
  selector: 'app-menu',
  standalone: true,
  imports: [MatExpansionModule, MenuItemComponent],
  templateUrl: './menu.component.html',
  styleUrl: './menu.component.scss'
})
export class MenuComponent implements OnInit {
  private readonly menuService = inject(MenuService);

  readonly items = this.menuService.items;

  ngOnInit(): void {
    this.menuService.load().subscribe();
  }
}
