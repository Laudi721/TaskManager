import { Injectable, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Subject, Subscription, filter } from 'rxjs';
import { AuthService } from '../auth/auth.service';

export interface EntityChangedEvent {
  entity: string;
  payload?: unknown;
}

const BACKEND_DOWN_GRACE_PERIOD_MS = 30_000;

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly auth = inject(AuthService);

  private readonly hubUrl = '/hubs/notifications';
  private connection: HubConnection | null = null;

  private readonly _events$ = new Subject<EntityChangedEvent>();
  readonly events$ = this._events$.asObservable();

  private readonly _connected = signal(false);
  readonly connected = this._connected.asReadonly();

  private readonly _reconnected$ = new Subject<void>();
  readonly reconnected$ = this._reconnected$.asObservable();

  private readonly _sessionRevoked$ = new Subject<{ reason?: string }>();
  readonly sessionRevoked$ = this._sessionRevoked$.asObservable();

  private readonly _backendDown$ = new Subject<void>();
  readonly backendDown$ = this._backendDown$.asObservable();

  private backendDownTimer: ReturnType<typeof setTimeout> | null = null;
  private userInitiatedStop = false;

  async start(): Promise<void> {
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      return;
    }

    this.userInitiatedStop = false;

    this.connection = new HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => this.auth.getToken() ?? ''
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: ctx =>
          ctx.elapsedMilliseconds < BACKEND_DOWN_GRACE_PERIOD_MS ? 2000 : null
      })
      .configureLogging(LogLevel.Warning)
      .build();

    this.connection.on('entity-changed', (message: EntityChangedEvent) => {
      this._events$.next(message);
    });

    this.connection.on('session-revoked', (payload: { reason?: string }) => {
      this._sessionRevoked$.next(payload ?? {});
    });

    this.connection.onreconnecting(() => {
      this._connected.set(false);
      this.armBackendDownTimer();
    });

    this.connection.onreconnected(() => {
      this._connected.set(true);
      this.cancelBackendDownTimer();
      this._reconnected$.next();
    });

    this.connection.onclose(() => {
      this._connected.set(false);
      this.cancelBackendDownTimer();
      if (!this.userInitiatedStop) {
        this._backendDown$.next();
      }
      this.userInitiatedStop = false;
    });

    try {
      await this.connection.start();
      this._connected.set(true);
    } catch (err) {
      console.warn('[realtime] connection failed:', err);
      this._connected.set(false);
      this.armBackendDownTimer();
    }
  }

  on(entities: string | string[], callback: (entity: string) => void): Subscription {
    const names = Array.isArray(entities) ? entities : [entities];
    return this.events$
      .pipe(filter(e => names.includes(e.entity)))
      .subscribe(e => callback(e.entity));
  }

  async stop(): Promise<void> {
    this.userInitiatedStop = true;
    this.cancelBackendDownTimer();
    if (!this.connection) {
      return;
    }
    try {
      await this.connection.stop();
    } finally {
      this.connection = null;
      this._connected.set(false);
    }
  }

  private armBackendDownTimer(): void {
    if (this.backendDownTimer) return;
    this.backendDownTimer = setTimeout(() => {
      this.backendDownTimer = null;
      this._backendDown$.next();
    }, BACKEND_DOWN_GRACE_PERIOD_MS);
  }

  private cancelBackendDownTimer(): void {
    if (this.backendDownTimer) {
      clearTimeout(this.backendDownTimer);
      this.backendDownTimer = null;
    }
  }
}
