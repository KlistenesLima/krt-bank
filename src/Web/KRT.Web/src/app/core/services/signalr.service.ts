import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Subject } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

export interface BalanceUpdate {
  accountId: string;
  balance: number;
  timestamp: string;
}

export interface TransactionStatusUpdate {
  accountId: string;
  transactionId: string;
  status: string;
  description: string;
  timestamp: string;
}

export interface AlertNotification {
  accountId: string;
  alertType: string;
  title: string;
  message: string;
  timestamp: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRService implements OnDestroy {
  private hubConnection: signalR.HubConnection | null = null;
  private accountId: string | null = null;

  // === Observables para componentes reagirem ===
  private connectionState$ = new BehaviorSubject<string>('Disconnected');
  private balanceUpdate$ = new Subject<BalanceUpdate>();
  private transactionStatus$ = new Subject<TransactionStatusUpdate>();
  private alert$ = new Subject<AlertNotification>();

  readonly connectionState = this.connectionState$.asObservable();
  readonly balanceUpdates = this.balanceUpdate$.asObservable();
  readonly transactionStatuses = this.transactionStatus$.asObservable();
  readonly alerts = this.alert$.asObservable();

  // Toast notifications queue
  private notifications$ = new Subject<{ type: string; title: string; message: string }>();
  readonly notifications = this.notifications$.asObservable();

  constructor(private authService: AuthService) {}

  /**
   * Inicia conexao SignalR e entra no grupo da conta.
   */
  async startConnection(accountId: string): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected) {
      if (this.accountId === accountId) return;
      await this.leaveGroup();
    }

    this.accountId = accountId;
    const token = this.authService.getToken();

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.apiUrl.replace('/api/v1', '') + '/hubs/transactions', {
        accessTokenFactory: () => token || '',
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.registerHandlers();
    this.registerConnectionEvents();

    try {
      await this.hubConnection.start();
      this.connectionState$.next('Connected');
      // connected

      await this.hubConnection.invoke('JoinAccountGroup', accountId);
      // joined group
    } catch (err) {
      console.error('[SignalR] Connection failed:', err);
      this.connectionState$.next('Error');
    }
  }

  /**
   * Registra handlers para eventos do servidor.
   */
  private registerHandlers(): void {
    if (!this.hubConnection) return;

    this.hubConnection.on('Connected', (data: any) => {
      // server confirmed
    });

    this.hubConnection.on('BalanceUpdated', (data: BalanceUpdate) => {
      // balance updated
      this.balanceUpdate$.next(data);
      this.notifications$.next({
        type: 'info',
        title: 'Saldo Atualizado',
        message: `Novo saldo: R$ ${data.balance.toFixed(2)}`
      });
    });

    this.hubConnection.on('TransactionStatus', (data: TransactionStatusUpdate) => {
      // transaction status
      this.transactionStatus$.next(data);

      const typeMap: Record<string, string> = {
        'Completed': 'success',
        'Rejected': 'error',
        'UnderReview': 'warning'
      };
      this.notifications$.next({
        type: typeMap[data.status] || 'info',
        title: `Pix ${data.status}`,
        message: data.description
      });
    });

    this.hubConnection.on('Alert', (data: AlertNotification) => {
      // alert received
      this.alert$.next(data);
      this.notifications$.next({
        type: data.alertType.includes('fraud') ? 'error' : 'warning',
        title: data.title,
        message: data.message
      });
    });
  }

  /**
   * Registra eventos de conexao/reconexao.
   */
  private registerConnectionEvents(): void {
    if (!this.hubConnection) return;

    this.hubConnection.onreconnecting((error) => {
      console.warn('[SignalR] Reconnecting...', error);
      this.connectionState$.next('Reconnecting');
    });

    this.hubConnection.onreconnected(async (connectionId) => {
      // reconnected
      this.connectionState$.next('Connected');
      if (this.accountId) {
        await this.hubConnection!.invoke('JoinAccountGroup', this.accountId);
      }
    });

    this.hubConnection.onclose((error) => {
      console.warn('[SignalR] Connection closed:', error);
      this.connectionState$.next('Disconnected');
    });
  }

  private async leaveGroup(): Promise<void> {
    if (this.hubConnection?.state === signalR.HubConnectionState.Connected && this.accountId) {
      try {
        await this.hubConnection.invoke('LeaveAccountGroup', this.accountId);
      } catch {}
    }
  }

  async stopConnection(): Promise<void> {
    await this.leaveGroup();
    if (this.hubConnection) {
      await this.hubConnection.stop();
      this.hubConnection = null;
    }
    this.connectionState$.next('Disconnected');
  }

  ngOnDestroy(): void {
    this.stopConnection();
  }
}
