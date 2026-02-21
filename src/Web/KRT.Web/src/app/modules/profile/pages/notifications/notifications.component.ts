import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-notifications',
  template: `
    <div class="notif-container page-with-nav">
      <header class="page-header">
        <button class="back-btn" (click)="router.navigate(['/profile'])">
          <mat-icon>arrow_back</mat-icon>
        </button>
        <h1>Notificacoes</h1>
        <button class="mark-btn" (click)="markAllRead()">
          <mat-icon>done_all</mat-icon>
        </button>
      </header>

      <div class="content fade-in">
        <div *ngFor="let group of groups">
          <div class="date-label">{{ group.label }}</div>
          <div class="notif-card" *ngFor="let n of group.items" [class.unread]="!n.read" (click)="n.read = true">
            <div class="notif-icon" [style.background]="n.color + '12'">
              <mat-icon [style.color]="n.color">{{ n.icon }}</mat-icon>
            </div>
            <div class="notif-body">
              <span class="notif-title">{{ n.title }}</span>
              <span class="notif-desc">{{ n.desc }}</span>
            </div>
            <div class="notif-meta">
              <span class="notif-time">{{ n.time }}</span>
              <div class="unread-dot" *ngIf="!n.read"></div>
            </div>
          </div>
        </div>

        <div class="empty-state" *ngIf="totalCount() === 0">
          <mat-icon>notifications_off</mat-icon>
          <p>Nenhuma notificacao</p>
        </div>
      </div>
    </div>
    <app-bottom-nav></app-bottom-nav>
  `,
  styles: [`
    .notif-container { min-height: 100vh; background: var(--krt-bg); }
    .page-header { display: flex; align-items: center; justify-content: space-between; padding: 16px 20px; background: #fff; border-bottom: 1px solid #F0F0F0; }
    .page-header h1 { font-size: 1.05rem; font-weight: 700; margin: 0; color: #1A1A2E; }
    .back-btn, .mark-btn { background: none; border: none; cursor: pointer; padding: 4px; color: #1A1A2E; display: flex; }
    .mark-btn mat-icon { color: #0047BB; }
    .content { padding: 16px 20px; max-width: 480px; margin: 0 auto; padding-bottom: 100px; }
    .date-label { font-size: 0.75rem; font-weight: 700; color: #9CA3AF; text-transform: uppercase; letter-spacing: 0.5px; margin: 20px 0 10px; padding-left: 4px; }
    .notif-card { display: flex; align-items: center; gap: 14px; background: #fff; border-radius: 16px; padding: 16px; margin-bottom: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.04); cursor: pointer; transition: all 0.2s; }
    .notif-card:hover { box-shadow: 0 4px 16px rgba(0,0,0,0.08); }
    .notif-card.unread { background: #F8FAFF; border-left: 3px solid #0047BB; }
    .notif-icon { width: 46px; height: 46px; border-radius: 14px; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
    .notif-icon mat-icon { font-size: 22px; width: 22px; height: 22px; }
    .notif-body { flex: 1; display: flex; flex-direction: column; gap: 3px; min-width: 0; }
    .notif-title { font-size: 0.9rem; font-weight: 600; color: #1A1A2E; }
    .notif-desc { font-size: 0.8rem; color: #6B7280; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .notif-meta { display: flex; flex-direction: column; align-items: flex-end; gap: 6px; flex-shrink: 0; }
    .notif-time { font-size: 0.72rem; color: #B0B8C4; font-weight: 500; }
    .unread-dot { width: 8px; height: 8px; border-radius: 50%; background: #0047BB; }
    .empty-state { text-align: center; padding: 60px 20px; }
    .empty-state mat-icon { font-size: 48px; width: 48px; height: 48px; color: #D0D5DD; }
    .empty-state p { color: #9CA3AF; margin-top: 8px; }
  `]
})
export class NotificationsComponent {
  constructor(public router: Router) {}

  groups = [
    {
      label: 'Hoje',
      items: [
        { title: 'Pix Recebido', desc: 'Voce recebeu R$ 150,00 de Maria Silva.', icon: 'south_west', color: '#00C853', time: '10:42', read: false },
        { title: 'Acesso Detectado', desc: 'Novo acesso na sua conta pelo dispositivo iPhone 14.', icon: 'security', color: '#7C3AED', time: '08:15', read: false },
      ]
    },
    {
      label: 'Ontem',
      items: [
        { title: 'Compra Aprovada', desc: 'Compra de R$ 89,90 no iFood aprovada.', icon: 'shopping_bag', color: '#FF6B35', time: '19:30', read: true },
        { title: 'Fatura Disponivel', desc: 'Sua fatura de janeiro esta disponivel. Valor: R$ 432,10.', icon: 'receipt_long', color: '#0047BB', time: '14:00', read: true },
      ]
    },
    {
      label: 'Esta semana',
      items: [
        { title: 'Rendimento CDI', desc: 'Seu cofre rendeu R$ 12,45 esta semana.', icon: 'trending_up', color: '#00D4AA', time: 'Seg', read: true },
        { title: 'Pix Enviado', desc: 'Transferencia de R$ 200,00 para Joao Santos.', icon: 'north_east', color: '#E53935', time: 'Seg', read: true },
      ]
    }
  ];

  markAllRead() { this.groups.forEach(g => g.items.forEach(i => i.read = true)); }
  totalCount(): number { return this.groups.reduce((a, g) => a + g.items.length, 0); }
}
