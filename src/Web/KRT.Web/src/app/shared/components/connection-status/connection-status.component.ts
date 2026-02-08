import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { SignalRService } from '../../../core/services/signalr.service';

@Component({
  selector: 'app-connection-status',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="connection-indicator" [class]="'status-' + status" [title]="statusText">
      <span class="dot"></span>
      <span class="label">{{ statusText }}</span>
    </div>
  `,
  styles: [`
    .connection-indicator {
      display: inline-flex;
      align-items: center;
      gap: 6px;
      padding: 4px 10px;
      border-radius: 20px;
      font-size: 11px;
      font-weight: 600;
      letter-spacing: 0.5px;
      text-transform: uppercase;
    }
    .dot {
      width: 8px;
      height: 8px;
      border-radius: 50%;
      display: inline-block;
    }
    .status-Connected .dot { background: #00c853; box-shadow: 0 0 6px #00c853; }
    .status-Connected .label { color: #00c853; }
    .status-Connected { background: rgba(0,200,83,0.1); }

    .status-Reconnecting .dot { background: #ff9100; animation: pulse 1s infinite; }
    .status-Reconnecting .label { color: #ff9100; }
    .status-Reconnecting { background: rgba(255,145,0,0.1); }

    .status-Disconnected .dot { background: #9e9e9e; }
    .status-Disconnected .label { color: #9e9e9e; }
    .status-Disconnected { background: rgba(158,158,158,0.1); }

    .status-Error .dot { background: #ff1744; }
    .status-Error .label { color: #ff1744; }
    .status-Error { background: rgba(255,23,68,0.1); }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.3; }
    }
  `]
})
export class ConnectionStatusComponent implements OnInit, OnDestroy {
  status = 'Disconnected';
  statusText = 'Offline';
  private sub!: Subscription;

  private statusMap: Record<string, string> = {
    'Connected': 'Online',
    'Reconnecting': 'Reconectando...',
    'Disconnected': 'Offline',
    'Error': 'Erro'
  };

  constructor(private signalR: SignalRService) {}

  ngOnInit(): void {
    this.sub = this.signalR.connectionState.subscribe(state => {
      this.status = state;
      this.statusText = this.statusMap[state] || state;
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}