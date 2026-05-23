import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { Subscription } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatSidenavModule } from '@angular/material/sidenav';
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

  readonly currentUser = this.auth.currentUser;

  ngOnInit(): void {
    this.realtime.start();
    this.revokedSub = this.realtime.sessionRevoked$.subscribe(() => {
      this.handleSessionRevoked();
    });
  }

  ngOnDestroy(): void {
    this.revokedSub?.unsubscribe();
    this.realtime.stop();
  }

  logout(): void {
    this.realtime.stop();
    this.openTabs.clearAll();
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  settings(): void {

  }

  private handleSessionRevoked(): void {
    // Drop everything in flight, clear local state, force the user back to login.
    // Any unsaved form changes are lost — by design (single-session enforcement).
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
}
