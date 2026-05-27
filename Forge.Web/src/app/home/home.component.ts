import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { AuthService } from '../auth/auth.service';
import { MenuComponent } from '../menu/menu.component';
import { OpenTabsBarComponent } from '../open-tabs/open-tabs-bar.component';
import { OpenTabsService } from '../open-tabs/open-tabs.service';
import { RealtimeService } from '../realtime/realtime.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    MatToolbarModule,
    MatSidenavModule,
    MatMenuModule,
    MatDividerModule,
    MatSnackBarModule,
    MenuComponent,
    OpenTabsBarComponent,
    RouterOutlet
  ],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly openTabs = inject(OpenTabsService);
  private readonly realtime = inject(RealtimeService);
  private readonly snack = inject(MatSnackBar);

  private revokedSub?: Subscription;
  private backendDownSub?: Subscription;

  readonly currentUser = this.auth.currentUser;

  ngOnInit(): void {
    this.realtime.start();
    this.revokedSub = this.realtime.sessionRevoked$.subscribe(() => {
      this.handleSessionRevoked();
    });
    this.backendDownSub = this.realtime.backendDown$.subscribe(() => {
      this.handleBackendDown();
    });
  }

  ngOnDestroy(): void {
    this.revokedSub?.unsubscribe();
    this.backendDownSub?.unsubscribe();
    this.realtime.stop();
  }

  logout(): void {
    this.realtime.stop();
    this.openTabs.clearAll();
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  userSettings(): void {
    this.router.navigateByUrl('/settings/user');
  }

  globalSettings(): void {
    // TODO: nawigacja do ustawień globalnych gdy ekran powstanie
  }

  private handleSessionRevoked(): void {
    this.realtime.stop();
    this.openTabs.clearAll();
    this.auth.logout();
    this.snack.open(
      'Twoja sesja została zakończona, ponieważ zalogowano się z innego miejsca.',
      'OK',
      { duration: 6000 }
    );
    this.router.navigateByUrl('/login');
  }

  private handleBackendDown(): void {
    this.realtime.stop();
    this.openTabs.clearAll();
    this.auth.logout();
    this.snack.open(
      'Utracono połączenie z serwerem. Spróbuj zalogować się ponownie za chwilę.',
      'OK',
      { duration: 0 }
    );
    this.router.navigateByUrl('/login');
  }
}
